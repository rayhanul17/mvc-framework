using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using mvc.framework.Data;
using mvc.framework.Models;
using mvc.framework.Services;
using System.Reflection;

namespace mvc.framework.Controllers
{
	[Authorize]
	public class AdminController : Controller
	{
		private readonly ILogger<AdminController> _logger;
		private readonly IDataAccessService _dataAccessService;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDbContext _context;

		public AdminController(
				UserManager<IdentityUser> userManager,
				RoleManager<IdentityRole> roleManager,
				IDataAccessService dataAccessService,
				ILogger<AdminController> logger,
				ApplicationDbContext context)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
			_dataAccessService = dataAccessService ?? throw new ArgumentNullException(nameof(dataAccessService));
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		[Authorize("Authorization")]
		public async Task<IActionResult> Roles()
		{
			var roleViewModel = new List<RoleViewModel>();
			var roles = await _roleManager.Roles.ToListAsync();
			foreach (var item in roles)
			{
				roleViewModel.Add(new RoleViewModel()
				{
					Id = item.Id,
					Name = item.Name,
				});
			}

			return View(roleViewModel);
		}

		[Authorize("Roles")]
		public IActionResult CreateRole()
		{
			return View(new RoleViewModel());
		}

		[HttpPost]
		[Authorize("Roles")]
		public async Task<IActionResult> CreateRole(RoleViewModel viewModel)
		{
			if (ModelState.IsValid)
			{
				var result = await _roleManager.CreateAsync(new IdentityRole() { Name = viewModel.Name });
				if (result.Succeeded)
				{
					return RedirectToAction(nameof(Roles));
				}
				else
				{
					ModelState.AddModelError("Name", string.Join(",", result.Errors));
				}
			}

			return View(viewModel);
		}

		[Authorize("Authorization")]
		public async Task<IActionResult> Menus()
		{
			var roleViewModel = new List<RoleViewModel>();
			var roles = await _roleManager.Roles.ToListAsync();
			foreach (var item in roles)
			{
				roleViewModel.Add(new RoleViewModel()
				{
					Id = item.Id,
					Name = item.Name,
				});
			}

			return View(roleViewModel);
		}

		[Authorize("Menus")]
		public IActionResult CreateMenu()
		{
			var viewModel = new NavigationMenuViewModel();
			// Before returning the view:
			viewModel.ParentMenuList = _context.NavigationMenu
				.Where(m => m.ParentMenuId == null)
				.OrderBy(m => m.DisplayOrder)
				.Select(m => new SelectListItem
				{
					Value = m.Id.ToString(),
					Text = m.Name
				}).ToList();


			return View(viewModel);
		}


		[HttpPost]
		[Authorize("Menus")]
		public async Task<IActionResult> CreateMenu(NavigationMenuViewModel viewModel)
		{
			if (ModelState.IsValid)
			{
				if (viewModel.ParentMenuId != null)
				{
					var parentMenu = await _context.NavigationMenu.FindAsync(viewModel.ParentMenuId);
					if (parentMenu == null)
						throw new InvalidOperationException("Parent menu not found.");
				}

				var menu = new NavigationMenu
				{
					Name = viewModel.Name,
					Area = viewModel.Area,
					ControllerName = viewModel.ControllerName,
					ActionName = viewModel.ActionName,
					ParentMenuId = viewModel.ParentMenuId,
					DisplayOrder = viewModel.DisplayOrder,
					Visible = true
				};

				_context.NavigationMenu.Add(menu);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Menus));
			}

			// Repopulate Parent Menu dropdown on error
			viewModel.ParentMenuList = _context.NavigationMenu
				.Where(m => m.ParentMenuId == null)
				.OrderBy(m => m.DisplayOrder)
				.Select(m => new SelectListItem
				{
					Value = m.Id.ToString(),
					Text = m.Name
				}).ToList();

