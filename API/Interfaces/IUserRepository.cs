using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser> GetUserByIdAsync(int id);
        Task<AppUser> GetUserByUsernameAsync(string username);
        Task<PagedList<MemberDtoWithoutIsVisible>> GetMembersAsync(UserParams userParams);
        bool GetUserLookingForAsync(AppUser user, int lookingForId);
        bool GetUserInterestAsync(AppUser user, int interestId);
    }
}