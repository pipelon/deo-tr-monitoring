using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class ControlFileHealthCheck : IHealthCheck
{
    private readonly IConfiguration _config;
    public ControlFileHealthCheck(IConfiguration config)
    {
        _config = config;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {

        var filePath = _config["HealthChecks:Config:ControlFiles:" + context.Registration.Name + ":FilePath"];
        var time = _config["HealthChecks:Config:ControlFiles:" + context.Registration.Name + ":Time"];
        try
        {
            //VERIFICO QUE EL ARCHIVO EXISTA ANTES DE PROCESARLO
            if (!File.Exists(filePath))
            {
                return await Task.FromResult(HealthCheckResult.Unhealthy("El archivo '" +
                filePath + "' no existe"));
            }

            //COMPARO LA ULTIMA FECHA DE ACTUALIZACION CONTRA LA FECHA ACTUAL
            DateTime lastUpdated = File.GetLastWriteTime(filePath);
            var timeElapsed = Math.Round((DateTime.Now - lastUpdated).TotalSeconds);

            //SI SUPERA LOS SEGUNDOS PARAMETRIZADOS
            if (timeElapsed > Int32.Parse(time))
            {         
                //REINICIO LOS SERVICIOS
                this.restartServices(context.Registration.Name);
                //RETORNO ERROR
                return await Task.FromResult(HealthCheckResult.Unhealthy("El archivo '" +
                filePath + "' no ha sido actualizado desde hace '" + timeElapsed + "' segundos"));
            }

            //ARCHIVO ACTUALIZANDOSE CON EXITO
            return await Task.FromResult(HealthCheckResult.Healthy());

        }
        catch (Exception)
        {
            return await Task.FromResult(HealthCheckResult.Unhealthy("Error en lectura del archivo"));
        }
    }

    private bool restartServices(string nameFile)
    {
        try
        {
            //OBTENGO LOS SERVICIOS QUE DEBEN REINICIARSE
            List<string> arrayServices = _config
                    .GetSection("HealthChecks:Config:ControlFiles:" + nameFile + ":RestartServicesOnError")
                    .Get<List<string>>();

            foreach (string service in arrayServices)
            {
                var ps1StartService = _config["HealthChecks:Config:ControlServices:" + service + ":Ps1StartPath"];
                var ps1StopService = _config["HealthChecks:Config:ControlServices:" + service + ":Ps1StopPath"];

                //PARO EL SERVICIO
                ProcessStartInfo stopInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy unrestricted -file \"{ps1StopService}\"",
                    UseShellExecute = false
                };
                Process.Start(stopInfo);

                //INICIO EL SERVICIO
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy unrestricted -file \"{ps1StartService}\"",
                    UseShellExecute = false
                };
                Process.Start(startInfo);                
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }



    }
}