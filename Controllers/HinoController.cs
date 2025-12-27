using Microsoft.AspNetCore.Mvc;
using MinhaPrimeiraApi.Interfaces;
using MinhaPrimeiraApi.Models;

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

    [HttpPost]
    public IActionResult CreateHino([FromBody] Hino hino)
    {
        Console.WriteLine(hino.Id);
        _hinoRepository.Add(hino);
        return CreatedAtAction(nameof(CreateHino), new { id = hino.Id }, hino);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateHino(int id, [FromBody] Hino hino)
    {
        if (id != hino.Id)
        {
            return BadRequest("id do hino não corresponde ao id da rota");
        }

        var hinoExistente = _hinoRepository.GetById(id);

        if (hinoExistente == null){
        return NotFound();}

        _hinoRepository.Update(hino);
        return NoContent();
    }
}