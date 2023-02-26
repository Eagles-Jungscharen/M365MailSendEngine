using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Eagels.MailSender.Models;
using Microsoft.Graph;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;

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
        List<MailDefinition> definitions = new List<MailDefinition>();
        foreach(var item in response.Value) {
            var attachments = await GetAttachmentsForMailDefintion(mailKey, log);
            var maildefinition = MailDefinition.BuildMailDefinition(item.Fields, item.Id,attachments);
            definitions.Add(maildefinition);
        }
        return definitions.FirstOrDefault();
    }
    private async Task<List<FileAttachment>> GetAttachmentsForMailDefintion(string mailKey,ILogger log) {
        IDriveItemChildrenCollectionPage itemsPage = await _graphClient.Sites[_siteId].Drive.Root.Children.Request().Top(5000).GetAsync();
        if (itemsPage.CurrentPage.Any(item=>item.Name == mailKey)) {
           IDriveItemChildrenCollectionPage files = await _graphClient.Sites[_siteId].Drive.Root.ItemWithPath(mailKey).Children.Request().Top(5000).GetAsync();
           List<FileAttachment> attachments = new List<FileAttachment>();
           foreach( var fileContent in files.CurrentPage)  {
                Microsoft.Graph.File file = fileContent.File;
                if (file != null) {
                    var content = await _graphClient.Sites[_siteId].Drive.Root.ItemWithPath(mailKey+"/"+fileContent.Name).Content.Request().GetAsync();
                    var memoryStream = new MemoryStream();
                    await content.CopyToAsync(memoryStream);
                    byte[] arr = memoryStream.ToArray();
                    attachments.Add(new FileAttachment(){
                        Name = fileContent.Name,
                        ContentType = file.MimeType,
                        ContentBytes = arr
                    });
                }
           }
           return attachments;
        }
        return new List<FileAttachment>();
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