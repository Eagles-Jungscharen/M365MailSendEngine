namespace Eagels.MailSender.Models;
public class InputBill
{
    public string Account { set; get; }
    public InputAddress Creditor { set; get; }
    public InputAddress Debitor { set; get; }
    public string Currency { set; get; }
    public decimal Amount { set; get; }
    public string ReferenceNumber { set; get; }
    public string InfoText { set; get; }
}
public class InputAddress
{
    public string Name { set; get; }
    public string AddressLine1 { set; get; }
    public string AddressLine2 { set; get; }
    public string CountryCode { set; get; }
}