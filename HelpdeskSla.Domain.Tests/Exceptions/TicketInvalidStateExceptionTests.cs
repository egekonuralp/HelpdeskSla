using HelpdeskSla.Domain.Exceptions;

namespace HelpdeskSla.Domain.Tests.Exceptions;

public class TicketInvalidStateExceptionTests
{
    [Fact]
    public void Constructor_ValidMessage_SetsMessage()
    {
        // Arrange
        var expectedMessage = "Ticket must be assigned before starting progress.";

        // Act
        var exception = new TicketInvalidStateException(expectedMessage);

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void TicketInvalidStateException_IsAnInvalidOperationException()
    {
        // Arrange
        var exception = new TicketInvalidStateException("Invalid ticket state.");

        // Act
        bool isInvalidOperationException = exception is InvalidOperationException;

        // Assert
        Assert.True(isInvalidOperationException);
    }
}
