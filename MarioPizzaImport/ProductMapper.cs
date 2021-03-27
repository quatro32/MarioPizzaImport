using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace MarioPizzaImport
{
    class ProductIngredientImport : Importer<productingredient>
    {
        public ProductIngredientImport(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

        override protected int Import(string filePath)
        {
            List<productingredient> allIngredients = new List<productingredient>();

            using (Stream storeStream = new FileStream(filePath, FileMode.Open))
            {
                using (StreamReader storeReader = new StreamReader(storeStream))
                {
                    List<string> allLineIngredientInformation = new List<string>();

                    string lineIngredientInformation = null;

                    while ((lineIngredientInformation = storeReader.ReadLine()) != null)
                    {
                        lineIngredientInformation = lineIngredientInformation.Trim();

                        if (lineIngredientInformation.Length > 0)
                        {
                            allLineIngredientInformation.Add(lineIngredientInformation);
                        }
                    }

                    if (allLineIngredientInformation.Count != 0)
                    {
                        productingredient productIngredient = CreateIngredientFromLine(lineIngredientInformation);
                        if (productIngredient != null)
                        {
                            allIngredients.Add(productIngredient);
                        }
                        allLineIngredientInformation.Clear();
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

        productingredient CreateIngredientFromLine(String ingredientLine)
        {


            string[] parts = ingredientLine.Split(';');

            string productName = parts[2];
            string ingredientName = parts[10];
            string ingredientAmount = parts[9];

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

            productingredient.amount = Int32.Parse(ingredientAmount);

            productingredient.ingredient = ingredient;

            return productingredient;
        }
    }
}