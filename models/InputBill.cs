namespace EaglesJungscharen.Azure.Mailsender.Models;
public class InputBill
{
    public required string Account { set; get; }
    public required InputAddress Creditor { set; get; }
    public required InputAddress Debitor { set; get; }
    public required string Currency { set; get; }
    public decimal? Amount { set; get; }
    public string? ReferenceNumber { set; get; }
    public string? InfoText { set; get; }
}
public class InputAddress
{
     public string? Name {set;get;}
        public string? Street {set;get;}
        public string? HouseNumber {set;get;}
        public string? PostalCode {set;get;}
        public string? Town {set;get;}
        public string? CountryCode {set;get;}
}