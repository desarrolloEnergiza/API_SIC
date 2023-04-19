using DataAccess.Interfases;
using Microsoft.AspNetCore.Mvc;

namespace DapperWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AlumnoController : Controller
{
    private readonly IAlumnoRepo _alumno;

    public AlumnoController(IAlumnoRepo alumno)
    {
        _alumno = alumno;
    }

    [HttpGet("GetAlumnos")]
    public IActionResult GetAlumnos()
    {
        try
        {
            var result = _alumno.GetAlumnos();
            return Ok(result);
        }
        catch (Exception)
        {
            return this.StatusCode(StatusCodes.Status500InternalServerError, "Datos no encontrados");
        }
    }

    [HttpGet("{id}/GetMulti")]
    public async Task<IActionResult> GetMulti(int id)
    {
        var alumno = await _alumno.GetMulti(id);
        if (alumno is null)
            return NotFound();
        return Ok(alumno);
    }

    [HttpGet("{id}", Name = "GetById")]
    public async Task<IActionResult> GetAlumno(int id)
    {
        var alumno = await _alumno.GetById(id);
        if (alumno is null)
            return NotFound();
        return Ok(alumno);
    }

}
