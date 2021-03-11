using System;
using System.Collections.Generic;
using System.IO;
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

            InsertBottoms(@"C:\Users\shnva\Desktop\pizzabodems.csv", database);

            Console.WriteLine("Done...");
            Console.ReadKey();
        }

        static void InsertBottoms(string path, dbi298845_prangersEntities db)
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

            using (StreamReader sr = new StreamReader(path))
            {
                String line;
                bool isHeaderLine = true;
                while ((line = sr.ReadLine()) != null)
                {
                    if (isHeaderLine)//skip first line in csv file, since it's an header line. we don't want that values
                    {
                        isHeaderLine = false;
                    }
                    else
                    {
                        //split all lines in csv to values
                        string[] parts = line.Split(';');
                        string name = parts[0];
                        string diameter = parts[1];
                        string description = parts[2];
                        decimal price = Convert.ToDecimal(parts[3]);

                        //check if bottom exists by it's name
                        var bottom = db.bottoms.SingleOrDefault(i => i.name == name);
                        if (bottom == null)//if not, create a new one
                        {
                            bottom = new bottom()
                            {
                                name = name,
                                diameter = Convert.ToInt32(diameter),
                                //description has to be added to database,
                            };

                            //if a bottom doesn't exists ALWAYS create a bottomprice, since it hasn't got any
                            var bottomprice = new bottomprice()
                            {
                                bottom = bottom,
                                countrycode = countrycode,
                                currency = "EUR",
                                startdate = DateTime.Now,
                                vat = 9.0m,
                                price = price
                            };

                            db.bottomprices.Add(bottomprice);
                            db.SaveChanges();
                            Console.WriteLine("1 bottom and bottomprice added...");
                        }
                    }
                }
            }
        }
    }
}
