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
        private List<product> products;
        private List<mapping> mappings;
        private List<ingredient> ingredients;

        public MappingParser(dbi298845_prangersEntities database)
        {
            this.database = database;

            this.products = database.products.ToList();
            this.mappings = database.mappings.ToList();
            this.ingredients = database.ingredients.ToList();
        }

        /**
         * Parse a given Order file for unknown ingredient or product names. If there is no prior mapping for them, add the name to the mapping table.
         */
        public void ParseMappingFromOrderFile(string filePath)
        {
            List<string> allLineInMapping = File.ReadAllLines(filePath).ToList();
            allLineInMapping.RemoveRange(0,5); // Remove the header.
            int lines = 0;

            foreach (string line in allLineInMapping)
            {
                lines++;

                string[] partCollection = line.Split(';');

                if (partCollection.Length <= 1)
                {
                    // Skip empty lines or lines with a comment.
                    continue;
                }
                if (partCollection.Length != 23)
                {
                    throw new InvalidDataException("Unexpected number of columns, a non-order table has been passed to the mapping function.");
                }

                string productName = partCollection[10].ToString().Trim();
                string extraIngredientList = partCollection[16].ToString().Trim();

                if (productName.Length > 0)
                {
                    // If the product already exists, or there is a mapping for it already, no need to map it again.
                    product product = products.SingleOrDefault(i => i.name == productName);
                    mapping existingMapping = mappings.SingleOrDefault(i => i.originalname == productName && i.isingredient == false);
                    if (product == null && existingMapping == null)
                    {
                        mapProduct(productName);
                    }
                }

                if (extraIngredientList.Length > 0)
                {
                    string[] ingredientNameCollection = extraIngredientList.Split(',');
                    foreach (string ingredientName in ingredientNameCollection)
                    {
                        string ingredientNameFormatted = ingredientName.Trim();

                        // If the ingredient already exists, or there is a mapping for it already, no need to map it again.
                        ingredient ingredient = ingredients.SingleOrDefault(i => i.name == ingredientNameFormatted);
                        mapping existingMapping = mappings.SingleOrDefault(i => i.originalname == ingredientNameFormatted && i.isingredient == true);
                        if (ingredient == null && existingMapping == null)
                        {
                            mapIngredient(ingredientNameFormatted);
                        }
                    }
                }

                Console.WriteLine("{0}/{1}", lines, allLineInMapping.Count);
            }

            database.SaveChanges();
        }

        private void mapProduct(string productName)
        {
            mapping mapping = new mapping();
            mapping.originalname = productName;
            database.mappings.Add(mapping);
            Console.WriteLine("new mapping created for product with name {0}", productName);
            mappings.Add(mapping);
        }
        private void mapIngredient(string ingredientName)
        {
            mapping mapping = new mapping();
            mapping.originalname = ingredientName;
            mapping.isingredient = true;
            database.mappings.Add(mapping);
            Console.WriteLine("new mapping created for ingredient with name {0}", ingredientName);
            mappings.Add(mapping);
        }
    }
}