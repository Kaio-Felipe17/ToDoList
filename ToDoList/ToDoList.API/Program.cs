using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ToDoList.API.Endpoints;
using ToDoList.API.Errors;
using ToDoList.Application.Behaviors;
using ToDoList.Application.Items.Create;
using ToDoList.Application.Mappings;
using ToDoList.Application.Repositories;
using ToDoList.Application.Validators.Item;
using ToDoList.Infrastructure.Context;
using ToDoList.Infrastructure.Repositories;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<ApiDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

        builder.Services.AddValidatorsFromAssemblyContaining<UpdateItemValidator>();

        #region MediatR
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateItemHandler).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        #endregion

        #region Automapper
        builder.Services.AddAutoMapper(cfg =>
            cfg.AddMaps(typeof(ItemMappingProfile).Assembly));
        #endregion

        #region Repositories
        builder.Services.AddScoped<IItemRepository, ItemRepository>();
        #endregion

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendLocal", policy =>
            {
                policy
                    .WithOrigins(
                        "http://127.0.0.1:5500",
                        "http://localhost:5500"
                    )
                    .WithMethods("GET", "POST")
                    .AllowAnyHeader();
            });
        });

        var app = builder.Build();
        app.UseCors("FrontendLocal");
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
    }
}

#region MediatR

#endregion
#region Automapper

#endregion
#region Repositories

#endregion
