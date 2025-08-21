using System.ComponentModel.DataAnnotations;

namespace Nexora.Web.Models.ViewModels;

public class ProfileUpdateViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Full Name")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Nickname")]
    [StringLength(50, ErrorMessage = "The {0} must be at max {1} characters long.")]
    public string? Nickname { get; set; }

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "About Me")]
    [StringLength(500, ErrorMessage = "The {0} must be at max {1} characters long.")]
    public string? Description { get; set; }

    [Display(Name = "Profile Picture")]
    public IFormFile? AvatarFile { get; set; }

    public string? CurrentAvatarUrl { get; set; }

    [Display(Name = "Remove current profile picture")]
    public bool RemoveAvatar { get; set; }
}

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}