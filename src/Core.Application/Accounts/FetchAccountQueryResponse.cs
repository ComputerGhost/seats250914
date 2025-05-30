namespace Core.Application.Accounts;
public class FetchAccountQueryResponse
{
    public bool IsEnabled { get; set; }

    public string Login { get; set; } = null!;
}
