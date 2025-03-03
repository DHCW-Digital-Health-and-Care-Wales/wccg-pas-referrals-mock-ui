using System.Globalization;
using System.Text.Json;

namespace WCCG.PAS.Referrals.UI.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<HttpResponseMessage> EnsureSuccessStatusCodeWithDataAsync(this HttpResponseMessage responseMessage,
        HttpContent httpContent)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            return responseMessage;
        }

        var exception =
            new HttpRequestException(
                string.Format(CultureInfo.InvariantCulture, "Response status code does not indicate success: {0} ({1})",
                    (int)responseMessage.StatusCode,
                    responseMessage.ReasonPhrase), null, responseMessage.StatusCode);
        var content = await httpContent.ReadFromJsonAsync<JsonElement>();

        if (content.TryGetProperty("message", out var messageElement))
        {
            exception.Data.Add("Details", messageElement.GetRawText());
        }

        throw exception;
    }
}
