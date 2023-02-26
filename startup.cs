using System;
using System.Net.Http;
using Eagels.MailSender.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;


[assembly: FunctionsStartup(typeof(Eagels.MailSender.Startup))]

namespace Eagels.MailSender;
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    { 
        string tenantId = System.Environment.GetEnvironmentVariable("tenantId", System.EnvironmentVariableTarget.Process);
        string applicationId = System.Environment.GetEnvironmentVariable("applicationId", System.EnvironmentVariableTarget.Process);
        string applicationSecret = System.Environment.GetEnvironmentVariable("applicationSecret", System.EnvironmentVariableTarget.Process);
        string siteId = System.Environment.GetEnvironmentVariable("siteId", System.EnvironmentVariableTarget.Process);
        string definitionListId = System.Environment.GetEnvironmentVariable("definitionListId", System.EnvironmentVariableTarget.Process);
        string incomingMailListId = System.Environment.GetEnvironmentVariable("incomingMailListId", System.EnvironmentVariableTarget.Process);
        string qrCodeUrl = System.Environment.GetEnvironmentVariable("qrCodeUrl", System.EnvironmentVariableTarget.Process);
        string qrCodeSecret = System.Environment.GetEnvironmentVariable("qrCodeSecret", System.EnvironmentVariableTarget.Process);


        builder.Services.AddSingleton<SharepointClient>(config=>{
            return new SharepointClient(tenantId,applicationId,applicationSecret,siteId,definitionListId,incomingMailListId);
        });
        builder.Services.AddSingleton<MailSendClient>(config=>{
            return new MailSendClient(tenantId,applicationId,applicationSecret);
        });
        builder.Services.AddHttpClient<QrCodeClient>(client =>
            {
                client.BaseAddress = new Uri(qrCodeUrl);
                client.DefaultRequestHeaders.Add("x-functions-key", qrCodeSecret);
            }).ConfigureHttpClient(config => new HttpClientHandler
            {
                UseCookies = false
            }); ;
    }
}