using Nexora.Web.Models.ViewModels;

namespace Nexora.Web.Models;

public class TagViewModel
{
    public string TagName { get; set; } = string.Empty;
    public List<BlogPostViewModel> Posts { get; set; } = new();
    public string SiteName { get; set; } = "Nexora Framework";
    public int PostCount { get; set; }
}