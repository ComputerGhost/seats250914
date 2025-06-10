namespace Public.SmokeTests.Utilities;

/// <summary>
/// Exactly the same as `IgnoreAttribute`, but it's not external, so its behavior can be toggled with a simple edit.
/// </summary>
public class SmokeTestAttribute : ConditionBaseAttribute
{
    /// <summary>
    /// Whether tests marked with this attribute should run.
    /// </summary>
    public const bool SHOULD_RUN_TESTS = false;

    public SmokeTestAttribute() : this(string.Empty)
    {
    }

    public SmokeTestAttribute(string? message) : base(ConditionMode.Include)
    {
        IgnoreMessage = message;
    }

    public override string? IgnoreMessage { get; }

    public override string GroupName => "Smoke Tests";

    public override bool ShouldRun => SHOULD_RUN_TESTS;
}
