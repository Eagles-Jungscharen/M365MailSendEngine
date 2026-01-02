using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Extensions.Logging;
using EaglesJungscharen.Azure.Mailsender.Models;
using Microsoft.Graph.Models;

namespace EaglesJungscharen.Azure.Mailsender.Services;

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
        var response = await _graphClient.Sites[this._siteId].Lists[this._incomingMailListId].Items.GetAsync(config =>
        {
            config.QueryParameters.Expand = new[] { "fields" };
            config.QueryParameters.Filter = "fields/status eq 'Draft'";
        });
        var items = response?.Value ?? [];
        return [.. items.Select(item =>
        {
            return MailRequest.BuildMRFromItem(item.Fields?.AdditionalData ?? new Dictionary<string,object>(), item.Id!);
        })];
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
        await _graphClient.Sites[this._siteId].Lists[this._incomingMailListId].Items[id].Fields.PatchAsync(fieldValueSet);
    }
    public async Task<MailDefinition?> GetMailDefinition(string mailKey, ILogger log)
    {
        var response = await _graphClient.Sites[this._siteId].Lists[this._definitionListId].Items.GetAsync(config =>
        {
            config.QueryParameters.Expand = new[] { "fields" };
            config.QueryParameters.Filter = $"fields/Title eq '{mailKey}'";
        });
        var items = response?.Value ?? [];

        List<MailDefinition> definitions = [];
        foreach (var item in items)
        {
            var attachments = await GetAttachmentsForMailDefintion(mailKey, log);
            var maildefinition = MailDefinition.BuildMailDefinition(item.Fields?.AdditionalData ?? new Dictionary<string, object>(), item.Id!, attachments);
            definitions.Add(maildefinition);
        }
        return definitions.FirstOrDefault();
    }
    private async Task<List<FileAttachment>> GetAttachmentsForMailDefintion(string mailKey, ILogger log)
    {
        var drive = await _graphClient.Sites[_siteId].Drive.GetAsync();
        if (drive is null)
        {
            return [];
        }
        var driveItem = await _graphClient.Drives[drive.Id].Root.ItemWithPath(mailKey).GetAsync();
        if (driveItem is null)
        {
            return [];
        }
        var children = await _graphClient.Drives[drive.Id].Items[driveItem.Id].Children.GetAsync();
        List<FileAttachment> attachments = new List<FileAttachment>();
        foreach (var fileContent in children?.Value ?? [])
        {
            var file = fileContent.File;
            if (file is not null)
            {
                var content = await _graphClient.Drives[drive.Id].Items[fileContent.Id].Content.GetAsync();
                if (content is not null)
                {
                    var memoryStream = new MemoryStream();
                    await content.CopyToAsync(memoryStream);
                    byte[] arr = memoryStream.ToArray();
                    attachments.Add(new FileAttachment()
                    {
                        Name = fileContent.Name,
                        ContentType = file.MimeType,
                        ContentBytes = arr
                    });
                }
            }
        }
        return attachments;

    }
}