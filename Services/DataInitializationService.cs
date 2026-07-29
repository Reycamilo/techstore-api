using Microsoft.AspNetCore.Identity;
using techstore_api.DataBase;
using techstore_api.DataBase.Entities;
using techstore_api.Services.Interfaces;
using TechStoreApi.Constants;

namespace techstore_api.Services
{
    public class DataInitializationService : IDataInitializationService
    {
        private readonly TiendaDbContext _context;
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<RoleEntity> _roleManager;

        public DataInitializationService(
            TiendaDbContext context,
            UserManager<UserEntity> userManager,
            RoleManager<RoleEntity> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<InitializationResult> InitializeDataAsync()
        {
            try
            {
                Console.WriteLine("🚀 Iniciando inicialización de datos por defecto...");

                // Inicializar roles
                var rolesResult = await InitializeRolesAsync();
                if (!rolesResult.Success)
                {
                    return rolesResult;
                }

                // Inicializar usuarios
                var usersResult = await InitializeUsersAsync();
                if (!usersResult.Success)
                {
                    return usersResult;
                }

                // Inicializar categorías de productos
                var productCategoriesResult = await InitializeProductCategoriesAsync();
                if (!productCategoriesResult.Success)
                {
                    return productCategoriesResult;
                }

                Console.WriteLine("✅ Datos inicializados correctamente");

                return new InitializationResult
                {
                    Success = true,
                    Message = "Datos inicializados correctamente durante la creación de la base de datos",
                    Details = new InitializationDetails
                    {
                        RolesCreated = rolesResult.Details?.RolesCreated ?? 0,
                        UsersCreated = usersResult.Details?.UsersCreated ?? 0,
                        ProductCategoriesCreated = productCategoriesResult.Details?.ProductCategoriesCreated ?? 0
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al inicializar datos: {ex.Message}");
                return new InitializationResult
                {
                    Success = false,
                    Message = $"Error al inicializar datos: {ex.Message}"
                };
            }
        }

        private async Task<InitializationResult> InitializeRolesAsync()
        {
            var roles = new[]
            {
                NombresDeRoles.ADMINISTRADOR,
                NombresDeRoles.VENDEDOR,
                NombresDeRoles.CLIENTE
            };

            var createdRoles = 0;

            foreach (var roleName in roles)
            {
                var role = new RoleEntity
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    createdRoles++;
                    Console.WriteLine($"✅ Rol '{roleName}' creado exitosamente");
                }
                else
                {
                    var errorMessage = $"Error al crear rol '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    Console.WriteLine($"❌ {errorMessage}");
                    return new InitializationResult
                    {
                        Success = false,
                        Message = errorMessage
                    };
                }
            }

            return new InitializationResult
            {
                Success = true,
                Message = $"Roles inicializados: {createdRoles} creados",
                Details = new InitializationDetails
                {
                    RolesCreated = createdRoles
                }
            };
        }

        private async Task<InitializationResult> InitializeUsersAsync()
        {
            var users = new[]
            {
                new { Email = "admin@admin.com", Password = "admin", FirstName = "Administrador", LastName = "Sistema", Role = NombresDeRoles.ADMINISTRADOR },
                new { Email = "vendedor@vendedor.com", Password = "vendedor", FirstName = "Vendedor", LastName = "Sistema", Role = NombresDeRoles.VENDEDOR },
                new { Email = "cliente@cliente.com", Password = "cliente", FirstName = "Cliente", LastName = "Sistema", Role = NombresDeRoles.CLIENTE }
            };

            var createdUsers = 0;

            foreach (var userInfo in users)
            {
                var user = new UserEntity
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    EmailConfirmed = true,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    PhoneNumberConfirmed = true,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0
                };

                var result = await _userManager.CreateAsync(user, userInfo.Password);
                if (result.Succeeded)
                {
                    // Asignar rol correspondiente
                    await _userManager.AddToRoleAsync(user, userInfo.Role);
                    createdUsers++;
                    Console.WriteLine($"✅ Usuario '{userInfo.Email}' creado exitosamente con rol '{userInfo.Role}'");
                }
                else
                {
                    var errorMessage = $"Error al crear usuario '{userInfo.Email}': {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    Console.WriteLine($"❌ {errorMessage}");
                    return new InitializationResult
                    {
                        Success = false,
                        Message = errorMessage
                    };
                }
            }

            return new InitializationResult
            {
                Success = true,
                Message = $"Usuarios inicializados: {createdUsers} creados",
                Details = new InitializationDetails
                {
                    UsersCreated = createdUsers
                }
            };
        }

        private async Task<InitializationResult> InitializeProductCategoriesAsync()
        {
            var productCategories = new[]
            {
                new CategoryEntity { Name = "Electrónicos", Type = "Product" },
                new CategoryEntity { Name = "Computadoras", Type = "Product" },
                new CategoryEntity { Name = "Smartphones", Type = "Product" },
                new CategoryEntity { Name = "Accesorios", Type = "Product" },
                new CategoryEntity { Name = "Hardware", Type = "Product" }
            };

            var createdCategories = 0;

            foreach (var category in productCategories)
            {
                category.CreadoPor = "Sistema";
                category.ActualizadoPor = "Sistema";
                category.FechaCreacion = DateTime.UtcNow;
                category.FechaActualizacion = DateTime.UtcNow;

                _context.Categories.Add(category);
                createdCategories++;
                Console.WriteLine($"✅ Categoría de producto '{category.Name}' creada exitosamente");
            }

            await _context.SaveChangesAsync();

            return new InitializationResult
            {
                Success = true,
                Message = $"Categorías de productos inicializadas: {createdCategories} creadas",
                Details = new InitializationDetails
                {
                    ProductCategoriesCreated = createdCategories
                }
            };
        }
    }

    // Clases para manejar los resultados
    public class InitializationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public InitializationDetails? Details { get; set; }
    }

    public class InitializationDetails
    {
        public int RolesCreated { get; set; }
        public int UsersCreated { get; set; }
        public int ProductCategoriesCreated { get; set; }
    }
}