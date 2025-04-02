namespace GameManagement.Application.DTOs
{
    public class RegistrationRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}