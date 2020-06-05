using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
namespace Toolshed.Jobs
{
    public partial class JobService
    {
        private CloudTable __jobInstancesTable;
        private CloudTable JobInstancesTable
        {
            get
            {
                if (__jobInstancesTable == null)
                {
                    __jobInstancesTable = TableClient.GetTableReference(TableAssist.JobInstances());
                }

                return __jobInstancesTable;
            }
        }

        public JobInstance GetJobInstance(Guid jobId, Guid instanceId)
        {
            var retrieveOperation = TableOperation.Retrieve<JobInstance>(jobId.ToString(), instanceId.ToString());
            var result = JobInstancesTable.Execute(retrieveOperation);
            return result.Result as JobInstance;
        }
        public async Task<JobInstance> GetJobInstanceAsync(Guid jobId, Guid instanceId)
        {
            var retrieveOperation = TableOperation.Retrieve<JobInstance>(jobId.ToString(), instanceId.ToString());
            var result = await JobInstancesTable.ExecuteAsync(retrieveOperation);
            return result.Result as JobInstance;
        }

        /// <summary>
        /// Returns the number of entities requested. No order is guaranteed because this table does not store records in a way that returns them in order
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="pageSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<PagedTableEntity<JobInstance>> GetJobInstancesAsync(Guid jobId, int pageSize, TableContinuationToken token = null)
        {
            var query = new TableQuery<JobInstance>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, jobId.ToString()));
            var segment = await JobInstancesTable.ExecuteQuerySegmentedAsync(query.Take(pageSize), token);

            var model = new PagedTableEntity<JobInstance>
            {
                Entities = segment.Results.ToList()
            };
            if (segment.ContinuationToken != null)
            {
                model.NextPartitionKey = segment.ContinuationToken.NextPartitionKey;
                model.NextRowKey = segment.ContinuationToken.NextRowKey;
                model.TargetLocation = segment.ContinuationToken.TargetLocation;
            }
            if (token != null)
            {
                model.PreviousPartitionKey = token.NextPartitionKey;
                model.PreviousRowKey = token.NextRowKey;
            }

            return model;
        }


        /// <summary>
        /// Returns all job instances by iterating over each segment returned
        /// </summary>
        public async Task<List<JobInstance>> GetJobInstancesAsync(Guid jobId)
        {
            var query = new TableQuery<JobInstance>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, jobId.ToString()));
            var segment = await JobInstancesTable.ExecuteQuerySegmentedAsync(query, null);

            var model = new List<JobInstance>();

            if (segment.Results != null)
            {
                model.AddRange(segment.Results.ToList());
            }

            while (segment.ContinuationToken != null)
            {
                segment = await JobInstanceDetailsTable.ExecuteQuerySegmentedAsync(query, segment.ContinuationToken);
                model.AddRange(segment.Results.ToList());
            }

            return model;
        }


        /// <summary>
        /// Returns all job instances for a given date. Instances will iterated over until all instances are returned
        /// </summary>
        public async Task<List<JobInstance>> GetJobInstancesAsync(Guid jobId, DateTime date)
        {
            var query = new TableQuery<JobInstance>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{jobId}-{date:yyyyMMdd}"));
            var segment = await JobInstancesTable.ExecuteQuerySegmentedAsync(query, null);

            var model = new List<JobInstance>();

            if (segment.Results != null && segment.Results.Count > 0)
            {
                model.AddRange(segment.Results);
            }

            while (segment.ContinuationToken != null)
            {
                segment = await JobInstanceDetailsTable.ExecuteQuerySegmentedAsync(query, segment.ContinuationToken);
                model.AddRange(segment.Results);
            }

            return model;
        }




        /// <summary>
        /// Inserts or updates/replaces a specified job instances
        /// </summary>
        public JobInstance Save(JobInstance jobInstance)
        {
            var insertOperation = TableOperation.InsertOrReplace(jobInstance);
            //TODO research performance and operation impact of insert vs. merge
            //var mergeOperation = TableOperation.InsertOrMerge(job);
            var result = JobInstancesTable.Execute(insertOperation);
            return (result.Result as JobInstance);
        }

        /// <summary>
        /// Inserts or updates/replaces a specified job instance
        /// </summary>
        public async Task<JobInstance> SaveAsync(JobInstance jobInstance)
        {
            var insertOperation = TableOperation.InsertOrReplace(jobInstance);
            var result = await JobInstancesTable.ExecuteAsync(insertOperation);
            return (result.Result as JobInstance);
        }

        /// <summary>
        /// Deletes the specified job instance
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public bool Delete(JobInstance jobInstance)
        {
            var deleteOperation = TableOperation.Delete(jobInstance);
            var result = JobInstancesTable.Execute(deleteOperation);
            return result.HttpStatusCode >= 200 && result.HttpStatusCode < 300;
        }

        /// <summary>
        /// Deletes the specified job instance
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public async Task<bool> DeleteAsync(JobInstance jobInstance)
        {
            var deleteOperation = TableOperation.Delete(jobInstance);
            var result = await JobInstancesTable.ExecuteAsync(deleteOperation);

            var succeeded = result.HttpStatusCode >= 200 && result.HttpStatusCode < 300;

            if (succeeded)
            {
                //TODO if we got here, but this fails then we have orphan records in the details table.
                //TODO return more than a simple boolean and/or create a cleanup method that can run on a schedule for such events
                JobServiceHelper.DeleteAllEntitiesInBatches(JobInstanceDetailsTable, jobInstance.PartitionKey);
            }

            return succeeded;
        }

        //TODO return table result or return boolean?
        public async Task<bool> DeleteJobInstanceAsync(Guid jobId, Guid instanceId)
        {
            var instance = await GetJobInstanceAsync(jobId, instanceId);
            return await DeleteAsync(instance);
        }

    }
}
