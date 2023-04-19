using DataAccess.Interfases;
using Microsoft.AspNetCore.Mvc;
using Model;

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
        var curso = await _root.GetData(id);
        if (curso is null)
            return NotFound();
        return Ok(curso);
    }
}
