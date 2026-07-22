namespace TechStoreApi.Dtos.Common
{
    public class ResponseDto<T>
    {
        public int StatusCode { get; set; }
        public bool Status { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
    }
}