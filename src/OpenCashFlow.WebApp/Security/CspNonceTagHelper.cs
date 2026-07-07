using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OpenCashFlow.WebApp.Security;

[HtmlTargetElement("script")]
[HtmlTargetElement("style")]
public sealed class CspNonceTagHelper : TagHelper
{
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (output.Attributes.ContainsName("nonce"))
        {
            return;
        }

        if (ViewContext.HttpContext.Items[CspNonce.HttpContextItemKey] is string nonce)
        {
            output.Attributes.SetAttribute("nonce", nonce);
        }
    }
}
