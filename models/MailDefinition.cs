using System.Collections.Generic;
using Eagels.MailSender.Extension;

public class MailDefinition
{
    public string MailKey { set; get; }
    public string ReplyTo { set; get; }
    public bool QrBill { set; get; }
    public string QrName { set; get; }
    public string QrLine1 { set; get; }
    public string QrLine2 { set; get; }
    public string QrCountryCode { set; get; }
    public string MailText { set; get; }
    public string MailSubject { set; get; }
    public string IBAN {set;get;}

    public static MailDefinition BuildMailDefinition(IDictionary<string, object> values, string id)
    {

        return new MailDefinition()
        {
            MailKey = values.GetString("mailkey"),
            MailSubject = values.GetString("mailsubject"),
            MailText = values.GetString("mailtext"),
            QrBill = values.GetBool("qrbill"),
            QrCountryCode = values.GetString("qrcountrycode"),
            QrLine1 = values.GetString("qrline1"),
            QrLine2 = values.GetString("qrline2"),
            QrName = values.GetString("qrname"),
            ReplyTo = values.GetString("replyto"),
            IBAN = values.GetString("iban")
        };
    }
}

