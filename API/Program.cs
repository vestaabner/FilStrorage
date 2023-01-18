using API;
using API.CustomMiddlewares;
using API.Hubs;
using Applicaiton;
using Application;
using Application.CQRS.ProductCommandQuery.Command;
using Core.IRepositories;
using Infrastructure;
using Infrastructure.Models;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Reflection;

//var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = Path.GetFullPath(Directory.GetCurrentDirectory()),
    WebRootPath = Path.GetFullPath(Directory.GetCurrentDirectory()),
    Args = args
});

//add SignalR
builder.Services.AddSignalR();

//fill configs from appsetting.json
builder.Services.AddOptions();
builder.Services.Configure<Configs>(builder.Configuration.GetSection("Configs"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMediatR(typeof(SaveProductCommand));

//50 Repo
builder.Services.AddRepositories();
builder.Services.AddUnitOfWork();
builder.Services.AddInfraUtility();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();


string connectionString = builder.Configuration.GetConnectionString("SqlConnection"); 

//register DbContext
builder.Services.AddDbContext<OnlineShopDbContext>(options => {
    options.UseSqlServer(connectionString);
});

builder.Services.AddSwagger();
builder.Services.AddJWT();

//register Application Service
//builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddApplicationServices();

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAPI",
      builder =>
      {
          builder.WithOrigins("*");
          builder.WithHeaders("*");
          builder.WithMethods("*");
      });
});

//dotnet add package Microsoft.Extensions.Caching.Memory
builder.Services.AddMemoryCache();

//register AutoMapper
var config = new AutoMapper.MapperConfiguration(cfg =>
{
    cfg.AddProfile(new Application.AutoMapperConfig());
});
var mapper = config.CreateMapper();
builder.Services.AddSingleton(mapper);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.WebRootPath, "Media")),
    RequestPath = "/Media"
});

app.UseRouting();
app.UseCors("MyAPI");

//call CustomMiddleware
app.UseLoggingMiddleware();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();



app.MapHub<ChatHub>("/chatHub");

//call DB Seed Data

app.Run();
