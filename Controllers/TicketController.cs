using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryApp.Models.ViewModels;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace InventoryApp.Controllers;

[Authorize]
public class TicketController : Controller
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public TicketController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Create(string? link, string? inventory)
    {
        var vm = new TicketViewModel
        {
            PageLink = link ?? Request.Headers["Referer"].ToString(),
            InventoryTitle = inventory
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // 1. Generate JSON for MVP
            var ticketData = new
            {
                reportedBy = User.Identity?.Name ?? "Unknown",
                inventory = model.InventoryTitle ?? "",
                link = model.PageLink ?? "",
                summary = model.Summary,
                priority = model.Priority,
                adminEmails = _config["ADMIN_EMAIL"] ?? "admin@example.com",
                createdAtUtc = DateTime.UtcNow.ToString("o")
            };

            string jsonString = JsonSerializer.Serialize(ticketData, new JsonSerializerOptions { WriteIndented = true });
            
            // 2. Upload to OneDrive via MS Graph API
            bool success = await UploadToOneDriveAsync(jsonString);

            if (success)
            {
                TempData["SuccessMessage"] = "Support ticket submitted successfully. Our team will review it shortly.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // In a real app we might fail, but for MVP/demo if keys are missing we simulate success or show a specific error
                string tenantId = _config["ONEDRIVE_TENANT_ID"] ?? "";
                if (string.IsNullOrEmpty(tenantId))
                {
                    // If not configured at all, simulate success for local demo if needed, 
                    // or just show the real error. Let's show the real error so it can be configured.
                    ModelState.AddModelError("", "OneDrive API is not configured. Please set ONEDRIVE_TENANT_ID, ONEDRIVE_CLIENT_ID, ONEDRIVE_CLIENT_SECRET, ONEDRIVE_USER_ID.");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to connect to OneDrive API. Please check configuration.");
                }
                return View(model);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error: {ex.Message}");
            return View(model);
        }
    }

    private async Task<bool> UploadToOneDriveAsync(string jsonContent)
    {
        string tenantId = _config["ONEDRIVE_TENANT_ID"] ?? "";
        string clientId = _config["ONEDRIVE_CLIENT_ID"] ?? "";
        string clientSecret = _config["ONEDRIVE_CLIENT_SECRET"] ?? "";
        string userId = _config["ONEDRIVE_USER_ID"] ?? ""; // Object ID of the user whose Drive we use

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(userId))
        {
            return false; // Missing config
        }

        using var client = _httpClientFactory.CreateClient();

        // A. Get Access Token (Client Credentials Flow)
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token");
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "scope", "https://graph.microsoft.com/.default" },
            { "client_secret", clientSecret },
            { "grant_type", "client_credentials" }
        });

        var tokenResponse = await client.SendAsync(tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            string err = await tokenResponse.Content.ReadAsStringAsync();
            throw new Exception($"Azure AD Token Error: {tokenResponse.StatusCode} - {err}");
        }

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(tokenJson);
        string accessToken = doc.RootElement.GetProperty("access_token").GetString() ?? "";

        // B. Upload File to OneDrive
        string fileName = $"ticket_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 4)}.json";
        
        // This uploads to the root of the specified user's OneDrive, into a folder named "SupportTickets"
        string uploadUrl = $"https://graph.microsoft.com/v1.0/users/{userId}/drive/root:/SupportTickets/{fileName}:/content";

        var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
        uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        uploadRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var uploadResponse = await client.SendAsync(uploadRequest);
        if (!uploadResponse.IsSuccessStatusCode)
        {
            string err = await uploadResponse.Content.ReadAsStringAsync();
            throw new Exception($"Graph API Upload Error: {uploadResponse.StatusCode} - {err}");
        }
        
        return true;
    }
}
