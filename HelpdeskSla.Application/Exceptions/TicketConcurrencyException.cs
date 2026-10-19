namespace HelpdeskSla.Application.Exceptions;

public sealed class TicketConcurrencyException : Exception
{
    public TicketConcurrencyException(string message, Exception? innerException = null) : base(message, innerException)
    {

    }
}