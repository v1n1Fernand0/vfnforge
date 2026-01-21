namespace VFNForge.SaaS.Contracts.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Signing key usada quando Authority nao for informado (cenarios self-contained).
    /// </summary>
    public string SigningKey { get; set; } = "CHANGEME-SUPER-SECRET-KEY";

    public string Issuer { get; set; } = "vfnforge";

    public string Audience { get; set; } = "vfnforge-api";

    public string? Authority { get; set; }

    public bool RequireHttpsMetadata { get; set; } = true;

    public bool ValidateLifetime { get; set; } = true;
}
