using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolshed.AzureStorage;
namespace Toolshed.Jobs
{
    public partial class JobService
    {


        private TableClient __jobInstanceDetailsTable;
        private TableClient JobInstanceDetailsTable
        {
            get
            {
                if (__jobInstanceDetailsTable == null)
                {
                    __jobInstanceDetailsTable = ServiceManager.GetTableClient(TableAssist.JobInstanceDetails());
                }

                return __jobInstanceDetailsTable;
            }
        }






        /// <summary>
        /// Returns all job instance details by iterating over each segment returned
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobInstanceDetail>> GetJobInstanceDetailsAsync(Guid instanceId)
        {
            return await JobInstanceDetailsTable.GetEntitiesAsync<JobInstanceDetail>(instanceId.ToString());

        }

        /// <summary>
        /// Returns a paged (segmented) list of job instance details
        /// </summary>
        /// <param name="pageSize">How many entities to return with each call</param>
        /// <param name="token">The continuation token from the previous call</param>
        /// <returns></returns>
        public async Task<List<JobInstanceDetail>> GetJobInstanceDetailsAsync(Guid instanceId, int pageSize)
        {
            return await JobInstanceDetailsTable.GetEntitiesAsync<JobInstanceDetail>(instanceId.ToString());
        }



        /// <summary>
        /// Inserts a specified job instance details
        /// </summary>
        public async Task SaveAsync(JobInstanceDetail job)
        {
            _ = await JobInstanceDetailsTable.UpsertEntityAsync(job);
        }

        /// <summary>
        /// Deletes the specified job
        /// </summary>
        public async Task DeleteAsync(JobInstanceDetail job)
        {
            await JobInstanceDetailsTable.DeleteEntityAsync(job.PartitionKey, job.RowKey);
        }

        /// <summary>
        /// Deletes all the associated entities for the specified job instance. This does not delete the job instance.
        /// </summary>
        /// <param name="jobInstanceProviderKey"></param>
        /// <returns>TODO// what should we return???</returns>
        public async Task DeleteAll(Guid instanceId)
        {
            var entities = await JobInstanceDetailsTable.GetEntitiesAsync<JobInstanceDetail>(instanceId.ToString());
            if (entities.Count > 0)
            {
                foreach (var item in entities)
                {
                    await DeleteAsync(item);
                }

            }
        }
    }
}
