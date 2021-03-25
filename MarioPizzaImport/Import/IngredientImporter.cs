using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarioPizzaImport.Import
{
    class IngredientImporter: Importer<ingredient>
    {
        public IngredientImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

        protected override int Import(string filePath)
        {
            int numberOfIngredientImported = 0;

            using (StreamReader sr = new StreamReader(filePath))
            {
                String line;
                bool isHeaderLine = true;
                while ((line = sr.ReadLine()) != null)
                {
                    if (isHeaderLine)
                    {
                        isHeaderLine = false;
                    }
                    else
                    {
                        string[] parts = line.Split(';');
                        string name = parts[0];
                        decimal price = Decimal.Parse(Regex.Match(parts[1], @"[0-9]+(\.[0-9]+)?").Value);

                        // Controleren of het ingredient al voorkomt
                        var ingredient = database.ingredients.SingleOrDefault(i => i.name == name);
                        if (ingredient == null)
                        {
                            ingredient = new ingredient()
                            {
                                name = name
                            };

                            var ingredientprice = new ingredientprice()
                            {
                                ingredient = ingredient,
                                startdate = DateTime.Now,
                                vat = 9.0m,
                                price = price,
                                currency = "EUR",
                                countrycode = countrycode
                            };

                            ingredient.ingredientprices.Add(ingredientprice);
                            database.ingredients.Add(ingredient);
                            database.SaveChanges();
                            Console.WriteLine("Ingredient added");

                            numberOfIngredientImported += 1;
                        }
                    }
                }
            }

            return numberOfIngredientImported;
        }
    }
}
