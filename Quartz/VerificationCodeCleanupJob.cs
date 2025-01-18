using MyProject.Repos;
using Quartz;

namespace MyProject.Quartz;

public class VerificationCodeCleanupJob(IVerificationCodeRepo verificationCodeRepo) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var currentTime = DateTime.UtcNow;
        var expiredCodes = verificationCodeRepo
            .Where(v => v.ExpiresAt <= currentTime || v.IsUsed)
            .ToList();

        if (expiredCodes.Any())
        {
            verificationCodeRepo.DeleteMany(expiredCodes);
            await verificationCodeRepo.SaveChangesAsync();
        }
    }
}