using AutoMapper;
using ModularHost.Web.Modules.Blog.Models.DTOs;
using ModularHost.Web.Modules.Blog.Models.Entities;
using System.Linq;

namespace ModularHost.Web.Modules.Blog.Mappings
{
    public class BlogMappingProfile : Profile
    {
        public BlogMappingProfile()
        {
            // BlogPost mappings
            CreateMap<BlogPost, BlogPostDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.FullName : string.Empty))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.BlogPostTags.Select(pt => pt.Tag)))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count(c => !c.IsDeleted)));

            CreateMap<CreateBlogPostDto, BlogPost>()
                .ForMember(dest => dest.BlogPostTags, opt => opt.Ignore());

            CreateMap<UpdateBlogPostDto, BlogPost>()
                .ForMember(dest => dest.BlogPostTags, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // Category mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.PostCount, opt => opt.MapFrom(src => src.BlogPosts.Count(p => p.IsPublished)));

            CreateMap<CreateCategoryDto, Category>();

            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // Tag mappings
            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.PostCount, opt => opt.MapFrom(src => src.BlogPostTags.Count(pt => pt.BlogPost.IsPublished)));

            CreateMap<CreateTagDto, Tag>();

            CreateMap<UpdateTagDto, Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // Comment mappings
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.PostTitle, opt => opt.MapFrom(src => src.Post != null ? src.Post.Title : string.Empty))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Anonymous"))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePicture : null));

            CreateMap<CreateCommentDto, Comment>();

            CreateMap<UpdateCommentDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
        }
    }
}