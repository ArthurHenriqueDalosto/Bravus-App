using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BravusApp.Client.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password, bool remember);
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

        public async Task<bool> LoginAsync(string email, string password, bool remember)
        {
            var res = await _http.PostAsJsonAsync("/api/auth/login", new { email, password });
            if (!res.IsSuccessStatusCode) return false;

            var dto = await res.Content.ReadFromJsonAsync<LoginResult>();
            if (string.IsNullOrWhiteSpace(dto?.Token)) return false;

            await _authProvider.NotifyUserAuthenticationAsync(dto.Token);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dto.Token);
            return true;
        }

        public async Task LogoutAsync()
        {
            await _authProvider.NotifyUserLogoutAsync();
            _http.DefaultRequestHeaders.Authorization = null;
        }

        private sealed record LoginResult(string Token);
    }
}
