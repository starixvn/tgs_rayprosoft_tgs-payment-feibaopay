using System.Reflection;
using Tgs.Payment.FeibaoPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeibaoPay(
        this IServiceCollection services,
        Action<FeibaoPayOptions> configureOptions,
        Action<IHttpClientBuilder> ConfigureAiClient)
    {
        ConfigureAiClient?.Invoke(services.AddHttpClient<FeibaoPayClient>());

        return services
            .AddOptions<FeibaoPayOptions>()
            .Configure(configureOptions)
            .Services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddSingleton<FeibaoPayCryptor>();
    }
}
