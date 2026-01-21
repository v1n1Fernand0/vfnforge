namespace VFNForge.SaaS.Domain.Common;

public sealed class DomainException : Exception
{
    public DomainException(DomainError error) : base(error.Message)
    {
        Error = error;
    }

    public DomainError Error { get; }
}
