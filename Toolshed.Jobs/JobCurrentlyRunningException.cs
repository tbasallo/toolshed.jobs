using System;
using System.Runtime.Serialization;

namespace Toolshed.Jobs
{
    [Serializable]
    public class JobCurrentlyRunningException : Exception
    {
        public string InstanceId { get; set; }
        public JobCurrentlyRunningException()
        {
        }

        public JobCurrentlyRunningException(string message) : base(message)
        {
        }

        public JobCurrentlyRunningException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public JobCurrentlyRunningException(Guid instanceId, string message = "Job instance currently running") : base($"{message}, {instanceId}") { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("InstanceId", InstanceId);
        }

        protected JobCurrentlyRunningException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
