namespace TechStoreApi.Dtos.Common
{
    public class PaginationDto<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public T? Records { get; set; }
    }
}