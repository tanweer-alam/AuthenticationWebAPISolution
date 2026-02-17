namespace API.Helpers
{
    public class UserRoles
    {
        public string RoleName { get; private set; }
        public int Level { get; private set; }
        public UserRoles(string roleName, int level)
        {
            RoleName = roleName;
            Level = level;
        }
        public static readonly UserRoles Admin = new UserRoles("Admin", 500);
        public static readonly UserRoles User = new UserRoles("User", 100);
        public static readonly UserRoles SuperAdmin = new UserRoles("SuperAdmin", 1000);
        public static readonly List<UserRoles> All = new List<UserRoles>() {User, Admin, SuperAdmin };
    }
}
