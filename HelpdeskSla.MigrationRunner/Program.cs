using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=HelpdeskSlaDb;Trusted_Connection=True;TrustServerCertificate=True;";

var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var context = new HelpdeskDbContext(options);

var savedTicket = context.Tickets
    .AsNoTracking()
    .OrderByDescending(t => t.Id)
    .FirstOrDefault();

if (savedTicket == null)
{
    Console.WriteLine("Veritabanında Ticket bulunamadı.");
    return;
}

Console.WriteLine($"Ticket Id: {savedTicket.Id}");
Console.WriteLine($"Başlık: {savedTicket.Title}");
Console.WriteLine($"Açıklama: {savedTicket.Description}");
Console.WriteLine($"Öncelik: {savedTicket.Priority}");
Console.WriteLine($"Durum: {savedTicket.Status}");
