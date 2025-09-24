
namespace Helpi.Domain.Exceptions;
public class MatchingException : DomainException
{
    public MatchingException(string message) : base(message) { }
    public MatchingException(string message, Exception innerException) : base(message, innerException) { }
}