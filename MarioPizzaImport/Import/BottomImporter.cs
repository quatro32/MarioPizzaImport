using System;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace MarioPizzaImport.Import
{
    class BottomImporter : Importer<bottom>
    {
        public BottomImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

        protected override int Import(string filePath)
        {
            DataTable bottomTable = new DataTable();

            using (OleDbConnection localDbConnection = new OleDbConnection(string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=Excel 12.0;", filePath)))
            {
                localDbConnection.Open();

                OleDbDataAdapter bottomAdapter = new OleDbDataAdapter("select * from [Sheet1$]", localDbConnection);
                bottomAdapter.Fill(bottomTable);

                localDbConnection.Close();
            }

            int numberOfBottomImported = 0;

            foreach (DataRow row in bottomTable.Rows)
            {
                string name = (string)row.ItemArray[0];
                double diameter = (double)row.ItemArray[1];
                string description = (string)row.ItemArray[2];
                decimal price = Convert.ToDecimal(row.ItemArray[3]);

                //check if bottom exists by it's name
                var bottom = database.bottoms.SingleOrDefault(i => i.name == name);
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

                    database.bottomprices.Add(bottomprice);
                    numberOfBottomImported += 1;

                    Console.WriteLine("1 bottom and bottomprice added...");
                }
            }

            database.SaveChanges();

            return numberOfBottomImported;
        }
    }
}