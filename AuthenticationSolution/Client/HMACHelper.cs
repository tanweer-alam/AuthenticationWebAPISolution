using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class HMACHelper
    {
        // Method to generate HMAC token
        public static string GenerateHmacToken(string method, string path, string clientId, string secretKey, string requestBody = "")
        {
            // Generate a unique nonce
            var nonce = Guid.NewGuid().ToString();
            // Get the current UTC timestamp as a Unix timestamp
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            // Build the request content by concatenating method, path, nonce, and timestamp
            var requestContent = new StringBuilder()
                .Append(method.ToUpper())
                .Append(path.ToUpper())
                .Append(nonce)
                .Append(timestamp);
            // If the HTTP method is POST or PUT, append the request body to the request content
            if (method == HttpMethod.Post.Method || method == HttpMethod.Put.Method)
            {
                requestContent.Append(requestBody);
            }
            // Convert secret key and request content to bytes
            var secretBytes = Encoding.UTF8.GetBytes(secretKey);
            var requestBytes = Encoding.UTF8.GetBytes(requestContent.ToString());
            // Create HMACSHA512instance with the secret key
            using var hmac = new HMACSHA512(secretBytes);
            // Compute the hash of the request content
            var computedHash = hmac.ComputeHash(requestBytes);
            // Convert the computed hash to base64 string (token)
            var computedToken = Convert.ToBase64String(computedHash);
            // Concatenate clientId, computedToken, nonce, and timestamp to form the final token
            return $"{clientId}|{computedToken}|{nonce}|{timestamp}";
        }

        // Helper method to send an API request and returns HttpResponseMessage
        public static async Task<HttpResponseMessage> SendRequestAsync(
            HttpClient client,    // HttpClient instance
            HttpMethod method,    // HTTP method (GET, POST, PUT, DELETE)
            string baseUrl,       // Base URL of the API
            string endpoint,      // API endpoint
            string clientId,      // Client identifier for HMAC
            string secretKey,     // Secret key for HMAC
            object? data = null)  // Optional Data
        {
            // Serialize data to JSON if provided
            var requestBody = data != null ? System.Text.Json.JsonSerializer.Serialize(data) : string.Empty;
            // Generate HMAC token for authentication
            var token = HMACHelper.GenerateHmacToken(method.Method, endpoint, clientId, secretKey, requestBody);
            // Construct the HTTP request
            var requestMessage = new HttpRequestMessage(method, $"{baseUrl}{endpoint}")
            {
                // Add request body if applicable (e.g., POST, PUT)
                Content = !string.IsNullOrEmpty(requestBody)
                    ? new StringContent(requestBody, Encoding.UTF8, "application/json")
                    : null
            };
            // Add Authorization header with HMAC token
            requestMessage.Headers.Add("Authorization", $"HMAC {token}");
            // Send the request and return the response
            return await client.SendAsync(requestMessage);
        }
    }
}
