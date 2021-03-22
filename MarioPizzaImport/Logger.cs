using System;
using System.Configuration;
using System.Data.SqlClient;

namespace MarioPizzaImport
{
    class Logger
    {
        private static Logger logger = null;

        public static Logger Instance
        {
            get
            {
                if (logger == null)
                {
                    logger = new Logger();
                }

                return logger;
            }
        }

        private SqlConnection connection;

        protected Logger()
        {
            this.connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbi298845_postalcodeImport"].ConnectionString);
            this.connection.Open();
        }

        public void LogError(string fileName, string errorString)
        {
            this.SaveLog("ERROR", fileName, errorString);
        }

        public void LogCorrection(string fileName, string stringInput, string stringCorrected)
        {
            this.SaveLog("CORRECTION", fileName, string.Format("Corrected {0} to {1} during import.", stringInput, stringCorrected));
        }

        private void SaveLog(string type, string fileName, string errorString)
        {
            Console.WriteLine("[{0}] {1}", type, errorString);

            SqlCommand command = this.connection.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "SaveLog";

            SqlParameter parameterStatus = command.CreateParameter();
            parameterStatus.ParameterName = "type";
            parameterStatus.Value = type;

            SqlParameter parameterFilename = command.CreateParameter();
            parameterFilename.ParameterName = "fileName";
            parameterFilename.Value = fileName;

            SqlParameter parameterErrorString = command.CreateParameter();
            parameterErrorString.ParameterName = "errorString";
            parameterErrorString.Value = errorString;

            command.Parameters.Add(parameterStatus);
            command.Parameters.Add(parameterFilename);
            command.Parameters.Add(parameterErrorString);

            command.ExecuteNonQuery();
        }
    }
}
