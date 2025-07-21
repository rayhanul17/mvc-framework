using Microsoft.AspNetCore.Mvc;
using mvc.framework.Services;

namespace mvc.framework.ViewComponents;

public class DefaultNavigationMenuViewComponent : ViewComponent
{
	private readonly IDataAccessService _dataAccessService;

	public DefaultNavigationMenuViewComponent(IDataAccessService dataAccessService)
	{
		_dataAccessService = dataAccessService;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var items = await _dataAccessService.GetMenuItemsAsync(HttpContext.User);

		return View(items);
	}
}

public class LeftNavigationMenuViewComponent : ViewComponent
{
    private readonly IDataAccessService _dataAccessService;

    public LeftNavigationMenuViewComponent(IDataAccessService dataAccessService)
    {
        _dataAccessService = dataAccessService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await _dataAccessService.GetMenuItemsAsync(HttpContext.User);

        return View(items);
    }
}

public class FooterNavigationMenuViewComponent : ViewComponent
{
    private readonly IDataAccessService _dataAccessService;

    public FooterNavigationMenuViewComponent(IDataAccessService dataAccessService)
    {
        _dataAccessService = dataAccessService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var items = await _dataAccessService.GetMenuItemsAsync(HttpContext.User);

        return View(items);
    }
}

