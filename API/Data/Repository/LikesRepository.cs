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
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                var sourceUser = await _context.Users.Where(x => x.Id == likesParams.UserId).Include(x => x.LikedUsers).Include(x => x.LikedByUsers).FirstOrDefaultAsync();
                var likes = _context.Likes.AsQueryable();
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                likes = likes.Where(like => like.TargetUser.IsBlocked == false);
                likes = likes.Where(like => like.TargetUser.IsDeleted == false);
                users = likes.Select(like => like.TargetUser);
                foreach (var user in sourceUser.LikedByUsers)
                {
                    users = users.Where(x => x.Id != user.SourceUserId);
                }
            }

            if (likesParams.Predicate == "likedBy")
            {
                var sourceUser = await _context.Users.Where(x => x.Id == likesParams.UserId).Include(x => x.LikedUsers).Include(x => x.LikedByUsers).FirstOrDefaultAsync();
                var likes = _context.Likes.AsQueryable();
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                likes = likes.Where(like => like.TargetUser.IsBlocked == false);
                likes = likes.Where(like => like.TargetUser.IsDeleted == false);
                users = likes.Select(like => like.SourceUser);
                foreach (var user in sourceUser.LikedUsers)
                {
                    users = users.Where(x => x.Id != user.TargetUserId);
                }
            }

            if (likesParams.Predicate == "connected")
            {
                if (!string.IsNullOrEmpty(likesParams.Search))
                {
                    // Users who liked the source user
                    var likedByUsers = _context.Likes
                        .Where(like => like.TargetUserId == likesParams.UserId)
                        .Where(like => like.SourceUser.IsBlocked == false)
                        .Where(like => like.SourceUser.IsDeleted == false)
                        .Where(like => like.SourceUser.KnownAs.ToLower().Contains(likesParams.Search.ToLower()) ||
                              (like.SourceUser.FirstName.ToLower() + " " + like.SourceUser.LastName.ToLower()).Contains(likesParams.Search.ToLower()))
                        .Select(like => like.SourceUser);

                    // Users liked by the source user
                    var likedUsers = _context.Likes
                        .Where(like => like.SourceUserId == likesParams.UserId)
                        .Where(like => like.TargetUser.IsBlocked == false)
                        .Where(like => like.TargetUser.IsDeleted == false)
                        .Where(like => like.TargetUser.KnownAs.ToLower().Contains(likesParams.Search.ToLower()) ||
                              (like.TargetUser.FirstName.ToLower() + " " + like.TargetUser.LastName.ToLower()).Contains(likesParams.Search.ToLower()))
                        .Select(like => like.TargetUser);

                    // Intersection of the two lists
                    users = likedUsers.Intersect(likedByUsers);
                }
                else
                {
                    // Users who liked the source user
                    var likedByUsers = _context.Likes
                        .Where(like => like.TargetUserId == likesParams.UserId)
                        .Where(like => like.SourceUser.IsBlocked == false)
                        .Where(like => like.SourceUser.IsDeleted == false)
                        .Select(like => like.SourceUser);

                    // Users liked by the source user
                    var likedUsers = _context.Likes
                        .Where(like => like.SourceUserId == likesParams.UserId)
                        .Where(like => like.TargetUser.IsBlocked == false)
                        .Where(like => like.TargetUser.IsDeleted == false)
                        .Select(like => like.TargetUser);

                    // Intersection of the two lists
                    users = likedUsers.Intersect(likedByUsers);
                }
            }

            var listUsers = users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalcuateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                Id = user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(listUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}