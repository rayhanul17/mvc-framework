using Microsoft.EntityFrameworkCore;
using Nexora.Application.Interfaces;
using Nexora.Core.Common;
using Nexora.Core.Entities;
using Nexora.Core.Interfaces;

namespace Nexora.Application.Services;

public class MenuRankService : IMenuRankService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public MenuRankService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<bool>> UpdateMenuRankAsync(int menuId, int newOrder)
    {
        try
        {
            var menu = await _unitOfWork.Repository<Menu>()
                .GetQueryable()
                .FirstOrDefaultAsync(m => m.Id == menuId);
                
            if (menu == null)
                return Result<bool>.Failure("Menu not found");
                
            var oldOrder = menu.Order;
            
            if (oldOrder == newOrder)
                return Result<bool>.Success(true);
            
            // Get all menus at the same level (same parent)
            var siblingMenus = await _unitOfWork.Repository<Menu>()
                .GetQueryable()
                .Where(m => m.ParentId == menu.ParentId && m.Id != menuId)
                .OrderBy(m => m.Order)
                .ToListAsync();
            
            // Update the target menu order
            menu.Order = newOrder;
            
            // Adjust other menu orders
            if (newOrder < oldOrder)
            {
                // Moving up - increment orders of menus between new and old position
                foreach (var sibling in siblingMenus.Where(m => m.Order >= newOrder && m.Order < oldOrder))
                {
                    sibling.Order++;
                }
            }
            else
            {
                // Moving down - decrement orders of menus between old and new position
                foreach (var sibling in siblingMenus.Where(m => m.Order > oldOrder && m.Order <= newOrder))
                {
                    sibling.Order--;
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating menu rank: {ex.Message}");
        }
    }
    
    public async Task<Result<bool>> ReorderMenusAsync(List<MenuOrderDto> menuOrders)
    {
        try
        {
            foreach (var menuOrder in menuOrders)
            {
                var menu = await _unitOfWork.Repository<Menu>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(m => m.Id == menuOrder.MenuId);
                    
                if (menu != null)
                {
                    menu.Order = menuOrder.Order;
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error reordering menus: {ex.Message}");
        }
    }
}

public class MenuOrderDto
{
    public int MenuId { get; set; }
    public int Order { get; set; }
}