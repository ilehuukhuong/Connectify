using API.Entities;

namespace API.Extensions
{
    public static class CalculateSimilarity
    {
        public static double CalculateUserSimilarity(AppUser user1, AppUser user2)
        {
            double similarity = 0;

            // Tính toán điểm tương đồng dựa trên các thuộc tính
            if (user1.GenderId == user2.GenderId)
                similarity += 0.2;

            if (user1.CityId == user2.CityId)
                similarity += 0.2;

            var commonInterests = user1.UserInterests.Select(ui => ui.InterestId).Intersect(user2.UserInterests.Select(ui => ui.InterestId));
            double checkCount = 0.3 * (double)commonInterests.Count() / Math.Max(user1.UserInterests.Count, user2.UserInterests.Count);
            if (checkCount > 0.0 )
            {
                similarity += checkCount;
            }

            var commonLookingFors = user1.UserLookingFors.Select(ul => ul.LookingForId).Intersect(user2.UserLookingFors.Select(ul => ul.LookingForId));

            checkCount = 0.3 * (double)commonLookingFors.Count() / Math.Max(user1.UserLookingFors.Count, user2.UserLookingFors.Count);

            if (checkCount > 0.0)
            {
                similarity += checkCount;
            }

            return similarity;
        }
    }
}