using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarioPizzaImport
{
    class Program
    {
        static void Main(string[] args)
        {
            var database = new dbi298845_prangersEntities();
            countrycode countrycode = getOrCreateDefaultCountryCode(database);

            PostalCodeImporter postalCodeImporter = new PostalCodeImporter(database, countrycode);
            postalCodeImporter.Run(@"C:\Users\shnva\Desktop\Postcode tabel.mdb");

            StoreImporter storeImporter = new StoreImporter(database, countrycode);
            storeImporter.Run(@"C:\Users\shnva\Desktop\Winkels Mario.txt");

            //Import single ingredients.
            InsertExtraIngredients(@"C:\Users\Peter\Downloads\MarioData\Extra ingredienten.csv", database, countrycode);

            //Import the relation between pizza and ingredients.
            IngredientImporter ingredientImporter = new IngredientImporter(database, countrycode);
            ingredientImporter.Run(@"C:\Users\Peter\Downloads\MarioData\pizza_ingredienten.csv");

            //InsertBottoms(@"C:\Users\Peter\Downloads\MarioData\pizzabodems.csv", database, countrycode);
            ProductImporter productImporter = new ProductImporter(database, countrycode);
            productImporter.Run(@"C:\Users\Peter\Downloads\MarioData\overige producten.csv");

            /**
             * Once all other changes are merged, we need to make this into a proper console application with multiple commands. Since the mapping should
             * be executed after importing products and ingredients.
             * Then the records in the mapping table need to be mapped by hand before orders are imported.
             */
            MappingParser mappingParser = new MappingParser(database);
            mappingParser.ParseMappingFromOrderFile(@"C:\Users\Peter\Downloads\MarioData\MarioOrderData01_10000.csv");

            Console.WriteLine("Done...");
            Console.ReadKey();
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
                        Decimal price = Decimal.Parse(Regex.Replace(parts[3], "[^0-9.]", ""));

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
                        decimal price = Decimal.Parse(Regex.Match(parts[1], @"[0-9]+(\.[0-9]+)?").Value);

                        // Controleren of het ingredient al voorkomt
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
                                price = price,
                                currency = "EUR",
                                countrycode = countrycode
                            };

                            ingredient.ingredientprices.Add(ingredientprice);
                            db.ingredients.Add(ingredient);
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
