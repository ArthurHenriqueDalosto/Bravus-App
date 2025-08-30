using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BravusApp.Client.Services
{
    public sealed class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;
        private ClaimsPrincipal _cached = new(new ClaimsIdentity());

        public JwtAuthStateProvider(IJSRuntime js)
        {
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "auth_token");
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwt.Claims, authenticationType: "jwt");
                _cached = new ClaimsPrincipal(identity);
                return new AuthenticationState(_cached);
            }
            catch
            {
                // token inválido → trata como não autenticado
                await _js.InvokeVoidAsync("localStorage.removeItem", "auth_token");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        // Chame isso após login para notificar a UI
        public async Task NotifyUserAuthenticationAsync(string token)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "auth_token", token);
            var state = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }

        // Chame isso após logout
        public async Task NotifyUserLogoutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "auth_token");
            _cached = new(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cached)));
        }
    }
}
