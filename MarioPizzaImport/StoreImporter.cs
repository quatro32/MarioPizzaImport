using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarioPizzaImport
{
    public class StoreImporter : Importer<store>
    {
        public StoreImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

        override protected List<store> Import(string filePath)
        {
            List<store> allStore = new List<store>();

            using (Stream storeStream = new FileStream(filePath, FileMode.Open))
            {
                using (StreamReader storeReader = new StreamReader(storeStream))
                {
                    List<string> allLineStoreInformation = new List<string>();
                    string lineStoreInformation = null;

                    while ((lineStoreInformation = storeReader.ReadLine()) != null)
                    {
                        lineStoreInformation = lineStoreInformation.Trim();

                        if (!lineStoreInformation.StartsWith("--") && lineStoreInformation.Length > 0)
                        {
                            allLineStoreInformation.Add(lineStoreInformation);
                        }
                        else if (lineStoreInformation.Length == 0 && allLineStoreInformation.Count > 0)
                        {
                            // Line is empty, means we encounter a new store.
                            allStore.Add(CreateStoreFromAllLine(allLineStoreInformation));
                            allLineStoreInformation.Clear();
                        }
                    }

                    if (allLineStoreInformation.Count != 0)
                    {
                        allStore.Add(CreateStoreFromAllLine(allLineStoreInformation));
                        allLineStoreInformation.Clear();
                    }
                }
            }
            
            List<string> allStoreName = new List<string>();
            allStore.ForEach(s => allStoreName.Add(s.name));

            // Fetch all existing stores.
            List<store> allStoreExisting = database.stores.Where(s => allStoreName.Contains(s.name)).ToList();
            List<store> allStoreNew = allStore.Where(s => allStoreExisting.Where(existing => existing.name.Equals(s.name)).Count() == 0).ToList();

            // Update existing stores.
            allStoreExisting.ForEach(storeExisting => UpdateStoreExisting(storeExisting, allStore));

            // Import.
            database.stores.AddRange(allStoreNew);
            database.SaveChanges();

            Console.WriteLine("Found {0} existing stores", allStoreExisting.Count);
            Console.WriteLine("Found {0} new stores", allStoreNew.Count);

            allStoreNew.ForEach(s => Console.WriteLine("Imported new store {0}", s.name));

            return allStore;
        }

        static void UpdateStoreExisting(store storeExisting, List<store> allStoreToImport)
        {
            bool isStoreUpdated = false;
            store storeToImport = allStoreToImport.Where(s => s.name == storeExisting.name).First();

            if (!storeExisting.phonenumber.Equals(storeToImport.phonenumber))
            {
                storeExisting.phonenumber = storeToImport.phonenumber;
                isStoreUpdated = true;
            }


            if (!storeExisting.address.postalcode.postalcode1.Equals(storeToImport.address.postalcode.postalcode1))
            {
                storeExisting.address = storeToImport.address;
                isStoreUpdated = true;
            }

            if (isStoreUpdated)
            {
                Console.WriteLine("Updated store {0} with new details.", storeExisting.name);
            }
        }

        store CreateStoreFromAllLine(List<string> allLineStoreInformation)
        {
            if (!allLineStoreInformation.Count.Equals(7))
            {
                Console.WriteLine("Store {0} has incomplete details.", allLineStoreInformation[0]);
            }

            store store = new store();
            store.name = allLineStoreInformation[0];
            store.address = CreateAddress(
                allLineStoreInformation[1], // street
                allLineStoreInformation[2], // housenumber
                allLineStoreInformation[3], // city
                allLineStoreInformation[4], // country
                allLineStoreInformation[5]  // postalCode
            );
            store.phonenumber = FormatPhoneNumber(allLineStoreInformation[6]);

            return store;
        }

        address CreateAddress(string street, string houseNumber, string city, string country, string postalCode)
        {
            address address = new address();
            address.postalcode = GetOrCreatePostalCode(street, postalCode, city);
            address.number = houseNumber;
            address.countrycode = ValidateCountryCode(country);

            return address;
        }

        postalcode GetOrCreatePostalCode(string street, string postalCodeString, string city)
        {
            string postalCodeFormatted = FormatPostalCode(postalCodeString);
            postalcode postalcode = this.database.postalcodes.Where(p => p.postalcode1 == postalCodeFormatted).FirstOrDefault();
            
            if (postalcode == null)
            {
                postalcode = new postalcode();
                postalcode.postalcode1 = postalCodeString;
                postalcode.city = FormatCity(city);
                postalcode.street = street;

                // TODO: Winkels hebben niet standaard een gemeente. Als postcodes zijn geimporteerd kunnen we een error gooien als we de postcode en dus township niet kenne.
                township township = new township();
                township.name = FormatCity(city);

                postalcode.township = township;
            }  else if (postalcode.street != street) {
                Console.WriteLine("Stret {0} in postalcode table isn't equal to {1} in import file.", postalcode.street, street);
            }

            return postalcode;
        }

        private static string FormatCity(string city)
        {
            List<string> allPart = city.Trim().ToLower().Split(' ').ToList();
            List<string> allPartUppercased = new List<string>();

            allPart.ForEach(p => allPartUppercased.Add(p.Substring(0, 1).ToUpper() + p.Substring(1)));
            
            return String.Join(" ", allPartUppercased);
        }

        private static string FormatPostalCode(string postalCode)
        {
            string regexPostalCode = "[0-9]{4}[A-Z]{2}";

            if (Regex.IsMatch(postalCode, regexPostalCode))
            {
                return postalCode;
            }
            else
            {
                string postalCodeRepaired = postalCode.Replace(" ", "");

                if (Regex.IsMatch(postalCodeRepaired, regexPostalCode))
                {
                    Console.WriteLine("Repaired postcode. Imported {0} as {1}.", postalCode, postalCodeRepaired);

                    return postalCodeRepaired;
                }
                else
                {
                    Console.WriteLine("Postal Code {0} is invalid and could not be repaired.", postalCode);

                    throw new Exception();
                }
            }
        }
        private static string FormatPhoneNumber(string phoneNumber)
        {
            string regexPhoneNumber = "[^0-9]+";

            return Regex.Replace(phoneNumber, regexPhoneNumber, "");
        }

        private static string ValidateCountryCode(string countryCode)
        {
            string regexPostalCode = "[A-Z]{2}";

            if (Regex.IsMatch(countryCode, regexPostalCode))
            {
                return countryCode;
            }
            else
            {
                Console.WriteLine("Country Code {0} is invalid and could not be repaired.", countryCode);

                throw new Exception();
            }
        }
    }
}
