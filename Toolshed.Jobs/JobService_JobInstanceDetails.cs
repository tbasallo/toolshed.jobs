using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
namespace Toolshed.Jobs
{
    public partial class JobService
    {


        private CloudTable __jobInstanceDetailsTable;
        private CloudTable JobInstanceDetailsTable
        {
            get
            {
                if (__jobInstanceDetailsTable == null)
                {
                    __jobInstanceDetailsTable = TableClient.GetTableReference(TableAssist.JobInstanceDetails());
                }

                return __jobInstanceDetailsTable;
            }
        }




        /// <summary>
        /// Returns all job instance details
        /// </summary>
        public List<JobInstanceDetail> GetJobInstanceDetails(string instanceId)
        {
            var query = new TableQuery<JobInstanceDetail>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, instanceId));
            var results = JobInstanceDetailsTable.ExecuteQuery(query);

            return results.ToList();
        }

        /// <summary>
        /// Returns all job instance details by iterating over each segment returned
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobInstanceDetail>> GetJobInstanceDetailsAsync(string instanceId)
        {
            var query = new TableQuery<JobInstanceDetail>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, instanceId));
            var segment = await JobInstanceDetailsTable.ExecuteQuerySegmentedAsync(query, null);

            var model = new List<JobInstanceDetail>();

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
        /// Returns a paged (segmented) list of job instance details
        /// </summary>
        /// <param name="pageSize">How many entities to return with each call</param>
        /// <param name="token">The continuation token from the previous call</param>
        /// <returns></returns>
        public async Task<PagedTableEntity<JobInstanceDetail>> GetJobInstanceDetailsAsync(string instanceId, int pageSize, TableContinuationToken token = null)
        {
            var query = new TableQuery<JobInstanceDetail>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, instanceId));
            TableQuerySegment<JobInstanceDetail> segment = await JobInstanceDetailsTable.ExecuteQuerySegmentedAsync(query.Take(pageSize), token);

            var model = new PagedTableEntity<JobInstanceDetail>
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
        /// Inserts a specified job instance details
        /// </summary>
        public JobInstanceDetail Save(JobInstanceDetail job)
        {
            var insertOperation = TableOperation.Insert(job);
            var result = JobInstanceDetailsTable.Execute(insertOperation);
            return (result.Result as JobInstanceDetail);
        }

        /// <summary>
        /// Inserts a specified job instance details
        /// </summary>
        public async Task<JobInstanceDetail> SaveAsync(JobInstanceDetail job)
        {
            var insertOperation = TableOperation.Insert(job);
            var result = await JobInstanceDetailsTable.ExecuteAsync(insertOperation);
            return (result.Result as JobInstanceDetail);
        }

        /// <summary>
        /// Deletes the specified job
        /// </summary>
        public async Task Delete(JobInstanceDetail job)
        {
            var deleteOperation = TableOperation.Delete(job);
            //TODO??? do weed to return something var result = await JobsTable.ExecuteAsync(deleteOperation);
            await JobInstanceDetailsTable.ExecuteAsync(deleteOperation);
        }

        /// <summary>
        /// Deletes all the associated entities for the specified job instance. This does not delete the job instance.
        /// </summary>
        /// <param name="jobInstanceProviderKey"></param>
        /// <returns>TODO// what should we return???</returns>
        public async Task DeleteAll(string instanceId)
        {
            await JobServiceHelper.DeleteAllEntitiesInBatchesAsync(JobInstanceDetailsTable, instanceId);
        }
    }
}
