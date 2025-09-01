using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Services;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Modules.Blog.Models.Entities;
using MRCMS.Modules.Blog.Services.Interfaces;
using MRCMS.Modules.Blog.Models.DTOs;
using AutoMapper;

namespace MRCMS.Modules.Blog.Services
{
    public class CommentService : BaseServiceWithDto<Comment, CommentDto, CreateCommentDto, UpdateCommentDto>, ICommentService
    {
        public CommentService(IRepository<Comment> repository, IUnitOfWork unitOfWork, IMapper mapper)
            : base(repository, unitOfWork, mapper)
        {
        }

        public async Task<IEnumerable<CommentDto>> GetPostCommentsAsync(Guid postId, bool includeReplies = true)
        {
            var query = _repository.Query()
                .Where(c => c.PostId == postId && !c.IsDeleted);

            if (!includeReplies)
            {
                query = query.Where(c => c.ParentCommentId == null);
            }
            else
            {
                query = query.Include(c => c.Replies);
            }

            var comments = await query
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<IEnumerable<CommentDto>> GetUserCommentsAsync(Guid userId, int page, int pageSize)
        {
            var comments = await _repository.Query()
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<IEnumerable<CommentDto>> GetPendingCommentsAsync(int page, int pageSize)
        {
            var comments = await _repository.Query()
                .Where(c => !c.IsApproved && !c.IsDeleted)
                .Include(c => c.User)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<bool> ApproveCommentAsync(Guid commentId)
        {
            var comment = await _repository.GetByIdAsync(commentId);
            if (comment == null) return false;

            comment.IsApproved = true;
            comment.UpdatedAt = DateTime.UtcNow;
            _repository.Update(comment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectCommentAsync(Guid commentId)
        {
            var comment = await _repository.GetByIdAsync(commentId);
            if (comment == null) return false;

            comment.IsApproved = false;
            comment.UpdatedAt = DateTime.UtcNow;
            _repository.Update(comment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteCommentAsync(Guid commentId)
        {
            var comment = await _repository.GetByIdAsync(commentId);
            if (comment == null) return false;

            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;
            _repository.Update(comment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}