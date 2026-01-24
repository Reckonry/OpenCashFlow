using AutoMapper;
using OpenCashFlow.Shared.Mappings;
using OpenCashFlow.Test.Utilities;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data; // ✅ Usa il vero ApplicationDbContext

public abstract class BaseTest
{
    protected readonly IMapper _mapper;

    protected BaseTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = config.CreateMapper();
    }

    protected ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    protected ApplicationDbContext GetInMemoryDbContextWithSeededData()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        TestHelpers.SeedAllTestData(context);
        return context;
    }
}