namespace techstore_api.Services.Interfaces
{
    public interface IDataInitializationService
    {
        Task<InitializationResult> InitializeDataAsync();
    }
}