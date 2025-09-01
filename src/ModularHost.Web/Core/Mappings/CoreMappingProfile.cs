using AutoMapper;
using MRCMS.Core.Models.Entities;
using MRCMS.Models.ViewModels;
using MRCMS.ViewModels;

namespace MRCMS.Core.Mappings
{
    public class CoreMappingProfile : Profile
    {
        public CoreMappingProfile()
        {
            // User mappings
            CreateMap<User, UserViewModel>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Will be populated by controller
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<User, UserDetailsViewModel>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Will be populated by controller
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<CreateUserViewModel, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Will be handled by UserManager
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.VersionNumber, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpiry, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore());

            CreateMap<User, EditUserViewModel>()
                .ForMember(dest => dest.SelectedRoles, opt => opt.Ignore()) // Will be populated by controller
                .ForMember(dest => dest.AllRoles, opt => opt.Ignore()); // Will be populated by controller

            CreateMap<EditUserViewModel, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Will be handled by UserManager
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.VersionNumber, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpiry, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore());

            // User summary mapping for admin dashboard
            CreateMap<User, UserSummary>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            // Role mappings
            CreateMap<Role, RoleViewModel>()
                .ForMember(dest => dest.UserCount, opt => opt.Ignore()); // Will be populated by controller

            CreateMap<CreateRoleViewModel, Role>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedName, opt => opt.Ignore()) // Will be handled by RoleManager
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.VersionNumber, opt => opt.Ignore());

            CreateMap<Role, EditRoleViewModel>();

            CreateMap<EditRoleViewModel, Role>()
                .ForMember(dest => dest.NormalizedName, opt => opt.Ignore()) // Will be handled by RoleManager
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.VersionNumber, opt => opt.Ignore());

            // Menu mappings
            CreateMap<Menu, MenuViewModel>();

            CreateMap<CreateMenuViewModel, Menu>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.VersionNumber, opt => opt.Ignore());

            CreateMap<Menu, EditMenuViewModel>();

            CreateMap<EditMenuViewModel, Menu>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.VersionNumber, opt => opt.Ignore());

            // Audit Log mappings
            CreateMap<AuditLog, AuditLogViewModel>();

            // Setting mappings
            CreateMap<Setting, SettingViewModel>();

            CreateMap<CreateSettingViewModel, Setting>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.VersionNumber, opt => opt.Ignore());

            CreateMap<Setting, EditSettingViewModel>();

            CreateMap<EditSettingViewModel, Setting>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.VersionNumber, opt => opt.Ignore());
        }
    }

    // Define missing ViewModels that are referenced but may not exist
    public class RoleViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsActive { get; set; }
        public int UserCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRoleViewModel
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class EditRoleViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public class MenuViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Icon { get; set; } = "";
        public int Order { get; set; }
        public string? ClaimType { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
        public Guid? ParentId { get; set; }
        public string? ModuleName { get; set; }
    }

    public class CreateMenuViewModel
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Icon { get; set; } = "";
        public int Order { get; set; }
        public string? ClaimType { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public Guid? ParentId { get; set; }
        public string? ModuleName { get; set; }
    }

    public class EditMenuViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Icon { get; set; } = "";
        public int Order { get; set; }
        public string? ClaimType { get; set; }
        public bool IsActive { get; set; }
        public bool IsVisible { get; set; }
        public Guid? ParentId { get; set; }
        public string? ModuleName { get; set; }
    }

    public class AuditLogViewModel
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; } = "";
        public string EntityId { get; set; } = "";
        public string Action { get; set; } = "";
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string UserAgent { get; set; } = "";
    }

    public class SettingViewModel
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsPublic { get; set; }
    }

    public class CreateSettingViewModel
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsPublic { get; set; }
    }

    public class EditSettingViewModel
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsPublic { get; set; }
    }
}