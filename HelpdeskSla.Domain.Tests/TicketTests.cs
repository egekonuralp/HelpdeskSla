using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using Microsoft.Extensions.Time.Testing;
using HelpdeskSla.Domain.Tests.TestData;
using System;
using Xunit;

namespace HelpdeskSla.Domain.Tests
{
    public class TicketTests
    {
        private static Ticket CreateTicket(TicketPriority priority = TicketPriority.Medium, TimeProvider? timeProvider = null)
        {
            return new Ticket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                priority, timeProvider);
        }

        private static Ticket CreateResolvedTicket()
        {
            var ticket = CreateTicket();

            ticket.Assign(5);
            ticket.StartProgress();
            ticket.Resolve();

            return ticket;
        }

        private static Ticket CreateInProgressTicket()
        {
            var ticket = CreateTicket();

            ticket.Assign(5);
            ticket.StartProgress();

            return ticket;
        }

        public static IEnumerable<object[]> SlaTargetTestData => new List<object[]>
        {
            new object[] { TicketPriority.Low, 24 },
            new object[] { TicketPriority.Medium, 12 },
            new object[] { TicketPriority.High, 8 },
            new object[] { TicketPriority.Critical, 2 }
        };

        [Fact]
        public void Constructor_ValidData_ShouldCreateOpenTicket()
        {
            // Arrange
            string title = "İnternet Çalışmıyor";
            string description = "Modem Bağlantısı Yok";
            TicketPriority priority = TicketPriority.High;

            // Act
            var ticket = new Ticket(title, description, priority);

            // Assert
            Assert.Equal(title, ticket.Title);
            Assert.Equal(description, ticket.Description);
            Assert.Equal(priority, ticket.Priority);
            Assert.Equal(TicketStatus.Open, ticket.Status);
        }

