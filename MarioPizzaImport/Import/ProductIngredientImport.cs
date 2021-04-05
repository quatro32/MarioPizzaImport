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
                productingredient productIngredient = GetOrCreateIngredientFromLine(row);
                if (productIngredient != null)
                {
                    allIngredients.Add(productIngredient);
                }
            }
           
            List<string> allIngredientNames = new List<string>();
            allIngredients.ForEach(s => allIngredientNames.Add(s.ingredient.name));

            List<productingredient> allExistingIngredients = database.productingredients.Where(s => allIngredientNames.Contains(s.ingredient.name)).ToList();
            List<productingredient> allNewIngredients = allIngredients.Where(s => allExistingIngredients.Where(existing => existing.ingredient.name.Equals(s.ingredient.name)).Count() == 0).ToList();

            database.productingredients.AddRange(allNewIngredients);
            database.SaveChanges();

            Console.WriteLine("Found {0} existing ingredients", allExistingIngredients.Count);
            Console.WriteLine("Found {0} new ingredients", allNewIngredients.Count);

            allNewIngredients.ForEach(s => Console.WriteLine("Imported new ingredient {0}", s.ingredient.name));

            return allIngredients.Count;
        }

        productingredient GetOrCreateIngredientFromLine(DataRow lineInformation)
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
                    CreateMappingForIngredient(ingredientName);

                    return null;
                }
            }

            productingredient productingredient = database.productingredients.SingleOrDefault(pi => pi.product.name == product.name && pi.ingredient.name == ingredient.name);

            if (productingredient == null)
            {
                productingredient = new productingredient();

                productingredient.product = product;
                productingredient.amount = ingredientAmount;

                productingredient.ingredient = ingredient;
            }

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
            price.price = Decimal.Parse(Regex.Replace(lineInformation.ItemArray[4].ToString(), "[^0-9.]", "")) / 100;

            sauce sauce = this.GetOrCreateSauce((string)lineInformation.ItemArray[11]);
            product.sauce = sauce;

            database.products.Add(product);
            Console.WriteLine(
                "New pizza {0} created with sauce {1}",
                (string)lineInformation.ItemArray[2],
                (string)lineInformation.ItemArray[11]
            );

            database.SaveChanges();

            return product;
        }

        private sauce GetOrCreateSauce(string saucename)
        {
            sauce sauce = this.database.sauces.SingleOrDefault(s => s.name == saucename);
            if (sauce == null)
            {
                sauce = new sauce();
                sauce.name = saucename;
            }
            return sauce;
        }
        private void CreateMappingForIngredient(string ingredientName)
        {
            mapping mapping = database.mappings.SingleOrDefault(m => m.originalname == ingredientName);

            if (mapping == null)
            {
                mapping = new mapping();
                mapping.originalname = ingredientName;
                mapping.isingredient = true;

                database.mappings.Add(mapping);

                database.SaveChanges();
            }
        }
    }
}