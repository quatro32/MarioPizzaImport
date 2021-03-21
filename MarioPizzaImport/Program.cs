using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarioPizzaImport
{
    class Program
    {
        static void Main(string[] args)
        {
            var database = new dbi298845_prangersEntities();
            countrycode countrycode = getOrCreateDefaultCountryCode(database);

            // MatchStreets(@"C:\Users\Jos\Downloads\MarioData\MarioOrderData01_10000.csv", database);

            //PostalCodeImporter postalCodeImporter = new PostalCodeImporter(database, countrycode);
            //postalCodeImporter.Run(@"C:\Users\Jos\Downloads\MarioData\Postcode tabel.mdb");

            StoreImporter storeImporter = new StoreImporter(database, countrycode);
            storeImporter.Run(@"C:\Users\Jos\Downloads\MarioData\Winkels Mario.txt");

            ////InsertExtraIngredients(@"C:\Users\shnva\Desktop\ingredienten.csv", database, countrycode);
            //InsertBottoms(@"C:\Users\shnva\Desktop\pizzabodems.csv", database, countrycode);
            //InsertProducts(@"C:\Users\shnva\Desktop\Overige producten.csv", database, countrycode);
           
            Console.WriteLine("Done...");
            Console.ReadKey();
        }

        private static void MatchStreets(string filePath, dbi298845_prangersEntities database)
        {
            List<postalcode> allPostalCodeWithOrder = new List<postalcode>();
            List<string> allStreetWithoutMatch = new List<string>();

            int current = 0;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string lineCurrent = null;

                while ((lineCurrent = reader.ReadLine()) != null)
                {
                    string[] allLinePart = lineCurrent.Split(';');

                    if (allLinePart[0].Length > 0 && allLinePart.Length > 5)
                    {
                        string streetWithHouseNumber = allLinePart[4];
                        string city = allLinePart[5];

                        postalcode postalcode = PostalCodeImporter.FindPostalCodeByStreetAndCity(streetWithHouseNumber, city, database);

                        if (postalcode == null)
                        {
                            allStreetWithoutMatch.Add(streetWithHouseNumber);
                        }
                        else
                        {
                            //Console.WriteLine("Found match");
                            allPostalCodeWithOrder.Add(postalcode);
                        }

                        current += 1;

                        if (current % 1000 == 0)
                        {
                            //Console.WriteLine("Did another 1000");

                            Console.WriteLine("Got {0} matches", allPostalCodeWithOrder.Count);
                            Console.WriteLine("Got {0} failures", allStreetWithoutMatch.Count);
                        } 
                    }
                }
            }

            Console.WriteLine("Got {0} matches", allPostalCodeWithOrder.Count);
            Console.WriteLine("Got {0} failures", allStreetWithoutMatch.Count);
        }

        static void InsertBottoms(string path, dbi298845_prangersEntities db, countrycode countrycode)
        {

            using (StreamReader sr = new StreamReader(path))
            {
                String line;
                bool isHeaderLine = true;
                while ((line = sr.ReadLine()) != null)
                {
                    if (isHeaderLine)//skip first line in csv file, since it's an header line. we don't want that values
                    {
                        isHeaderLine = false;
                    }
                    else
                    {
                        //split all lines in csv to values
                        string[] parts = line.Split(';');
                        string name = parts[0];
                        string diameter = parts[1];
                        string description = parts[2];
                        decimal price = Convert.ToDecimal(parts[3]);

                        //check if bottom exists by it's name
                        var bottom = db.bottoms.SingleOrDefault(i => i.name == name);
                        if (bottom == null)//if not, create a new one
                        {
                            bottom = new bottom()
                            {
                                name = name,
                                diameter = Convert.ToInt32(diameter),
                                //description has to be added to database,
                            };

                            //if a bottom doesn't exists ALWAYS create a bottomprice, since it hasn't got any
                            var bottomprice = new bottomprice()
                            {
                                bottom = bottom,
                                countrycode = countrycode,
                                currency = "EUR",
                                startdate = DateTime.Now,
                                vat = 9.0m,
                                price = price
                            };

                            db.bottomprices.Add(bottomprice);
                            db.SaveChanges();
                            Console.WriteLine("1 bottom and bottomprice added...");
                        }
                    }
                }
            }
        }

        static void InsertExtraIngredients(string path, dbi298845_prangersEntities db, countrycode countrycode)
        {

            using (StreamReader sr = new StreamReader(path))
            {
                String line;
                bool isHeaderLine = true;
                while ((line = sr.ReadLine()) != null)
                {
                    if (isHeaderLine)
                    {
                        isHeaderLine = false;
                    }
                    else
                    {
                
                        string[] parts = line.Split(';');
                        string name = parts[0];
                        decimal amount = Convert.ToDecimal(parts[1]);
           

                        // Controleren op het ingredient al voorkomt
                        var ingredient = db.ingredients.SingleOrDefault(i => i.name == name);
                        if (ingredient == null)
                        {
                            ingredient = new ingredient()
                            {
                               name = name
                            };


                            var ingredientprice = new ingredientprice()
                            {
                                ingredient = ingredient,
                                startdate = DateTime.Now,
                                vat = 9.0m,
                                price = amount,
                                currency = "EUR",
                                countrycode = countrycode

                            };
                        

                            db.ingredientprices.Add(ingredientprice);
                            db.SaveChanges();
                            Console.WriteLine("Ingredient added");
                        }
                    }
                }
            }
        }

        private static countrycode getOrCreateDefaultCountryCode(dbi298845_prangersEntities db)
        {
            //check if countrycode exists, so we can create a bottomprice for NL stores
            var countrycode = db.countrycodes.SingleOrDefault(i => i.code == "NL");
            if (countrycode == null) // if it doesnt exists, create a new countrycode
            {
                countrycode = new countrycode()
                {
                    code = "NL"
                };

                db.countrycodes.Add(countrycode);
                db.SaveChanges();
            }

            return countrycode;
        }
    }
}
