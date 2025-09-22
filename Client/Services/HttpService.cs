using BravusApp.Shared.ResponseModels;
using System.Net.Http.Json;
using System.Text.Json;

namespace BravusApp.Client.Services
{
    public interface IHttpService
    {
        Task<RequestResponse<T>> GetAsync<T>(string url, CancellationToken ct = default);
        Task<RequestResponse<T>> GetResponseAsync<T>(string url, CancellationToken ct = default); // alias
        Task<RequestResponse<T>> PostAsync<TReq, T>(string url, TReq body, CancellationToken ct = default);
        Task<RequestResponse<T>> PutAsync<TReq, T>(string url, TReq body, CancellationToken ct = default);
        Task DeleteAsync(string url, CancellationToken ct = default);
    }

    public class HttpService : IHttpService
    {
        static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        private readonly HttpClient http;
        public HttpService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<RequestResponse<T>> GetResponseAsync<T>(string url, CancellationToken ct = default)
        {
            return await GetAsync<T>(url, ct);
        }

        public async Task<RequestResponse<T>> GetAsync<T>(string url, CancellationToken ct = default)
        {
            using var resp = await http.GetAsync(url, ct);
            return await HandleResponse<T>(resp, ct);
        }

        public async Task<RequestResponse<T>> PostAsync<TReq, T>(string url, TReq body, CancellationToken ct = default)
        {
            using var resp = await http.PostAsJsonAsync(url, body, _json, ct);
            return await HandleResponse<T>(resp, ct);
        }

        public async Task<RequestResponse<T>> PutAsync<TReq, T>(string url, TReq body, CancellationToken ct = default)
        {
            using var resp = await http.PutAsJsonAsync(url, body, _json, ct);
            return await HandleResponse<T>(resp, ct);
        }

        public async Task DeleteAsync(string url, CancellationToken ct = default)
        {
            using var resp = await http.DeleteAsync(url, ct);
            await EnsureSuccessOrThrow(resp, ct);
        }

        static async Task<RequestResponse<T>> HandleResponse<T>(HttpResponseMessage resp, CancellationToken ct)
        {
            var payload = await resp.Content.ReadFromJsonAsync<RequestResponse<T>>(_json, ct)
                          ?? throw new ApiException("Resposta vazia ou inválida do servidor.");

            if (!resp.IsSuccessStatusCode)
            {
                var msg = string.IsNullOrWhiteSpace(payload.Message)
                    ? $"Erro HTTP {(int)resp.StatusCode}"
                    : payload.Message;
                throw new ApiException(msg);
            }

            return payload;
        }

        static async Task EnsureSuccessOrThrow(HttpResponseMessage resp, CancellationToken ct)
        {
            if (resp.IsSuccessStatusCode) return;

            RequestResponse<object>? payload = null;
            try { payload = await resp.Content.ReadFromJsonAsync<RequestResponse<object>>(_json, ct); } catch { }
            var body = payload?.Message ?? (await resp.Content.ReadAsStringAsync(ct));
            var msg = string.IsNullOrWhiteSpace(body) ? $"HTTP {(int)resp.StatusCode}" : body;
            throw new ApiException(msg);
        }

        public sealed class ApiException(string message) : Exception(message);
    }
}
