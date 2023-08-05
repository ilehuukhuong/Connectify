namespace API.Interfaces
{
    public interface IContentModeratorService
    {
        Task<bool> IsInappropriateText(string text);
    }
}