using Quartz;

namespace MyProject.Quartz;

public static class QuartzConfigurationExtensions
{
    public static IServiceCollection AddQuartzJobs(this IServiceCollection services)
    {
        // Cấu hình Quartz
        services.AddQuartz(q =>
        {
            q.SchedulerId = "MyProject-Scheduler";
            q.SchedulerName = "Quartz Scheduler";
            q.UseMicrosoftDependencyInjectionJobFactory();  // Đảm bảo sử dụng DI để tạo job
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
    
            q.AddJob<VerificationCodeCleanupJob>(opts => opts.WithIdentity("cleanup-verification-code-job")); // Đăng ký job
            q.AddTrigger(opts => opts
                .ForJob("cleanup-verification-code-job")  // Trigger cho job
                .WithIdentity("cleanup-verification-code-trigger")
                .StartNow()  // Bắt đầu ngay lập tức
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever())); // Chạy định kỳ mỗi 5 phút
        });

        // Cấu hình Quartz Hosted Service
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
