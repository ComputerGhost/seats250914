using Core.Application.Reservations;
using Core.Domain.Authorization;
using ErrorOr;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Public.Models.ViewModels;

public class ReserveSeatsViewModel
{
    private TimeSpan _timeUntilExpiration;

    public ReserveSeatsViewModel()
    {
        var culture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
        PreferredLanguageAbbreviated = culture;
    }

    /* Display only */

    [BindNever]
    public AuthorizationRejectionReason? FailureReason { get; set; } = null;

    public string TimeUntilExpirationText => _timeUntilExpiration.ToString(@"mm\:ss");

    public string TimeUntilExpirationPeriod => _timeUntilExpiration.ToString(@"\P\Tmm\Mss\S");

    [BindNever]
    public required string OrganizerEmail { get; set; } = null!;

    [BindNever]
    public required IEnumerable<int> SeatNumbers { get; set; }

    public string SeatNumbersJoined => string.Join(", ", SeatNumbers ?? []);

    /* Form fields */

    public string Action { get; set; } = null!;

    public bool AgreeToTerms { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    public string PreferredLanguageAbbreviated { get; set; }

    public ReserveSeatsCommand ToReserveSeatsCommand(string ipAddress, LockSeatsCommandResponse seatLocks)
    {
        var preferredLangauge = PreferredLanguageAbbreviated switch
        {
            "en" => "영어",
            "ko" => "한국어",
            "vi" => "베트남어",
            "zh-Hans" => "중국어",
            _ => throw new NotImplementedException($"Language abbreviation ${PreferredLanguageAbbreviated} is not valid."),
        };

        return new ReserveSeatsCommand
        {
            IpAddress = ipAddress,
            Email = Email,
            Name = Name,
            PhoneNumber = PhoneNumber,
            PreferredLanguage = preferredLangauge,
            SeatLocks = seatLocks.SeatLocks,
        };
    }

    public ReserveSeatsViewModel WithError(Error error)
    {
        Debug.Assert(error.Metadata != null);
        Debug.Assert(error.Metadata["details"] != null);
        var authResult = (AuthorizationResult)error.Metadata["details"];
        FailureReason = authResult.FailureReason;

        return this;
    }

    public ReserveSeatsViewModel WithExpiration(DateTimeOffset expiration)
    {
        _timeUntilExpiration = expiration - DateTimeOffset.UtcNow;
        return this;
    }
}
