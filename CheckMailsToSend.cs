using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eagels.MailSender;
using Eagels.MailSender.Models;
using Eagels.MailSender.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Eagles.MailSender;

public class CheckMailsToSend
{
    private readonly SharepointClient _sharepointClient;
    private readonly QrCodeClient _qrCodeClient;
    private readonly MailSendClient _mailSendClient;

    public CheckMailsToSend(SharepointClient sharepointClient, QrCodeClient qrCodeClient, MailSendClient mailSendClient)
    {
        _sharepointClient = sharepointClient;
        _qrCodeClient = qrCodeClient;
        _mailSendClient = mailSendClient;
    }
    [FunctionName("CheckMailsToSend")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
    {
        log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        Dictionary<string,MailDefinition> definitions = new Dictionary<string, MailDefinition>();
        try
        {
            List<MailRequest> requests = await _sharepointClient.GetIncomingMails(log);
            foreach (MailRequest request in requests)
            {
                if (!definitions.ContainsKey(request.MailKey)) {
                    MailDefinition md = await _sharepointClient.GetMailDefinition(request.MailKey,log);
                    if (md !=null) {
                        definitions.Add(request.MailKey, md);
                    }
                }
                MailDefinition definition = null;
                if (definitions.TryGetValue(request.MailKey, out definition)) {
                    MailSendBuilder builder = new MailSendBuilder(definition.ReplyTo);
                    builder.AddContent(definition.MailText).AddSubject(definition.MailSubject).AddSendTo(request.EMail);
                    if(definition.QrBill) {
                        byte[] qrCode = await _qrCodeClient.GetQRCode(new InputBill(){
                            Account = definition.IBAN,
                            Currency = request.Currency,
                            Amount = request.Amount,
                            InfoText = request.InfoText,
                            Creditor = new InputAddress() {
                                AddressLine1 = definition.QrLine1,
                                AddressLine2 = definition.QrLine2,
                                Name = definition.QrName,
                                CountryCode = definition.QrCountryCode
                            },
                            Debitor = new InputAddress() {
                                AddressLine1 = request.AddressLine1,
                                AddressLine2 = request.AddressLine2,
                                Name = request.FirstName +" "+request.LastName,
                                CountryCode = request.CountryCode
                            }
                        });
                        builder.AddQRCode(qrCode);
                    }
                    definition.Attachments.ForEach(att=>builder.AddAttachment(att));
                    ProcessPlaceHolder(builder,request);
                    await _mailSendClient.SendMail(builder);
                    await _sharepointClient.UpdateMailAsSent(request.Id);
                }
            }
        }
        catch (Exception e)
        {
            log.LogError(e, "Error in MailHandling");
        }
    }
    private void ProcessPlaceHolder(MailSendBuilder builder, MailRequest request) {
        builder.ReplacePlaceHolders("{firstname}", request.FirstName);
        builder.ReplacePlaceHolders("{lastname}", request.LastName);
        builder.ReplacePlaceHolders("{additionalinfos}", request.AdditionalInfos);
        builder.ReplacePlaceHolders("{amount}", request.Amount.ToString("N2"));
        builder.ReplacePlaceHolders("{currency}", request.Currency);
        builder.ReplacePlaceHolders("{addressline1}", request.AddressLine1);
        builder.ReplacePlaceHolders("{addressline2}", request.AddressLine2);
        builder.ReplacePlaceHolders("{infotext}", request.InfoText);
    }
}

