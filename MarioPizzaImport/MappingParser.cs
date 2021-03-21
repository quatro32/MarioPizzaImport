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
            using (StreamReader sr = new StreamReader(filePath))
            {
                // Skip the header line.
                sr.ReadLine();
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] partCollection = line.Split(';');

                    if(partCollection.Length != 23)
                    {
                        throw new InvalidDataException("Unexpected number of columns, a non-order table has been passed to the mapping function.");
                    }

                    string productName = partCollection[11];
                    string extraIngredientList = partCollection[17];

                    if(productName.Length > 0)
                    {
                        // If the product already exists, or there is a mapping for it already, no need to map it again.
                        product product = database.products.SingleOrDefault(i => i.name == productName);
                        mapping existingMapping = database.mappings.SingleOrDefault(i => i.originalname == productName && i.isingredient == false);
                        if(product == null && existingMapping == null)
                        {
                            mapProduct(productName);
                        }
                    }

                    if(extraIngredientList.Length > 0)
                    {
                        string[] ingredientNameCollection = extraIngredientList.Split(',');
                        foreach(string ingredientName in ingredientNameCollection)
                        {
                            // If the ingredient already exists, or there is a mapping for it already, no need to map it again.
                            ingredient ingredient = database.ingredients.SingleOrDefault(i => i.name == ingredientName);
                            mapping existingMapping = database.mappings.SingleOrDefault(i => i.originalname == ingredientName && i.isingredient == true);
                            if (ingredient == null && existingMapping == null)
                            {
                                mapIngredient(ingredientName);
                            }
                        }
                    }
                    database.SaveChanges();
                }
            }
        }

        private void mapProduct(string productName)
        {
            mapping mapping = new mapping();
            mapping.originalname = productName;
            database.mappings.Add(mapping);
            Console.WriteLine("new mapping created for product with name {0}", productName);
        }
        private void mapIngredient(string ingredientName)
        {
            mapping mapping = new mapping();
            mapping.originalname = ingredientName;
            mapping.isingredient = true;
            database.mappings.Add(mapping);
            Console.WriteLine("new mapping created for ingredient with name {0}", ingredientName);
        }
    }
}
