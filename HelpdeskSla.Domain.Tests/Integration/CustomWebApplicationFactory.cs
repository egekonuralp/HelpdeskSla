using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HelpdeskSla.Domain.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITicketRepository>();
            services.RemoveAll<IUnitOfWork>();

            services.AddSingleton<FakeTicketRepository>();

            services.AddScoped<ITicketRepository>(provider => provider.GetRequiredService<FakeTicketRepository>());

            services.AddScoped<IUnitOfWork, FakeUnitOfWork>();
        });
    }
}