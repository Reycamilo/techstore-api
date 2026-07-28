using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using techstore_api.DataBase;
using techstore_api.Services;
using techstore_api.Services.Interfaces;
using techstore_api.Helpers;
using techstore_api.Filters;
using Microsoft.AspNetCore.Mvc;
using techstore_api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TiendaDbContext>(options =>
options.UseSqlServer(builder.Configuration
.GetConnectionString("DefaultConnection")));

// Acceder al contexto de la peticin HTTP
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfiles>());

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ValidarEstadoDeModeloAtributo));
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});



// Registrar servicios personalizados aquí
builder.Services.AddTransient<IUsersService, UsersService>();
builder.Services.AddTransient<IRolesService, RolesService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();


builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfig(builder.Configuration);


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Configurar archivos estáticos para servir imágenes
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();