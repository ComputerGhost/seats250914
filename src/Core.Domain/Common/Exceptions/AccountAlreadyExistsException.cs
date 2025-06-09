namespace Core.Domain.Common.Exceptions;
public class AccountAlreadyExistsException(string login) : Exception
{
    public string Login { get; set; } = login;
}
