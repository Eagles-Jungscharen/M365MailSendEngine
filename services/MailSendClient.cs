using Azure.Identity;
using EaglesJungscharen.Azure.MailSender.Models;
using Microsoft.Graph;
using Microsoft.Graph.Users.Item.SendMail;

namespace Eagels.MailSender.Services;

public class MailSendClient (GraphServiceClient graphClient)
{

    private GraphServiceClient _graphClient = graphClient;

    public async Task SendMail(MailSendBuilder builder)
    {
        var body = new SendMailPostRequestBody
        {
            Message = builder.BuildMailMessage(),
            SaveToSentItems = true
        };

        await _graphClient.Users[builder.GetSendFrom()].SendMail.PostAsync(body);
    }
}