using BestAuth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;


namespace BestAuth.Infrastructure
{
    public static class SeedData
    {
        private static readonly IReadOnlyCollection<string> DefaultRoles = ["Admin", "User"];

        public static async Task EnsureSeedDataAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            foreach (var roleName in DefaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new Role
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpperInvariant()
                    };

                    await roleManager.CreateAsync(role);
                }
            }

            await EnsureAdminUserAsync(userManager);
            await EnsureSampleRequestsAsync(services);
        }

        private static async Task EnsureAdminUserAsync(UserManager<User> userManager)
        {
            const string defaultAdminLogin = "admin";
            const string defaultAdminPassword = "admin";
            const string adminRole = "Admin";

            var adminUser = await userManager.FindByNameAsync(defaultAdminLogin);
            if (adminUser != null)
            {
                if (!await userManager.IsInRoleAsync(adminUser, adminRole))
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }

                return;
            }

            adminUser = User.Create(defaultAdminLogin, defaultAdminLogin, "Administrator");
            adminUser.PasswordHash = userManager.PasswordHasher.HashPassword(adminUser, defaultAdminPassword);

            var createResult = await userManager.CreateAsync(adminUser);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to create default admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            var roleResult = await userManager.AddToRoleAsync(adminUser, adminRole);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to assign admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        private static async Task EnsureSampleRequestsAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();

            if (context.Requests.Any())
            {
                return;
            }

            var sampleRequests = new[]
            {
                Request.Create("John Smith", "+1-555-0101", "Need technical support for account setup"),
                Request.Create("Mary Johnson", "+1-555-0102", "Request to upgrade subscription plan"),
                Request.Create("Robert Davis", "+1-555-0103", "Having trouble with two-factor authentication"),
                Request.Create("Patricia Wilson", "+1-555-0104", "Inquiry about API documentation"),
                Request.Create("Michael Brown", "+1-555-0105", null),
                Request.Create("Jennifer Garcia", "+1-555-0106", "Password reset request"),
                Request.Create("William Martinez", "+1-555-0107", "Question about billing and invoices"),
                Request.Create("Linda Anderson", "+1-555-0108", "Report a security concern"),
                Request.Create("David Thomas", "+1-555-0109", "Request for bulk export of data"),
                Request.Create("Barbara Taylor", "+1-555-0110", "Feedback about the platform features"),
            };

            // Set different statuses and timestamps for realistic variety
            sampleRequests[0].Status = RequestStatus.InProgress;
            sampleRequests[0].CreatedAtUtc = DateTime.UtcNow.AddDays(-5);

            sampleRequests[1].Status = RequestStatus.Completed;
            sampleRequests[1].CreatedAtUtc = DateTime.UtcNow.AddDays(-10);

            sampleRequests[2].Status = RequestStatus.InProgress;
            sampleRequests[2].CreatedAtUtc = DateTime.UtcNow.AddDays(-2);

            sampleRequests[3].Status = RequestStatus.Completed;
            sampleRequests[3].CreatedAtUtc = DateTime.UtcNow.AddDays(-15);

            sampleRequests[4].Status = RequestStatus.New;
            sampleRequests[4].CreatedAtUtc = DateTime.UtcNow.AddHours(-1);

            sampleRequests[5].Status = RequestStatus.Completed;
            sampleRequests[5].CreatedAtUtc = DateTime.UtcNow.AddDays(-7);

            sampleRequests[6].Status = RequestStatus.New;
            sampleRequests[6].CreatedAtUtc = DateTime.UtcNow.AddHours(-3);

            sampleRequests[7].Status = RequestStatus.Cancelled;
            sampleRequests[7].CreatedAtUtc = DateTime.UtcNow.AddDays(-20);

            sampleRequests[8].Status = RequestStatus.InProgress;
            sampleRequests[8].CreatedAtUtc = DateTime.UtcNow.AddDays(-3);

            sampleRequests[9].Status = RequestStatus.New;
            sampleRequests[9].CreatedAtUtc = DateTime.UtcNow;

            await context.Requests.AddRangeAsync(sampleRequests);
            await context.SaveChangesAsync();
        }
    }
}

