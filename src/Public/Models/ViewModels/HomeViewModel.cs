namespace Public.Models.ViewModels;

public class HomeViewModel
{
    /// <summary>
    /// Disable the Kakao maps embed.
    /// </summary>
    /// <remarks>
    /// Kakao maps is poorly designed, and it's dirtying my tests. 
    /// This disables it so that I can properly test *my* code.
    /// </remarks>
    public bool ShouldRenderMap { get; set; }
}
