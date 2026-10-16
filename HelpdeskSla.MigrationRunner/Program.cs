using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=HelpdeskSlaDb;Trusted_Connection=True;TrustServerCertificate=True;";

var services = new ServiceCollection();

services.AddDbContext<HelpdeskDbContext>(options => options.UseSqlServer(connectionString));

services.AddScoped<ITicketRepository, EfTicketRepository>();

services.AddScoped<TicketService>();

using var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();

var ticketService = scope.ServiceProvider.GetRequiredService<TicketService>();

var ticket = ticketService.CreateTicket(
    "DI üzerinden oluşturulan Ticket",
    "TicketService ve EfTicketRepository birlikte çalışıyor.",
    TicketPriority.High);

Console.WriteLine($"Ticket kaydedildi! Id: {ticket.Id}");