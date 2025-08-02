using System;
using System.Threading.Tasks;

namespace Toolshed.Jobs
{
    public class JobManager
    {
        private JobService Jobs { get; set; }


        public Job Job { get; private set; }
        public JobInstance Instance { get; private set; }

        /// <summary>
        /// If set to true, will abort a running instance and continue if the instance has been running longer than MinimumMinutesRunningForInstanceAbortion
        /// </summary>
        public bool IsRunningExceptionAborted { get; set; }

        /// <summary>
        /// The default number of minutes that an instance must be running before it will be aborted IF IsRunningExceptionAborted is true.
        /// When setting IsRunningExceptionAborted to true, this property should be adjusted to reflect the number of minutes that is acceptable. The default is 240 minutes (4 hours)
        /// </summary>
        public int MinimumMinutesRunningForInstanceAbortion { get; set; } = 240;

        public JobManager()
        {
            Jobs = new JobService();
        }

        public async Task LoadJobAsync(Guid jobId, string version = ServiceManager.DefaultVersionName)
        {
            if(Job is null || Job.Id != jobId || Job.Version != version)
            {
                Job = await Jobs.GetJobAsync(jobId, version);
                if (Job == null)
                {
                    throw new NullReferenceException("Job not found (" + jobId + ")");
                }
            }
        }
        public async Task StartOrLoadJobAsync(Guid jobId, Guid instanceId, string message = "Started")
        {
            await LoadJobAsync(jobId);

            await LoadInstanceAsync(instanceId);
            if (Instance is null)
            {
                await StartJobAsync(message, instanceId);
            }
        }
        public async Task StartJobAsync(Guid jobId, string message = "Started", Guid? instanceId = null)
        {
            await LoadJobAsync(jobId);
            await StartJobAsync(message, instanceId);
        }
        public async Task StartJobAsync(string message = "Started", Guid? instanceId = null)
        {
            if (!Job.IsMultipleRunningInstancesAllowed && Job.IsRunning)
            {
                if (IsRunningExceptionAborted && DateTime.UtcNow.Subtract(Job.LastInstanceStatusOn.Value).TotalMinutes >= MinimumMinutesRunningForInstanceAbortion)
                {
                    Instance = await Jobs.GetJobInstanceAsync(Job.Id, Job.LastInstanceId);
                    if (Instance != null)
                    {
                        await AbortInstanceAsync("Aborted due to running longer than maximum run time");
                    }
                    else
                    {
                        Job.IsRunning = false;
                        await Jobs.SaveAsync(Job);
                    }
                }
                else
                {
                    throw new JobCurrentlyRunningException(Job.LastInstanceId);
                }
            }

            await FinalStart(message, instanceId);
        }




        private async Task SaveAsync(JobInstanceDetail detail)
        {
            await Jobs.SaveAsync(Job);
            await Jobs.SaveAsync(Instance);
            await Jobs.SaveAsync(Instance.GetWithDailyPartition());
            await Jobs.SaveAsync(detail);
        }





        public async Task LoadInstanceAsync(Guid instanceId)
        {
            Instance = await Jobs.GetJobInstanceAsync(Job.Id, instanceId);
        }



        public async Task CompleteJobAsync(string message = "Completed")
        {
            await SaveAsync(Complete(message));
        }
        public async Task AbortInstanceAsync(string message = "Job instance manually aborted")
        {
            await SaveAsync(Abort(message));
        }


        public async Task AddAsync(Exception exception, string? message = null)
        {
            await AddAsync(JobLogLevel.Exception, exception.ToString());

            if(!string.IsNullOrEmpty(message))
            {
                await AddAsync(JobLogLevel.Exception, $"{exception.Message}: {message}");
            }
        }
        public async Task AddAsync(JobLogLevel type, string detailsFormat, object arg0)
        {
            await AddAsync(type, string.Format(detailsFormat, arg0));
        }
        public async Task AddAsync(JobLogLevel type, string detailsFormat, object arg0, object arg1)
        {
            await AddAsync(type, string.Format(detailsFormat, arg0, arg1));
        }
        public async Task AddAsync(JobLogLevel type, string detailsFormat, object arg0, object arg1, object arg2)
        {
            await AddAsync(type, string.Format(detailsFormat, arg0, arg1, arg2));
        }
        public async Task AddAsync(JobLogLevel type, string detailsFormat, params object[] args)
        {
            await AddAsync(type, string.Format(detailsFormat, args));
        }
        public async Task AddAsync(JobLogLevel type, string details)
        {
            var detail = Generate(type, details);
            await SaveAsync(detail);
        }


        public async Task AddInfoAsync(string message)
        {
            await AddAsync(JobLogLevel.Info, message);
        }
        public async Task AddWarningAsync(string message)
        {
            await AddAsync(JobLogLevel.Warning, message);
        }
        public async Task AddErrorAsync(string message)
        {
            await AddAsync(JobLogLevel.Error, message);
        }








