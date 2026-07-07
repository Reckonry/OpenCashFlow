using OpenCashFlow.API.Mapping;
using System.Reflection;

namespace OpenCashFlow.Api.AppStart;

public static class AutoMapperAppStart
{
    public static WebApplicationBuilder AppStartConfigureAutoMapper(this WebApplicationBuilder builder)
    {
        var mapperAssemblies = new[]
        {
            typeof(Program).Assembly,
            typeof(MappingProfile).Assembly
        };

        try
        {
            builder.Services.AddAutoMapper(_ => { }, mapperAssemblies);
        }
        catch (ReflectionTypeLoadException ex)
        {
            var details = string.Join("\n---\n",
                ex.LoaderExceptions.Select(le => le?.Message + (le is FileNotFoundException f ? $" | Missing: {f.FileName}" : "")));
            Console.WriteLine(details);
            throw;
        }
        return builder;
    }
}
