using Microsoft.AspNetCore.Mvc;
using MinhaPrimeiraApi.Interfaces;
using MinhaPrimeiraApi.Models;
using MinhaPrimeiraApi.Utils;

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

    // FromRoute => /api/hino/pesquisar/paz
    // FromQuery => /api/hino/pesquisar?texto=paz

    [HttpGet("pesquisar")]
    public IActionResult PesquisarHinos([FromQuery] string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return Ok(new List<Hino>());
        }

        var hinos = _hinoRepository.Pesquisar(texto);

        // Normalizar o texto de busca
        var textoNormalizado = TextNormalizer.Normalizar(texto);
        var palavras = textoNormalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (palavras.Length == 0)
        {
            return Ok(new List<Hino>());
        }

        // Ordenar hinos por prioridade
        var hinosOrdenados = hinos.OrderBy(hino =>
        {
            var letraNormalizada = TextNormalizer.Normalizar(hino.Letra);
            
            // 1º prioridade: palavras em sequência
            var sequenciaCompleta = string.Join(" ", palavras);
            if (letraNormalizada.Contains(sequenciaCompleta))
            {
                return 1;
            }

            // 2º prioridade: todas as palavras presentes (mas não necessariamente em sequência)
            if (palavras.All(palavra => letraNormalizada.Contains(palavra)))
            {
                return 2;
            }

            // 3º prioridade: apenas algumas palavras presentes
            return 3;
        }).ThenBy(hino => hino.Id).ToList();

        return Ok(hinosOrdenados);
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