using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JsonReflector;
namespace JsonReflector
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
           
        }


        Session ResolveSession(HttpContext ctx)
        {
            var integration = ctx.RequestServices.GetRequiredService<IDispatcherIntegration>();

            var found = ctx.Request.Headers.TryGetValue("x-remote-session-id", out var sessionId);

            if (!found)
            {
                // clean session on every request
                return integration.CreateSession();
            }

            var dispatcher = ctx.RequestServices.GetService<Dispatcher>();

            var stored = dispatcher.Sessions.TryGetValue(sessionId, out var foundSession);

            if (stored)
            {
                return foundSession;
            }

            var blankSession = integration.CreateSession();
            dispatcher.Sessions.Add(sessionId, blankSession);

            return blankSession;

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/run", async context =>
                {

                    var d = context.RequestServices.GetRequiredService<Dispatcher>();
                    var cont = await context.Request.Body.GetRawBytesAsync();
                    var ses = ResolveSession(context);
                    var ret = d.DispatchJson(cont, ses);
                    await context.Response.WriteAsync(ret.AsString());
                });

                endpoints.MapGet("/", async context =>
                {

                    var d = context.RequestServices.GetRequiredService<Dispatcher>();
                    var ret = d.Describe();
                    await context.Response.WriteAsync(ret.AsString());
                });
            });
        }
    }
}
