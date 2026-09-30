namespace ReceptionTracker.Domain.Common;

/// <summary>
/// Thrown when a business rule is violated.
/// Kept distinct from technical exceptions so the API can map it to a client error.
/// </summary>
public class DomainException(string message) : Exception(message);