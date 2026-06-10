using Campanhas.Domain.Shared.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Campanhas.Infrastructure.Services.Messaging;

namespace Campanhas.Infrastructure.Extensions
{
    public static class MessagingExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.AddScoped<IMessageService, MessageService>();

            var applicationType = configuration["Application:Type"] ?? "LOCAL";

            services.AddMassTransit(x =>
            {
                x.AddDelayedMessageScheduler();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseInMemoryOutbox();

                    cfg.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(30)));

                    var host = configuration["RabbitMq:Host"];
                    var user = configuration["RabbitMq:Username"];
                    var pass = configuration["RabbitMq:Password"];

                    // AWS usa URI completa; todos os outros ambientes usam hostname simples
                    if (applicationType == "AWS")
                    {
                        cfg.Host(new Uri(host!), "/", h =>
                        {
                            h.Username(user!);
                            h.Password(pass!);
                        });
                    }
                    else
                    {
                        cfg.Host(host, "/", h =>
                        {
                            h.Username(user!);
                            h.Password(pass!);
                        });
                    }

                    cfg.ConfigureEndpoints(context);
                });
            });

            // Não bloqueia o startup se o RabbitMQ ainda não estiver disponível
            services.AddOptions<MassTransitHostOptions>().Configure(options =>
            {
                options.WaitUntilStarted = false;
                options.StartTimeout = TimeSpan.FromSeconds(30);
                options.StopTimeout = TimeSpan.FromSeconds(15);
            });

            logger.LogInformation(" ***** Masstransit service inicializado.");

            return services;
        }
    }
}

       