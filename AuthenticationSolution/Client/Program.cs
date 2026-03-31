// Proper Client ID, Secret, and Base URL of the API
using Client;

var clientCode = "Client1";
var secretKey = "Secret@123";
var baseUrl = "https://localhost:7131";
var client = new HttpClient
{
    // Default timeout for HttpClient in .NET is 100 seconds;
    // override it here to 5 Minutes
    Timeout = TimeSpan.FromMinutes(5)
};
try
{
    // Create (POST Request)
    var projectDto = new
    {
        ProjectName = "ProjectC",
        StartDate  = "2026-04-01",
        EendDate = "2026-12-31",
        ClientCode = "Client1"
    };
    var response = await HMACHelper.SendRequestAsync(client, HttpMethod.Post, baseUrl, "/api/project", clientCode, secretKey, projectDto);
    if (response.IsSuccessStatusCode)
    {
        // Log success for POST request
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine("POST Response: Project Created Successfully");
        Console.WriteLine($"Response Content: {responseContent}");
    }
    else
    {
        // Log error details for POST request
        Console.WriteLine($"POST Error: {response.StatusCode} - {response.ReasonPhrase}");
    }
    // Get All Projects (GET Request)
    response = await HMACHelper.SendRequestAsync(client, HttpMethod.Get, baseUrl, "/api/project", clientCode, secretKey);
    if (response.IsSuccessStatusCode)
    {
        // Log success for GET all employees request
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine("\nGET Response: Projects Retrieved Successfully");
        Console.WriteLine($"Response Content: {responseContent}");
    }
    else
    {
        // Log error details for GET all employees request
        Console.WriteLine($"GET Error: {response.StatusCode} - {response.ReasonPhrase}");
    }
    
}
catch (Exception ex)
{
    // Log any unexpected exceptions
    Console.WriteLine($"Unexpected Error: {ex.Message}");
}
Console.ReadKey();
        
