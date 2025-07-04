namespace CMS.FrontendIntegrationTests;

/// <summary>
/// Ignores a test unless running against localhost.
/// </summary>
public class LocalOnlyAttribute : ConditionBaseAttribute
{
    public LocalOnlyAttribute() : this(string.Empty)
    {
    }

    public LocalOnlyAttribute(string? message) : base(ConditionMode.Include)
    {
        IgnoreMessage = message;
    }
    
    public override string? IgnoreMessage { get; }

    public override string GroupName => "Local Tests";

    public override bool ShouldRun
    {
        get
        {
            var targetUri = new UriBuilder(ConfigurationAccessor.Instance.TargetUrl);
            return targetUri.Host == "localhost";
        }
    }
}
