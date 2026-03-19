using Microsoft.AspNetCore.Mvc;
using Hinario.Interfaces;
using Hinario.Models;
using Hinario.Utils;

[ApiController]
[Route("api/[controller]")]
public class HinoController : ControllerBase
{
    private readonly IHinoRepository _hinoRepository;
    private readonly ICampinaGrandeMineracaoService _mineracaoService;

    public HinoController(
        IHinoRepository hinoRepository,
        ICampinaGrandeMineracaoService mineracaoService)
    {
        _hinoRepository = hinoRepository;
        _mineracaoService = mineracaoService;
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
            return Ok(new List<HinoResultadoPesquisaDto>());
        }

        var hinos = _hinoRepository.Pesquisar(texto);

        // Normalizar o texto de busca
        var textoNormalizado = TextNormalizer.Normalizar(texto);
        var palavras = textoNormalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (palavras.Length == 0)
        {
            return Ok(new List<HinoResultadoPesquisaDto>());
        }

        // Termos para negrito no preview: primeiro a frase completa, depois cada palavra (sem duplicados),
        // ordenados por tamanho desc para priorizar frases/termos maiores.
        var termosNegrito = new List<string> { textoNormalizado };
        termosNegrito.AddRange(palavras);
        var termosNegritoArray = termosNegrito
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .OrderByDescending(t => t.Length)
            .ToArray();

        var hinosOrdenados = hinos
            .Select(h =>
            {
                var letraNormalizada = TextNormalizer.Normalizar(h.Letra ?? "");
                var temFraseExata = letraNormalizada.Contains(textoNormalizado);
                var totalPalavrasEncontradas = palavras.Count(p => letraNormalizada.Contains(p));
                var temTodasPalavras = totalPalavrasEncontradas == palavras.Length;

                return new
                {
                    Hino = h,
                    TemFraseExata = temFraseExata,
                    TemTodasPalavras = temTodasPalavras,
                    TotalPalavrasEncontradas = totalPalavrasEncontradas
                };
            })
            .OrderByDescending(x => x.TemFraseExata)
            .ThenByDescending(x => x.TemTodasPalavras)
            .ThenByDescending(x => x.TotalPalavrasEncontradas)
            .Select(x => x.Hino);

        int pagina = 1;
        int tamanhoPagina = 10;

        // Montar DTOs com o trecho (preview) para cada hino
        var resultado = hinosOrdenados.Select(hino => new HinoResultadoPesquisaDto
        {
            Id = hino.Id,
            Identificador = hino.Identificador,
            Titulo = hino.Titulo ?? string.Empty,
            Letra = hino.Letra,
            Trecho = TrechoPesquisaHelper.BuildTrecho(hino, termosNegritoArray)
        })
        .Skip((pagina - 1) * tamanhoPagina)
        .Take(tamanhoPagina)
        .ToList();

        return Ok(resultado);
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

        if (hinoExistente == null)
            return NotFound();

        hinoExistente.Titulo = string.IsNullOrEmpty(hino.Titulo) ? hinoExistente.Titulo : hino.Titulo;
        hinoExistente.Letra = string.IsNullOrEmpty(hino.Letra) ? hinoExistente.Letra : hino.Letra;
        hinoExistente.Identificador = string.IsNullOrEmpty(hino.Identificador) ? hinoExistente.Identificador : hino.Identificador;

        _hinoRepository.Update(hinoExistente);
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

    /// <summary>
    /// Minera um cântico (1-100) do site Igreja em Campina Grande e persiste se ainda não estiver cadastrado.
    /// </summary>
    /// <param name="numero">Número do cântico (1 a 100)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [HttpGet("minerar/canticos/{numero:int}")]
    public async Task<IActionResult> MinerarCantico(
        int numero,
        CancellationToken cancellationToken = default)
    {
        if (numero < 1 || numero > 100)
        {
            return BadRequest(new { mensagem = "O número do cântico deve estar entre 1 e 100." });
        }

        var identificador = $"C-{numero}";
        var existente = _hinoRepository.GetByIdentificador(identificador);
        if (existente != null)
        {
            return Ok(new
            {
                mensagem = "Hino já cadastrado.",
                hino = existente
            });
        }

        var hino = await _mineracaoService.ExtrairCanticoAsync(numero, cancellationToken);

        if (hino == null)
        {
            return StatusCode(502, new
            {
                mensagem = "Não foi possível extrair o hino do site (página indisponível ou formato inesperado)."
            });
        }

        _hinoRepository.Add(hino);
        return CreatedAtAction(nameof(GetHino), new { id = hino.Id }, hino);
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