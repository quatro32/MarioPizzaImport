using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace MarioPizzaImport
{
    class IngredientImporter : Importer<productingredient>
    {
        public IngredientImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

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
                        allIngredients.Add(CreateIngredientFromAllLine(allLineIngredientInformation));
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

        productingredient CreateIngredientFromAllLine(List<string> allLineIngredientInformation)
        {

            string productName = allLineIngredientInformation[2];
            string ingredientName = allLineIngredientInformation[10];

            ingredient ingredient = database.ingredients.Where(s => s.name == ingredientName).First();

            productingredient productingredient = new productingredient();

            productingredient.product = database.products.Where(s => s.name == productName).First();
            productingredient.amount = Convert.ToInt32(allLineIngredientInformation[9]);
            productingredient.ingredient = ingredient;

            return productingredient;
            
        }

        
    }
}
