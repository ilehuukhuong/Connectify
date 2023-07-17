namespace API.Interfaces
{
    public interface INSFWChecker
    {
        Task<bool> IsNSFWPhoto(IFormFile file);
    }
}
