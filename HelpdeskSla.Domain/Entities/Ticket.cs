using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Domain.Exceptions;

namespace HelpdeskSla.Domain.Entities
{
    public class Ticket
    {
        public int Id { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public TicketPriority Priority { get; private set; }

        public TicketStatus Status { get; private set; }

        public int? AssignedUserId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? ResolvedAt { get; private set; }

        private readonly TimeProvider _timeProvider;

        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        public TimeSpan? ResolutionTime
        {
            get
            {
                if (ResolvedAt == null)
                {
                    return null;
                }

                return ResolvedAt.Value - CreatedAt;
            }
        }

        public TimeSpan SlaTarget
        {
            get
            {
                return Priority switch
                {
                    TicketPriority.Low => TimeSpan.FromHours(24),
                    TicketPriority.Medium => TimeSpan.FromHours(12),
                    TicketPriority.High => TimeSpan.FromHours(8),
                    TicketPriority.Critical => TimeSpan.FromHours(2),
                    _ => throw new ArgumentOutOfRangeException(nameof(Priority))
                };
            }
        }

        public DateTimeOffset SlaDeadline
        {
            get
            {
                return CreatedAt + SlaTarget;
            }
        }

        public TimeSpan? RemainingSlaTime
        {
            get
            {
                if (ResolvedAt != null)
                {
                    return null;
                }

                return SlaDeadline - _timeProvider.GetUtcNow();
            }
        }

        public bool? IsSlaBreached
        {
            get
            {
                if (ResolutionTime == null)
                {
                    return null;
                }

                return ResolutionTime.Value > SlaTarget;
            }
        }

        public SlaStatus SlaStatus
        {
            get
            {
                if (ResolvedAt != null)
                {
                    return IsSlaBreached == true
                        ? SlaStatus.Breached
                        : SlaStatus.Met;
                }

                return IsCurrentlyOverdue()
                    ? SlaStatus.Overdue
                    : SlaStatus.OnTrack;
            }
        }

        public Ticket(string title, string description, TicketPriority priority, TimeProvider? timeProvider = null)
        {
            EnsureNotEmpty(title, nameof(title), "Ticket title cannot be empty.");
            EnsureNotEmpty(description, nameof(description), "Ticket description cannot be empty.");

            if (!Enum.IsDefined(priority))
            {
                throw new ArgumentOutOfRangeException(nameof(priority), "Invalid ticket priority.");
            }

            _timeProvider = timeProvider ?? TimeProvider.System;

            Title = title;
            Description = description;
            Priority = priority;
            Status = TicketStatus.Open;
            CreatedAt = _timeProvider.GetUtcNow();
        }

        private Ticket()
        {
            Title = null!;
            Description = null!;
            _timeProvider = TimeProvider.System;
        }

        private static void EnsureNotEmpty(string? value, string parameterName, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(errorMessage, parameterName);
            }
        }

        private void EnsureTicketIsEditable(string errorMessage)
        {
            if (Status == TicketStatus.Resolved || Status == TicketStatus.Closed)
            {
                throw new TicketInvalidStateException(errorMessage);
            }
        }

        public void Assign(int userId)
        {
            if (Status != TicketStatus.Open)
            {
                throw new TicketInvalidStateException("Only tickets in the Open status can be assigned.");
            }

            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "The user ID must be greater than zero.");
            }

            AssignedUserId = userId;
            Status = TicketStatus.Assigned;
        }

        public void StartProgress()
        {
            if (Status != TicketStatus.Assigned)
            {
                throw new TicketInvalidStateException("Ticket must be assigned before starting progress.");
            }

            Status = TicketStatus.InProgress;
        }

        public void WaitForCustomer()
        {
            if (Status != TicketStatus.InProgress)
            {
                throw new TicketInvalidStateException("Only tickets in the InProgress status can have their customer response put on hold.");
            }

            Status = TicketStatus.PendingCustomer;
        }

        public void ResumeProgress()
        {
            if (Status != TicketStatus.PendingCustomer)
            {
                throw new TicketInvalidStateException("Only tickets in the PendingCustomer status can be continued.");
            }

            Status = TicketStatus.InProgress;
        }

        public void Resolve()
        {
            if (Status != TicketStatus.InProgress)
            {
                throw new TicketInvalidStateException("Only tickets in the InProgress status can be resolved.");
            }

            Status = TicketStatus.Resolved;
            ResolvedAt = _timeProvider.GetUtcNow();
        }

        public void Close()
        {
            if (Status != TicketStatus.Resolved)
            {
                throw new TicketInvalidStateException("The ticket must be in the \"Resolved\" status before it can be closed.");
            }

            Status = TicketStatus.Closed;
        }

        public bool IsCurrentlyOverdue()
        {
            if (ResolvedAt != null)
            {
                return false;
            }

            var elapsedTime = _timeProvider.GetUtcNow() - CreatedAt;

            return elapsedTime > SlaTarget;
        }

        public void ChangePriority(TicketPriority newPriority)
        {
            if (!Enum.IsDefined(newPriority))
            {
                throw new ArgumentOutOfRangeException(nameof(newPriority), "Invalid ticket priority.");
            }

            EnsureTicketIsEditable("The priority of a resolved or closed ticket cannot be changed.");

            if (Priority == newPriority)
            {
                return;
            }

            Priority = newPriority;
        }

        public void UpdateTitle(string newTitle)
        {
            EnsureNotEmpty(newTitle, nameof(newTitle), "Ticket title cannot be empty.");
            EnsureTicketIsEditable("The title of a resolved or closed ticket cannot be changed.");

            if (Title == newTitle)
            {
                return;
            }

            Title = newTitle;
        }

        public void UpdateDescription(string newDescription)
        {
            EnsureNotEmpty(newDescription, nameof(newDescription), "Ticket description cannot be empty");
            EnsureTicketIsEditable("The description of a resolved or closed ticket cannot be changed.");

            if (Description == newDescription)
            {
                return;
            }

            Description = newDescription;
        }
    }
}
