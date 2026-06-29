using ChurchApi.Middleware;
using Serilog;
using Serilog.Events;

namespace ChurchApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        app.UseApplicationSwagger();

        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, exception) =>
            {
                if (httpContext.Request.Path.StartsWithSegments("/health"))
                {
                    return LogEventLevel.Debug;
                }

                return exception is null && httpContext.Response.StatusCode < StatusCodes.Status500InternalServerError
                    ? LogEventLevel.Information
                    : LogEventLevel.Error;
            };

            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("Endpoint", httpContext.GetEndpoint()?.DisplayName);
            };
        });

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapApplicationHealthChecks();

        return app;
    }
}
