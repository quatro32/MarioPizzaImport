using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarioPizzaImport
{
    class Program
    {
        private static int GetMonthNumberFromString(string monthString)
        {
            switch (monthString.ToLower())
            {
                case "januari":
                    return 1;
                case "februari":
                    return 2;
                case "maart":
                    return 3;
                case "april":
                    return 4;
                case "mei":
                    return 5;
                case "juni":
                    return 6;
                case "juli":
                    return 7;
                case "augustus":
                    return 8;
                case "september":
                    return 9;
                case "oktober":
                    return 10;
                case "november":
                    return 11;
                case "december":
                    return 12;
                default:
                    return 1;
            }
        }

        private static DateTime GetDateTimeFromLongDateString(string dateString)
        {
            string[] paths = dateString.Split(' ');
            return new DateTime(Convert.ToInt32(paths[3]), GetMonthNumberFromString(paths[2]), Convert.ToInt32(paths[1]));
        }

        private static DateTime GetDateTimeFromLongDateString(string dateString, string timeString)
        {
            return GetDateTimeFromLongDateString(dateString) + TimeSpan.Parse(timeString);
        }

        private static void ImportOrders(string path, dbi298845_prangersEntities db)
        {
            //Winkelnaam;Klantnaam;TelefoonNr;Email;Adres;Woonplaats;Besteldatum;AfleverType;AfleverDatum;AfleverMoment;Product;PizzaBodem;PizzaSaus;Prijs;Bezorgkosten;Aantal;Extra Ingrediënten;Prijs Extra Ingrediënten;Regelprijs;Totaalprijs;Gebruikte Coupon;Coupon Korting;Te Betalen
            List<order> orders = new List<order>();

            using (StreamReader sr = new StreamReader(path))
            {
                int row = 1;
                String line;
                order order = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if(row > 6 && line != string.Empty)
                    {
                        string[] paths = line.Split(';');
                        if (paths[0] != string.Empty)
                        {
                            order = new order();

                            store store = db.stores.SingleOrDefault(i => i.name.ToUpper() == paths[0].ToUpper());
                            order.store = store ?? throw new Exception(string.Format("Store {0} does not exists!", paths[0]));

                            order.clientname = paths[1];
                            order.phonenumber = paths[2];
                            //add email field to order table, path[3]
                            //add address entity to order, get postalcode from database by using a lookup query, paths[4],paths[5]
                            order.datecreated = GetDateTimeFromLongDateString(paths[6]);

                            deliverytype deliverytype = db.deliverytypes.SingleOrDefault(i => i.name.ToUpper() == paths[7].ToUpper());
                            order.deliverytype = deliverytype ?? throw new Exception(string.Format("Deliverytype {0} does not exists!", paths[7]));

                            order.datedelivered = GetDateTimeFromLongDateString(paths[8], paths[9]);
                            //add deliverycosts to order table
                        }

                        orderline orderline = new orderline();
                        orderline.order = order;

                        //search if product exists
                        product product = db.products.SingleOrDefault(i => i.name.ToUpper() == paths[10].ToUpper());
                        orderline.product = product ?? throw new Exception(string.Format("Product {0} does not exists!", paths[10]));
                        //after we delete exception, create product and also create/select product type

                        orderline.amount = Convert.ToInt32(paths[15]);

                        string[] extraIngredients = paths[16].Split(',');
                        if (extraIngredients.Length > 0)
                        {
                            foreach(var ei in extraIngredients)
                            {
                                ingredient ingredient = db.ingredients.SingleOrDefault(i => i.name.ToUpper() == ei.Trim().ToUpper());
                                //TODO: look for mapping, else create new/exception
                                productorderingredient productorderingredient = new productorderingredient();
                                productorderingredient.ingredient = ingredient ?? throw new Exception(string.Format("Product {0} does not exists!", ei));
                                productorderingredient.orderline = orderline;
                            }
                        }

                    }
                    row++;
                }
            }

            //db.BulkInsert<order>(orders, );
        }

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
