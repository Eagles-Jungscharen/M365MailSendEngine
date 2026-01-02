using Eagels.MailSender.Extension;
using Microsoft.Graph.Models;

namespace EaglesJungscharen.Azure.Mailsender.Models;
public class MailDefinition
{
    public required string MailKey { set; get; }
    public string? ReplyTo { set; get; }
    public bool QrBill { set; get; }
    public string? QrName { set; get; }
    public string? QrStreet { set; get; }
    public string? QrHouseNumber { set; get; }
    public string? QrPostalCode { set; get; }
    public string? QrTown { set; get; }
    public string? QrCountryCode { set; get; }
    public string? MailText { set; get; }
    public string? MailSubject { set; get; }
    public string? IBAN {set;get;}
    public List<FileAttachment>? Attachments {set;get;}

    public static MailDefinition BuildMailDefinition(IDictionary<string, object> values, string id, List<FileAttachment> attachments)
    {

        return new MailDefinition()
        {
            MailKey = values.GetString("mailkey"),
            MailSubject = values.GetString("mailsubject"),
            MailText = values.GetString("mailtext"),
            QrBill = values.GetBool("qrbill"),
            QrCountryCode = values.GetString("qrcountrycode"),
            QrStreet = values.GetString("qrstreet"),
            QrHouseNumber = values.GetString("qrhousenumber"),
            QrPostalCode = values.GetString("qrpostalcode"),
            QrTown = values.GetString("qrtown"),
            QrName = values.GetString("qrname"),
            ReplyTo = values.GetString("replyto"),
            IBAN = values.GetString("iban"),
            Attachments = attachments
        };
    }
}
