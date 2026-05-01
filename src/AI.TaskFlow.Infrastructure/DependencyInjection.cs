using AI.TaskFlow.Application.Interfaces;
using AI.TaskFlow.Infrastructure.Data;
using AI.TaskFlow.Infrastructure.Repositories;
using AI.TaskFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.TaskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var isLocalDbConnection = !string.IsNullOrWhiteSpace(connectionString) &&
            connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(connectionString) || (isLocalDbConnection && !OperatingSystem.IsWindows()))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("AI.TaskFlow"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
