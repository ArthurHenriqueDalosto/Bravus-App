using BravusApp.Shared.RequestModels;
using BravusApp.Shared.ResponseModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;

namespace BravusApp.Client.Services
{
    public interface IAuthService
    {
        Task<RequestResponse<string>> LoginAsync(LoginRequest model);
        Task LogoutAsync();
    }


    public sealed class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private readonly JwtAuthStateProvider _authProvider;

        public AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider provider)
        {
            _http = http; _js = js;
            _authProvider = (JwtAuthStateProvider)provider;
        }

        public async Task<RequestResponse<string>> LoginAsync(LoginRequest model)
        {
            var res = await _http.PostAsJsonAsync("/api/auth/login", model);

            var dto = await res.Content.ReadFromJsonAsync<RequestResponse<string>>();

            if (dto == null)
                return RequestResponse<string>.Fail("Erro ao se conectar com o servidor");

            if (dto.Success && !string.IsNullOrWhiteSpace(dto.Data))
            {
                await _authProvider.NotifyUserAuthenticationAsync(dto.Data);
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dto.Data);
            }

            return dto;
        }

        public async Task LogoutAsync()
        {
            await _authProvider.NotifyUserLogoutAsync();
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }
}