        async Task FinalStart(string message, Guid? instanceId = null)
        {
            Instance = new JobInstance(Job.Id, instanceId.GetValueOrDefault(Guid.NewGuid()), Job.Version);

            var detail = new JobInstanceDetail(Instance.JobId, Instance.InstanceId)
            {
                Date = DateTime.UtcNow,
                Type = JobDetailType.Started,
                Details = message
            };

            Instance.TotalDetails = 1;
            Instance.LastOn = detail.Date;
            Instance.LastType = detail.Type;
            Instance.StartedOn = Instance.LastOn;

            Job.LastInstanceStatusOn = detail.Date;
            Job.LastInstanceStatus = detail.Type.ToString();
            Job.IsRunning = true;
            Job.TotalInstancesRunning++;
            Job.TotalInstances++;
            Job.TotalLifetimeInstances++;
            Job.LastInstanceId = Instance.InstanceId;
            Job.HasException = Instance.HasException;
            Job.HasError = Instance.HasError;
            Job.HasWarning = Instance.HasWarning;

            await SaveAsync(detail);
        }
        JobInstanceDetail Complete(string message)
        {
            var detail = new JobInstanceDetail(Instance.JobId, Instance.InstanceId)
            {
                Date = DateTime.UtcNow,
                Type = JobDetailType.Complete,
                Details = message
            };

            Instance.TotalDetails++;
            Instance.LastOn = detail.Date;
            Instance.LastType = detail.Type;
            Instance.CompletedOn = detail.Date;
            Instance.RunningTimeInSeconds = Math.Round(Instance.CompletedOn.Value.Subtract(Instance.StartedOn).TotalSeconds, 2);

            Job.LastInstanceStatus = detail.Type;
            Job.LastInstanceStatusOn = detail.Date;
            Job.LastInstanceId = Instance.InstanceId;
            Job.IsRunning = false;
            Job.LastInstanceRunningTimeInSeconds = Instance.RunningTimeInSeconds;
            Job.HasException = Instance.HasException;
            Job.HasError = Instance.HasError;
            Job.HasWarning = Instance.HasWarning;

            return detail;
        }
        JobInstanceDetail Abort(string message)
        {
            if (Instance == null)
            {
                throw new ArgumentNullException("No instance to abort");
            }

            var now = DateTime.UtcNow;
            Instance.TotalDetails++;

            JobInstanceDetail detail;
            if (Instance.CompletedOn.HasValue)
            {
                detail = new JobInstanceDetail(Instance.JobId, Instance.InstanceId)
                {
                    Date = DateTime.UtcNow,
                    Type = JobDetailType.Info,
                    Details = "Instance already completed, abort request ignored"
                };
            }
            else
            {
                Instance.CompletedOn = now;
                Instance.HasError = true;
                Instance.LastOn = now;
                Instance.LastType = JobDetailType.Aborted;
                Instance.RunningTimeInSeconds = Math.Round(Instance.CompletedOn.Value.Subtract(Instance.StartedOn).TotalSeconds, 2);

                if (Job.LastInstanceId == Instance.InstanceId)
                {
                    Job.HasError = true;
                    Job.TotalErrors++;
                    Job.IsRunning = false;
                    Job.LastInstanceStatusOn = now;
                    Job.LastInstanceStatus = JobDetailType.Aborted;
                    Job.LastInstanceRunningTimeInSeconds = Instance.RunningTimeInSeconds;
                }

                detail = new JobInstanceDetail(Instance.JobId, Instance.InstanceId)
                {
                    Date = DateTime.UtcNow,
                    Type = JobDetailType.Aborted,
                    Details = message
                };
            }

            return detail;
        }
        JobInstanceDetail Generate(JobLogLevel type, string details)
        {
            var detail = new JobInstanceDetail(Instance.JobId, Instance.InstanceId)
            {
                Date = DateTime.UtcNow,
                Type = type.ToString(),
                Details = details
            };

            if (type == JobLogLevel.Warning)
            {
                Instance.HasWarning = true;
            }
            if (type == JobLogLevel.Exception)
            {
                Instance.HasException = true;
            }
            if (type == JobLogLevel.Error)
            {
                Instance.HasError = true;
            }

            Instance.TotalDetails++;
            Instance.LastOn = detail.Date;
            Instance.LastType = detail.Type;
            Instance.RunningTimeInSeconds = Math.Round(DateTime.UtcNow.Subtract(Instance.StartedOn).TotalSeconds, 2);

            Job.LastInstanceStatus = detail.Type;
            Job.LastInstanceStatusOn = detail.Date;
            Job.LastInstanceId = Instance.InstanceId;
            Job.LastInstanceRunningTimeInSeconds = Instance.RunningTimeInSeconds;

            Job.HasException = Instance.HasException;
            Job.HasError = Instance.HasError;
            Job.HasWarning = Instance.HasWarning;

            if (Instance.HasException)
            {
                Job.TotalExceptions++;
            }
            if (Instance.HasError)
            {
                Job.TotalErrors++;
            }
            if (Instance.HasWarning)
            {
                Job.TotalWarnings++;
            }

            return detail;
        }
    }
}
