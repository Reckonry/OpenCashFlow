using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;

public static class ControllerExtensions
{
    public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewPath, TModel model, bool partial = false)
    {
        controller.ViewData.Model = model;

        using var sw = new StringWriter();

        var viewEngine = controller.HttpContext.RequestServices.GetRequiredService<IRazorViewEngine>();
        var viewResult = viewEngine.GetView(executingFilePath: null, viewPath, isMainPage: !partial);

        if (!viewResult.Success)
        {
            throw new InvalidOperationException($"View '{viewPath}' non trovata.");
        }

        var viewContext = new ViewContext(
            controller.ControllerContext,
            viewResult.View,
            controller.ViewData,
            controller.TempData,
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }

}
