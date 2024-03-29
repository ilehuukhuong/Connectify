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
        Task<MemberDto> GetMemberAsync(string username);
        Task<BlockUserDto> BlockUserAsync(int UserId);
        Task<UserLookingFor> GetUserLookingForEntityAsync(AppUser user, LookingFor lookingFor);
        Task<UserInterest> GetUserInterestForEntityAsync(AppUser user, Interest interest);
        Task<PagedList<MemberDtoWithoutIsVisible>> GetRecommendedMembersAsync(AppUser currentUser, UserParams userParams);
    }
}