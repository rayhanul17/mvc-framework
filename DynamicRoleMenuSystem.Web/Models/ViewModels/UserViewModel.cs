using System.ComponentModel.DataAnnotations;

namespace DynamicRoleMenuSystem.Web.Models.ViewModels;

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Avatar URL")]
    public string? AvatarUrl { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Email Confirmed")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "Phone Number Confirmed")]
    public bool PhoneNumberConfirmed { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Updated At")]
    public DateTime? UpdatedAt { get; set; }

    public List<string> AssignedRoles { get; set; } = new List<string>();
    public List<string> SelectedRoleIds { get; set; } = new List<string>();
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Avatar URL")]
    public string? AvatarUrl { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<string> SelectedRoleIds { get; set; } = new List<string>();
}

public class EditUserViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Avatar URL")]
    public string? AvatarUrl { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Email Confirmed")]
    public bool EmailConfirmed { get; set; }

    [Display(Name = "Phone Number Confirmed")]
    public bool PhoneNumberConfirmed { get; set; }

    public List<string> SelectedRoleIds { get; set; } = new List<string>();
}

public class UserDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> AssignedRoles { get; set; } = new List<string>();
}