using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net;
using System.Net.Http.Json;

namespace IIABlazorWebAssembly.Services
{
    public class APIService(HttpClient http, IAccessTokenProvider authService)
    {
        private async Task SetToken()
        {
            var accessToken = await authService.RequestAccessToken();
            accessToken.TryGetToken(out var token);
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token?.Value ?? "ERROR");
        }

        public async Task<string> GetDataAsync(string apiUrl)
        {
            await this.SetToken();

            var response = await http.GetAsync(http.BaseAddress + apiUrl);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<(HttpStatusCode StatusCode, T? Data)> PostDataAsync<T>(string apiUrl, object data)
        {
            await this.SetToken();

            var response = await http.PostAsJsonAsync(http.BaseAddress + apiUrl, data);

            T? result = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<T>()
                : default;

            return (response.StatusCode, result);
        }

        public async Task<HttpStatusCode> PostDataAsync(string apiUrl, object data)
        {
            await this.SetToken();

            var response = await http.PostAsJsonAsync(http.BaseAddress + apiUrl, data);

            return response.StatusCode;
        }

        public async Task<(HttpStatusCode StatusCode, T? Data)> GetDataAsync<T>(string apiUrl)
        {
            await this.SetToken();

            var response = await http.GetAsync(http.BaseAddress + apiUrl);
            return (response.StatusCode, await response.Content.ReadFromJsonAsync<T>());
        }

        public async Task<HttpStatusCode> DeleteDataAsync(string apiUrl)
        {
            await this.SetToken();

            var response = await http.DeleteAsync(http.BaseAddress + apiUrl);
            return response.StatusCode;
        }

        public async Task<HttpStatusCode> PutDataAsync(string apiUrl, object data)
        {
            await this.SetToken();

            var response = await http.PutAsJsonAsync(http.BaseAddress + apiUrl, data);

            return response.StatusCode;
        }
    }
}