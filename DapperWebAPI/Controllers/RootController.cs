using DataAccess.Interfases;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DapperWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RootController : Controller
{
    private readonly IRootRepo _root;
    private readonly IBackgroundJobClient _backgroundJobClient;
	
	public RootController(IRootRepo root, IBackgroundJobClient backgroundJobClient)
	{
        _root = root;
        _backgroundJobClient = backgroundJobClient;
    }

    [HttpGet("{id}/GetData")]
    public async Task<IActionResult> GetData(int id)
    {
        try {

            var curso = await _root.GetData(id);
            if (curso is null)
                return NotFound();
            var json = JsonConvert.SerializeObject(curso);
            Logs.saveLog(DateTime.Now.ToString("yyyy-MM-dd HH"), ""+json);
            return Ok(json);
        } catch (Exception e) {
            var error = new { message = e.Message, stackTrace = e.StackTrace };
            return this.StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }
}
