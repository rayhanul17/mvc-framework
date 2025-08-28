using Nexora.Application.Services;
using Nexora.Core.Common;

namespace Nexora.Application.Interfaces;

public interface IMenuRankService
{
    Task<Result<bool>> UpdateMenuRankAsync(int menuId, int newOrder);
    Task<Result<bool>> ReorderMenusAsync(List<MenuOrderDto> menuOrders);
}