using Microsoft.AspNetCore.Mvc;
using MinhaPrimeiraApi.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class HinoController : ControllerBase
{
    private readonly IHinoRepository _hinoRepository;

    public HinoController(IHinoRepository hinoRepository)
    {
        _hinoRepository = hinoRepository;
    }

    [HttpGet]
    public IActionResult GetHinos()
    {
        return Ok(_hinoRepository.GetAll());
    }
}