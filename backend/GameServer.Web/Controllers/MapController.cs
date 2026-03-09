using Microsoft.AspNetCore.Mvc;
using GameServerApp.Contracts.Services;
using GameServerApp.Dtos;

namespace GameServer.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MapController : ControllerBase
{
    private readonly IProceduralWorldService _proceduralWorldService;

    public MapController(IProceduralWorldService proceduralWorldService)
    {
        _proceduralWorldService = proceduralWorldService;
    }

    [HttpGet("objects")]
    public IActionResult GetMapObjects()
    {
        try
        {
            var mapObjects = _proceduralWorldService.GetAllMapObjects();

            return Ok(mapObjects);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Erro ao obter objetos do mapa", error = ex.Message });
        }
    }
}