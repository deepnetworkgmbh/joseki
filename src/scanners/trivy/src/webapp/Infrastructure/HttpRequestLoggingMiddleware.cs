using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using Serilog;
using Serilog.Events;

namespace webapp.Infrastructure
{
    /// <summary>
    /// Writes logs in common format for any incoming HTTP request.
    /// </summary>
    public class HttpRequestLoggingMiddleware
    {
        private const string MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        private static readonly ILogger Log = Serilog.Log.ForContext<HttpRequestLoggingMiddleware>();

        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next <see cref="RequestDelegate"/> to execute. </param>
        public HttpRequestLoggingMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// Executes a middleware.
        /// </summary>
        /// <param name="httpContext">Current <see cref="HttpContext"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext httpContext)
        {
            _ = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

            var start = Stopwatch.GetTimestamp();
            try
            {
                await this.next(httpContext);

                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());

                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

                var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext) : Log;
                if (!(httpContext.Request.Path.StartsWithSegments("/monitoring") ||
                      httpContext.Request.Path.StartsWithSegments("/swagger")))
                {
                    log.Write(
                        level,
                        MessageTemplate,
                        httpContext.Request.Method,
                        GetPath(httpContext),
                        statusCode,
                        elapsedMs);
                }
            }
            catch (Exception ex)
            {
                LogForErrorContext(httpContext)
                    .Error(
                        ex,
                        MessageTemplate,
                        httpContext.Request.Method,
                        GetPath(httpContext),
                        500,
                        Stopwatch.GetTimestamp());
                throw;
            }
        }

        private static ILogger LogForErrorContext(HttpContext httpContext)
        {
            const string userAgentHeaderName = "User-Agent";
            var userAgent = httpContext.Request.Headers.FirstOrDefault(x => x.Key == userAgentHeaderName);
            return Log.ForContext("UserAgent", userAgent.Value);
        }

        /// <summary>
        /// Get elapsed milliseconds based on elapsed ticks.
        /// </summary>
        /// <param name="start">Ticks on a start.</param>
        /// <param name="stop">Ticks in the end.</param>
        /// <returns>Elapsed milliseconds.</returns>
        private static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }

        private static string GetPath(HttpContext httpContext)
        {
            return httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
        }
    }
}