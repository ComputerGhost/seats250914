namespace Public.FrontendIntegrationTests;

/// <summary>
/// Ignores a test unless running against localhost.
/// </summary>
public class LocalOnlyAttribute(string? message = "") : ConditionBaseAttribute(ConditionMode.Include)
{
    public override string? IgnoreMessage { get; } = message;

    public override string GroupName => (Mode == ConditionMode.Include) ? "Local Tests" : "Non-local Tests";

    public ConditionMode Mode { get; set; }

    public override bool ShouldRun
    {
        get
        {
            var targetUri = new UriBuilder(ConfigurationAccessor.Instance.TargetUrl);
            return (Mode != ConditionMode.Include) ^ (targetUri.Host == "localhost");
        }
    }
}