			return View(viewModel);
		}


		[Authorize("Authorization")]
		public async Task<IActionResult> Users()
		{
			var userViewModel = new List<UserViewModel>();
			var users = await _userManager.Users.ToListAsync();
			foreach (var item in users)
			{
				userViewModel.Add(new UserViewModel()
				{
					Id = item.Id,
					Email = item.Email,
					UserName = item.UserName,
				});
			}

			return View(userViewModel);
		}

		[Authorize("Users")]
		public async Task<IActionResult> EditUser(string id)
		{
			var viewModel = new UserViewModel();
			if (!string.IsNullOrWhiteSpace(id))
			{
				var user = await _userManager.FindByIdAsync(id);
				var userRoles = await _userManager.GetRolesAsync(user);

				viewModel.Email = user?.Email;
				viewModel.UserName = user?.UserName;

				var allRoles = await _roleManager.Roles.ToListAsync();
				viewModel.Roles = allRoles.Select(x => new RoleViewModel()
				{
					Id = x.Id,
					Name = x.Name,
					Selected = userRoles.Contains(x.Name)
				}).ToArray();

			}

			return View(viewModel);
		}

		[HttpPost, Authorize("Users")]
		public async Task<IActionResult> EditUser(UserViewModel viewModel)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByIdAsync(viewModel.Id);
				var userRoles = await _userManager.GetRolesAsync(user);

				await _userManager.RemoveFromRolesAsync(user, userRoles);
				await _userManager.AddToRolesAsync(user, viewModel.Roles?.Where(x => x.Selected).Select(x => x.Name));

				return RedirectToAction(nameof(Users));
			}

			return View(viewModel);
		}

		[Authorize("Authorization")]
		public async Task<IActionResult> EditRolePermission(string id)
		{
			var permissions = new List<NavigationMenuViewModel>();
			if (!string.IsNullOrWhiteSpace(id))
			{
				permissions = await _dataAccessService.GetPermissionsByRoleIdAsync(id);
			}

			return View(permissions);
		}

		[HttpPost, Authorize("Authorization")]
		public async Task<IActionResult> EditRolePermission(string id, List<NavigationMenuViewModel> viewModel)
		{
			if (ModelState.IsValid)
			{
				var permissionIds = viewModel.Where(x => x.Permitted).Select(x => x.Id);

				await _dataAccessService.SetPermissionsByRoleIdAsync(id, permissionIds);
				return RedirectToAction(nameof(Roles));
			}

			return View(viewModel);
		}

		[HttpGet]
		public IActionResult GetAreas()
		{
			var areas = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => typeof(Controller).IsAssignableFrom(type) && !type.IsAbstract)
				.Select(type => type.GetCustomAttribute<AreaAttribute>()?.RouteValue)
				.Distinct()
				.Where(a => !string.IsNullOrEmpty(a))
				.ToList();
			return Json(areas);
		}

		[HttpGet]
		public IActionResult GetControllers(string area)
		{
			var controllers = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => typeof(Controller).IsAssignableFrom(type) && !type.IsAbstract)
				.Where(type => (type.GetCustomAttribute<AreaAttribute>()?.RouteValue ?? "") == (area ?? ""))
				.Select(type => type.Name.Replace("Controller", ""))
				.Distinct()
				.ToList();
			return Json(controllers);
		}

		[HttpGet]
		public IActionResult GetActions(string area, string controller2)
		{
			var controllerType = Assembly.GetExecutingAssembly()
									.GetTypes()
									.FirstOrDefault(type =>
										typeof(Controller).IsAssignableFrom(type) &&
										!type.IsAbstract &&
										type.Name.Equals(controller2 + "Controller", StringComparison.OrdinalIgnoreCase) &&
										(
											(type.GetCustomAttribute<AreaAttribute>()?.RouteValue ?? "") == (area ?? "")
											|| type.Namespace?.Contains($".Areas.{area}.Controllers", StringComparison.OrdinalIgnoreCase) == true
										)
									);

			if (controllerType == null)
				return Json(new List<string>());
			var actions = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(m => m.IsPublic && !m.IsDefined(typeof(NonActionAttribute)))
				.Select(m => m.Name)
				.Distinct()
				.ToList();
			return Json(actions);
		}
	}
}