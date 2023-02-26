using System.Collections.Generic;
using Eagels.MailSender.Extension;

namespace Eagels.MailSender.Models;
public class MailRequest
{
    public string MailKey { set; get; }
    public string EMail { set; get; }
    public string FirstName { set; get; }
    public string LastName { set; get; }
    public string AddressLine1 { set; get; }
    public string AddressLine2 { set; get; }
    public string AdditionalInfos { set; get; }
    public string CountryCode { set; get; }
    public string Status { set; get; }
    public string Id { set; get; }
    public decimal Amount { set; get; }
    public string Currency { set; get; }
    public string InfoText { set; get; }

    public static MailRequest BuildMRFromItem(Dictionary<string, object> values, string id)
    {
        return new MailRequest()
        {
            Id = id,
            MailKey = values.GetString("Title"),
            AddressLine1 = values.GetString("addressline1"),
            AddressLine2 = values.GetString("addressline2"),
            AdditionalInfos = values.GetString("additinonalinfos"),
            FirstName = values.GetString("firstname"),
            LastName = values.GetString("lastname"),
            EMail = values.GetString("email"),
            CountryCode = values.GetString("countrycode"),
            Status = values.GetString("status"),
            Amount = values.GetDecimal("amount"),
            Currency = values.GetString("currency"),
            InfoText = values.GetString("infotext")
        };
    }
}