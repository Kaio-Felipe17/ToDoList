using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ToDoList.API.Endpoints;
using ToDoList.API.Errors;
using ToDoList.Application.Items.Create;
using ToDoList.Application.Mappings;
using ToDoList.Application.Repositories;
using ToDoList.Infrastructure.Context;
using ToDoList.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateItemHandler).Assembly));

builder.Services.AddAutoMapper(cfg =>
    cfg.AddMaps(typeof(ItemMappingProfile).Assembly));

builder.Services.AddScoped<IItemRepository, ItemRepository>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapItemEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
        options.WithTitle("ToDoList API").WithTheme(ScalarTheme.Kepler));
}

app.Run();