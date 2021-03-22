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

            InsertExtraIngredients(@"C:\Users\Peter\Downloads\MarioData\Extra ingredienten.csv", database, countrycode);

            BottomImporter bottomImporter = new BottomImporter(database, countrycode);
            bottomImporter.Run(@"C:\Users\Peter\Downloads\MarioData\pizzabodems.csv");

            ProductImporter productImporter = new ProductImporter(database, countrycode);
            productImporter.Run(@"C:\Users\Peter\Downloads\MarioData\overige producten.csv");

            //Import the relation between pizza and ingredients.
            IngredientImporter ingredientImporter = new IngredientImporter(database, countrycode);
            ingredientImporter.Run(@"C:\Users\Peter\Downloads\MarioData\pizza_ingredienten.csv");

            ExtraIngredientImporter extraIngredientImporter = new ExtraIngredientImporter(database, countrycode);
            extraIngredientImporter.Run(@"C:\Users\Peter\Downloads\MarioData\Extra Ingredienten.csv");

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

        static void InsertExtraIngredients(string path, dbi298845_prangersEntities db, countrycode countrycode)
        {
            
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
