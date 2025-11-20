using LMS.Data;
using LMS.Helpers;
using LMS.Repositories;
using LMS.Repositories.Impl.Communication;
using LMS.Repositories.Impl.Info;
using LMS.Repositories.Interfaces.Communication;
using LMS.Repositories.Interfaces.Info;
using LMS.Services.Impl;

using LMS.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VNPAY;
using VNPAY.Extensions;
using VNPAY.Models;
using VNPAY.Models.Enums;
using VNPAY.Models.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// DI
var connectionString = builder.Configuration.GetConnectionString("SqlServer");
builder.Services.AddDbContext<LMS.Data.CenterDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

//  Generic
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
builder.Services.AddScoped(typeof(ICrudService<,>), typeof(CrudService<,>));

// Register Repositories
builder.Services.AddRepositories();

// Register Services
builder.Services.AddApplicationServices();

// Register Helpers
builder.Services.AddScoped<EmailHelper>();

// Add MemoryCache for token storage
builder.Services.AddMemoryCache();

builder.Services.AddVnPayConfig(builder.Configuration);
builder.Services.AddStudentServices();

// AuthZN
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddAuthorizationPolicies();

// Session
builder.Services.AddSessionServices();

var app = builder.Build();

// Seed data on startup (only in development)
if (app.Environment.IsDevelopment())
{
    var services = app.Services.CreateScope().ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CenterDbContext>();
        await DataSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();
