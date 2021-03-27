using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace MarioPizzaImport.Import
{
    class ProductIngredientImport : Importer<productingredient>
    {
        public ProductIngredientImport(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

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
                    Logger.Instance.LogError(filePath, "Could not find product named " + ingredientName);
                    return null;
                }
            }

            if (ingredient == null)
            {
                ingredient = GetMappedIngredient(ingredientName);
                if (ingredient == null)
                {
                    Logger.Instance.LogError(filePath, String.Format("Could not find ingredient named {0} for product {1}", ingredientName, productName));
                    return null;
                }
            }

            productingredient productingredient = new productingredient();

            productingredient.product = product;
            productingredient.amount = ingredientAmount;

            productingredient.ingredient = ingredient;

            return productingredient;
        }
    }
}