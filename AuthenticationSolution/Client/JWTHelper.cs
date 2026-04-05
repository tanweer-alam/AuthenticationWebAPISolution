using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client
{
    public class JWTHelper
    {
        private static readonly string baseUrl = "https://localhost:7131";
        private static readonly string clientId = "client1";
        private static string accessToken = string.Empty;
        private static readonly System.Text.Json.JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };
        public static async Task Execute()
        {
            // 1. Login to get JWT + Refresh Token
            Console.WriteLine("Logging in...");
            var auth = await LoginAsync("tanweet9955@gmail.com", "Shaikh95@#", clientId);
            if (auth == null)
            {
                Console.WriteLine("Login failed!");
                return;
            }
            accessToken = auth.AccessToken;
            Console.WriteLine("Login successful!");
            Console.WriteLine($"Access Token: {accessToken.Substring(0, 20)}...");
            // 2. Call protected endpoint with JWT
            await CallProtectedApiAsync();
        }
        static async Task<AuthResponse?> LoginAsync(string email, string password, string clientId)
        {
            using (var client = new HttpClient())
            {
                var loginReq = new LoginRequest
                {
                    Email = email,
                    Password = password,
                    ClientCode = clientId
                };
                var content = new StringContent(JsonSerializer.Serialize(loginReq), Encoding.UTF8, "application/json");
                var resp = await client.PostAsync($"{baseUrl}/api/user/login", content);
                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Login failed: {resp.StatusCode}");
                    return null;
                }
                var body = await resp.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AuthResponse>(body, jsonOptions);
            }
        }
        static async Task CallProtectedApiAsync()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var resp = await client.GetAsync($"{baseUrl}/api/client");
                if (resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine("\n[Protected API Response]:");
                    Console.WriteLine(body);
                }
                else
                {
                    Console.WriteLine($"\n[Protected API] Failed: {resp.StatusCode}");
                    var body = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine(body);
                }
            }
        }

    }
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ClientCode { get; set; } = null!;
    }
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}
