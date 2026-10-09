namespace N3.Backend.Domain.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist. Returned as 404 Not Found.
/// </summary>
public class NotFoundException(string resource, object key)
    : Exception($"{resource} with id '{key}' was not found.");
