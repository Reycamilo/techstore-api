namespace techstore_api.Dtos.Categories
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Product" o "Service"
    }
}