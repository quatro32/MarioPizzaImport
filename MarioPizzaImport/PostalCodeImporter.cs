using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;

namespace MarioPizzaImport
{
    class PostalCodeImporter : Importer<postalcode>
    {
        public PostalCodeImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

        protected override List<postalcode> Import(string filePath)
        {
            List<township> allTownshipImported = new List<township>();
            List<postalcode> allPostalCodeImported = new List<postalcode>();

            OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder(@"Provider=Microsoft.JET.OLEDB.4.0;");
            connectionStringBuilder.DataSource = filePath;

            using (var postalCodeDatabaseConnection = new OleDbConnection(connectionStringBuilder.ConnectionString))
            {
                postalCodeDatabaseConnection.Open();

                OleDbCommand cmd = postalCodeDatabaseConnection.CreateCommand();
                cmd.CommandText = "SELECT A13_POSTCODE,A13_REEKSIND,A13_BREEKPUNT_VAN,A13_BREEKPUNT_TEM,A13_WOONPLAATS,A13_STRAATNAAM,N42_GEM_NAAM FROM `POSTCODES` LEFT JOIN `GEMEENTEN` ON POSTCODES.A13_GEMEENTECODE = GEMEENTEN.N42_GEM_KODE";

                OleDbDataReader dataReader = cmd.ExecuteReader();
                DateTime timeBatchStart = DateTime.Now;
    
                using (var bulkInsert = new SqlBulkCopy("Server=mssql.fhict.local;initial catalog=dbi366191_wachtwoord;User Id=dbi366191_wachtwoord;Password=wachtwoord", SqlBulkCopyOptions.UseInternalTransaction))
                {
                    bulkInsert.DestinationTableName = "postalcode_import";
                    bulkInsert.EnableStreaming = true;
                    bulkInsert.BulkCopyTimeout = 300;
                    bulkInsert.NotifyAfter = 25000;
                        

                    bulkInsert.ColumnMappings.Add("A13_POSTCODE", "postalcode");
                    bulkInsert.ColumnMappings.Add("A13_REEKSIND", "iseven");
                    bulkInsert.ColumnMappings.Add("A13_BREEKPUNT_VAN", "startingnumber");
                    bulkInsert.ColumnMappings.Add("A13_BREEKPUNT_TEM", "endingnumber");
                    bulkInsert.ColumnMappings.Add("A13_WOONPLAATS", "city");
                    bulkInsert.ColumnMappings.Add("A13_STRAATNAAM", "street");
                    bulkInsert.ColumnMappings.Add("N42_GEM_NAAM", "township");

                    bulkInsert.SqlRowsCopied += (object sender, SqlRowsCopiedEventArgs e) =>
                    {
                        Console.WriteLine("Copied {0} rows.", e.RowsCopied);
                    };

                    bulkInsert.WriteToServer(dataReader);
                }

                database.Dispose();
                //databaseConnection.Close();
                postalCodeDatabaseConnection.Close();

                return allPostalCodeImported;

            }

            // TODO: Run a stored procedure to transform the details and insert them in to the final table.
            
            return allPostalCodeImported;
        }

        private static string FormatTownshipName(string townshipName)
        {
            List<string> allPart = townshipName.Trim().ToLower().Split(' ').ToList();
            List<string> allPartUppercased = new List<string>();

            allPart.ForEach(p => allPartUppercased.Add(p.Substring(0, 1).ToUpper() + p.Substring(1)));

            return String.Join(" ", allPartUppercased);
        }
    }
}
