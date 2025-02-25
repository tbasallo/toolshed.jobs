using Azure.Data.Tables;

namespace Toolshed.Jobs
{
    public static class ServiceManager
    {
        /// <summary>
        /// The name used as the default version when a version is not provided
        /// </summary>
        internal const string DefaultVersionName = "default";

        internal static string ConnectionString { get; private set; }
        internal static string TablePrefix { get; set; }


        public static void InitConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public static TableClient GetTableClient(string tableName)
        {
            return new TableClient(ConnectionString, tableName);
        }
    }
}
