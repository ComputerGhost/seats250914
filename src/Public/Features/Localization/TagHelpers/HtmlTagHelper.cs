using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Public.Features.Localization.Extensions;

namespace Public.Features.Localization.TagHelpers;

public class HtmlTagHelper : TagHelper
{
    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var requestCulture = ViewContext.HttpContext.GetRequestCulture();

        output.TagName = "html";
        output.Attributes.Add("lang", requestCulture.Name);
    }
}
