using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Jobs
{
    public static class JobServiceManager
    {
        /// <summary>
        /// The name used as the default version when a version is not provided
        /// </summary>
        internal const string DefaultVersionName = "default";

        internal static string StorageName { get; private set; }
        internal static string ConnectionKey { get; private set; }
        internal static StorageConnectionType StorageConnectionType { get; private set; }
        internal static string TablePrefix { get; set; }

        public static void InitStorageKey(string storageName, string storageKey, string tablePrefix = null)
        {
            StorageName = storageName;
            ConnectionKey = storageKey;
            StorageConnectionType = StorageConnectionType.Key;
            TablePrefix = tablePrefix;
        }
        public static void InitConnectionString(string connectionString, string tablePrefix = null)
        {
            ConnectionKey = connectionString;
            StorageConnectionType = StorageConnectionType.Key;
            TablePrefix = tablePrefix;
        }


        public async static Task CreateTablesIfNotExistsAsync()
        {
            CloudStorageAccount storageAccount;
            if (StorageConnectionType == StorageConnectionType.Key)
            {
                storageAccount = new CloudStorageAccount(new StorageCredentials(StorageName, ConnectionKey), true);
            }
            else if (StorageConnectionType == StorageConnectionType.ConnectionString)
            {
                storageAccount = CloudStorageAccount.Parse(ConnectionKey);
            }
            else
            {
                throw new NotImplementedException("Unknown Unimplemented Connection to Storage");
            }

            var tableClient = storageAccount.CreateCloudTableClient();
            var tasks = new Task<bool>[]
            {
                tableClient.GetTableReference(TableAssist.Jobs()).CreateIfNotExistsAsync(),
                tableClient.GetTableReference(TableAssist.JobInstances()).CreateIfNotExistsAsync(),
                tableClient.GetTableReference(TableAssist.JobInstanceDetails()).CreateIfNotExistsAsync(),
            };

            await Task.WhenAll(tasks);
        }
        public static void CreateTablesIfNotExists()
        {
            CloudStorageAccount storageAccount;
            if (StorageConnectionType == StorageConnectionType.Key)
            {
                storageAccount = new CloudStorageAccount(new StorageCredentials(JobServiceManager.StorageName, JobServiceManager.ConnectionKey), true);
            }
            else if (StorageConnectionType == StorageConnectionType.ConnectionString)
            {
                storageAccount = CloudStorageAccount.Parse(JobServiceManager.ConnectionKey);
            }
            else
            {
                throw new NotImplementedException("Unknown Unimplemented Connection to Storage");
            }

            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.GetTableReference(TableAssist.Jobs()).CreateIfNotExists();
            tableClient.GetTableReference(TableAssist.JobInstances()).CreateIfNotExists();
            tableClient.GetTableReference(TableAssist.JobInstanceDetails()).CreateIfNotExists();
        }
    }
}