        [Fact]
        public void Constructor_EmptyTitle_ShouldThrowArgumentException()
        {
            // Arrange 
            string title = "";
            string description = "Bilgisayar açılmıyor.";
            TicketPriority priority = TicketPriority.Low;

            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new Ticket(title, description, priority);
            });

            // Assert 
            Assert.Equal("title", exception.ParamName);
        }

        [Fact]
        public void Constructor_EmptyDescription_ShouldThrowArgumentException()
        {
            // Arrange
            string title = "Bilgisayar Açılmıyor";
            string description = "   ";
            TicketPriority priority = TicketPriority.Medium;

            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new Ticket(title, description, priority);
            });

            // Assert
            Assert.Equal("description", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        public void Constructor_InvalidTitle_ShouldThrowArgumentException(string invalidTitle)
        {
            // Arrange 
            string description = "Bilgisayar Çalışmıyor";
            TicketPriority priority = TicketPriority.Critical;

            // Act 
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new Ticket(invalidTitle, description, priority);
            });

            // Assert
            Assert.Equal("title", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        public void Constructor_InvalidDescription_ShouldThrowArgumentException(string invalidDescription)
        {
            // Arrange 
            string title = "Bilgisayar Çalışmıyor";
            TicketPriority priority = TicketPriority.Critical;

            // Act 
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                new Ticket(title, invalidDescription, priority);
            });

            // Assert
            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public void Assign_OpenTicket_ShouldAssignUser()
        {
            // Arrange 
            var ticket = new Ticket(
                "İnternet Çalışmıyor",
                "Bağlantı sağlayamıyorum.",
                TicketPriority.High);

            int userId = 5;

            // Act
            ticket.Assign(userId);

            // Assert
            Assert.Equal(userId, ticket.AssignedUserId);
            Assert.Equal(TicketStatus.Assigned, ticket.Status); 
        }

        [Fact]
        public void Assign_InvalidUserId_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet Çalışmıyor",
                "Bağlantı sağlayamıyorum.",
                TicketPriority.Medium);

            int userId = 0;

            // Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ticket.Assign(userId);
            });

            // Assert
            Assert.Equal("userId", exception.ParamName);
            Assert.Equal(TicketStatus.Open, ticket.Status);
            Assert.Null(ticket.AssignedUserId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Assign_AlreadyAssignedTicket_ShouldThrowInvalidOperationException(int invalidUserId)
        {
            // Arrange 
            var ticket = new Ticket(
                "İnternet Çalışmıyor",
                "Bağlantı sağlayamıyorum",
                TicketPriority.High);

            // Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ticket.Assign(invalidUserId);
            });

            // Assert
            Assert.Equal("userId", exception.ParamName);
            Assert.Equal(TicketStatus.Open, ticket.Status);
            Assert.Null(ticket.AssignedUserId);
        }

        [Fact]
        public void StartProgress_AssignedTicket_ShouldChangeStatusToInProgress()
        {
            // Arrange 
            var ticket = new Ticket(
                "İnternet Çalışmıyor",
                ">Bağlantı sağlayamıyorum.",
                TicketPriority.Medium);

            ticket.Assign(5);

            // Act
            ticket.StartProgress();

            // Assert
            Assert.Equal(TicketStatus.InProgress, ticket.Status);
            Assert.Equal(5, ticket.AssignedUserId);
        }

        [Fact]
        public void StartProgress_OpenTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet Çalışmıyor",
                "Bağlantı sağlayamıyorum.",
                TicketPriority.Medium);

            // Act
            Assert.Throws<InvalidOperationException>(() =>
            {
                ticket.StartProgress();
            });

            // Assert
            Assert.Equal(TicketStatus.Open, ticket.Status);
            Assert.Null(ticket.AssignedUserId);
        }

        [Fact] 
        public void WaitForCustomer_InProgressTicket_ShouldChangeStatusToPendingCustomer()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet Çalışmıyor",
                "Modem bağlantısı yok.",
                TicketPriority.Medium);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            ticket.WaitForCustomer();

            // Assert
            Assert.Equal(TicketStatus.PendingCustomer, ticket.Status);
            Assert.Equal(5, ticket.AssignedUserId);
        }

        [Fact]
        public void WaitForCustomer_AssignedTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet Çalışmıyor",
                "Modem bağlantısı yok.",
                TicketPriority.Medium);

            ticket.Assign(5);

            // Act
            Assert.Throws<InvalidOperationException>(() =>
            {
                ticket.WaitForCustomer();
            });

            // Assert 
            Assert.Equal(TicketStatus.Assigned, ticket.Status); 
            Assert.Equal(5, ticket.AssignedUserId);
        }

        [Fact]
        public void ResumeProgress_PendingCustomerTicket_ShouldChangeStatusToInProgress()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.High);

            ticket.Assign(5);
            ticket.StartProgress();
            ticket.WaitForCustomer();

            // Act
            ticket.ResumeProgress();

            // Assert
            Assert.Equal(TicketStatus.InProgress, ticket.Status);
            Assert.Equal(5, ticket.AssignedUserId);
        }

        [Fact]
        public void ResumeProgress_OpenTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.High);

            // Act
            Assert.Throws<InvalidOperationException>(() =>
            {
                ticket.ResumeProgress();
            });

            // Assert
            Assert.Equal(TicketStatus.Open, ticket.Status);
            Assert.Null(ticket.AssignedUserId);
        }

        [Fact]
        public void Resolve_InProgressTicket_ShouldChangeStatusToResolved()
        {
            // Arrange 
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.High);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            ticket.Resolve();

            // Assert
            Assert.Equal(TicketStatus.Resolved, ticket.Status);
            Assert.Equal(5, ticket.AssignedUserId);
        }

        [Fact]
        public void Resolve_AssignedTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Low);

            ticket.Assign(5);

            // Act
            Assert.Throws<InvalidOperationException>(() =>
            {
                ticket.Resolve();
            });

            // Assert
            Assert.Equal(TicketStatus.Assigned, ticket.Status);
            Assert.Equal(5, ticket.AssignedUserId);
        }

        [Fact]
        public void Close_ResolvedTicket_ShouldChangeStatusToClosed()
        {
            // Arrange 
            var ticket = CreateResolvedTicket();

            // Act
            ticket.Close();

            // Assert
            Assert.Equal(TicketStatus.Closed, ticket.Status);
            Assert.Equal(5, ticket.AssignedUserId);
        }

        [Fact] 
        public void Close_InProgressTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var ticket = CreateInProgressTicket();

            // Act
            Assert.Throws<InvalidOperationException>(() =>
            {
                ticket.Close();
            });

            // Assert
            Assert.Equal(TicketStatus.InProgress, ticket.Status);
            Assert.Equal(5, ticket.AssignedUserId);
        }

        [Fact]
        public void Constructor_ValidData_ShouldSetCreatedAt()
        {
            // Arrange
            var beforeCreation = DateTimeOffset.UtcNow;

            // Act
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Low);

            // Assert
            var afterCreation = DateTimeOffset.UtcNow;  

            Assert.InRange(ticket.CreatedAt, beforeCreation, afterCreation);
        }

        [Fact]
        public void CreatedAt_ShouldNotChangeAfterStatusTransitions()
        {
            // Arrange 
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Low);

            var createdAt = ticket.CreatedAt;

            // Act
            ticket.Assign(5);
            ticket.StartProgress();
            ticket.Resolve();
           
            // Assert
            Assert.Equal(createdAt, ticket.CreatedAt);
        }

        [Fact]
        public void Constructor_NewTicket_ShouldHaveNullResolvedAt()
        {
            // Arrange & Act
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Low);

            // Assert
            Assert.Null(ticket.ResolvedAt);
        }

        [Fact]
        public void Resolve_InProgressTicket_ShouldSetResolvedAt()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Low);

            ticket.Assign(5);
            ticket.StartProgress();

            var beforeResolve = DateTimeOffset.UtcNow;

            // Act
            ticket.Resolve();

            // Assert
            var afterResolve = DateTimeOffset.UtcNow;
            Assert.NotNull(ticket.ResolvedAt);
            Assert.InRange(ticket.ResolvedAt!.Value, beforeResolve, afterResolve);
        }

        [Fact]
        public void Resolve_AssignedTicket_ShouldNotSetResolvedAt()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Low);

            ticket.Assign(5);

            // Act
            Assert.Throws<InvalidOperationException>(() =>
            {
                ticket.Resolve();
            });

            // Assert
            Assert.Equal(TicketStatus.Assigned, ticket.Status);
            Assert.Equal(5, ticket.AssignedUserId);
            Assert.Null(ticket.ResolvedAt);
        }

        [Fact]
        public void Constructor_NewTicket_ShouldHaveNullResolutionTime()
        {
            // Arrange & Act
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.High);

            // Assert
            Assert.Null(ticket.ResolutionTime);
        }

        [Fact]
        public void Resolve_InProgressTicket_ShouldCalculateResolutionTime()
        {
            // Arrange 
            var ticket = CreateInProgressTicket();

            // Act
            ticket.Resolve();

            // Assert
            Assert.NotNull(ticket.ResolutionTime);
            Assert.Equal(ticket.ResolvedAt!.Value - ticket.CreatedAt, ticket.ResolutionTime);
        }

        [Fact]
        public void Close_ResolvedTicket_ShouldPreserveResolutionTime()
        {
            // Arrange
            var ticket = CreateResolvedTicket();

            var beforeClose = ticket.ResolutionTime;

            // Act
            ticket.Close();

            // Assert
            Assert.Equal(beforeClose, ticket.ResolutionTime);
        }

        [Fact]
        public void Constructor_CriticalPriority_ShouldHaveTwoHourSlaTarget()
        {
            // Arrange & Act
            var ticket = new Ticket(
                "Sunucu erişilemiyor",
                "Üretim ortamındaki sunucu yanıt vermiyor",
                TicketPriority.Critical);

            // Assert
            Assert.Equal(TimeSpan.FromHours(2), ticket.SlaTarget);
        }

        [Theory]
        [InlineData(TicketPriority.Low, 24)]
        [InlineData(TicketPriority.Medium, 12)]
        [InlineData(TicketPriority.High, 8)]
        [InlineData(TicketPriority.Critical, 2)]
        public void Constructor_ValidPriority_ShouldSetCorrectSlaTarget(TicketPriority priority, int expectedHours)
        {
            // Arrange & Act
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                priority);

            // Assert
            Assert.Equal(TimeSpan.FromHours(expectedHours), ticket.SlaTarget);
        }

        [Fact]
        public void Constructor_NewTicket_ShouldHaveUnknownSlaBreach()
        {
            // Arrange & Act
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Critical);

            // Assert
            Assert.Null(ticket.IsSlaBreached);
        }

        [Fact]
        public void Resolve_TicketWithinSlaTarget_ShouldNotBeBreached()
        {
            // Arrange
            var ticket = CreateInProgressTicket();

            // Act
            ticket.Resolve();

            // Assert
            Assert.NotNull(ticket.IsSlaBreached);
            Assert.False(ticket.IsSlaBreached.Value);
        }

        [Fact]
        public void Resolve_CriticalTicketAfterThreeHours_ShouldBeSlaBreached()
        {
            // Arrange 
            var startTime = new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero);

            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            fakeTime.Advance(TimeSpan.FromHours(3));
            ticket.Resolve();

            // Assert
            Assert.Equal(TimeSpan.FromHours(3), ticket.ResolutionTime);
            Assert.Equal(TimeSpan.FromHours(2), ticket.SlaTarget);
            Assert.True(ticket.IsSlaBreached == true);
        }

        [Fact]
        public void Resolve_CriticalTicketExactlyAtSlaTarget_ShouldNotBeBreached()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            fakeTime.Advance(TimeSpan.FromHours(2));
            ticket.Resolve();

            // Assert
            Assert.Equal(TimeSpan.FromHours(2), ticket.ResolutionTime);
            Assert.Equal(false, ticket.IsSlaBreached);
        }

        [Fact]
        public void IsCurrentlyOverdue_UnresolvedCriticalTicketAfterThreeHours_ShouldReturnTrue()
        {
            // Arrange 
            var startTime = new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            fakeTime.Advance(TimeSpan.FromHours(3));

            // Assert
            Assert.True(ticket.IsCurrentlyOverdue());
            Assert.Null(ticket.ResolvedAt);
        }

        [Fact]
        public void IsCurrentlyOverdue_UnresolvedCriticalTicketExactlyAtTarget_ShouldReturnFalse()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 10, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            // Act
            fakeTime.Advance(TimeSpan.FromHours(2));

            // Assert
            Assert.False(ticket.IsCurrentlyOverdue());
        }

        [Fact]
        public void Constructor_CriticalTicket_ShouldSetCorrectSlaDeadline()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            // Act
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            // Assert
            var expectedDeadline = startTime.AddHours(2);
            Assert.Equal(expectedDeadline, ticket.SlaDeadline);
        }

        [Theory]
        [InlineData(TicketPriority.Low, 24)]
        [InlineData(TicketPriority.Medium, 12)]
        [InlineData(TicketPriority.High, 8)]
        [InlineData(TicketPriority.Critical, 2)]
        public void Constructor_ValidPriority_ShouldCalculateCorrectSlaDeadline(TicketPriority priority, int expectedHours)
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            // Act
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                priority, fakeTime);

            // Assert
            var expectedDeadline = startTime.AddHours(expectedHours);
            Assert.Equal(expectedDeadline, ticket.SlaDeadline);
        }

        [Fact]
        public void RemainingSlaTime_CriticalTicketAfterOneHour_ShouldReturnOneHour()
        {
            // Arrange 
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            // Act
            fakeTime.Advance(TimeSpan.FromHours(1));

            // Assert
            Assert.Equal(TimeSpan.FromHours(1), ticket.RemainingSlaTime);
        }

        [Fact]
        public void RemainingSlaTime_CriticalTicketAfterThreeHours_ShouldReturnNegativeOneHour()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            // Act
            fakeTime.Advance(TimeSpan.FromHours(3));

            // Assert
            Assert.Equal(TimeSpan.FromHours(-1), ticket.RemainingSlaTime);
        }

        [Fact]
        public void RemainingSlaTime_CriticalTicketExactlyAtDeadline_ShouldReturnZero()
        {
            // Arrange 
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            // Act
            fakeTime.Advance(TimeSpan.FromHours(2));

            // Assert
            Assert.Equal(TimeSpan.Zero, ticket.RemainingSlaTime);
        }

        [Fact]
        public void RemainingSlaTime_ResolvedTicket_ShouldReturnNull()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            fakeTime.Advance(TimeSpan.FromHours(1));
            ticket.Resolve();

            // Assert
            Assert.Null(ticket.RemainingSlaTime);
        }

        [Fact]
        public void SlaStatus_NewCriticalTicket_ShouldBeOnTrack()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            // Act
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            // Assert
            Assert.Equal(SlaStatus.OnTrack, ticket.SlaStatus);
        }

        [Fact]
        public void SlaStatus_UnresolvedCriticalTicketAfterThreeHours_ShouldBeOverdue()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            // Act
            fakeTime.Advance(TimeSpan.FromHours(3));

            // Assert
            Assert.Equal(SlaStatus.Overdue, ticket.SlaStatus);
        }

        [Fact]
        public void SlaStatus_CriticalTicketResolvedWithinTarget_ShouldBeMet()
        {
            // Arrange 
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            fakeTime.Advance(TimeSpan.FromHours(1));
            ticket.Resolve();

            // Assert
            Assert.Equal(SlaStatus.Met, ticket.SlaStatus);
        }

        [Fact]
        public void SlaStatus_CriticalTicketResolvedAfterTarget_ShouldBeBreached()
        {
            // Arrange 
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            fakeTime.Advance(TimeSpan.FromHours(3));
            ticket.Resolve();

            // Assert
            Assert.Equal(SlaStatus.Breached, ticket.SlaStatus);
        }

        [Fact]
        public void SlaStatus_CriticalTicketResolvedExactlyAtTarget_ShouldBeMet()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            fakeTime.Advance(TimeSpan.FromHours(2));
            ticket.Resolve();

            // Assert
            Assert.Equal(SlaStatus.Met, ticket.SlaStatus);
        }

        [Fact]
        public void ChangePriority_OpenTicket_ShouldUpdatePriorityAndSlaTarget()
        {
            // Arrange
            var ticket = new Ticket(
                "Sunucu yavaş çalışıyor",
                "Uygulamada performans sorunu var",
                TicketPriority.Medium);

            // Act
            ticket.ChangePriority(TicketPriority.Critical);

            // Assert
            Assert.Equal(TicketPriority.Critical, ticket.Priority);
            Assert.Equal(TimeSpan.FromHours(2), ticket.SlaTarget);
        }

        [Fact]
        public void ChangePriority_ResolvedTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var ticket = CreateResolvedTicket();
            var priority = ticket.Priority;

            // Act
            Assert.Throws<InvalidOperationException>(() =>
            {
                ticket.ChangePriority(TicketPriority.Critical);
            });

            // Assert
            Assert.Equal(priority, ticket.Priority);
            Assert.Equal(TimeSpan.FromHours(12), ticket.SlaTarget);
        }

        [Fact]
        public void ChangePriority_InvalidPriority_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Medium);

            var originalPriority = ticket.Priority;

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() =>           
                ticket.ChangePriority((TicketPriority)999));

            // Assert
            Assert.Equal(originalPriority, ticket.Priority);
        }

        [Fact]
        public void ChangePriority_SamePriority_ShouldNotChangeTicket()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Medium);

            var createTime = ticket.CreatedAt;

            // Act
            ticket.ChangePriority(TicketPriority.Medium);

            // Assert
            Assert.Equal(TicketPriority.Medium, ticket.Priority);
            Assert.Equal(createTime, ticket.CreatedAt);
        }

        [Fact]
        public void ChangePriority_MediumToCritical_ShouldUpdateSlaDeadline()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Medium,
                fakeTime);

            // Act
            ticket.ChangePriority(TicketPriority.Critical);

            // Assert
            var expectedDeadline = startTime.AddHours(2);

            Assert.Equal(expectedDeadline, ticket.SlaDeadline);
            Assert.Equal(startTime, ticket.CreatedAt);
        }

        [Fact]
        public void ChangePriority_ClosedTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var ticket = CreateResolvedTicket();
            ticket.Close();

            var priority = ticket.Priority;

            // Act
            Assert.Throws<InvalidOperationException>(() => 
                ticket.ChangePriority(TicketPriority.Critical));

            // Assert
            Assert.Equal(priority, ticket.Priority);
        }

        [Fact]
        public void ChangePriority_ToCriticalAfterFiveHours_ShouldBecomeOverdue()
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Medium,
                fakeTime);

            // Act
            fakeTime.Advance(TimeSpan.FromHours(5));
            Assert.Equal(SlaStatus.OnTrack, ticket.SlaStatus);
            ticket.ChangePriority(TicketPriority.Critical);

            // Assert
            Assert.Equal(SlaStatus.Overdue, ticket.SlaStatus);
        }

        [Fact]
        public void UpdateTitle_OpenTicket_ShouldUpdateTitle()
        {
            // Arrange
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Üretim sunucusuna erişilemiyor",
                TicketPriority.Medium);

            var originalCreatedAt = ticket.CreatedAt;
            var originalDeadline = ticket.SlaDeadline;

            // Act
            ticket.UpdateTitle("İnternet tamamen kesildi.");

            // Assert
            Assert.Equal("İnternet tamamen kesildi.", ticket.Title);
            Assert.Equal(originalCreatedAt, ticket.CreatedAt);
            Assert.Equal(originalDeadline, ticket.SlaDeadline);
        }

        [Fact]
        public void UpdateTitle_ResolvedTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var ticket = CreateResolvedTicket();
            var title = ticket.Title;

            // Act
            Assert.Throws<InvalidOperationException>(() => 
                ticket.UpdateTitle("Yeni başlık"));

            // Assert
            Assert.Equal(title, ticket.Title);
        }

        [Fact]
        public void UpdateTitle_WhitespaceTitle_ShouldThrowArgumentException()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Medium);

            var title = ticket.Title;

            // Act
            Assert.Throws<ArgumentException>(() =>
                ticket.UpdateTitle("   "));

            // Assert
            Assert.Equal(title, ticket.Title);
        }

        [Fact]
        public void UpdateTitle_SameTitle_ShouldNotChangeTicket()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Medium);

            var title = ticket.Title;
            var create = ticket.CreatedAt;

            // Act
            ticket.UpdateTitle(ticket.Title);

            // Assert
            Assert.Equal(title, ticket.Title);
            Assert.Equal(create, ticket.CreatedAt);
        }

        [Fact]
        public void UpdateDescription_OpenTicket_ShouldUpdateDescription()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Medium);

            // Act
            ticket.UpdateDescription("Modem yeniden başlatıldı ancak sorun devam ediyor.");

            // Assert
            Assert.Equal(
                "Modem yeniden başlatıldı ancak sorun devam ediyor.",
                ticket.Description);
        }

        [Fact]
        public void UpdateDescription_WhitespaceDescription_ShouldThrowArgumentException()
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Medium);

            var desc = ticket.Description;

            // Act
            Assert.Throws<ArgumentException>(() => 
                ticket.UpdateDescription("   "));

            // Assert
            Assert.Equal(desc, ticket.Description);
        }

        [Fact]
        public void UpdateDescription_ClosedTicket_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var ticket = CreateResolvedTicket();
            var desc = ticket.Description;
            
            ticket.Close();

            // Act
            Assert.Throws<InvalidOperationException>(() =>
                ticket.UpdateDescription("Yeni açıklama"));

            // Assert
            Assert.Equal(desc, ticket.Description);
        }

        [Fact]
        public void Resolve_OpenTicket_ShouldThrowAndPreserveState()
        {
            // Arrange
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                TicketPriority.Critical);

            // Act
            Assert.Throws<InvalidOperationException>(() =>
                ticket.Resolve());

            // Assert
            Assert.Equal(TicketStatus.Open, ticket.Status);
            Assert.Null(ticket.ResolvedAt);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public void Assign_InvalidUserId_ShouldThrowAndPreserveState(int userId)
        {
            // Arrange
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Bağlantı problemi",
                TicketPriority.Medium);

            // Act
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ticket.Assign(userId));

            // Assert
            Assert.Equal(TicketStatus.Open, ticket.Status);
            Assert.Null(ticket.AssignedUserId);
        }

        [Fact]
        public void StartProgress_OpenTicket_ShouldThrowAndPreserveState()
        {
            // Arrange
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                TicketPriority.Critical);

            // Act
            Assert.Throws<InvalidOperationException>(() => 
                ticket.StartProgress());

            // Assert
            Assert.Equal(TicketStatus.Open, ticket.Status);
            Assert.Null(ticket.AssignedUserId);
        }

        [Fact]
        public void WaitForCustomer_AssignedTicket_ShouldThrowAndPreserveState()
        {
            // Arrange
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                TicketPriority.Critical);

            ticket.Assign(5);

            // Act
            Assert.Throws<InvalidOperationException>(() => 
                ticket.WaitForCustomer());

            // Assert
            Assert.Equal(TicketStatus.Assigned, ticket.Status);
            Assert.Equal(5, ticket.AssignedUserId);
        }

        [Fact]
        public void Close_InProgressTicket_ShouldThrowAndPreserveState()
        {
            // Arrange
            var ticket = CreateInProgressTicket();

            // Act
            Assert.Throws<InvalidOperationException>(() =>
                ticket.Close());

            // Assert
            Assert.Equal(TicketStatus.InProgress, ticket.Status);
            Assert.Null(ticket.ResolvedAt);
        }

        [Fact]
        public void ChangePriority_ResolvedTicket_ShouldThrowAndPreservePriority()
        {
            // Arrange
            var ticket = CreateResolvedTicket();
            var priority = TicketPriority.Medium;
            var resolvedAt = ticket.ResolvedAt;

            // Act
            Assert.Throws<InvalidOperationException>(() => 
                ticket.ChangePriority(TicketPriority.Critical));

            // Assert
            Assert.Equal(priority, ticket.Priority);
            Assert.Equal(TicketStatus.Resolved, ticket.Status);
            Assert.Equal(resolvedAt, ticket.ResolvedAt);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(100)]
        public void Constructor_InvalidPriority_ShouldThrow(int priorityValue)
        {        
            // Arrange
            var priority = (TicketPriority)priorityValue;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Ticket("Başlık", "Açıklama", priority));
        }

        [Theory]
        [MemberData(nameof(SlaTargetTestData))]
        public void SlaTarget_ShouldMatchPriority(TicketPriority priority, int expectedHours)
        {
            // Arrange
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                priority);

            // Act
            var actualTarget = ticket.SlaTarget;

            // Assert
            Assert.Equal(TimeSpan.FromHours(expectedHours), actualTarget);
        }

        [Theory]
        [ClassData(typeof(ChangePriorityTestData))]
        public void ChangePriority_ShouldUpdateSlaTarget(TicketPriority newPriority, int expectedHours)
        {
            // Arrange
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                TicketPriority.Medium);

            // Act
            ticket.ChangePriority(newPriority);
            var actualTarget = ticket.SlaTarget;

            // Assert
            Assert.Equal(newPriority, ticket.Priority);
            Assert.Equal(TimeSpan.FromHours(expectedHours), actualTarget);
        }

        [Theory]
        [ClassData(typeof(SlaBreachTestData))]
        public void IsSlaBreached_ShouldMatchResolutionTime(TimeSpan resolutionDuration, bool expectedBreach)
        {
            // Arrange
            var startTime = new DateTimeOffset(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);
            var fakeTime = new FakeTimeProvider(startTime);

            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                TicketPriority.Critical,
                fakeTime);

            ticket.Assign(5);
            ticket.StartProgress();

            // Act
            fakeTime.Advance(resolutionDuration);
            ticket.Resolve();

            // Assert
            Assert.Equal(expectedBreach, ticket.IsSlaBreached);
        }

        [Fact]
        public void Assign_ZeroUserId_ShouldThrowWithCorrectParamName()
        {
            // Arrange 
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                TicketPriority.High);

            // Act
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
                ticket.Assign(0));

            // Assert
            Assert.Equal("userId", exception.ParamName);
            Assert.Equal(TicketStatus.Open, ticket.Status);
            Assert.Null(ticket.AssignedUserId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_InvalidTitle_ShouldThrowWithCorrectParamName(string invalidTitle)
        {
            // Arrange
            var description = "Sunucuya erişilemiyor";
            var priority = TicketPriority.High;

            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                new Ticket(invalidTitle, description, priority));

            // Assert
            Assert.Equal("title", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_InvalidDescription_ShouldThrowWithCorrectParamName(string invalidDescription)
        {
            // Arrange
            var title = "Sunucu çalışmıyor";
            var priority = TicketPriority.Critical;

            // Act
            var exception = Assert.Throws<ArgumentException>(() => 
                new Ticket(title, invalidDescription, priority));

            // Assert
            Assert.Equal("description", exception.ParamName);
        }

        [Fact]
        public void UpdateTitle_EmptyTitle_ShouldThrowWithCorrectMessage()
        {
            // Arrange
            var ticket = new Ticket(
                "Sunucu çalışmıyor",
                "Sunucuya erişilemiyor",
                TicketPriority.High);

            var originalTitle = ticket.Title;

            // Act
            var exception = Assert.Throws<ArgumentException>(() =>
                ticket.UpdateTitle(""));

            // Assert
            Assert.Equal("newTitle", exception.ParamName);
            Assert.Contains("Ticket başlığı boş olamaz.", exception.Message);
            Assert.Equal(originalTitle, ticket.Title);
        }

        [Fact]
        public void CreateTicket_WithoutPriority_ShouldUseMedium()
        {
            // Arrange & Act
            var ticket = CreateTicket();

            // Assert
            Assert.Equal(TicketPriority.Medium, ticket.Priority);
        }

        [Fact]
        public void CreateTicket_WithCriticalPriority_ShouldUseCritical()
        {
            // Arrange & Act
            var ticket = CreateTicket(TicketPriority.Critical);

            // Assert
            Assert.Equal(TicketPriority.Critical, ticket.Priority);
        }
    }
}
