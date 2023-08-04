using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var sourceUser = await _context.Users.Where(x => x.Id == likesParams.UserId).Include(x => x.LikedUsers).Include(x => x.LikedByUsers).FirstOrDefaultAsync();
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.TargetUser);
                foreach (var user in sourceUser.LikedByUsers)
                {
                    users = users.Where(x => x.Id != user.SourceUserId);
                }
            }

            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
                foreach (var user in sourceUser.LikedUsers)
                {
                    users = users.Where(x => x.Id != user.TargetUserId);
                }
            }

            if (likesParams.Predicate == "connected")
            {
                // Users who liked the source user
                var likedByUsers = _context.Likes
                    .Where(like => like.TargetUserId == likesParams.UserId)
                    .Select(like => like.SourceUser);

                // Users liked by the source user
                var likedUsersTemp = _context.Likes
                    .Where(like => like.SourceUserId == likesParams.UserId)
                    .Select(like => like.TargetUser);

                // Intersection of the two lists
                users = likedUsersTemp.Intersect(likedByUsers);
            }

            var likedUsers = users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalcuateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                Id = user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}