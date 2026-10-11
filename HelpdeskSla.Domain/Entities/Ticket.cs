using HelpdeskSla.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public  Ticket(string title, string description, TicketPriority priority, TimeProvider? timeProvider = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Ticket Başlığı Boş Olamaz !!! ", nameof(title));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Ticket Açıklaması Boş Olamaz !!! ", nameof(description));
            }

            if (!Enum.IsDefined(priority))
            {
                throw new ArgumentOutOfRangeException(nameof(priority), "Geçersiz Ticket Önceliği");
            }

            _timeProvider = timeProvider ?? TimeProvider.System;

            Title = title;
            Description = description;
            Priority = priority;
            Status = TicketStatus.Open;         
            CreatedAt = _timeProvider.GetUtcNow();
        }

        public void Close()
        {
            if (Status != TicketStatus.Resolved)
            {
                throw new InvalidOperationException("Ticket'ın Kapatılabilmesi İçin Önce Resolved Durumunda Olması Gerekir.");
            }

            Status = TicketStatus.Closed;
        }

        public void Assign(int userId)
        {
            if (Status != TicketStatus.Open)
            {
                throw new InvalidOperationException("Yalnızca Open Durumundaki Ticket Atanabilir.");
            }

            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "Kullanıcı ID'si Sıfırdan Büyük Olmalı.");
            }

            AssignedUserId = userId;
            Status = TicketStatus.Assigned;
        }

        public void StartProgress()
        {
            if (Status != TicketStatus.Assigned)
            {
                throw new InvalidOperationException("Yalnızca Assigned Durumundaki Ticket Üzerinde Çalışmaya Başlanabilir !");
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

        public bool IsCurrentlyOverdue()
        {
            if (ResolvedAt != null)
            {
                return false;
            }

            var elapsedTime = _timeProvider.GetUtcNow() - CreatedAt;

            return elapsedTime > SlaTarget;
        }
    }
}

