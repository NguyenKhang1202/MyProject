namespace MyProject.Domain.Keycloaks;

public class KeycloakOptions
{
    public string Authority { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string ResponseType { get; set; } = "code";
    public bool SaveTokens { get; set; } = true;
    public bool RequireHttpsMetadata { get; set; } = true;
    public string Audience { get; set; } = default!;
}
