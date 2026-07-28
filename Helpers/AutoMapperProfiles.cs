
using AutoMapper;
using techstore_api.DataBase.Entities;
using techstore_api.Dtos.Categories;
using techstore_api.Dtos.Orders;
using techstore_api.Dtos.Products;
using techstore_api.Dtos.Security.Roles;
using techstore_api.Dtos.Security.Users;

namespace techstore_api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Roles
            CreateMap<RoleCreateDto, RoleEntity>();
            CreateMap<RoleEditDto, RoleEntity>();
            CreateMap<RoleEntity, RoleDto>();

            // Usuarios
            CreateMap<UserCreateDto, UserEntity>();
            CreateMap<UserEditDto, UserEntity>();
            CreateMap<UserEntity, UserDto>();

            // Categorías
            CreateMap<CategoryCreateDto, CategoryEntity>();
            CreateMap<CategoryEditDto, CategoryEntity>();
            CreateMap<CategoryEntity, CategoryDto>();

            // Productos
            CreateMap<ProductCreateDto, ProductEntity>();
            CreateMap<ProductEditDto, ProductEntity>();
            CreateMap<ProductEntity, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : string.Empty));

            // Órdenes
            CreateMap<OrderCreateDto, OrderEntity>();
            CreateMap<OrderEditDto, OrderEntity>();
            CreateMap<OrderEntity, OrderDto>();
            CreateMap<OrderDetailCreateDto, OrderDetailEntity>();
            CreateMap<OrderDetailEditDto, OrderDetailEntity>();
            CreateMap<OrderDetailEntity, OrderDetailDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
        }
    }
}