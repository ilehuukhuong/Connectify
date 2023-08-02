using API.DTOs;
using API.Entities;
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

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();

            query = query.Where(u => u.IsBlocked == false);

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

            return await PagedList<MemberDto>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider),
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
                .SingleOrDefaultAsync(x => x.UserName == username);
        }
    }
}