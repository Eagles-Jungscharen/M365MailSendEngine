using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Eagels.MailSender.Models;
using Newtonsoft.Json;

namespace Eagels.MailSender.Services;

public class QrCodeClient {
    private readonly HttpClient _client;
    public QrCodeClient (HttpClient client) {
        _client = client;
    }

    public async Task<byte[]> GetQRCode(InputBill inputBill) {
        HttpContent content = new StringContent(JsonConvert.SerializeObject(inputBill),Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("api/GenerateQRBill?png=1", content);
        return await response.Content.ReadAsByteArrayAsync();;
    }

}