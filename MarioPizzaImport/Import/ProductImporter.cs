using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarioPizzaImport.Import
{
    class ProductImporter : Importer<product>
    {
        public ProductImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

        protected override int Import(string path)
        {
            int insertedProducts = 0;

            DataTable productTable = new DataTable();

            // Begin Excel
            using (OleDbConnection localDbConnection = new OleDbConnection(string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=Excel 12.0;", filePath)))
            {
                localDbConnection.Open();

                OleDbDataAdapter dataAdapter = new OleDbDataAdapter("select * from [Sheet1$]", localDbConnection);
                dataAdapter.Fill(productTable);

                localDbConnection.Close();
            }

            foreach (DataRow row in productTable.Rows)
            {
                //categorie;subcategorie;productnaam;productomschrijving;prijs;spicy;vegetarisch;available;amount;name
                if (row.ItemArray.Length < 7)
                {
                    Logger.Instance.LogError(path, "Could not create product for line: " + row.ToString());
                    continue;
                }
                string categoryName = (string) row.ItemArray[0];
                string subcategoryName = (string) row.ItemArray[1];
                string name = (string)row.ItemArray[2];
                string description = row.ItemArray[3].ToString();
                Decimal price = Decimal.Parse(Regex.Replace(Convert.ToString(row.ItemArray[4]).Replace(".", ","), "[^0-9,]", ""));
                bool spicy = ((string)row.ItemArray[5]).ToUpper() == "JA";
                bool vegetarian = ((string) row.ItemArray[6]).ToUpper() == "JA";

                productcategory category = findOrCreateCategory(categoryName);
                productcategory subcategory = findOrCreateCategory(subcategoryName, category);

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

            return insertedProducts;
        }

        /**
         * find or create a category. If a category is supplied for the second parameter, use it as a parent category.
         */
        public productcategory findOrCreateCategory(string categoryName, productcategory category = null)
        {
            if(category == null)
            {
                // Retrieve or create the top level category
                category = database.productcategories.SingleOrDefault(i => i.name == categoryName);
                if (category == null)
                {
                    category = new productcategory();
                    category.name = categoryName;
                    database.productcategories.Add(category);
                    Console.WriteLine("Added new top level category called {0}", categoryName);
                    database.SaveChanges();
                    category = database.productcategories.SingleOrDefault(i => i.name == categoryName);
                }

                return category;
            }

                // Retrieve or create the subcategory
                productcategory subcategory = database.productcategories.SingleOrDefault(i => i.name == categoryName && i.parentproductcategoryid == category.id);
                if (subcategory == null)
                {
                    subcategory = new productcategory();
                    subcategory.name = categoryName;
                    subcategory.parentproductcategoryid = category.id;
                    database.productcategories.Add(subcategory);
                    Console.WriteLine("Added new child to category {0} called {1}", category.name, categoryName);
                    database.SaveChanges();
                    subcategory = database.productcategories.SingleOrDefault(i => i.name == categoryName && i.parentproductcategoryid == category.id);
                }
            return subcategory;
        }
    }
}
