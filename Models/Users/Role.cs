using System.Runtime.Serialization;

namespace Sonic.Models
{
    public class Role
    {
        public static string Admin = "Admin";
        public static string Player = "Player";
        public static string Manager = "Manager";
        public static string Viewer = "Viewer";

        public static List<string> GetAllRoles()
        {
            return new List<string>
            {
                Admin,
                Player,
                Manager,
                Viewer
            };
        }
    }
}