using System;
using System.Net;
using Microsoft.Azure.Cosmos.Table;

namespace Toolshed.Jobs
{
    public partial class JobService
    {
        private CloudTableClient TableClient { get; }

        public JobService()
        {
            CloudStorageAccount storageAccount;
            if (ServiceManager.StorageConnectionType == StorageConnectionType.Key)
            {
                storageAccount = new CloudStorageAccount(new StorageCredentials(ServiceManager.StorageName, ServiceManager.ConnectionKey), true);
            }
            else if (ServiceManager.StorageConnectionType == StorageConnectionType.ConnectionString)
            {
                storageAccount = CloudStorageAccount.Parse(ServiceManager.ConnectionKey);
            }
            else
            {
                throw new NotImplementedException("Unknown Unimplemented Connection to Storage");
            }

            //makes sense here...how small?
            ServicePointManager.FindServicePoint(storageAccount.TableEndpoint).UseNagleAlgorithm = true;
            TableClient = storageAccount.CreateCloudTableClient();
        }
    }
}
