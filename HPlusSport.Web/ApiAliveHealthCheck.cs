using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HPlusSport.Web
{
    public class ApiAliveHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var wc = new WebClient())
            {
                try
                {
                    var result = wc.DownloadString("https://localhost:5101/alive");
                    if (result == "true")
                    {
                        return Task.FromResult(HealthCheckResult.Healthy("good"));
                    }
                }
                catch (Exception ex)
                {
                    return
                        Task.FromResult(HealthCheckResult.Degraded("bad"));
                }
            }

            return
                Task.FromResult(HealthCheckResult.Degraded("bad"));
        }
    }
}
