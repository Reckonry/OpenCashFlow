using OpenCashFlow.Shared.Mappings;
using System.Reflection;

namespace OpenCashFlow.Api.AppStart;

public static class AutoMapperAppStart
{
    public static WebApplicationBuilder AppStartConfigureAutoMapper(this WebApplicationBuilder builder)
    {
#if DEBUG
        try
        {
            builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(MappingProfile).Assembly);
        }
        catch (ReflectionTypeLoadException ex)
        {
            var details = string.Join("\n---\n",
                ex.LoaderExceptions.Select(le => le?.Message + (le is FileNotFoundException f ? $" | Missing: {f.FileName}" : "")));
            Console.WriteLine(details);
            throw;
        }
#endif
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        return builder;
    }
}