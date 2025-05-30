namespace Core.Application.Accounts;
public class ListAccountsQueryResponseItem
{
    public required bool IsEnabled { get; init; }

    public required string Login { get; init; } = null!;
}
