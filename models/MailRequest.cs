using Eagels.MailSender.Extension;

namespace EaglesJungscharen.Azure.Mailsender.Models;
public class MailRequest
{
    public required string MailKey { set; get; }
    public required string EMail { set; get; }
    public string? FirstName { set; get; }
    public string? LastName { set; get; }
    public string? StreetAndHouseNumber { set; get; }
    public string? PostalCode { set; get; }
    public string? Town { set; get; }
    public string? AdditionalInfos { set; get; }
    public string? CountryCode { set; get; }
    public string? Status { set; get; }
    public required string Id { set; get; }
    public decimal? Amount { set; get; }
    public string? Currency { set; get; }
    public string? InfoText { set; get; }

    public static MailRequest BuildMRFromItem(IDictionary<string, object> values, string id)
    {
        return new MailRequest()
        {
            Id = id,
            MailKey = values.GetString("Title"),
            StreetAndHouseNumber = values.GetString("street"),
            PostalCode = values.GetString("postalcode"),
            Town = values.GetString("town"),
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