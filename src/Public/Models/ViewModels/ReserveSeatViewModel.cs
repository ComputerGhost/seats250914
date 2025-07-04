using Core.Application.Reservations;
using Core.Domain.Authorization;
using ErrorOr;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Public.Models.ViewModels;

public class ReserveSeatViewModel
{
    private TimeSpan _timeUntilExpiration;

    public ReserveSeatViewModel()
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
    public required int SeatNumber { get; set; }

    /* Form fields */

    public string Action { get; set; } = null!;

    public bool AgreeToTerms { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    public string PreferredLanguageAbbreviated { get; set; }

    public ReserveSeatCommand ToReserveSeatCommand(string ipAddress, LockSeatCommandResponse seatLock)
    {
        var preferredLangauge = PreferredLanguageAbbreviated switch
        {
            "en" => "English",
            "ko" => "한국어",
            _ => throw new NotImplementedException($"Language abbreviation ${PreferredLanguageAbbreviated} is not valid."),
        };

        return new ReserveSeatCommand
        {
            IpAddress = ipAddress,
            Email = Email,
            Name = Name,
            PhoneNumber = PhoneNumber,
            PreferredLanguage = preferredLangauge,
            SeatKey = seatLock.SeatKey,
            SeatNumber = seatLock.SeatNumber,
        };
    }

    public ReserveSeatViewModel WithError(Error error)
    {
        Debug.Assert(error.Metadata != null);
        Debug.Assert(error.Metadata["details"] != null);
        var authResult = (AuthorizationResult)error.Metadata["details"];
        FailureReason = authResult.FailureReason;

        return this;
    }

    public ReserveSeatViewModel WithExpiration(DateTimeOffset expiration)
    {
        _timeUntilExpiration = expiration - DateTimeOffset.UtcNow;
        return this;
    }
}
