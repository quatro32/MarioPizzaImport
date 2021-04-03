using System;
using System.Linq;
using MarioPizzaImport.Command;

namespace MarioPizzaImport
{
    class Program
    {
        static void Main(string[] allInputString)
        {
            dbi298845_prangersEntities database = new dbi298845_prangersEntities();
            countrycode countrycode = GetOrCreateDefaultCountryCode(database);

            // Create the router to route the commands through.
            CommandRouter commandRouter = new CommandRouter(database, countrycode);
            
            // Register all possible commands.
            commandRouter.Register(new CommandImportPostalCode());
            commandRouter.Register(new CommandImportStore());
            commandRouter.Register(new CommandImportBottom());
            commandRouter.Register(new CommandImportProduct());
            commandRouter.Register(new CommandImportIngredient());
            commandRouter.Register(new CommandImportPizzaIngredient());
            commandRouter.Register(new CommandParseMapping());
            commandRouter.Register(new CommandImportOrder());
            
            // Execute the command
            commandRouter.Execute(allInputString);
            Console.ReadKey();
        }

        private static countrycode GetOrCreateDefaultCountryCode(dbi298845_prangersEntities db)
        {
            //check if countrycode exists, so we can create a bottomprice for NL stores
            var countrycode = db.countrycodes.SingleOrDefault(i => i.code == "NL");
            if (countrycode == null) // if it doesnt exists, create a new countrycode
            {
                countrycode = new countrycode()
                {
                    code = "NL"
                };

                db.countrycodes.Add(countrycode);
                db.SaveChanges();
            }

            return countrycode;
        }
    }
}
