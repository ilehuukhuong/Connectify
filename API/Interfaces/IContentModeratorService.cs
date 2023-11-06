namespace API.Interfaces
{
    public interface IContentModeratorService
    {
        Task<bool> IsInappropriateText(string text);
        Task<string> GetAllTermListsAsync();
        Task<string> CreateTermListAsync(string name, string description);
        Task AddTermToListAsync(string listId, string term, string language = "eng");
        Task DeleteTermFromListAsync(string listId, string term, string language = "eng");
        Task RefreshTermListAsync(string listId);
        Task DeleteTermListAsync(string listId);
        Task<string> GetAllTermsInTermListAsync(string listId, string language = "eng");
    }
}