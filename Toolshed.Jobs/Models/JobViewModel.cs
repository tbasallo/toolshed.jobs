using System.Collections.Generic;

namespace Toolshed.Jobs
{
    public class JobViewModel
    {
        public Job Job { get; set; }
        public List<JobInstanceDetail> Details { get; set; }
    }
}
