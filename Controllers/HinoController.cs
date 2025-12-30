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

    [HttpGet("identificador/{identificador}")]
    public IActionResult GetHinoByIdentificador(string identificador)
    {
        if (string.IsNullOrWhiteSpace(identificador))
        {
            return BadRequest("Identificador não pode ser vazio");
        }

        var hino = _hinoRepository.GetByIdentificador(identificador);

        if (hino == null)
        {
            return NotFound();
        }

        return Ok(hino);
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


    [HttpGet("{id}")]
    public IActionResult GetHino(int id)
    {
        var hino = _hinoRepository.GetById(id);
        if (hino == null)
        {
            return NotFound();
        }
        return Ok(hino);
    }

    [HttpPost("importar")]
    public IActionResult ImportarHinos([FromBody] List<HinoImportDto> hinosImport)
    {
        if (hinosImport == null || hinosImport.Count == 0)
        {
            return BadRequest("Lista de hinos não pode ser vazia");
        }

        var hinos = new List<Hino>();
        var erros = new List<string>();

        foreach (var hinoImport in hinosImport)
        {
            try
            {
                // Validar dados obrigatórios
                if (string.IsNullOrWhiteSpace(hinoImport.Title))
                {
                    erros.Add($"Hino com ID {hinoImport.Id}: Título é obrigatório");
                    continue;
                }

                var letra = hinoImport.Lyrics?.FullText ?? string.Empty;
                if (string.IsNullOrWhiteSpace(letra))
                {
                    erros.Add($"Hino '{hinoImport.Title}': Letra não encontrada");
                    continue;
                }

                // Verificar se já existe um hino com o mesmo identificador
                var identificador = hinoImport.Id.ToString();
                var hinoExistente = _hinoRepository.GetByIdentificador(identificador);
                
                if (hinoExistente != null)
                {
                    erros.Add($"Hino '{hinoImport.Title}' (ID: {hinoImport.Id}): Já existe um hino com este identificador");
                    continue;
                }

                var hino = new Hino
                {
                    Identificador = identificador,
                    Titulo = hinoImport.Title,
                    Letra = letra
                };

                hinos.Add(hino);
            }
            catch (Exception ex)
            {
                erros.Add($"Erro ao processar hino '{hinoImport.Title}': {ex.Message}");
            }
        }

        if (hinos.Count == 0)
        {
            return BadRequest(new { 
                mensagem = "Nenhum hino foi importado", 
                erros = erros 
            });
        }

        try
        {
            _hinoRepository.AddRange(hinos);
            
            var resultado = new
            {
                mensagem = $"Importação concluída: {hinos.Count} hino(s) importado(s) com sucesso",
                importados = hinos.Count,
                erros = erros.Count > 0 ? erros : null
            };

            if (erros.Count > 0)
            {
                return StatusCode(207, resultado); // 207 Multi-Status para indicar sucesso parcial
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                mensagem = "Erro ao salvar hinos no banco de dados", 
                erro = ex.Message 
            });
        }
    }
}