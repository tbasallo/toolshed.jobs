using System;
using System.Runtime.Serialization;

namespace Toolshed.Jobs
{
    [Serializable]
    public class JobCurrentlyRunningException : Exception
    {
        public JobCurrentlyRunningException()
        {
        }

        public JobCurrentlyRunningException(string message) : base(message)
        {
        }

        public JobCurrentlyRunningException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public JobCurrentlyRunningException(Guid instanceId, double totalMinutesRunning, string message = "Job instance currently running") : base($"{message}, running for {totalMinutesRunning} minutes ({instanceId})") { }
    }
}
