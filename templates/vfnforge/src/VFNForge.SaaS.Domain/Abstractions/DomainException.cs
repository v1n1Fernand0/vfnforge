namespace VFNForge.SaaS.Domain.Abstractions;

public sealed class DomainException : Exception
{
    public DomainException(DomainError error) : base(error.Message)
    {
        Error = error;
    }

    public DomainError Error { get; }
}
