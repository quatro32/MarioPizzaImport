using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarioPizzaImport
{
    class Program
    {
        static void Main(string[] args)
        {
            var database = new dbi298845_prangersEntities();

            var maincategory = new productcategory()
            {
                name = "pizza's",
            };

            database.productcategories.Add(maincategory);
            database.SaveChanges();

            Console.ReadKey();
        }
    }
}
