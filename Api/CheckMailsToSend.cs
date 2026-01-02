
using Eagels.MailSender;
using Eagels.MailSender.Services;
using EaglesJungscharen.Azure.Mailsender.Models;
using EaglesJungscharen.Azure.Mailsender.Services;
using EaglesJungscharen.Azure.MailSender.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EaglesJungscharen.Azure.Mailsender.Api;

public class CheckMailsToSend(ILogger<CheckMailsToSend> logger, SharepointClient sharepointClient, QrCodeClient qrCodeClient, MailSendClient mailSendClient)
{
    private readonly SharepointClient _sharepointClient = sharepointClient;
    private readonly QrCodeClient _qrCodeClient = qrCodeClient;
    private readonly MailSendClient _mailSendClient = mailSendClient;
    private readonly ILogger<CheckMailsToSend> _logger = logger;

    [Function("CheckMailsToSend")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        Dictionary<string,MailDefinition> definitions = new Dictionary<string, MailDefinition>();
        try
        {
            List<MailRequest> requests = await _sharepointClient.GetIncomingMails(_logger);
            foreach (MailRequest request in requests)
            {
                if (!definitions.ContainsKey(request.MailKey)) {
                    MailDefinition md = await _sharepointClient.GetMailDefinition(request.MailKey,_logger);
                    if (md !=null) {
                        definitions.Add(request.MailKey, md);
                    }
                }
                if (definitions.TryGetValue(request.MailKey, out MailDefinition? definition))
                {
                    MailSendBuilder builder = new(definition.ReplyTo);
                    builder.AddContent(definition.MailText).AddSubject(definition.MailSubject).AddSendTo(request.EMail);
                    if (definition.QrBill)
                    {
                        byte[] qrCode = await _qrCodeClient.GetQRCode(new InputBill()
                        {
                            Account = definition.IBAN,
                            Currency = request.Currency,
                            Amount = request.Amount,
                            InfoText = request.InfoText,
                            Creditor = new InputAddress()
                            {
                                Street = definition.QrStreet,
                                HouseNumber = definition.QrHouseNumber,
                                PostalCode = definition.QrPostalCode,
                                Town = definition.QrPostalCode,
                                Name = definition.QrName,
                                CountryCode = definition.QrCountryCode
                            },
                            Debitor = new InputAddress()
                            {
                                Street = request.StreetAndHouseNumber,
                                PostalCode = request.PostalCode,
                                Town = request.Town,
                                Name = request.FirstName + " " + request.LastName,
                                CountryCode = request.CountryCode
                            }
                        });
                        builder.AddQRCode(qrCode);
                    }
                    definition.Attachments.ForEach(att => builder.AddAttachment(att));
                    ProcessPlaceHolder(builder, request);
                    await _mailSendClient.SendMail(builder);
                    await _sharepointClient.UpdateMailAsSent(request.Id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in MailHandling");
        }
    }
    private void ProcessPlaceHolder(MailSendBuilder builder, MailRequest request) {
        builder.ReplacePlaceHolders("{firstname}", request.FirstName);
        builder.ReplacePlaceHolders("{lastname}", request.LastName);
        builder.ReplacePlaceHolders("{additionalinfos}", request.AdditionalInfos);
        builder.ReplacePlaceHolders("{amount}", request.Amount?.ToString("N2"));
        builder.ReplacePlaceHolders("{currency}", request.Currency);
        builder.ReplacePlaceHolders("{address}", request.StreetAndHouseNumber);
        builder.ReplacePlaceHolders("{postalCode}", request.PostalCode);
        builder.ReplacePlaceHolders("{town}", request.Town);
        builder.ReplacePlaceHolders("{infotext}", request.InfoText);
    }
}

