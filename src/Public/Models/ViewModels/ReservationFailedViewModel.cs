using Core.Domain.Authorization;

namespace Public.Models.ViewModels;

public class ReservationFailedViewModel
{
    public AuthorizationRejectionReason Reason { get; set; }
}
