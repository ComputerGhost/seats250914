using Core.Application.Emails;
using EmailSender.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace EmailSender;
internal class Worker(IServiceProvider services) : BackgroundService
{
    const int MAX_ATTEMPTS = 3;
    const int POLLING_DELAY = 10;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var mediator = services.GetRequiredService<IMediator>();
        var processor = services.GetRequiredService<EmailProcessorService>();
        Log.Information("Started EmailSender worker.");

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await mediator.Send(new ListPendingEmailsQuery
            {
                MaxAttempts = MAX_ATTEMPTS,
            }, cancellationToken);

            foreach (var email in result.Data)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await processor.Process(email, cancellationToken);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(POLLING_DELAY), cancellationToken);
        }

        Log.Information("Stopped EmailSender worker.");
    }
}
