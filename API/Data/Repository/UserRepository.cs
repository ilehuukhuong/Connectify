using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public Task<BlockUserDto> BlockUserAsync(int UserId)
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(u => u.Id == UserId);
            return _mapper.ProjectTo<BlockUserDto>(query).FirstOrDefaultAsync();
        }

        public async Task<PagedList<MemberDtoWithoutIsVisible>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users
                .AsSplitQuery()
                .AsQueryable();

            query = query.Where(u => u.IsBlocked == false);
            query = query.Where(u => u.IsDeleted == false);

            if (userParams.Gender != null)
            {
                var genderId = _context.Genders.FirstOrDefaultAsync(g => g.Name.ToLower() == userParams.Gender.ToLower()).Result.Id;
                query = query.Where(u => u.GenderId == genderId);
            }
            else
            {
                query = query.Where(u => u.GenderId != -1);
            }

            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            if (userParams.Distance > 0)
            {
                query = query.Where(u =>
                    Math.Acos(Math.Sin(userParams.CurrentLatitude.Value * Math.PI / 180) * Math.Sin(u.Latitude * Math.PI / 180) +
                              Math.Cos(userParams.CurrentLatitude.Value * Math.PI / 180) * Math.Cos(u.Latitude * Math.PI / 180) *
                              Math.Cos((u.Longitude - userParams.CurrentLongitude.Value) * Math.PI / 180)) * 6371 <= userParams.Distance);
            }

            var likedUserIds = _context.Likes.Where(like => like.SourceUserId == userParams.CurrentUserId).Select(like => like.TargetUserId);
            var likedByUsersIds = _context.Likes.Where(like => like.TargetUserId == userParams.CurrentUserId).Select(like => like.SourceUserId);

            query = query.Where(u => u.IsVisible || likedByUsersIds.Contains(u.Id));

            query = query.Where(u => !likedUserIds.Contains(u.Id));

            query = query.Where(u => u.UserName != userParams.CurrentUsername);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            return await PagedList<MemberDtoWithoutIsVisible>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDtoWithoutIsVisible>(_mapper.ConfigurationProvider),
                userParams.PageNumber,
                userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .Include(g => g.Gender)
                .Include(l => l.UserLookingFors).ThenInclude(l => l.LookingFor)
                .Include(i => i.UserInterests).ThenInclude(i => i.Interest)
                .Include(c => c.City)
                .AsSplitQuery()
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public Task<UserLookingFor> GetUserLookingForEntityAsync(AppUser user, LookingFor lookingFor)
        {
            return _context.UserLookingFors.FirstOrDefaultAsync(x => x.User == user && x.LookingFor == lookingFor);
        }

        public Task<UserInterest> GetUserInterestForEntityAsync(AppUser user, Interest interest)
        {
            return _context.UserInterests.FirstOrDefaultAsync(x => x.User == user && x.Interest == interest);
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDtoWithoutIsVisible>> GetRecommendedMembersAsync(AppUser currentUser, UserParams userParams)
        {
            var likedUserIds = _context.Likes.Where(like => like.SourceUserId == currentUser.Id).Select(like => like.TargetUserId);
            var likedByUsersIds = _context.Likes.Where(like => like.TargetUserId == currentUser.Id).Select(like => like.SourceUserId);

            var allUsers = await _context.Users
                .Include(p => p.Photos)
                .Include(g => g.Gender)
                .Include(l => l.UserLookingFors).ThenInclude(l => l.LookingFor)
                .Include(i => i.UserInterests).ThenInclude(i => i.Interest)
                .Include(c => c.City)
                .Where(u => u.IsBlocked == false)
                .Where(u => u.IsDeleted == false)
                .Where(u => u.IsVisible || likedByUsersIds.Contains(u.Id))
                .Where(u => !likedUserIds.Contains(u.Id))
                .Where(u => u.UserName != currentUser.UserName)
                .AsSplitQuery()
                .ToListAsync();

            var recommendedUsers = allUsers
                .Where(user => CalculateSimilarity.CalculateUserSimilarity(currentUser, user) > userParams.Similarity / 10.0)
                .OrderByDescending(user => user.LastActive)
                .Select(_mapper.Map<MemberDtoWithoutIsVisible>);

            return PagedList<MemberDtoWithoutIsVisible>.CreateListAsync(recommendedUsers, userParams.PageNumber, userParams.PageSize);
        }
    }
}