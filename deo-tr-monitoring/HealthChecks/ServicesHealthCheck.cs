using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class ServicesHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;
    public ServicesHealthCheck(IConfiguration config)
    {
        _config = config;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        //OBTENGO EL NOMBRE DEL SERVICIO Y EL PSI DE INICIO
        var nameService = _config["HealthChecks:Config:ControlServices:" + context.Registration.Name + ":Name"];
        var ps1PathService = _config["HealthChecks:Config:ControlServices:" + context.Registration.Name + ":Ps1StartPath"];
        try
        {
            //SI EL SERVICIO NO ESTA ACTIVO
            if (Process.GetProcessesByName(nameService).Length == 0)
            {
                //INICIO EL SERVICIO
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy unrestricted -file \"{ps1PathService}\"",
                    UseShellExecute = false
                };
                Process.Start(startInfo);
                //ERROR EN PANTALLA
                return await Task.FromResult(HealthCheckResult.Unhealthy("El servicio no se encuentra activo"));
            }
            else
            {
                //SERVICIO ACTIVO Y CORRIENDO
                return await Task.FromResult(HealthCheckResult.Healthy());
            }
        }
        catch (Exception)
        {
            return await Task.FromResult(HealthCheckResult.Unhealthy("El servicio no se encuentra activo"));
        }
    }
}