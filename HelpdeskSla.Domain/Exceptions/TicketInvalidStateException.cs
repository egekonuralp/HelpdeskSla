namespace HelpdeskSla.Domain.Exceptions;

public sealed class TicketInvalidStateException : InvalidOperationException
{
    public TicketInvalidStateException(string message) : base(message)
    {
        
    }
}