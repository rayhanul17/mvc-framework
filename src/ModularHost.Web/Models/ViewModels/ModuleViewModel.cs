using System.Collections.Generic;

namespace MRCMS.Models.ViewModels
{
    public class ModuleViewModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string TablePrefix { get; set; }
        public bool IsActive { get; set; }
        public int EntityCount { get; set; }
        public string[] Tables { get; set; }
    }

    public class ModuleDetailsViewModel : ModuleViewModel
    {
        public bool IsEnabled { get; set; }
        public new List<TableInfo> Tables { get; set; } = new List<TableInfo>();
        public List<string> AppliedMigrations { get; set; } = new List<string>();
        public List<string> PendingMigrations { get; set; } = new List<string>();
    }

    public class TableInfo
    {
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public string FullTableName { get; set; }
    }
}