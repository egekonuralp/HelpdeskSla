using HelpdeskSla.Application.Common.Options;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Enums;
using Microsoft.Extensions.Options;

namespace HelpdeskSla.Domain.Tests.Common;

public class SlaPolicyTests
{
    private static SlaPolicy CreatePolicy()
    {
        var options = Options.Create(new SlaOptions
        {
            LowHours = 30,
            MediumHours = 15,
            HighHours = 10,
            CriticalHours = 3
        });

        return new SlaPolicy(options);
    }

    [Theory]
    [InlineData(TicketPriority.Low, 30)]
    [InlineData(TicketPriority.Medium, 15)]
    [InlineData(TicketPriority.High, 10)]
    [InlineData(TicketPriority.Critical, 3)]
    public void GetSlaTarget_ValidPriority_ReturnsHoursFromOptions(TicketPriority priority, int expectedHours)
    {
        // Arrange
        var policy = CreatePolicy();

        // Act
        var result = policy.GetSlaTarget(priority);

        // Assert
        Assert.Equal(TimeSpan.FromHours(expectedHours), result);
    }

    [Fact]
    public void GetSlaTarget_InvalidPriority_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var policy = CreatePolicy();
        var invalidPriority = (TicketPriority)99;

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            policy.GetSlaTarget(invalidPriority));

        // Assert
        Assert.Equal("priority", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SlaPolicy(null!));

        // Assert
        Assert.Equal("options", exception.ParamName);
    }
}