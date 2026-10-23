using Microsoft.Extensions.DependencyInjection;

namespace JobBoard.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddScoped<IJobBoardService, JobBoardService>();
        return services;
    }
}
