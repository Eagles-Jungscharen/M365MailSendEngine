
using Azure.Identity;
using Eagels.MailSender.Services;
using EaglesJungscharen.Azure.Mailsender.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

string tenantId = System.Environment.GetEnvironmentVariable("tenantId", System.EnvironmentVariableTarget.Process);
string applicationId = System.Environment.GetEnvironmentVariable("applicationId", System.EnvironmentVariableTarget.Process);
string applicationSecret = System.Environment.GetEnvironmentVariable("applicationSecret", System.EnvironmentVariableTarget.Process);
string siteId = System.Environment.GetEnvironmentVariable("siteId", System.EnvironmentVariableTarget.Process);
string definitionListId = System.Environment.GetEnvironmentVariable("definitionListId", System.EnvironmentVariableTarget.Process);
string incomingMailListId = System.Environment.GetEnvironmentVariable("incomingMailListId", System.EnvironmentVariableTarget.Process);
string qrCodeUrl = System.Environment.GetEnvironmentVariable("qrCodeUrl", System.EnvironmentVariableTarget.Process);
string qrCodeSecret = System.Environment.GetEnvironmentVariable("qrCodeSecret", System.EnvironmentVariableTarget.Process);


builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<GraphServiceClient>(config =>
{
    var options = new TokenCredentialOptions
    {
        AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
    };
    var clientSecretCredential = new ClientSecretCredential(tenantId, applicationId, applicationSecret, options);
    return new GraphServiceClient(clientSecretCredential);
});

builder.Services.AddSingleton<SharepointClient>(config =>
{
    return new SharepointClient(tenantId, applicationId, applicationSecret, siteId, definitionListId, incomingMailListId);
});
builder.Services.AddSingleton<MailSendClient>();
builder.Services.AddHttpClient<QrCodeClient>(client =>
    {
        client.BaseAddress = new Uri(qrCodeUrl);
        client.DefaultRequestHeaders.Add("x-functions-key", qrCodeSecret);
    }).ConfigureHttpClient(config => new HttpClientHandler
    {
        UseCookies = false
    });

builder.Build().Run();
