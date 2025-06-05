namespace Public.SmokeTests.Utilities;
internal class MyUriBuilder(string uri) : UriBuilder(uri)
{
    private string? _subdomain;

    public MyUriBuilder WithSubdomain(string subdomain)
    {
        _subdomain = subdomain;
        return this;
    }

    public MyUriBuilder WithScheme(string scheme)
    {
        Scheme = scheme;
        return this;
    }

    public override string ToString()
    {
        Port = (Scheme == "https") ? 443 : 80;

        if (!string.IsNullOrEmpty(_subdomain))
        {
            Host = _subdomain + "." + Host;
        }

        return base.ToString();
    }
}
