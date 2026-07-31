using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

                // Inicializar productos de ejemplo
                var productsResult = await InitializeProductsAsync();
                if (!productsResult.Success)
                {
                    return productsResult;
                }
                Console.WriteLine($"📦 {productsResult.Message}");

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

        private async Task<InitializationResult> InitializeProductsAsync()
        {
            var vendedor = await _userManager.FindByEmailAsync("vendedor@vendedor.com");
            if (vendedor == null)
            {
                return new InitializationResult
                {
                    Success = false,
                    Message = "No se encontró el vendedor para asignar los productos de ejemplo."
                };
            }

            var categorias = await _context.Categories
                .Where(c => c.Type == "Product")
                .ToListAsync();

            int CategoriaId(string nombre) =>
                categorias.FirstOrDefault(c => c.Name == nombre)?.Id ?? categorias.First().Id;

            var productos = new[]
            {
                new ProductEntity { Name = "Monitor HP 24\"", Description = "Monitor Full HD de 24 pulgadas con teclado y mouse incluidos.", Price = 189.99m, Stock = 12, CategoryId = CategoriaId("Computadoras"), SellerId = vendedor.Id, ImageUrl = "/images/products/1-20250729153307.png" },
                new ProductEntity { Name = "PC de Escritorio HP", Description = "Computadora de escritorio ideal para oficina y hogar.", Price = 649.00m, Stock = 6, CategoryId = CategoriaId("Computadoras"), SellerId = vendedor.Id, ImageUrl = "/images/products/20250729153945-676d9508-c29c-4c67-9efe-fdfbca8e8473.png" },
                new ProductEntity { Name = "iPhone X", Description = "Smartphone Apple iPhone X de 64GB, pantalla Super Retina.", Price = 499.99m, Stock = 8, CategoryId = CategoriaId("Smartphones"), SellerId = vendedor.Id, ImageUrl = "/images/products/20250729155301-a9dcc7d6-265c-4b08-981f-a2846e503032.png" },
                new ProductEntity { Name = "Teclado Mecánico RGB", Description = "Teclado mecánico retroiluminado con switches rojos.", Price = 39.90m, Stock = 25, CategoryId = CategoriaId("Accesorios"), SellerId = vendedor.Id },
                new ProductEntity { Name = "Mouse Inalámbrico", Description = "Mouse inalámbrico ergonómico de 2.4GHz.", Price = 15.50m, Stock = 30, CategoryId = CategoriaId("Accesorios"), SellerId = vendedor.Id },
                new ProductEntity { Name = "Memoria RAM 16GB DDR4", Description = "Módulo de memoria RAM 16GB DDR4 a 3200MHz.", Price = 54.99m, Stock = 20, CategoryId = CategoriaId("Hardware"), SellerId = vendedor.Id },
            };

            var creados = 0;
            foreach (var producto in productos)
            {
                producto.CreadoPor = "Sistema";
                producto.ActualizadoPor = "Sistema";
                producto.FechaCreacion = DateTime.UtcNow;
                producto.FechaActualizacion = DateTime.UtcNow;

                _context.Products.Add(producto);
                creados++;
            }

            await _context.SaveChangesAsync();

            return new InitializationResult
            {
                Success = true,
                Message = $"Productos de ejemplo inicializados: {creados} creados"
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