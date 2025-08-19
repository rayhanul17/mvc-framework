namespace Nexora.Web.Models;

public class BlogLandingViewModel
{
    public string SiteName { get; set; } = "Nexora Blog";
    public string SiteDescription { get; set; } = "Explore insights, tutorials, and industry trends";
    public List<BlogPostSummary> FeaturedPosts { get; set; } = new();
    public List<BlogPostSummary> RecentPosts { get; set; } = new();
    public List<BlogPostSummary> PopularPosts { get; set; } = new();
    public List<CategoryInfo> Categories { get; set; } = new();
    public List<TagInfo> PopularTags { get; set; } = new();
}

public class BlogPostSummary
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FeaturedImageUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public int ViewCount { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class CategoryInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PostCount { get; set; }
    public int DisplayOrder { get; set; }
}

public class TagInfo
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}