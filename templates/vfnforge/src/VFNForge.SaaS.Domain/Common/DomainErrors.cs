namespace VFNForge.SaaS.Domain.Common;

public static class DomainErrors
{
    public static class Tenant
    {
        public static readonly DomainError InvalidIdentifier = new("Tenant.InvalidId", "TenantId nao pode ser vazio.");
        public static readonly DomainError InvalidName = new("Tenant.InvalidName", "Nome do tenant precisa possuir pelo menos 3 caracteres.");
        public static readonly DomainError AlreadyExists = new("Tenant.AlreadyExists", "Ja existe um tenant com este identificador.");
        public static readonly DomainError NotFound = new("Tenant.NotFound", "Tenant nao encontrado.");
        public static readonly DomainError Inactive = new("Tenant.Inactive", "Tenant desativado.");
    }
}
