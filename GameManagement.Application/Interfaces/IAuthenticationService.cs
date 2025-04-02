using System.Threading.Tasks;
using GameManagement.Application.DTOs;

namespace GameManagement.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
        Task<AuthenticationResponse> RegisterAsync(RegistrationRequest request);
    }
}