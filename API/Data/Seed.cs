using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task ClearConnections(DataContext context)
        {
            context.Connections.RemoveRange(context.Connections);
            await context.SaveChangesAsync();
        }

        public static async Task SeedGender(DataContext context)
        {
            if (await context.Genders.AnyAsync()) return;

            var genderData = await File.ReadAllTextAsync("Data/DatabaseDataSeed/GenderSeedData.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var genders = JsonSerializer.Deserialize<List<Gender>>(genderData);

            foreach (var gender in genders)
            {
                context.Genders.Add(gender);
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedInterest(DataContext context)
        {
            if (await context.Interests.AnyAsync()) return;

            var interestData = await File.ReadAllTextAsync("Data/DatabaseDataSeed/InterestSeedData.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var interests = JsonSerializer.Deserialize<List<Interest>>(interestData);

            foreach (var interest in interests)
            {
                context.Interests.Add(interest);
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedLookingFor(DataContext context)
        {
            if (await context.LookingFors.AnyAsync()) return;

            var lookingForData = await File.ReadAllTextAsync("Data/DatabaseDataSeed/LookingForSeedData.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var lookingFors = JsonSerializer.Deserialize<List<LookingFor>>(lookingForData);

            foreach (var lookingFor in lookingFors)
            {
                context.LookingFors.Add(lookingFor);
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedUsers(UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var userData = await File.ReadAllTextAsync("Data/DatabaseDataSeed/UserSeedData.json");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            var roles = new List<AppRole>
            {
                new AppRole{Name = "Member"},
                new AppRole{Name = "Admin"},
                new AppRole{Name = "Moderator"},
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            foreach (var user in users)
            {
                user.UserName = user.UserName.ToLower();
                user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
                user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser
            {
                UserName = "admin",
                GenderId = 1,
                IsVisible = false,
                DateOfBirth = DateOnly.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd"))
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
        }
    }
}