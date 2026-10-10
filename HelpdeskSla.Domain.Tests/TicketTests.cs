using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using Xunit;

namespace HelpdeskSla.Domain.Tests
{
    public class TicketTests
    {
        private static Ticket CreateResolvedTicket()
        {
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Medium);

            ticket.Assign(5);
            ticket.StartProgress();
            ticket.Resolve();

            return ticket;
        }

        private static Ticket CreateInProgressTicket()
        {
            var ticket = new Ticket(
                "İnternet çalışmıyor",
                "Modem bağlantısı yok",
                TicketPriority.Medium);

            ticket.Assign(5);
            ticket.StartProgress();

            return ticket;
        }

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
    }
}
