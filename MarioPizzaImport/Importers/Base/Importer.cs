using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarioPizzaImport
{
    public abstract class Importer<T>
    {
        protected dbi298845_prangersEntities database;
        protected countrycode countrycode;
        protected string filePath;

        public Importer(dbi298845_prangersEntities database, countrycode countrycode)
        {
            this.database = database;
            this.countrycode = countrycode;
        }

        public void Run(string filePath)
        {
            this.filePath = filePath;

            Console.WriteLine(">>> Starting import of {0}.", typeof(T).Name);

            DateTime timeImportStart = DateTime.Now;
            int numberOfImportedElement = this.Import(filePath);
            DateTime timeImportEnd = DateTime.Now;

            Console.WriteLine(">>> Imported {0} {1}s in {2} seconds.", numberOfImportedElement, typeof(T).Name, timeImportEnd.Subtract(timeImportStart).TotalSeconds);
        }

        abstract protected int Import(string filePath);

        protected string GetFilePath()
        {
            return this.filePath;
        }

        protected product GetMappedProduct(string productName)
        {
            mapping mapping = database.mappings.SingleOrDefault(i => i.originalname == productName && !i.isingredient && i.mappedto != null);
            if(mapping != null)
            {
                return database.products.SingleOrDefault(i => i.name == mapping.mappedto);
            }
            return null;
        }

        protected ingredient GetMappedIngredient(string ingredientName)
        {
            mapping mapping = database.mappings.SingleOrDefault(i => i.originalname == ingredientName && i.isingredient && i.mappedto != null);
            if (mapping != null)
            {
                return database.ingredients.SingleOrDefault(i => i.name == mapping.mappedto);
            }
            return null;
        }
    }
}