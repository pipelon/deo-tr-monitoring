using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace deo_tr_monitoring
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks()                
                /*.AddCheck<PingHealthCheck>("PingMvmsw590")
                .AddCheck<PingHealthCheck>("PingMidOra")
                .AddCheck<PingHealthCheck>("PingGoogle")*/
                .AddCheck<ServicesHealthCheck>("Domain")
                .AddCheck<ServicesHealthCheck>("Core")
                .AddCheck<ServicesHealthCheck>("Tasks")
                .AddCheck<ServicesHealthCheck>("GenReal")
                .AddCheck<ControlFileHealthCheck>("DEOACOPLADO_OFE_AGCXRECURSO.txt")
                .AddCheck<ControlFileHealthCheck>("DEOACOPLADO_OFE_AGCXUNIDAD.txt")
                .AddCheck<ControlFileHealthCheck>("DEOACOPLADO_OFE_MAXRECUROS.txt")
                .AddCheck<ControlFileHealthCheck>("DEOACOPLADO_OFE_MAXUNIDADES.txt")
                .AddCheck<ControlFileHealthCheck>("DEOACOPLADO_OFE_RECURSOS.txt")
                .AddCheck<ControlFileHealthCheck>("DEOACOPLADO_OFE_UNIDADES.txt")
                .AddCheck<ControlFileHealthCheck>("DEOACOPLADO_RED_ZONAS.txt")
                .AddCheck<ControlFileHealthCheck>("DEOACOPLADO_RED_ZONAS_DEF.txt")
                .AddCheck<ControlFileHealthCheck>("DEOTR_OFE_AGCXRECURSO.txt")
                .AddCheck<ControlFileHealthCheck>("DEOTR_OFE_AGCXUNIDAD.txt")
                .AddCheck<ControlFileHealthCheck>("DEOTR_OFE_MARGINALES.txt")
                .AddCheck<ControlFileHealthCheck>("DEOTR_OFE_MAXRECUROS.txt")
                .AddCheck<ControlFileHealthCheck>("DEOTR_OFE_MAXUNIDADES.txt")
                .AddCheck<ControlFileHealthCheck>("DEOTR_OFE_RECURSOS.txt")
                .AddCheck<ControlFileHealthCheck>("DEOTR_OFE_UNIDADES.txt")
                .AddCheck<ControlFileHealthCheck>("DEOTR_RED_ZONAS.txt")
                .AddCheck<ControlFileHealthCheck>("DEOTR_RED_ZONAS_DEF.txt")
                .AddCheck<ControlFileHealthCheck>("BranchNoEncontradasCS.dat")
                .AddCheck<ControlFileHealthCheck>("BranchNoEncontrados.dat")
                .AddCheck<ControlFileHealthCheck>("BranchNoEncontradosCM.dat")
                .AddCheck<ControlFileHealthCheck>("BranchNoSupervisados.dat")
                .AddCheck<ControlFileHealthCheck>("BranchOffName.dat")
                .AddCheck<ControlFileHealthCheck>("GD.txt");
            services.AddHealthChecksUI().AddSqliteStorage($"Data Source=healthchecksdb");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksUI(setup =>
                {
                    setup.UIPath = "/monitoring";
                    setup.AddCustomStylesheet("Assets/css/deotr.css");
                });
                endpoints.MapControllers();
            });

            app.UseHealthChecks("/healthcheck", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });
            app.UseHealthChecksUI();
        }
    }
}
