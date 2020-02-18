using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
namespace Toolshed.Jobs
{
    public partial class JobService
    {
        private CloudTable __jobsTable;
        private CloudTable JobsTable
        {
            get
            {
                if (__jobsTable == null)
                {
                    __jobsTable = TableClient.GetTableReference(TableAssist.Jobs());
                }

                return __jobsTable;
            }
        }


        /// <summary>
        /// Inserts or updates/replaces a specified job
        /// </summary>
        public Job Save(Job job)
        {
            var insertOperation = TableOperation.InsertOrReplace(job);
            var result = JobsTable.Execute(insertOperation);
            return (result.Result as Job);
        }

        /// <summary>
        /// Inserts or updates/replaces a specified job
        /// </summary>
        public async Task<Job> SaveAsync(Job job)
        {
            var insertOperation = TableOperation.InsertOrReplace(job);
            var result = await JobsTable.ExecuteAsync(insertOperation);
            return (result.Result as Job);
        }

        /// <summary>
        /// Deletes the specified job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public async Task<bool> DeleteAsync(Job job)
        {
            var deleteOperation = TableOperation.Delete(job);
            var result = await JobsTable.ExecuteAsync(deleteOperation);

            var succeeded = result.HttpStatusCode >= 200 && result.HttpStatusCode < 300;

            if (succeeded)
            {
                //TODO if we got here, but this fails then we have orphan records
                //TODO return more than a simple boolean and/or create a cleanup method that can run on a schedule for such events
                var instances = await GetJobInstancesAsync(job.Id);
                foreach (var instance in instances)
                {
                    await DeleteAsync(instance);
                }
            }

            return succeeded;
        }



        /// <summary>
        /// Returns all jobs
        /// </summary>
        /// <returns></returns>
        public List<Job> GetJobs()
        {
            var query = new TableQuery<Job>();
            var results = JobsTable.ExecuteQuery(query);

            return results.ToList();
        }

        /// <summary>
        /// Returns all jobs by iterating over each segment returned
        /// </summary>
        /// <returns></returns>
        public async Task<List<Job>> GetJobsAsync()
        {
            var query = new TableQuery<Job>();
            var segment = await JobsTable.ExecuteQuerySegmentedAsync(query, null);

            var model = new List<Job>();

            if (segment.Results != null)
            {
                model.AddRange(segment.Results.ToList());
            }

            while (segment.ContinuationToken != null)
            {
                segment = await JobsTable.ExecuteQuerySegmentedAsync(query, segment.ContinuationToken);
                model.AddRange(segment.Results.ToList());
            }

            return model;
        }

        /// <summary>
        /// Returns a paged (segmented) list of jobs
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<PagedTableEntity<Job>> GetJobsAsync(int pageSize, TableContinuationToken token = null)
        {
            TableQuery<Job> query = new TableQuery<Job>();
            TableQuerySegment<Job> segment = await JobsTable.ExecuteQuerySegmentedAsync(query.Take(pageSize), token);

            var model = new PagedTableEntity<Job>
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
        /// Gets the specified job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public Job GetJob(string jobName)
        {
            var query = new TableQuery<Job>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, jobName.ToLowerInvariant()));
            return JobsTable.ExecuteQuery(query).FirstOrDefault();
        }

        /// <summary>
        /// Gets the specified job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public Job GetJob(string jobName, Guid id)
        {
            var retrieveOperation = TableOperation.Retrieve<Job>(jobName.ToLowerInvariant(), id.ToString());
            return JobsTable.Execute(retrieveOperation).Result as Job;
        }


        /// <summary>
        /// Gets the specified job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public async Task<Job> GetJobAsync(string jobName)
        {
            var query = new TableQuery<Job>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, jobName.ToLowerInvariant()));
            var result = await JobsTable.ExecuteQuerySegmentedAsync(query, null);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Gets the specified job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public async Task<Job> GetJobAsync(string jobName, Guid id)
        {
            var retrieveOperation = TableOperation.Retrieve<Job>(jobName.ToLowerInvariant(), id.ToString());
            var result = await JobsTable.ExecuteAsync(retrieveOperation);
            return result.Result as Job;
        }
    }
}
