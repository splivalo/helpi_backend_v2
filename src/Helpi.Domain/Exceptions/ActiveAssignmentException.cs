namespace Helpi.Domain.Exceptions;

public class ActiveAssignmentException : Exception
{
    public ActiveAssignmentException(string message, Exception innerException)
        : base(message, innerException) { }

    public ActiveAssignmentException(string message) : base(message) { }
}