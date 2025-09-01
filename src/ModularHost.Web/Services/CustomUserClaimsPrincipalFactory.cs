using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MRCMS.Core.Models.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MRCMS.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Add IsSuperAdmin claim based on the user's IsSuperAdmin field
            if (user.IsSuperAdmin)
            {
                identity.AddClaim(new Claim("IsSuperAdmin", "true"));
            }

            // Add other custom claims
            identity.AddClaim(new Claim("FullName", user.FullName));
            identity.AddClaim(new Claim("UserName", user.UserName ?? ""));
            identity.AddClaim(new Claim("AvatarUrl", user.AvatarUrl));
            identity.AddClaim(new Claim("IsActive", user.IsActive.ToString()));
            identity.AddClaim(new Claim("UserId", user.Id.ToString()));

            return identity;
        }
    }
}