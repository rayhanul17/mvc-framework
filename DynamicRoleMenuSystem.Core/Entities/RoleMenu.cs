namespace DynamicRoleMenuSystem.Core.Entities;

public class RoleMenu : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;
    public int MenuId { get; set; }
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    
    public virtual ApplicationRole Role { get; set; } = null!;
    public virtual Menu Menu { get; set; } = null!;
}