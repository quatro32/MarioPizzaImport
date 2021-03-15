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

            PostalCodeImporter postalCodeImporter = new PostalCodeImporter(database, countrycode);
            postalCodeImporter.Run(@"C:\Users\shnva\Desktop\Postcode tabel.mdb");

            StoreImporter storeImporter = new StoreImporter(database, countrycode);
            storeImporter.Run(@"C:\Users\shnva\Desktop\Winkels Mario.txt");

            InsertBottoms(@"C:\Users\shnva\Desktop\pizzabodems.csv", database, countrycode);
            InsertProducts(@"C:\Users\shnva\Desktop\Overige producten.csv", database, countrycode);

            Console.WriteLine("Done...");
            Console.ReadKey();
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
                    //categorie;subcategorie;productnaam;productomschrijving;prijs;spicy;vegetarisch
                    string[] parts = line.Split(';');
                    string categoryName = parts[0];
                    string subcategoryName = parts[1];
                    string name = parts[2];
                    string description = parts[3];
                    Decimal price = Decimal.Parse(Regex.Replace(parts[4], "[^0-9.]", ""));
                    bool spicy = parts[5].ToUpper() == "JA";
                    bool vegetarian = parts[6].ToUpper() == "JA";

                    // Retrieve or create the top level category
                    productcategory category = database.productcategories.SingleOrDefault(i => i.name == categoryName);
                    if(category == null)
                    {
                        category = new productcategory();
                        category.name = categoryName;
                        database.productcategories.Add(category);
                        Console.WriteLine("Added new top level category called {0}", categoryName);
                    }

                    // Retrieve or create the subcategory
                    productcategory subcategory = database.productcategories.SingleOrDefault(i => i.name == subcategoryName && i.parentproductcategoryid == category.id);
                    if(subcategory == null)
                    {
                        subcategory = new productcategory();
                        subcategory.name = subcategoryName;
                        subcategory.parentproductcategoryid = category.id;
                        database.productcategories.Add(subcategory);
                        Console.WriteLine("Added new child to category {0} called {1}", categoryName, subcategoryName);
                    }

                    product product = database.products.SingleOrDefault(i => i.name == name);
                    if(product == null)
                    {
                        product = new product();
                        product.name = name;
                        // We are missing a description field on product.
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
                        // We are missing a description field on product.
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
                    database.SaveChanges();
                }
            }
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
