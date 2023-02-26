using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Graph;

namespace Eagels.MailSender.Services;

public class MailSendClient {

    private GraphServiceClient _graphClient;

    public MailSendClient(string tenantId, string applicationId, string applicationSecret) {
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };
        var clientSecretCredential = new ClientSecretCredential(tenantId, applicationId, applicationSecret, options);
        _graphClient = new GraphServiceClient(clientSecretCredential);
    }

    public async Task SendMail(MailSendBuilder builder) {
        await _graphClient.Users[builder.GetSendFrom()].SendMail(builder.BuildMailMessage()).Request().PostAsync();
    }
}