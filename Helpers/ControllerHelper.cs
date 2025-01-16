using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MyProject.Helpers;

public class ControllerHelper : ControllerBase
{
    public static 
#nullable disable
        IActionResult TryCatch(
            ControllerBase controller,
            string apiName,
            ControllerHelper.TryCatchDelegate del)
    {
        try
        {
            return del();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Exception: " + apiName);
            return (IActionResult) controller.StatusCode(500, (object) ex.Message);
        }
    }

    public static async Task<IActionResult> TryCatchAsync(
        ControllerBase controller,
        string apiName,
        ControllerHelper.TryCatchDelegateAsync del)
    {
        try
        {
            return await del();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Exception: " + apiName);
            return (IActionResult) controller.StatusCode(500, (object) ex.Message);
        }
    }

    public delegate IActionResult TryCatchDelegate();

    public delegate Task<IActionResult> TryCatchDelegateAsync();
}