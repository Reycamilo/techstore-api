using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using techstore_api.DataBase;

var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDbContext<TiendaDbContext>(options => 
    options.UseSqlServer(builder.Configuration
    .GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.Run();
