using Serilog;

namespace MyProject.Helpers;

public class RepoHelper
{
    public static async Task<T> TryCatchAsync<T>(string repoMethod, RepoHelper.TryCatchDelegateAsync<T> del)
    {
        T obj;
        try
        {
            obj = await del();
        }
        catch (Exception ex)
        {
            string messageTemplate = "Exception => " + repoMethod;
            Log.Fatal(ex, messageTemplate);
            throw;
        }
        return obj;
    }

    public delegate Task<T> TryCatchDelegateAsync<T>();
}