using DataAccess.Interfases;
using Microsoft.AspNetCore.Mvc;

namespace DapperWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RootController : Controller
{
    private readonly IRootRepo _root;
	
	public RootController(IRootRepo root)
	{
        _root = root;
    }

    [HttpGet("{id}/GetData")]
    public async Task<IActionResult> GetData(int id)
    {
        try {
            var curso = await _root.GetData(id);
            if (curso is null)
                return NotFound();
            return Ok(curso);
        } catch (Exception e) {
            var error = new { message = e.Message, stackTrace = e.StackTrace };
            return this.StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }
}
