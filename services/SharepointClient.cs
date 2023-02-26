using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Eagels.MailSender.Models;
using Microsoft.Graph;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eagels.MailSender.Services;

public class SharepointClient
{
    private GraphServiceClient _graphClient;
    private readonly string _siteId;
    private readonly string _definitionListId;
    private readonly string _incomingMailListId;
    public SharepointClient(string tenantId, string applicationId, string applicationSecret, string siteId, string definitionListId, string incomingMailListId)
    {
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };
        var clientSecretCredential = new ClientSecretCredential(tenantId, applicationId, applicationSecret, options);
        _graphClient = new GraphServiceClient(clientSecretCredential);
        this._siteId = siteId;
        this._definitionListId = definitionListId;
        this._incomingMailListId = incomingMailListId;
    }

    public async Task<List<MailRequest>> GetIncomingMails(ILogger log)
    {
        GraphResponse<ListItemsCollectionResponse> all = await _graphClient.Sites[this._siteId].Lists[this._incomingMailListId].Items.Request()
        .Expand("fields")
        .Filter("fields/status eq 'Draft'").GetResponseAsync();
        ListResponse response = JsonConvert.DeserializeObject<ListResponse>(await all.Content.ReadAsStringAsync());
        return response.Value.Select(item =>
        {
            log.LogInformation(JsonConvert.SerializeObject(item.Fields));
            return MailRequest.BuildMRFromItem(item.Fields, item.Id);
        }).ToList();
    }

    public async Task UpdateMailAsSent(string id)
    {
        var fieldValueSet = new FieldValueSet
        {
            AdditionalData = new Dictionary<string, object>()
            {
                {"status", "Sent"},
            }
        };
        await _graphClient.Sites[this._siteId].Lists[this._incomingMailListId].Items[id].Fields.Request().UpdateAsync(fieldValueSet);
    }
    public async Task<MailDefinition> GetMailDefinition(string mailKey, ILogger log)
    {
        GraphResponse<ListItemsCollectionResponse> all = await _graphClient.Sites[this._siteId].Lists[this._definitionListId].Items.Request()
        .Expand("fields")
        .Filter($"fields/Title eq '{mailKey}'").GetResponseAsync();
        ListResponse response = JsonConvert.DeserializeObject<ListResponse>(await all.Content.ReadAsStringAsync());
        return response.Value.Select(item =>
        {
            log.LogInformation(JsonConvert.SerializeObject(item.Fields));
            return MailDefinition.BuildMailDefinition(item.Fields, item.Id);
        }).FirstOrDefault();
    }
}

public class ListResponse
{
    public List<ItemResponse> Value;
}
public class ItemResponse
{
    public string Id { set; get; }
    public Dictionary<string, object> Fields { set; get; }
}