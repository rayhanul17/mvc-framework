namespace mvc.framework.Models
{
	public class UserViewModel
	{
		public string? Id { get; set; }
		public string? UserName { get; set; }
		public string? Email { get; set; }
		public string? Name { get; set; }
		public string? Image { get; set; }
		public DateTime? BirthOfDate { get; set; }
		public RoleViewModel[]? Roles { get; set; }
	}
}