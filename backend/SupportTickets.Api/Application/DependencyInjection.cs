using SupportTickets.Api.Application.Services;

namespace SupportTickets.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
