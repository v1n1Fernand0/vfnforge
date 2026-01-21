namespace VFNForge.SaaS.Domain.Abstractions;

public static class DomainErrors
{
    public static class Tenant
    {
        public static readonly DomainError InvalidIdentifier = new("Tenant.InvalidId", "TenantId nao pode ser vazio.");
        public static readonly DomainError InvalidName = new("Tenant.InvalidName", "Nome do tenant precisa possuir pelo menos 3 caracteres.");
    }
}
