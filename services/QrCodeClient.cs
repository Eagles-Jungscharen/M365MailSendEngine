using System.Text;
using System.Text.Json;
using EaglesJungscharen.Azure.Mailsender.Models;

namespace EaglesJungscharen.Azure.Mailsender.Services;

public class QrCodeClient(HttpClient client) {
    private readonly HttpClient _client = client;
    
    public async Task<byte[]> GetQRCode(InputBill inputBill) {
        var payload = JsonSerializer.Serialize(inputBill);
        HttpContent content = new StringContent(payload,Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("api/GenerateQRBill?png=1", content);
        return await response.Content.ReadAsByteArrayAsync();;
    }

}