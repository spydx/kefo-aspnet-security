// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Client;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World!");

const string appSettingsJson = "appsettings.json";

var configuration = new ConfigurationBuilder()
    .AddJsonFile(appSettingsJson)
    .Build();

var azureAdConfig = configuration
    .GetSection("AzureAd")
    .Get<AzureAd>();
if (azureAdConfig is null)
{
    Console.WriteLine("Unable to Get config");
    return;
}

Console.WriteLine($"TenantId: {azureAdConfig.TenantId}");

var client = new ClientSecretCredential(azureAdConfig.TenantId, azureAdConfig.ClientId, azureAdConfig.Secret);

const string scope = "api://1851a809-314c-4d44-9844-382ce9f64f85/.default";
var tokenRequestContext = new TokenRequestContext(new []{ scope });

var tokenResponse = client.GetToken(tokenRequestContext, default);

Console.WriteLine(tokenResponse.Token);

var apiClient = new HttpClient();
apiClient.SetBearerToken(tokenResponse.Token);

// const string apiUrl = "https://localhost:5001";
const string apiUrl = "https://kefo-securedapi.azurewebsites.net";

var apiResponse = await apiClient.GetAsync($"{apiUrl}/WeatherForecast");
CheckRequestStatus(apiResponse);

var content = await apiResponse.Content.ReadAsStringAsync();
var jsonDoc = JsonDocument.Parse(content).RootElement;
var jsonSerializerOptions = new JsonSerializerOptions
{
    WriteIndented = true,
};

Console.WriteLine(JsonSerializer.Serialize(jsonDoc, jsonSerializerOptions));

var apiRespTagged = await apiClient.GetAsync($"{apiUrl}/Tagged");
CheckRequestStatus(apiRespTagged);

var tagContent = await apiRespTagged.Content.ReadAsStringAsync();
Console.WriteLine(tagContent);

void CheckRequestStatus(HttpResponseMessage response)
{
    if (response.IsSuccessStatusCode) return;

    Console.WriteLine();
    Console.WriteLine($"Error contacting the api, responseCode is: {response.StatusCode}");
    Console.WriteLine("Exiting...");
    Environment.Exit(-1);
}

var apiAllTagged = await apiClient.GetAsync($"{apiUrl}/Tagged/All");
CheckRequestStatus(apiAllTagged);

var tagAllContent = await apiAllTagged.Content.ReadAsStringAsync();
Console.WriteLine(tagAllContent);

/*
var flagScopes = new string[]
{
    "/.default",
    "api://1851a809-314c-4d44-9844-382ce9f64f85/SecretFlag"
};
var flagTokenContext = new TokenRequestContext(flagScopes);

var flagTokenResp = client.GetToken(flagTokenContext, default);

Console.WriteLine(flagTokenResp.Token);
*/