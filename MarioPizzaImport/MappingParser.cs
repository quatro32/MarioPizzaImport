using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarioPizzaImport
{
    class MappingParser
    {
        private readonly dbi298845_prangersEntities database;

        public MappingParser(dbi298845_prangersEntities database)
        {
            this.database = database;
        }

        /**
         * Parse a given Order file for unknown ingredient or product names. If there is no prior mapping for them, add the name to the mapping table.
         */
        public void ParseMappingFromOrderFile(string filePath)
        {
            Console.WriteLine(">>> Starting parsing of mapping for '{0}'.", filePath);

            string[] allLine = File.ReadAllLines(filePath);
            int lineNumber = 0;

            List<string> allProductName = new List<string>();
            List<string> allIngredientName = new List<string>();

            foreach (string line in allLine)
            {
                lineNumber++;
                string[] partCollection = line.Split(';');

                if (partCollection.Length <= 1)
                {
                    // Skip
                    continue;
                }
                else if (partCollection.Length != 23)
                {
                    throw new InvalidDataException("Unexpected number of columns, a non-order table has been passed to the mapping function.");
                }

                string productName = partCollection[10].Trim();
                string extraIngredientList = partCollection[16];

                if (productName.Length > 0)
                {
                    allProductName.Add(productName);
                }

                if (extraIngredientList.Length > 0)
                {
                    string[] ingredientNameCollection = extraIngredientList.Split(',');
                    foreach (string ingredientName in ingredientNameCollection)
                    {
                        string ingredientNameTrimmed = ingredientName.Trim();

                        if (ingredientNameTrimmed.Length > 0)
                        {
                            allIngredientName.Add(ingredientNameTrimmed);
                        }
                    }
                }
            }

            List<ingredient> allIngredientExisting = this.database.ingredients.Where(i => allIngredientName.Contains(i.name)).ToList();
            List<product> allProductExisting = this.database.products.Where(p => allProductName.Contains(p.name)).ToList();

            List<string> allProductNotExisting = allProductName.Where(p => !allProductExisting.Exists(p2 => p2.name == p)).Distinct().ToList();
            List<string> allIngredientNotExisting = allIngredientName.Where(i => !allIngredientExisting.Exists(i2 => i2.name == i)).Distinct().ToList();

            int numberOfAddedIngredient = 0;
            int numberOfAddedProduct = 0;

            foreach (string productNotExisting in allProductNotExisting)
            {
                if (database.mappings.Where(m => m.originalname == productNotExisting).Count() <= 0)
                {
                    MapProduct(productNotExisting);
                    numberOfAddedProduct++;
                }
            }

            foreach (string ingredientNotExisting in allIngredientNotExisting)
            {
                if (database.mappings.Where(m => m.originalname == ingredientNotExisting).Count() <= 0)
                {
                    MapIngredient(ingredientNotExisting);
                    numberOfAddedIngredient++;
                }
            }

            database.SaveChanges();

            Console.WriteLine(">>> Mapped {0} products and {1} ingredients", numberOfAddedProduct, numberOfAddedIngredient);
        }

        private void MapProduct(string productName)
        {
            mapping mapping = new mapping();
            mapping.originalname = productName;
            database.mappings.Add(mapping);
            Console.WriteLine("New mapping created for product with name {0}", productName);
        }
        private void MapIngredient(string ingredientName)
        {
            mapping mapping = new mapping();
            mapping.originalname = ingredientName;
            mapping.isingredient = true;
            database.mappings.Add(mapping);
            Console.WriteLine("New mapping created for ingredient with name {0}", ingredientName);
        }
    }
}