namespace NetCore.ViewModels
{
    public class LoginViewModel
    {
        public int UserId { get; set; }

        public string Token { get; set; }

        public string FullName { get; set; }

        public DateTime Expires { get; set; }
    }
}