namespace GameManagement.Application.DTOs
{
    public class AuthenticationResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }

        // Método de fábrica para crear una respuesta exitosa
        public static AuthenticationResponse Successful(string token, string username)
        {
            return new AuthenticationResponse
            {
                Success = true,
                Token = token,
                Username = username,
                Message = "Authentication successful"
            };
        }

        // Método de fábrica para crear una respuesta fallida
        public static AuthenticationResponse Failed(string message)
        {
            return new AuthenticationResponse
            {
                Success = false,
                Message = message
            };
        }
    }
}