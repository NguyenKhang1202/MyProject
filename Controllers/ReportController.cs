using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Helpers;
using Stimulsoft.Report;

namespace MyProject.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportController(ILogger<ReportController> logger) : ControllerBase
{
    private static string _userid = "userId";

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserReport([Required] int userId)
    {
        return await ControllerHelper.TryCatchAsync(this, "GetUserReport", async () =>
        {
            StiReport report = new StiReport();
            report.Load("D:\\data_phenikaa\\Report.mrt");
            report.Dictionary.Variables[_userid].Value = userId.ToString();
            await report.RenderAsync();
            using var stream = new MemoryStream();
            await report.ExportDocumentAsync(StiExportFormat.Pdf, stream);
            stream.Position = 0;
            return File(stream.ToArray(), "application/pdf", "UserReport.pdf");
        });
    }
}