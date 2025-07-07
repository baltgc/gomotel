using System;

namespace Gomotel.Domain.Exceptions;

/// <summary>
/// Base exception class for all domain-related exceptions
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message) { }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
