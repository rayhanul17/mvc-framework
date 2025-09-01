using AutoMapper;
using MRCMS.Core.Models.Entities;
using MRCMS.Modules.Blog.Models.DTOs;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Modules.Blog.Models.ViewModels;
using System.Linq;

namespace MRCMS.Modules.BlogModule.Mappings
{
    public class BlogMappingProfile : Profile
    {
        public BlogMappingProfile()
        {
            // BlogPost mappings
            CreateMap<BlogPost, BlogPostDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                    src.Author != null ? src.Author.FullName : "Unknown"))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => 
                    src.Category != null ? src.Category.Name : "Uncategorized"))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom((src, dest, destMember, context) => 
                    src.BlogPostTags != null && src.BlogPostTags.Any() ? 
                        context.Mapper.Map<List<TagDto>>(src.BlogPostTags.Where(pt => pt.Tag != null).Select(pt => pt.Tag).ToList()) : 
                        new List<TagDto>()))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => 
                    src.Comments != null ? src.Comments.Count : 0))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));

            CreateMap<CreateBlogPostDto, BlogPost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.BlogPostTags, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateBlogPostDto, BlogPost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.BlogPostTags, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // Category mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.PostCount, opt => opt.MapFrom(src => 
                    src.BlogPosts != null ? src.BlogPosts.Count : 0));

            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BlogPosts, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BlogPosts, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Tag mappings
            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.PostCount, opt => opt.MapFrom(src => 
                    src.BlogPostTags != null ? src.BlogPostTags.Count : 0));

            CreateMap<CreateTagDto, Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BlogPostTags, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpdateTagDto, Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BlogPostTags, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Comment mappings
            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                    src.User != null ? src.User.FullName : "Anonymous"))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => 
                    src.User != null ? src.User.ProfilePicture : null))
                .ForMember(dest => dest.PostTitle, opt => opt.MapFrom(src => 
                    src.Post != null ? src.Post.Title : "Unknown Post"));

            CreateMap<CreateCommentDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<UpdateCommentDto, Comment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Post, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PostId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCommentId, opt => opt.Ignore());

            // ViewModel mappings for UI
            CreateMap<BlogPost, BlogPostViewModel>()
                .ForMember(dest => dest.SelectedTagIds, opt => opt.MapFrom(src => 
                    src.BlogPostTags != null ? src.BlogPostTags.Select(pt => pt.TagId).ToList() : new List<Guid>()))
                .ForMember(dest => dest.FeaturedImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.Tags, opt => opt.Ignore());

            CreateMap<BlogPostViewModel, CreateBlogPostDto>()
                .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.SelectedTagIds));

            CreateMap<BlogPostViewModel, UpdateBlogPostDto>()
                .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.SelectedTagIds));

            CreateMap<CreateBlogPostDto, BlogPostViewModel>()
                .ForMember(dest => dest.SelectedTagIds, opt => opt.MapFrom(src => src.TagIds))
                .ForMember(dest => dest.FeaturedImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.Tags, opt => opt.Ignore());

            CreateMap<UpdateBlogPostDto, BlogPostViewModel>()
                .ForMember(dest => dest.SelectedTagIds, opt => opt.MapFrom(src => src.TagIds))
                .ForMember(dest => dest.FeaturedImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.Tags, opt => opt.Ignore());

            CreateMap<BlogPostDto, BlogPostViewModel>()
                .ForMember(dest => dest.SelectedTagIds, opt => opt.MapFrom(src => 
                    src.Tags != null ? src.Tags.Select(t => t.Id).ToList() : new List<Guid>()))
                .ForMember(dest => dest.FeaturedImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.Tags, opt => opt.Ignore())
                .ForMember(dest => dest.MetaKeywords, opt => opt.MapFrom(src => src.MetaKeywords));

            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryViewModel, Category>()
                .ForMember(dest => dest.BlogPosts, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<Tag, TagViewModel>();
            CreateMap<TagViewModel, Tag>()
                .ForMember(dest => dest.BlogPostTags, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Author (User) mappings
            CreateMap<User, AuthorDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

            // BlogPostTag mappings
            CreateMap<BlogPostTag, BlogPostTagDto>()
                .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src.Tag));
        }
    }
}