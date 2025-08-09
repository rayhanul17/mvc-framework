using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using DynamicRoleMenuSystem.Application.Interfaces;

namespace DynamicRoleMenuSystem.Application.Services;

public class AreaDiscoveryService : IAreaDiscoveryService
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorProvider;

    public AreaDiscoveryService(IActionDescriptorCollectionProvider actionDescriptorProvider)
    {
        _actionDescriptorProvider = actionDescriptorProvider;
    }

    public List<string> GetAllAreas()
    {
        var areas = new HashSet<string>();
        var actionDescriptors = _actionDescriptorProvider.ActionDescriptors.Items;

        foreach (var descriptor in actionDescriptors.OfType<ControllerActionDescriptor>())
        {
            var areaName = descriptor.ControllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue;
            if (!string.IsNullOrEmpty(areaName))
            {
                areas.Add(areaName);
            }
        }

        // Add empty string for no area (default)
        areas.Add("");
        
        return areas.OrderBy(a => a).ToList();
    }

    public List<ControllerInfo> GetControllersInArea(string areaName)
    {
        var controllers = new Dictionary<string, ControllerInfo>();
        var actionDescriptors = _actionDescriptorProvider.ActionDescriptors.Items;

        foreach (var descriptor in actionDescriptors.OfType<ControllerActionDescriptor>())
        {
            var currentAreaName = descriptor.ControllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue ?? "";
            
            if (currentAreaName == areaName)
            {
                var controllerName = descriptor.ControllerName;
                if (!controllers.ContainsKey(controllerName))
                {
                    controllers[controllerName] = new ControllerInfo
                    {
                        Name = controllerName,
                        DisplayName = controllerName.Replace("Controller", ""),
                        Actions = new List<string>()
                    };
                }
                
                if (!controllers[controllerName].Actions.Contains(descriptor.ActionName))
                {
                    controllers[controllerName].Actions.Add(descriptor.ActionName);
                }
            }
        }

        return controllers.Values.OrderBy(c => c.Name).ToList();
    }

    public List<string> GetActionsInController(string areaName, string controllerName)
    {
        var actions = new HashSet<string>();
        var actionDescriptors = _actionDescriptorProvider.ActionDescriptors.Items;

        foreach (var descriptor in actionDescriptors.OfType<ControllerActionDescriptor>())
        {
            var currentAreaName = descriptor.ControllerTypeInfo.GetCustomAttribute<AreaAttribute>()?.RouteValue ?? "";
            
            if (currentAreaName == areaName && descriptor.ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(descriptor.ActionName);
            }
        }

        return actions.OrderBy(a => a).ToList();
    }
}