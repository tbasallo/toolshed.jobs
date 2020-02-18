using System;
using Toolshed.Jobs;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World! - " + args.ToString());

            JobServiceManager.InitStorageKey("baspin", "7CeKKBkxqCE2nwARKVgT8FLZGBj+JvIlukCkn/kGuonY5jSauBrnViZRBZwjdFwM+074ayXiSmdGurwnGIp1jg==", "test");
            //JobServiceManager.CreateTablesIfNotExists();

            var testJObName = "Test Job";
            var id = Guid.Parse("361ffbb0-3ef2-40a9-842c-fa4bfeb1dcde");

            //var j = new JobService();
            //j.Save(new Job(testJObName, Guid.NewGuid()) { CreatedOn = DateTime.UtcNow, Description = "A test job" });

            var m = new JobManager(testJObName, id);
            m.StartJob();

            m.Add(JobLogLevel.Info, "Keep it up!!");

            m.CompleteJob();






            Console.ReadLine();
        }
    }
}
