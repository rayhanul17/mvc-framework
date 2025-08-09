namespace DynamicRoleMenuSystem.Application.Interfaces;

public interface IAreaDiscoveryService
{
    List<string> GetAllAreas();
    List<ControllerInfo> GetControllersInArea(string areaName);
    List<string> GetActionsInController(string areaName, string controllerName);
}

public class ControllerInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = new();
}