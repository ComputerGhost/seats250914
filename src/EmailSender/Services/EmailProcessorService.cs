using Core.Application.Emails;
using Core.Application.Reservations;
using Core.Domain.Common.Enumerations;
using MediatR;
using Serilog;
using System.Text.RegularExpressions;

namespace EmailSender.Services;
internal partial class EmailProcessorService(
    IMediator mediator, 
    RazorTemplateService templateService,
    SmtpEmailSender sender)
{
    public async Task<bool> Process(ListPendingEmailsQueryResponseItem email, CancellationToken cancellationToken)
    {
        if (CalculateNextAttemptTime(email.AttemptCount) > DateTimeOffset.Now)
        {
            return false;
        }

        var attemptNumber = email.AttemptCount + 1;
        Log.Information("Email #{Id} is being processed for attempt #{attemptNumber}.", email.Id, attemptNumber);

        try
        {
            await CreateAndSend(email, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                // If cancelled, exit early and don't mark the email as sent.
                return false;
            }

            // Do not pass the cancellation token here. We want to do this if we get here.
            await mediator.Send(new MarkEmailAsSucceededCommand(email.Id));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Email #{Id} failed with exception message: {@Message}", email.Id, ex.Message);
            Log.Debug("Stack trace is: {StackTrace}", ex.StackTrace);
            // Do not pass the cancellation token here. We want to do this if we get here.
            await mediator.Send(new MarkEmailAsFailedCommand(email.Id));
            return false;
        }
    }

    [GeneratedRegex(@"<title>(.*?)<\/title>")]
    private static partial Regex TitleRegex();

    private DateTimeOffset CalculateNextAttemptTime(int attemptCount)
    {
        List<int> delays = [0, 600, 1800];
        var delay = (attemptCount < delays.Count) ? delays[attemptCount] : delays[^1];
        return DateTimeOffset.Now.AddSeconds(delay);
    }

    private Task CreateAndSend(ListPendingEmailsQueryResponseItem email, CancellationToken cancellationToken)
    {
        switch (email.EmailType)
        {
            case EmailType.UserSubmittedReservation:
                return CreateAndSend_UserSubmittedReservation(email, cancellationToken);
            default:
                throw new NotImplementedException();
        }
    }

    private async Task CreateAndSend_UserSubmittedReservation(ListPendingEmailsQueryResponseItem email, CancellationToken cancellationToken)
    {
        const EmailType type = EmailType.UserSubmittedReservation;
        Log.Information("Email #{Id} was resolved as type {type}.", email.Id, type);

        var result = await mediator.Send(new FetchReservationQuery(email.ReferenceId), cancellationToken);
        if (result.IsError)
        {
            throw new Exception($"Reservation #{email.ReferenceId} could not be loaded.");
        }

        var reservation = result.Value;

        const string TEMPLATE_NAME = "UserSubmittedReservation";
        var languageId = NormalizeLanguage(result.Value.PreferredLanguage);
        var body = await templateService.Render(TEMPLATE_NAME, languageId, reservation);
        var subject = TitleRegex().Match(body).Groups[1].Value;

        await sender.SendEmail(reservation.Email, subject, body, cancellationToken);
    }

    /// <summary>
    ///  Convert various language strings into their two-character id.
    /// </summary>
    /// <param name="language"></param>
    /// <returns></returns>
    private string NormalizeLanguage(string language)
    {
        if (language.Length == 2)
        {
            return language.ToLower();
        }

        return language.ToLower() switch
        {
            "english" or "영어" => "en",
            "korean" or "한국어" => "ko",
            _ => "en",
        };
    }
}
