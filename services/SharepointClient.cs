using Microsoft.Graph;

namespace Eagels.MailSender.Services;

public class SharepointClient {
    private GraphServiceClient _graphClient;
    private readonly string _siteId;
    private readonly string _definitionListId;
    private readonly string _incomingMailListId;

    public SharepointClient(string tenantId, string applicationId, string applicationSecret, string siteId, string definitionListId, string incomingMailListId) {

    }
}