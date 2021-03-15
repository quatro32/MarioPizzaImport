using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;

namespace MarioPizzaImport
{
    class PostalCodeImporter : Importer<postalcode>
    {
        public PostalCodeImporter(dbi298845_prangersEntities database, countrycode countrycode) : base(database, countrycode) { }

        protected override int Import(string filePath)
        {
            List<township> allTownshipImported = new List<township>();
            List<postalcode> allPostalCodeImported = new List<postalcode>();

            OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder(@"Provider=Microsoft.JET.OLEDB.4.0;");
            connectionStringBuilder.DataSource = filePath;

            SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbi298845_prangersEntities"].ConnectionString);
            sqlConnection.Open();

            using (var postalCodeDatabaseConnection = new OleDbConnection(connectionStringBuilder.ConnectionString))
            {
                postalCodeDatabaseConnection.Open();

                OleDbCommand cmd = postalCodeDatabaseConnection.CreateCommand();
                cmd.CommandText = "SELECT A13_POSTCODE,A13_REEKSIND,A13_BREEKPUNT_VAN,A13_BREEKPUNT_TEM,A13_WOONPLAATS,A13_STRAATNAAM,N42_GEM_NAAM FROM `POSTCODES` LEFT JOIN `GEMEENTEN` ON POSTCODES.A13_GEMEENTECODE = GEMEENTEN.N42_GEM_KODE";

                OleDbDataReader dataReader = cmd.ExecuteReader();
                DateTime timeBatchStart = DateTime.Now;
    
                using (var bulkInsert = new SqlBulkCopy(sqlConnection))
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

                postalCodeDatabaseConnection.Close();
            }

            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCommand.CommandText = "ImportPostalCode";
            sqlCommand.CommandTimeout = 600;

            sqlCommand.ExecuteNonQuery();
           
            return database.addresses.Count();
        }
    }
}
