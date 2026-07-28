using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using techstore_api.DataBase;
using techstore_api.Services;
using techstore_api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDbContext<TiendaDbContext>(options => 
    options.UseSqlServer(builder.Configuration
    .GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();


// Registrar servicios personalizados aquí
builder.Services.AddTransient<IUsersService, UsersService>();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.Run();
