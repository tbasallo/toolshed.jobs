using System;
using System.Diagnostics;
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

        public JobManager(Guid jobId, string version = ServiceManager.DefaultVersionName)
        {
            Jobs = new JobService();
            Job = Jobs.GetJob(jobId, version);

            if (Job == null)
            {
                throw new NullReferenceException("Job not found (" + jobId + ")");
            }
        }

        private async Task SaveAsync(JobInstanceDetail detail)
        {
            Job = await Jobs.SaveAsync(Job);
            Instance = await Jobs.SaveAsync(Instance);
            await Jobs.SaveAsync(detail);

            if (detail.Type == JobDetailType.Started.ToString())
            {
                await Jobs.SaveAsync(Instance.GetWithDailyPartition());
            }
            if (detail.Type == JobDetailType.Complete.ToString())
            {
                await Jobs.SaveAsync(Instance.GetWithDailyPartition());
            }
        }
        private void Save(JobInstanceDetail detail)
        {
            Job = Jobs.Save(Job);
            Instance = Jobs.Save(Instance);
            Jobs.Save(detail);


            if (detail.Type == JobDetailType.Started.ToString())
            {
                Jobs.Save(Instance.GetWithDailyPartition());
            }
            if (detail.Type == JobDetailType.Complete.ToString())
            {
                Jobs.Save(Instance.GetWithDailyPartition());
            }
        }


        public async Task<bool> StartOrLoadJobAsync(Guid instanceId, string message = "Started")
        {
            var instanceExists = await LoadInstanceAsync(instanceId);
            if (!instanceExists)
            {
                var details = await StartAsync(message, instanceId);
                await SaveAsync(details);
            }

            return Instance != null;
        }
        public bool StartOrLoadJob(Guid instanceId, string message = "Started")
        {
            var instanceExists = LoadInstance(instanceId);
            if (!instanceExists)
            {
                var details = Start(message, instanceId);
                Save(details);
            }

            return Instance != null;
        }

        public async Task StartJobAsync(string message = "Started", Guid? instanceId = null)
        {
            var details = await StartAsync(message, instanceId);
            await SaveAsync(details);
        }
        public void StartJob(string message = "Started", Guid? instanceId = null)
        {
            var details = Start(message, instanceId);
            Save(details);
        }


        private async Task<JobInstanceDetail> StartAsync(string message, Guid? instanceId = null)
        {
            if (!Job.IsMultipleRunningInstancesAllowed && Job.IsRunning)
            {
                if (IsRunningExceptionAborted && DateTime.UtcNow.Subtract(Job.LastInstanceStatusOn.Value).TotalMinutes >= MinimumMinutesRunningForInstanceAbortion)
                {
                    Instance = await Jobs.GetJobInstanceAsync(Job.Id, Job.LastInstanceId);
                    await AbortInstanceAsync("Aborted due to running longer than maximum run time");
                }
                else
                {
                    throw new JobCurrentlyRunningException(Job.LastInstanceId);
                }
            }

            return FinalStart(message, instanceId);
        }
        private JobInstanceDetail Start(string message, Guid? instanceId = null)
        {
            if (!Job.IsMultipleRunningInstancesAllowed && Job.IsRunning)
            {
                if (IsRunningExceptionAborted && DateTime.UtcNow.Subtract(Job.LastInstanceStatusOn.Value).TotalMinutes >= MinimumMinutesRunningForInstanceAbortion)
                {
                    Instance = Jobs.GetJobInstance(Job.Id, Job.LastInstanceId);
                    AbortInstance("Aborted due to running longer than maximum run time");
                }
                else
                {
                    throw new JobCurrentlyRunningException(Job.LastInstanceId);
                }
            }

            return FinalStart(message, instanceId);
        }
        private JobInstanceDetail FinalStart(string message, Guid? instanceId = null)
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
            Job.TotalInstances++;
            Job.LastInstanceId = Instance.InstanceId;
            Job.IsRunning = true;

            return detail;
        }


        public bool LoadInstance(Guid instanceId)
        {
            Instance = Jobs.GetJobInstance(Job.Id, instanceId);
            return Instance != null;
        }
        public async Task<bool> LoadInstanceAsync(Guid instanceId)
        {
            Instance = await Jobs.GetJobInstanceAsync(Job.Id, instanceId);
            return Instance != null;
        }



        public async Task CompleteJobAsync(string message = "Completed")
        {
            await SaveAsync(Complete(message));
        }
        public void CompleteJob(string message = "Completed")
        {
            Save(Complete(message));
        }
        private JobInstanceDetail Complete(string message)
        {
            var detail = new JobInstanceDetail(Instance.JobId, Instance.InstanceId)
            {
                Date = DateTime.UtcNow,
                Type = JobDetailType.Complete,
                Details = message
            };

            Job.LastInstanceStatus = detail.Type;
            Job.LastInstanceStatusOn = detail.Date;
            Job.LastInstanceId = Instance.InstanceId;
            Job.IsRunning = false;

            Instance.TotalDetails++;
            Instance.LastOn = detail.Date;
            Instance.LastType = detail.Type;
            Instance.CompletedOn = detail.Date;

            Instance.RunningTimeInSeconds = Math.Round(Instance.CompletedOn.Value.Subtract(Instance.StartedOn).TotalSeconds, 2);
            Job.LastInstanceRunningTimeInSeconds = Instance.RunningTimeInSeconds;

            return detail;
        }


        public async Task AbortInstanceAsync(string message = "Job instance manually aborted")
        {
            await SaveAsync(Abort(message));
        }
        public void AbortInstance(string message = "Job instance manually aborted")
        {
            Save(Abort(message));
        }
        private JobInstanceDetail Abort(string message)
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


        public async Task AddAsync(Exception exception)
        {
            await AddAsync(JobLogLevel.Exception, exception.ToString());
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

        public void Add(Exception exception)
        {
            Add(JobLogLevel.Exception, exception.ToString());
        }
        public void Add(JobLogLevel type, string detailsFormat, object arg0)
        {
            Add(type, string.Format(detailsFormat, arg0));
        }
        public void Add(JobLogLevel type, string detailsFormat, object arg0, object arg1)
        {
            Add(type, string.Format(detailsFormat, arg0, arg1));
        }
        public void Add(JobLogLevel type, string detailsFormat, object arg0, object arg1, object arg2)
        {
            Add(type, string.Format(detailsFormat, arg0, arg1, arg2));
        }
        public void Add(JobLogLevel type, string detailsFormat, params object[] args)
        {
            Add(type, string.Format(detailsFormat, args));
        }
        public void Add(JobLogLevel type, string details)
        {
            var detail = Generate(type, details);
            Save(detail);
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
            Job.HasWarning = Instance.HasWarning;
            Job.HasException = Instance.HasException;
            Job.HasError = Instance.HasError;

            return detail;
        }
    }
}
