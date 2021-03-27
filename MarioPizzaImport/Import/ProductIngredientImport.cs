using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarioPizzaImport.Import
{
    class ProductIngredientImport : Importer<productingredient>
    {
        public ProductIngredientImport(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode)
        {
            this.productImporter = new ProductImporter(database, countrycode);    
        }
        private ProductImporter productImporter;

        override protected int Import(string filePath)
        {
            List<productingredient> allIngredients = new List<productingredient>();
            List<DataRow> allLineIngredientInformation = new List<DataRow>();

            // Create Table
            DataTable productIngredients = new DataTable();

            // Begin Excel
            using (OleDbConnection localDbConnection = new OleDbConnection(string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=Excel 12.0;", filePath)))
            {
                localDbConnection.Open();

                OleDbDataAdapter dataAdapter = new OleDbDataAdapter("select * from [Sheet1$]", localDbConnection);
                dataAdapter.Fill(productIngredients);

                localDbConnection.Close();
            }
            // End Excel

            foreach (DataRow row in productIngredients.Rows)
            {
                 allLineIngredientInformation.Add(row);

                if (allLineIngredientInformation.Count != 0)
                {
                    foreach (DataRow lineInformation in allLineIngredientInformation)
                    {
                        productingredient productIngredient = CreateIngredientFromAllLine(lineInformation);
                        if (productIngredient != null)
                        {
                            allIngredients.Add(productIngredient);
                        }
                    }
                }
            }
           
            List<string> allIngredientNames = new List<string>();
            allIngredients.ForEach(s => allIngredientNames.Add(s.ingredient.name));

            List<productingredient> allExistingIngredients = database.productingredients.Where(s => allIngredientNames.Contains(s.ingredient.name)).ToList();
            List<productingredient> allNewIngredients = allIngredients.Where(s => allExistingIngredients.Where(existing => existing.ingredient.name.Equals(s.ingredient.name)).Count() == 0).ToList();

            database.productingredients.AddRange(allIngredients);
            database.SaveChanges();

            Console.WriteLine("Found {0} existing ingredients", allExistingIngredients.Count);
            Console.WriteLine("Found {0} new ingredients", allNewIngredients.Count);

            allNewIngredients.ForEach(s => Console.WriteLine("Imported new ingredient {0}", s.ingredient.name));

            return allIngredients.Count;
        }

        productingredient CreateIngredientFromAllLine(DataRow lineInformation)
        {

            string productName = (string)lineInformation.ItemArray[2];
            string ingredientName = (string)lineInformation.ItemArray[10];
            int ingredientAmount = Convert.ToInt32(lineInformation.ItemArray[9]);

            product product = database.products.SingleOrDefault(s => s.name == productName);
            ingredient ingredient = database.ingredients.SingleOrDefault(s => s.name == ingredientName);

            if (product == null)
            {
                product = GetMappedProduct(productName);
                if (product == null)
                {
                    product = this.createPizzaProduct(lineInformation);
                }
            }

            if (ingredient == null)
            {
                ingredient = GetMappedIngredient(ingredientName);
                if (ingredient == null)
                {
                    Logger.Instance.LogError(filePath, String.Format("Could not find ingredient named {0} to add to product {1}", ingredientName, productName));
                    return null;
                }
            }

            productingredient productingredient = new productingredient();

            productingredient.product = product;
            productingredient.amount = ingredientAmount;

            productingredient.ingredient = ingredient;

            return productingredient;
        }

        /**
         * Create pizza products from the pizzaIngredient table. Also imports the sauce since the pizza_ingredient import file
         * is the only source for both sauces and pizzas.
         */
        private product createPizzaProduct(DataRow lineInformation)
        {
            productcategory category = this.productImporter.findOrCreateCategory((string)lineInformation.ItemArray[0]);
            product product = new product();
            product.name = (string)lineInformation.ItemArray[2];
            product.productcategory1 = this.productImporter.findOrCreateCategory((string)lineInformation.ItemArray[1], category);
            product.description = (string)lineInformation.ItemArray[3];
            product.isspicy = lineInformation.ItemArray[6].ToString().ToUpper() == "JA" ;
            product.isvegetarian = lineInformation.ItemArray[7].ToString().ToUpper() == "JA";

            productprice price = new productprice();
            price.countrycode = this.countrycode;
            price.price = Decimal.Parse(Regex.Replace((string)lineInformation.ItemArray[4], "[^0-9.]", "")); ;
            price.product = product;

            sauce sauce = new sauce();
            sauce.name = (string)lineInformation.ItemArray[11];
            product.sauce = sauce;

            database.products.Add(product);
            Console.WriteLine(
                "New pizza {0} created with sauce {1}",
                (string)lineInformation.ItemArray[2],
                (string)lineInformation.ItemArray[11]
            );
            return product;
        }
    }
}