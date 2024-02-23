using System.Diagnostics;
using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.MessageQueue.Nats.Serializers;
using Tgs.Payment.FeibaoPay;
using Tgs.Payment.FeibaoPay.MessageQueue;

var builder = WebApplication.CreateBuilder(args);
{
    _ = builder.Logging
        .AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            _ = options.AddFluentdForwardExporter(fluentOptions =>
            {
                var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
                fluentOptions.Host = nodeIp;
                fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
                fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
                    "MachineName",
                    () => Environment.MachineName));
            });
        });

    _ = builder.Services
        .AddControllers();

    _ = builder.Services
        .AddFeibaoPay(options => builder.Configuration.GetSection("FeibaoPay").Bind(options),
            clientBuilder => clientBuilder
                .ConfigureHttpClient((sp, http) =>
                {
                    var options = sp.GetRequiredService<IOptions<FeibaoPayOptions>>().Value;

                    http.BaseAddress = options.PaymentApiUrl;
                })
                .AddLoggingHandler<FeibaoPayOptions>())
        .AddPaymentClient()
        .AddPaymentServiceClient(httpBuilder => httpBuilder
            .ConfigureHttpClient((sp, http) =>
            {
                var options = sp.GetRequiredService<IOptions<FeibaoPayOptions>>().Value;

                http.BaseAddress = options.PaymentServiceBaseUrl;
            }));

    _ = builder.Services
        .AddKeyedSingleton(
            "Nats",
            (sp, _) => new NatsConnection(NatsOpts.Default with
            {
                Url = builder.Configuration.GetConnectionString("Nats")!,
                SerializerRegistry = TgsNatsSerializerRegistration.Default
            }))
        .AddMessageQueue()
        .AddNatsMessageQueue(config => config
            .ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats"))
            .AddProcessor(
                ProcessorTypes.DepositProcessorType,
                "tgs.payment.feibaopay.deposit",
                "tgs.payment.feibaopay.deposit")
            .AddProcessor(
                ProcessorTypes.WithdrawProcessorType,
                "tgs.payment.feibaopay.withdraw",
                "tgs.payment.feibaopay.withdraw")
            .AddHandler(
                ProcessorTypes.DepositTimeoutCheckHandlerType,
                $"tgs.payment.feibaopay.deposit.schedule.reject",
                $"tgs.payment.feibaopay.deposit.schedule.reject")
            .AddProcessor(
                ProcessorTypes.GetBalanceProcessorType,
                "tgs.payment.feibaopay.balance",
                "tgs.payment.feibaopay.balance"))
        .AddNatsGlobPatternExchange("*");

    var excludeExtensions = new[] { "html", "ico", "png", "jpg" };
    _ = builder.Services
        .AddProfileViewer()
        .AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
            logging.MediaTypeOptions.AddText("application/x-www-form-urlencoded");
            logging.MediaTypeOptions.AddText("text/html");
            logging.MediaTypeOptions.AddText("multipart/form-data");
        })
        .AddHealthChecks()
        .Services
        .AddOpenTelemetry()
        .WithTracing(traceBuilder => traceBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddEnvironmentVariableDetector())
            .AddSource("Tgs.*")
            .AddSource("TGS.*")
            .AddSource("Valhalla.*")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddProfileViewExporter()
            .AddOtlpExporter())
        .WithMetrics(meterBuiler => meterBuiler
            .AddRuntimeInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter())
        .Services
        .Configure<AspNetCoreTraceInstrumentationOptions>(options => options.Filter =
            httpContext => !httpContext.Request.Path.HasValue
                || !httpContext.Request.Path.StartsWithSegments("/swagger")
                && !httpContext.Request.Path.StartsWithSegments("/healthz")
                && !httpContext.Request.Path.StartsWithSegments("/metrics")
                && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                && !excludeExtensions.Any(
                    ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));
}

var app = builder.Build();
{
    _ = app.UseHttpLogging();
    _ = app.MapControllers();
    _ = app.MapHealthChecks("/healthz");
    app.MapProfileViewer();

    _ = app.Use(async (context, next) =>
    {
        await next().ConfigureAwait(false);

        var logger = context.RequestServices.GetService<ILoggerFactory>()
            ?.CreateLogger("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware");

        if (logger?.IsEnabled(LogLevel.Information) == true)
        {
            using var stream = new StreamReader(context.Request.Body);

            _ = await stream.ReadToEndAsync().ConfigureAwait(false);
        }
    });
}

app.Run();
