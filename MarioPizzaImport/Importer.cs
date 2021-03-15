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

        public Importer(dbi298845_prangersEntities database, countrycode countrycode)
        {
            this.database = database;
            this.countrycode = countrycode;
        }

        public void Run(string filePath)
        {
            Console.WriteLine(">>> Starting import of {0}.", typeof(T).Name);

            DateTime timeImportStart = DateTime.Now;
            List<T> allImportedElement = this.Import(filePath);
            DateTime timeImportEnd = DateTime.Now;

            Console.WriteLine(">>> Imported {0} {1}s in {2} seconds.", allImportedElement.Count, typeof(T).Name, timeImportEnd.Subtract(timeImportStart).TotalSeconds);
        }

        abstract protected List<T> Import(string filePath);
    }
}