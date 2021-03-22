using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
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
            storeImporter.Run(@"C:\Users\shnva\Desktop\\Winkels Mario.txt");

            BottomImporter bottomImporter = new BottomImporter(database, countrycode);
            bottomImporter.Run(@"C:\Users\shnva\Desktop\pizzabodems.xlsx");

            InsertExtraIngredients(@"C:\Users\shnva\Desktop\ingredienten.csv", database, countrycode);
            InsertProducts(@"C:\Users\shnva\Desktop\Overige producten.csv", database, countrycode);

            Console.WriteLine("Done...");
            Console.ReadKey();
        }

        DataTable LoadWorksheetInDataTable(string fileName, string sheetName)
        {
            DataTable sheetData = new DataTable();
            using (OleDbConnection conn = this.returnConnection(fileName))
            {
                conn.Open();
                // retrieve the data using data adapter
                OleDbDataAdapter sheetAdapter = new OleDbDataAdapter("select * from [" + sheetName + "$]", conn);
                sheetAdapter.Fill(sheetData);
                conn.Close();
            }
            return sheetData;
        }

        private OleDbConnection returnConnection(string fileName)
        {
            return new OleDbConnection();
        }

        private static void InsertProducts(string path, dbi298845_prangersEntities database, countrycode countrycode)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                // Skip the header line.
                sr.ReadLine();
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    //categorie;subcategorie;productnaam;productomschrijving;prijs;spicy;vegetarisch;available;amount;name
                    string[] parts = line.Split(';');
                    string categoryName = parts[0];
                    string subcategoryName = parts[1];
                    string name = parts[2];
                    string description = parts[3];
                    Decimal price = Decimal.Parse(Regex.Replace(parts[4], "[^0-9.]", ""));
                    bool spicy = parts[5].ToUpper() == "JA";
                    bool vegetarian = parts[6].ToUpper() == "JA";

                    int ingredientAmount = Convert.ToInt32(parts[8]);
                    string ingredientName = parts[9];

                    // Retrieve or create the top level category
                    productcategory category = database.productcategories.SingleOrDefault(i => i.name == categoryName);
                    if (category == null)
                    {
                        category = new productcategory();
                        category.name = categoryName;
                        database.productcategories.Add(category);
                        Console.WriteLine("Added new top level category called {0}", categoryName);
                    }

                    // Retrieve or create the subcategory
                    productcategory subcategory = database.productcategories.SingleOrDefault(i => i.name == subcategoryName && i.parentproductcategoryid == category.id);
                    if (subcategory == null)
                    {
                        subcategory = new productcategory();
                        subcategory.name = subcategoryName;
                        subcategory.parentproductcategoryid = category.id;
                        database.productcategories.Add(subcategory);
                        Console.WriteLine("Added new child to category {0} called {1}", categoryName, subcategoryName);
                    }

                    product product = database.products.SingleOrDefault(i => i.name == name);
                    if (product == null)
                    {
                        product = new product();
                        product.name = name;
                        //product.description = description;
                        product.isspicy = spicy;
                        product.isvegetarian = vegetarian;
                        product.productcategory = subcategory.id;
                        database.products.Add(product);

                        // Create a productprice for the current price.
                        productprice productprice = new productprice();
                        productprice.product = product;
                        productprice.price = price;
                        productprice.startdate = DateTime.Now;
                        database.productprices.Add(productprice);
                        Console.WriteLine("New product {0} created", name);
                    }
                    // If the product already exists, update every field except name, this includes creating a new price.
                    else
                    {
                        //product.description = description;
                        product.isspicy = spicy;
                        product.isvegetarian = vegetarian;
                        product.productcategory = subcategory.id;

                        // Create a productprice for the current price.
                        productprice productprice = new productprice();
                        productprice.product = product;
                        productprice.price = price;
                        productprice.startdate = DateTime.Now;
                        database.productprices.Add(productprice);
                        Console.WriteLine("Updating existing product {0}", name);
                    }

                    ingredient ingredient = database.ingredients.SingleOrDefault(i => i.name == ingredientName);


                    if (ingredient == null)
                    {
                        Console.WriteLine("Cannot resove {0} from ingredients", ingredient);

                    }
                    else
                    {

                        var productingredient = new productingredient();
                        productingredient.ingredient = ingredient;
                        productingredient.product = product;
                        productingredient.amount = ingredientAmount;
                        Console.WriteLine("Added {0} to {1}", ingredient.name, product.name);
                        database.productingredients.Add(productingredient);
                    }
                    database.SaveChanges();
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
