using HelpdeskSla.Domain.Enums;

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
            EnsureNotEmpty(title, nameof(title), "Ticket başlığı boş olamaz.");
            EnsureNotEmpty(description, nameof(description), "Ticket açıklaması boş olamaz.");

            if (!Enum.IsDefined(priority))
            {
                throw new ArgumentOutOfRangeException(nameof(priority), "Geçersiz Ticket önceliği.");
            }

            _timeProvider = timeProvider ?? TimeProvider.System;

            Title = title;
            Description = description;
            Priority = priority;
            Status = TicketStatus.Open;
            CreatedAt = _timeProvider.GetUtcNow();
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
                throw new InvalidOperationException(errorMessage);
            }
        }

        public void Assign(int userId)
        {
            if (Status != TicketStatus.Open)
            {
                throw new InvalidOperationException("Yalnızca Open durumundaki Ticket atanabilir.");
            }

            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "Kullanıcı ID'si sıfırdan büyük olmalı.");
            }

            AssignedUserId = userId;
            Status = TicketStatus.Assigned;
        }

        public void StartProgress()
        {
            if (Status != TicketStatus.Assigned)
            {
                throw new InvalidOperationException("Yalnızca Assigned durumundaki Ticket üzerinde çalışmaya başlanabilir.");
            }

            Status = TicketStatus.InProgress;
        }

        public void WaitForCustomer()
        {
            if (Status != TicketStatus.InProgress)
            {
                throw new InvalidOperationException("Yalnızca InProgress durumundaki Ticket müşteri yanıtı beklemeye alınabilir.");
            }

            Status = TicketStatus.PendingCustomer;
        }

        public void ResumeProgress()
        {
            if (Status != TicketStatus.PendingCustomer)
            {
                throw new InvalidOperationException("Yalnızca PendingCustomer durumundaki Ticket devam ettirilebilir.");
            }

            Status = TicketStatus.InProgress;
        }

        public void Resolve()
        {
            if (Status != TicketStatus.InProgress)
            {
                throw new InvalidOperationException("Yalnızca InProgress durumundaki Ticket çözülebilir.");
            }

            Status = TicketStatus.Resolved;
            ResolvedAt = _timeProvider.GetUtcNow();
        }

        public void Close()
        {
            if (Status != TicketStatus.Resolved)
            {
                throw new InvalidOperationException("Ticket'ın kapatılabilmesi için önce Resolved durumunda olması gerekir.");
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
                throw new ArgumentOutOfRangeException(nameof(newPriority), "Geçersiz Ticket önceliği.");
            }

            EnsureTicketIsEditable("Çözülmüş veya kapatılmış Ticket'ın önceliği değiştirilemez.");

            if (Priority == newPriority)
            {
                return;
            }

            Priority = newPriority;
        }

        public void UpdateTitle(string newTitle)
        {
            EnsureNotEmpty(newTitle, nameof(newTitle), "Ticket başlığı boş olamaz.");
            EnsureTicketIsEditable("Çözülmüş veya kapatılmış Ticket'ın başlığı değiştirilemez.");

            if (Title == newTitle)
            {
                return;
            }

            Title = newTitle;
        }

        public void UpdateDescription(string newDescription)
        {
            EnsureNotEmpty(newDescription, nameof(newDescription), "Ticket açıklaması boş olamaz.");
            EnsureTicketIsEditable("Çözülmüş veya kapatılmış Ticket'ın açıklaması değiştirilemez.");

            if (Description == newDescription)
            {
                return;
            }

            Description = newDescription;
        }
    }
}
