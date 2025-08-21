using Microsoft.AspNetCore.Identity;
using Nexora.Core.Entities;

namespace Nexora.Infrastructure.Data.SeedData;

public static class UserSeeder
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
    {
        // Only seed essential admin users
        await SeedSuperAdminAsync(userManager);
        await SeedAdminAsync(userManager);
    }
    
    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var superAdminEmail = "superadmin@example.com";
        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdminUser == null)
        {
            superAdminUser = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FullName = "Super Administrator",
                Nickname = "SuperAdmin",
                Description = "Super administrator with full system access",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = true
            };

            var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
            }
        }
        else
        {
            // Update existing superadmin user to have IsSuperAdmin flag
            if (!superAdminUser.IsSuperAdmin)
            {
                superAdminUser.IsSuperAdmin = true;
                await userManager.UpdateAsync(superAdminUser);
            }
        }
    }
    
    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator",
                Nickname = "Admin",
                Description = "Administrator with Blog management access",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = false
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }
    
    private static async Task SeedSupportTeamAsync(UserManager<ApplicationUser> userManager)
    {
        // Seed Customer Support Admin
        var supportAdminEmail = "support.admin@example.com";
        if (await userManager.FindByEmailAsync(supportAdminEmail) == null)
        {
            var supportAdmin = new ApplicationUser
            {
                UserName = supportAdminEmail,
                Email = supportAdminEmail,
                FullName = "Sarah Johnson",
                Nickname = "Sarah",
                Description = "Customer Support Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = false
            };
            
            var result = await userManager.CreateAsync(supportAdmin, "Support@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(supportAdmin, "CustomerSupportAdmin");
            }
        }

        // Seed Customer Support Manager
        var supportManagerEmail = "support.manager@example.com";
        if (await userManager.FindByEmailAsync(supportManagerEmail) == null)
        {
            var supportManager = new ApplicationUser
            {
                UserName = supportManagerEmail,
                Email = supportManagerEmail,
                FullName = "Michael Davis",
                Nickname = "Mike",
                Description = "Customer Support Manager",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsSuperAdmin = false
            };
            
            var result = await userManager.CreateAsync(supportManager, "Manager@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(supportManager, "CustomerSupportManager");
            }
        }

        // Seed Customer Support Agents
        string[] agentNames = { "Emily Wilson", "James Brown", "Lisa Martinez", "Robert Taylor" };
        string[] agentNicknames = { "Emily", "James", "Lisa", "Rob" };
        
        for (int i = 0; i < agentNames.Length; i++)
        {
            var agentEmail = $"agent{i + 1}@example.com";
            if (await userManager.FindByEmailAsync(agentEmail) == null)
            {
                var agent = new ApplicationUser
                {
                    UserName = agentEmail,
                    Email = agentEmail,
                    FullName = agentNames[i],
                    Nickname = agentNicknames[i],
                    Description = "Customer Support Agent",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsSuperAdmin = false
                };
                
                var result = await userManager.CreateAsync(agent, "Agent@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(agent, "CustomerSupportAgent");
                }
            }
        }
    }
    
    private static async Task SeedTestUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Seed Test Customers (testuser1 to testuser5)
        for (int i = 1; i <= 5; i++)
        {
            var testEmail = $"testuser{i}@example.com";
            var testUser = await userManager.FindByEmailAsync(testEmail);

            if (testUser == null)
            {
                testUser = new ApplicationUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    FullName = $"Test User {i}",
                    Nickname = $"Tester{i}",
                    Description = $"Test user account {i} for testing purposes",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsSuperAdmin = false
                };

                var result = await userManager.CreateAsync(testUser, "TestUser@123");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, "CustomerSupportCustomer");
                }
            }
        }
    }
    
    private static async Task SeedTestTeamMembersAsync(UserManager<ApplicationUser> userManager)
    {
        // Additional team members for testing various scenarios
        var blogAuthorEmail = "blog.author@example.com";
        if (await userManager.FindByEmailAsync(blogAuthorEmail) == null)
        {
            var blogAuthor = new ApplicationUser
            {
                UserName = blogAuthorEmail,
                Email = blogAuthorEmail,
                FullName = "John Doe",
                Nickname = "JohnD",
                Description = "Blog Content Author",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                IsActive = true
            };
            
            var result = await userManager.CreateAsync(blogAuthor, "Author@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(blogAuthor, "Administrator");
            }
        }
        
        // Content Editor
        var editorEmail = "content.editor@example.com";
        if (await userManager.FindByEmailAsync(editorEmail) == null)
        {
            var editor = new ApplicationUser
            {
                UserName = editorEmail,
                Email = editorEmail,
                FullName = "Jane Smith",
                Nickname = "JaneS",
                Description = "Content Editor",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                IsActive = true
            };
            
            var result = await userManager.CreateAsync(editor, "Editor@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(editor, "Administrator");
            }
        }
    }
}