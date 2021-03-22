using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarioPizzaImport
{
    class ProductImporter : Importer<product>
    {
        public ProductImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

        protected override int Import(string path)
        {
            int insertedProducts = 0;

            using (StreamReader sr = new StreamReader(path))
            {
                // Skip the header line.
                sr.ReadLine();
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    //categorie;subcategorie;productnaam;productomschrijving;prijs;spicy;vegetarisch;available;amount;name
                    string[] parts = line.Split(';');
                    if (parts.Length < 7)
                    {
                        Logger.Instance.LogError(path, "Could not create product for line containing data: " + line);
                        continue;
                    }
                    string categoryName = parts[0];
                    string subcategoryName = parts[1];
                    string name = parts[2];
                    string description = parts[3];
                    Decimal price = Decimal.Parse(Regex.Replace(parts[4], "[^0-9.]", ""));
                    bool spicy = parts[5].ToUpper() == "JA";
                    bool vegetarian = parts[6].ToUpper() == "JA";

                    // Retrieve or create the top level category
                    productcategory category = database.productcategories.SingleOrDefault(i => i.name == categoryName);
                    if (category == null)
                    {
                        category = new productcategory();
                        category.name = categoryName;
                        database.productcategories.Add(category);
                        Console.WriteLine("Added new top level category called {0}", categoryName);
                        database.SaveChanges();
                        category = database.productcategories.SingleOrDefault(i => i.name == categoryName);
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
                        database.SaveChanges();
                        subcategory = database.productcategories.SingleOrDefault(i => i.name == subcategoryName && i.parentproductcategoryid == category.id);
                    }

                    product product = database.products.SingleOrDefault(i => i.name == name);
                    if (product == null)
                    {
                        product = new product();
                        product.name = name;
                        product.description = description;
                        product.isspicy = spicy;
                        product.isvegetarian = vegetarian;
                        product.productcategory = subcategory.id;
                        database.products.Add(product);

                        // Create a productprice for the current price.
                        productprice productprice = new productprice();
                        productprice.product = product;
                        productprice.price = price;
                        productprice.currency = "EUR";
                        productprice.startdate = DateTime.Now;
                        productprice.countrycode = countrycode;
                        database.productprices.Add(productprice);
                        Console.WriteLine("New product {0} created", name);

                        insertedProducts++;
                    }
                    // If the product already exists, update every field except name, this includes creating a new price.
                    else
                    {
                        product.description = description;
                        product.isspicy = spicy;
                        product.isvegetarian = vegetarian;
                        product.productcategory = subcategory.id;

                        // Create a productprice for the current price.
                        productprice productprice = new productprice();
                        productprice.product = product;
                        productprice.price = price;
                        productprice.currency = "EUR";
                        productprice.startdate = DateTime.Now;
                        productprice.countrycode = countrycode;
                        database.productprices.Add(productprice);
                        Console.WriteLine("Updating existing product {0}", name);
                    }
                    database.SaveChanges();
                }
            }
            return insertedProducts;
        }
    }
}
