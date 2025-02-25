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
        private TableClient __jobsTable;
        private TableClient JobsTable
        {
            get
            {
                if (__jobsTable == null)
                {
                    __jobsTable = ServiceManager.GetTableClient(TableAssist.Jobs());
                }

                return __jobsTable;
            }
        }

        /// <summary>
        /// Create a job
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="isEnabled"></param>
        /// <param name="isMultipleRunningInstancesAllowed"></param>
        /// <param name="jobId"></param>
        /// <param name="version"></param>
        /// <param name="lastInstanceStatus"></param>
        /// <returns></returns>
        public async Task CreateJob(string name, string description, bool isEnabled = true, bool isMultipleRunningInstancesAllowed = true, Guid? jobId = null, string version = null, string lastInstanceStatus = "None")
        {
            var job = new Job(jobId.GetValueOrDefault(Guid.NewGuid()), version)
            {
                CreatedOn = DateTime.UtcNow,
                IsEnabled = isEnabled,
                IsMultipleRunningInstancesAllowed = isMultipleRunningInstancesAllowed,
                LastInstanceId = Guid.Empty,
                LastInstanceStatus = lastInstanceStatus,
                Name = name,
                Description = description
            };
            _ = await JobsTable.UpsertEntityAsync(job);
        }



        /// <summary>
        /// Inserts or updates/replaces a specified job
        /// </summary>
        public async Task SaveAsync(Job job)
        {
            _ = await JobsTable.UpsertEntityAsync(job);
        }

        /// <summary>
        /// Deletes the specified job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public async Task DeleteAsync(Job job)
        {                        
            //TODO return more than a simple boolean and/or create a cleanup method that can run on a schedule for such events
            var instances = await GetJobInstancesAsync(job.Id);
            foreach (var instance in instances)
            {
                await DeleteAsync(instance);
            }

            //if we got here, then all the records are deleted and we can move forward with life
            _ = await JobsTable.DeleteEntityAsync(job.PartitionKey, job.RowKey);
        }

        /// <summary>
        /// Returns all jobs by iterating over each segment returned
        /// </summary>
        /// <returns></returns>
        public async Task<List<Job>> GetJobsAsync()
        {
            return await JobsTable.GetEntitiesAsync<Job>();
        }


        /// <summary>
        /// Gets the specified job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public async Task<Job> GetJobAsync(Guid jobId)
        {
            var jobs = await JobsTable.GetEntitiesAsync<Job>(jobId.ToString());
            if(jobs.Count == 0)
            {
                return null;
            }
            return jobs.FirstOrDefault();
        }

        /// <summary>
        /// Gets the specified job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>Indicates whether the operation was successful</returns>
        public async Task<Job> GetJobAsync(Guid jobId, string version)
        {
            return  await JobsTable.GetEntityWhenExistsAsync<Job>(jobId.ToString(), version);
        }
    }
}