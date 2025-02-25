using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Data.Tables;

using Toolshed.AzureStorage;
using Toolshed.Jobs.Models;
namespace Toolshed.Jobs
{
    public partial class JobService
    {
        private TableClient __jobInstancesTable;
        private TableClient JobInstancesTable
        {
            get
            {
                if (__jobInstancesTable == null)
                {
                    __jobInstancesTable = ServiceManager.GetTableClient(TableAssist.JobInstances());
                }

                return __jobInstancesTable;
            }
        }


        public async Task<JobInstance> GetJobInstanceAsync(Guid jobId, Guid instanceId)
        {
            return await JobInstancesTable.GetEntityWhenExistsAsync<JobInstance>(jobId.ToString(), instanceId.ToString());
        }
        public async Task<JobInstance> GetJobInstanceAsync(Guid jobId, DateTime date, Guid instanceId)
        {
            return await JobInstancesTable.GetEntityWhenExistsAsync<JobInstance>($"{jobId}-{date:yyyyMMdd}", instanceId.ToString());
        }

        /// <summary>
        /// Returns all job instances by iterating over each segment returned
        /// </summary>
        public async Task<List<JobInstance>> GetJobInstancesAsync(Guid jobId)
        {
            return await JobInstancesTable.GetEntitiesAsync<JobInstance>(jobId.ToString());
        }


        /// <summary>
        /// Returns all job instances for a given date. Instances will iterated over until all instances are returned
        /// </summary>
        public async Task<List<JobInstance>> GetJobInstancesAsync(Guid jobId, DateTime date)
        {
            return await JobInstancesTable.GetEntitiesAsync<JobInstance>($"{jobId}-{date:yyyyMMdd}");
        }




        /// <summary>
        /// Inserts or updates/replaces a specified job instance
        /// </summary>
        public async Task SaveAsync(JobInstance jobInstance)
        {
            _ = await JobInstancesTable.UpsertEntityAsync(jobInstance);
        }

        /// <summary>
        /// Deletes the specified job instance
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public async Task DeleteAsync(JobInstance jobInstance)
        {
            _ = await JobInstancesTable.DeleteEntityAsync(jobInstance.PartitionKey, jobInstance.RowKey);

            var history = await GetJobInstanceAsync(jobInstance.JobId, jobInstance.StartedOn, jobInstance.InstanceId);
            if (history != null)
            {
                _ = await JobInstancesTable.DeleteEntityAsync(history.PartitionKey, history.RowKey);
            }

            var moreHistory = await JobInstanceDetailsTable.GetEntitiesAsync<SimpleModel>(jobInstance.PartitionKey);
            if (moreHistory.Count > 0)
            {
                foreach (var item in moreHistory)
                {
                    _ = await JobInstancesTable.DeleteEntityAsync(item.PartitionKey, item.RowKey);

                }
            }
        }
    }
}
