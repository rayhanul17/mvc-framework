using Nexora.Web.Models.ViewModels;

namespace Nexora.Web.Models;

public class CategoryViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string CategoryDescription { get; set; } = string.Empty;
    public List<BlogPostViewModel> Posts { get; set; } = new();
    public string SiteName { get; set; } = "Nexora Framework";
}