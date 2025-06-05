namespace Public.ViewModels;

public class LayoutViewModel
{
    /*
     * Shared views like '_Layout.cshtml' cannot use an `IViewLocalizer`, because 
     * that option only works when the file matches the route. Instead, a generic 
     * localizer such as `IStringLocalizer<>` must be used. This file provides the 
     * required type parameter for that.
     * 
     * It doesn't matter that there is nothing in this class.
     */
}
