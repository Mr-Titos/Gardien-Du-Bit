using IIABlazorWebAssembly;
using IIABlazorWebAssembly.Models;
using IIABlazorWebAssembly.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var downStreamAPIConfig = builder.Configuration.GetSection("DownstreamAPI").Get<DownStreamAPIConfig>();
GeneralConfiguration generalConfig = builder.Configuration
    .GetSection("GeneralConfiguration")
    .Get<GeneralConfiguration>() ?? throw new Exception("Error with the configuration appsettings.json");



builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(downStreamAPIConfig!.BaseAddress!) });

builder.Services.AddMsalAuthentication<RemoteAuthenticationState, CustomUserAccount>(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(downStreamAPIConfig!.Scopes!);
    options.UserOptions.RoleClaim = "appRole";
}).AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, CustomUserAccount, CustomAccountFactoryService>();

builder.Services.AddScoped<APIService>();
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<VaultService>();
builder.Services.AddScoped<MudAlertService>();
builder.Services.AddSingleton<GeneralConfiguration>(generalConfig);

await builder.Build().RunAsync();