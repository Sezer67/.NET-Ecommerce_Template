namespace ECommerce.UserService.Model
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public List<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}