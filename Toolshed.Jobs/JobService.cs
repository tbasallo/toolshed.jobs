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
            if (JobServiceManager.StorageConnectionType == StorageConnectionType.Key)
            {
                storageAccount = new CloudStorageAccount(new StorageCredentials(JobServiceManager.StorageName, JobServiceManager.ConnectionKey), true);
            }
            else if (JobServiceManager.StorageConnectionType == StorageConnectionType.ConnectionString)
            {
                storageAccount = CloudStorageAccount.Parse(JobServiceManager.ConnectionKey);
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
