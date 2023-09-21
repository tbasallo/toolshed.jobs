using System.Collections.Generic;

namespace Toolshed.Jobs
{
    public class PagedTableEntity<T>
    {
        public List<T> Entities { get; set; }
        public string NextPartitionKey { get; set; }
        public string NextRowKey { get; set; }
        public string PreviousPartitionKey { get; set; }
        public string PreviousRowKey { get; set; }
    }
}
