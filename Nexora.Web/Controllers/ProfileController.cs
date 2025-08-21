using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Application.Services;
using Nexora.Core.Entities;
using Nexora.Web.Models.ViewModels;

namespace Nexora.Web.Controllers;

[Authorize]
public class ProfileController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        IFileUploadService fileUploadService)
    {
        _userManager = userManager;
        _fileUploadService = fileUploadService;
    }

    public IActionResult Index()
    {
        // Redirect to Update action for profile editing
        return RedirectToAction(nameof(Update));
    }

    [HttpGet]
    public async Task<IActionResult> Update()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new ProfileUpdateViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            Nickname = user.Nickname,
            PhoneNumber = user.PhoneNumber,
            Description = user.Description,
            CurrentAvatarUrl = _fileUploadService.GetImageUrl(user.AvatarUrl)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileUpdateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Check if email is being changed and if it's already taken
        if (user.Email != model.Email)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                model.CurrentAvatarUrl = _fileUploadService.GetImageUrl(user.AvatarUrl);
                return View(model);
            }
        }

        // Handle avatar upload
        if (model.AvatarFile != null && model.AvatarFile.Length > 0)
        {
            if (!_fileUploadService.IsValidImage(model.AvatarFile))
            {
                ModelState.AddModelError("AvatarFile", "Please upload a valid image file (JPG, PNG, GIF, WebP) less than 5MB.");
                model.CurrentAvatarUrl = _fileUploadService.GetImageUrl(user.AvatarUrl);
                return View(model);
            }

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                await _fileUploadService.DeleteImageAsync(user.AvatarUrl);
            }

            // Upload new avatar
            var uploadedPath = await _fileUploadService.UploadImageAsync(
                model.AvatarFile, 
                FileUploadService.PROFILE_IMAGES_FOLDER);
            
            if (!string.IsNullOrEmpty(uploadedPath))
            {
                user.AvatarUrl = uploadedPath;
            }
        }
        else if (model.RemoveAvatar && !string.IsNullOrEmpty(user.AvatarUrl))
        {
            // Remove avatar if requested
            await _fileUploadService.DeleteImageAsync(user.AvatarUrl);
            user.AvatarUrl = null;
        }

        // Update user information
        user.FullName = model.FullName;
        user.Nickname = model.Nickname;
        user.Email = model.Email;
        user.UserName = model.Email; // Keep username same as email
        user.PhoneNumber = model.PhoneNumber;
        user.Description = model.Description;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            SetSuccessMessage("Profile updated successfully!", showAfterRedirect: true);
            return RedirectToAction(nameof(Update));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        model.CurrentAvatarUrl = _fileUploadService.GetImageUrl(user.AvatarUrl);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            SetErrorMessage("Please check your password entries.", showAfterRedirect: true);
            return RedirectToAction(nameof(Update));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            SetSuccessMessage("Password changed successfully!", showAfterRedirect: true);
            LogInformation("User {UserId} changed password successfully", user.Id);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            SetErrorMessage($"Password change failed: {errors}", showAfterRedirect: true);
        }

        return RedirectToAction(nameof(Update));
    }
}