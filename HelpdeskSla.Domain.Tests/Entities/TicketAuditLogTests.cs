using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using Microsoft.Extensions.Time.Testing;
using HelpdeskSla.Domain.Tests.TestData;
using HelpdeskSla.Domain.Tests.Builders;
using System;
using Xunit;
using System.Threading.Tasks;

namespace HelpdeskSla.Domain.Tests;

public class TicketAuditLogTests
{
    [Fact]
    public void Constructor_ValidInputs_CreatesAuditLog()
    {
        // Arrange 
        var ticketId = 15;
        var oldTitle = "Eski Başlık";
        var newTitle = "Yeni Başlık";

        // Act
        var auditLog = new TicketAuditLog(ticketId, oldTitle, newTitle);

        // Assert
        Assert.Equal(ticketId, auditLog.TicketId);
        Assert.Equal(oldTitle, auditLog.OldTitle);
        Assert.Equal(newTitle, auditLog.NewTitle);
        Assert.True(auditLog.ChangedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Constructor_InvalidTicketId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var ticketId = 0;
        var oldTitle = "Eski Başlık";
        var newTitle = "Yeni Başlık";

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
            new TicketAuditLog(ticketId, oldTitle, newTitle));

        // Assert         
        Assert.Equal("ticketId", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_InvalidTicketId_ThrowsArgumentOutOfRangeException_WithTheory(int invalidTicketId)
    {
        // Arrange
        var oldTitle = "Eski Başlık";
        var newTitle = "Yeni Başlık";

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TicketAuditLog(invalidTicketId, oldTitle, newTitle));

        // Assert         
        Assert.Equal("ticketId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyOldTitle_ThrowsArgumentException(string invalidTicketOldTitle)
    {
        // Arrange
        var ticketId = 15;
        var newTitle = "Yeni Başlık";

        // Act
        var exception = Assert.Throws<ArgumentException>(() => 
            new TicketAuditLog(ticketId, invalidTicketOldTitle, newTitle));

        // Asserrt
        Assert.Equal("oldTitle", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyNewTitle_ThrowsArgumentException(string invalidTicketnewTitle)
    {
        // Arrange
        var ticketId = 15;
        var oldTitle = "Eski Başlık";

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new TicketAuditLog(ticketId, oldTitle, invalidTicketnewTitle));

        // Asserrt
        Assert.Equal("newTitle", exception.ParamName);
    }

    [Fact]
    public void Constructor_SameTitles_ThrowsArgumentException()
    {
        // Arrange
        var ticketId = 15;
        var oldTitle = "Aynı Başlık";
        var newTitle = "Aynı Başlık";

        // Act
        var exception = Assert.Throws<ArgumentException>(() => 
            new TicketAuditLog(ticketId, oldTitle, newTitle));

        // Assert
        Assert.Equal("newTitle", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithFakeTimeProvider_SetsChangedAtCorrectly()
    {
        // Arrange
        var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(startTime);

        var ticketId = 15;
        var oldTitle = "Eski Başlık";
        var newTitle = "Yeni Başlık";

        // Act
        var auditLog = new TicketAuditLog(ticketId, oldTitle, newTitle, fakeTime);

        // Assert
        Assert.Equal(startTime, auditLog.ChangedAt);
    }
}
