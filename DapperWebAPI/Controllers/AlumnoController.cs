using DataAccess.Interfases;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

    [HttpGet("GetApiResponse")]
    public IActionResult GetApiResponse()
    {
        try
        {
            Console.WriteLine("4");
            var apiKey = "https://auladigital.sence.cl/gestor/API/avance-sic/historialEnvios?rutOtec=76697561-5&idSistema=1350&token=41121748-F370-403D-8F03-2F9F7E5C40D2";
            Console.WriteLine("5");
            var result = _alumno.GetApiResponse(apiKey);
            // Convierte el objeto JObject a una cadena JSON
            var jsonResult = result.ToString();
            return Content(jsonResult, "application/json");
        }
        catch (Exception e)
        {
            var error = new { message = e.Message, stackTrace = e.StackTrace };
            return this.StatusCode(StatusCodes.Status500InternalServerError, error);
        }
    }

    [HttpPost("PostApiRequest")]
    public async Task<IActionResult> PostApiRequest()
    {
        try
        {
            // Obtener el cuerpo raw de la solicitud HTTP
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string bodyRaw = await reader.ReadToEndAsync();
                var apiKey = "https://auladigital.sence.cl/gestor/API/avance-sic/enviarAvance";
                var result = await _alumno.PostApiRequest(apiKey); // Esperar a que la tarea se complete

                // Convertir los strings en objetos JSON
                var jsonResults = result.Select(JsonConvert.DeserializeObject);

                // Convertir los objetos JSON en una cadena JSON
                var jsonResult = JsonConvert.SerializeObject(jsonResults);
                return Content(jsonResult, "application/json");
            }
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
