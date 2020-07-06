using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class PingHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;
    public PingHealthCheck(IConfiguration config)
    {
        _config = config;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (var ping = new Ping())
            {
                var nameServer = _config["HealthChecks:Config:" + context.Registration.Name];
                var reply = ping.Send(nameServer);
                if (reply.Status != IPStatus.Success)
                {
                    return await Task.FromResult(HealthCheckResult.Unhealthy("No hay conexion al servidor"));
                }
                if (reply.RoundtripTime > 100)
                {
                    return await Task.FromResult(HealthCheckResult.Degraded());
                }
                return await Task.FromResult(HealthCheckResult.Healthy());
            }
        }
        catch (Exception)
        {
            return await Task.FromResult(HealthCheckResult.Unhealthy("No hay conexion al servidor222"));
        }
    }
}