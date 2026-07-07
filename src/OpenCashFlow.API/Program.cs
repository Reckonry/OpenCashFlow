using OpenCashFlow.API.Services;
using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application;
using OpenCashFlow.Infrastructure;
using OpenCashFlow.API.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using OpenCashFlow.Infrastructure.Persistence;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using OpenCashFlow.Api.AppStart;
using Asp.Versioning.Routing;
using Asp.Versioning;
var builder = WebApplication.CreateBuilder(args);

builder.AppStartConfigureConfiguration();
builder.AppStartConfigureLogging();
//builder.AppStartConfigureSentry();
builder.AppStartConfigureDatabase();
builder.AppStartConfigureRateLimiting();
builder.AppStartConfigureAuth();
builder.AppStartConfigureEmail();
builder.AppStartConfigureSlack();
builder.AppStartConfigureSwagger();
builder.AppStartConfigureAutoMapper();
builder.AppStartConfigureCors();

// Controllers + DI custom (repositories/services) restano dove sono o li spostiamo dopo
builder.Services.AddControllers();
builder.Services.AddOpenCashFlowApplication();
builder.Services.AddOpenCashFlowInfrastructure();
#region Repositories
#endregion

#region Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICashService, CashService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
#endregion

// Enable inline route constraint :apiVersion (e.g. v{version:apiVersion})
builder.Services.AddRouting(options =>
{
    options.ConstraintMap["apiVersion"] = typeof(ApiVersionRouteConstraint);
});
// API Versioning services (required by ApiVersionRouteConstraint)
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;

    // Se usi route tipo /api/v{version:apiVersion}/...
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

//builder.Services.AddMemoryCache();
//builder.Services.AddScoped<IFeatureService, OpenCashFlow.API.Services.FeatureService>();
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// porta il flag sentry dal builder all'app
//app.AppStartPropagateSentryFlag(builder);

app.AppStartUseSwaggerIfDev();

await app.AppStartApplyMigrationsAndSeeds();

app.AppStartConfigureExceptionHandling();
app.AppStartConfigureMiddlewarePipeline();

app.Run();

public partial class Program { }
