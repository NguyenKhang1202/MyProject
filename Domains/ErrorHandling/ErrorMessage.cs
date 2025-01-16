using System.Text.Json.Serialization;

namespace MyProject.Domain.ErrorHandling;

public class ErrorMessage
{
    [JsonPropertyName("error_code")]
    public int Code { get; set; }

    [JsonPropertyName("error_message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public object Data { get; set; }
}