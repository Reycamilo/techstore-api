using Microsoft.EntityFrameworkCore;
using techstore_api.DataBase;
using techstore_api.Services;
using techstore_api.Services.Interfaces;
using techstore_api.Helpers;
using techstore_api.Filters;
using Microsoft.AspNetCore.Mvc;
using techstore_api.Extensions;
using System.Reflection;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TiendaDbContext>(options =>
options.UseSqlServer(builder.Configuration
.GetConnectionString("DefaultConnection")));

// Acceder al contexto de la peticin HTTP
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IRolesService, RolesService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IDataInitializationService, DataInitializationService>();


builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfig(builder.Configuration);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API de la Tienda",
        Description = "API para la gestión de productos, categorías, órdenes, usuarios y roles."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Configuración del esquema de seguridad JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingrese su token JWT en el campo de texto de abajo.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // Requisito de seguridad en la versión v2 de OpenAPI
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", hostDocument: doc, externalResource: null),
            new List<string>()
        }
    });
});

var app = builder.Build();

// Inicializar base de datos y datos por defecto
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<TiendaDbContext>();

        // Crear la base de datos si no existe
        var databaseCreated = await context.Database.EnsureCreatedAsync();

        if (databaseCreated)
        {
            Console.WriteLine("🗄️ Base de datos creada por primera vez");

            // Inicializar datos por defecto solo si la base de datos se creó por primera vez
            var dataInitializationService = scope.ServiceProvider.GetRequiredService<IDataInitializationService>();
            var result = await dataInitializationService.InitializeDataAsync();

            if (result.Success)
            {
                Console.WriteLine("✅ Datos inicializados automáticamente durante la creación de la base de datos");
                Console.WriteLine($"📊 Resumen: {result.Message}");
                if (result.Details != null)
                {
                    Console.WriteLine($"   - Roles creados: {result.Details.RolesCreated}");
                    Console.WriteLine($"   - Usuarios creados: {result.Details.UsersCreated}");
                    Console.WriteLine($"   - Categorías de productos creadas: {result.Details.ProductCategoriesCreated}");
                }
                Console.WriteLine("👥 Usuarios disponibles:");
                Console.WriteLine("   - admin@admin.com / admin (ADMINISTRADOR)");
                Console.WriteLine("   - vendedor@vendedor.com / vendedor (VENDEDOR)");
                Console.WriteLine("   - cliente@cliente.com / cliente (CLIENTE)");
            }
            else
            {
                Console.WriteLine($"❌ Error al inicializar datos: {result.Message}");
            }
        }
        else
        {
            Console.WriteLine("ℹ️ Base de datos ya existe, saltando inicialización automática");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error durante la inicialización de la base de datos: {ex.Message}");
        // No lanzar la excepción para que la aplicación pueda continuar
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API de la Tienda v1");
        options.RoutePrefix = string.Empty; // Para que Swagger UI se cargue en la raíz de la URL
        options.ConfigObject.AdditionalItems["syntaxHighlight"] = false;
        options.ConfigObject.AdditionalItems["displayRequestDuration"] = true;
        // Configuración para expandir automáticamente las operaciones y modelos
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        options.DefaultModelsExpandDepth(-1); // Para no mostrar los modelos por defecto
    });

    // Redirecciones personalizadas para Swagger
    app.MapGet("/swagger/index.html", () => Results.Redirect("/index.html"));
    app.MapGet("/swagger", () => Results.Redirect("/index.html"));
}

app.UseHttpsRedirection();

// Configurar archivos estáticos para servir imágenes
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();