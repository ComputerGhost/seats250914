namespace Public.Views.Shared;

/*
 * Shared views like '_Layout.cshtml' cannot use an `IViewLocalizer`, because 
 * that option only works when the file matches the route. Instead, a generic 
 * localizer such as `IStringLocalizer<>` must be used. This file provides the 
 * required type parameter for that.
 */

#pragma warning disable IDE1006 // Naming Styles
public class _Layout
#pragma warning restore IDE1006 // Naming Styles
{
}
