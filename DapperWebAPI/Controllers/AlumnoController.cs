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
        catch (Exception e)
        {
            var error = new { message = e.Message, stackTrace = e.StackTrace };
            return this.StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    [HttpGet("{id}/GetMulti")]
    public async Task<IActionResult> GetMulti(int id)
    {
        try
        {
            var alumno = await _alumno.GetMulti(id);
            if (alumno is null)
                return NotFound();
            return Ok(alumno);
        } catch (Exception e)
        {
            var error = new { message = e.Message, stackTrace = e.StackTrace };
            return this.StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    [HttpGet("{id}", Name = "GetById")]
    public async Task<IActionResult> GetAlumno(int id)
    {
        try
        {
            var alumno = await _alumno.GetById(id);
            if (alumno is null)
                return NotFound();
            return Ok(alumno);
        } catch (Exception e)
        {
            var error = new { message = e.Message, stackTrace = e.StackTrace };
            return this.StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

}
