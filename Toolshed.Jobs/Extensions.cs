using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

namespace Toolshed.Jobs;

public static class Extensions
{
    /// <summary>
    /// Add required services to dependency injection
    /// </summary>
    /// <param name="services"></param>
    public static void AddToolshedAuditing(this IServiceCollection services, string azureStorageConnectionString)
    {
        ServiceManager.InitConnectionString(azureStorageConnectionString);

        services.AddTransient<JobManager>();
        services.AddTransient<JobService>();
    }
}