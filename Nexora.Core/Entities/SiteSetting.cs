namespace Nexora.Core.Entities;

public class SiteSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SettingCategory Category { get; set; }
    public SettingType Type { get; set; }
    public string? ValidValues { get; set; } // JSON array for dropdown/radio options
    public bool IsRequired { get; set; } = false;
    public bool IsSystemSetting { get; set; } = false; // Cannot be deleted if true
    public int Order { get; set; } = 0;
    public string? UpdatedBy { get; set; } // Keep for backward compatibility, will map to ModifiedBy
}

public enum SettingCategory
{
    Branding = 1,
    Theme = 2,
    Layout = 3,
    Security = 4,
    Features = 5,
    Contact = 6,
    Social = 7,
    Advanced = 8
}

public enum SettingType
{
    Text = 1,
    Number = 2,
    Boolean = 3,
    Color = 4,
    Url = 5,
    Email = 6,
    TextArea = 7,
    Select = 8,
    Json = 9,
    File = 10
}