using MinhaPrimeiraApi.Context;
using MinhaPrimeiraApi.Interfaces;
using MinhaPrimeiraApi.Models;
using MinhaPrimeiraApi.Utils;

public class HinoRepository : IHinoRepository
{
    private readonly HinarioApiContext _context;
    private readonly List<string> ordemTipos = new() { "H", "C", "S", "HC", "L" };


    public HinoRepository(HinarioApiContext context)
    {
        _context = context;
    }

    public List<Hino> GetAll()
    {
        return _context.Hinos.ToList();
    }

    public Hino? ObterProximoPorTipoENumeroAsync(string tipo, int numero)
    {
        // 1. Tenta próximo hino do mesmo tipo
        var proximo = _context.Hinos
            .Where(h => h.Identificador.StartsWith(tipo + "-"))
            .AsEnumerable() // <- traz para memória aqui
            .Where(h => int.Parse(h.Identificador.Replace(tipo + "-", string.Empty)) > numero)
            .OrderBy(h => int.Parse(h.Identificador.Replace(tipo + "-", string.Empty)))
            .FirstOrDefault();

        if (proximo != null)
            return proximo;

        // 2. Tenta próximas siglas
        var index = ordemTipos.IndexOf(tipo);
        for (int i = index + 1; i < ordemTipos.Count; i++)
        {
            var tipoAtual = ordemTipos[i];
            var primeiro = _context.Hinos
                .Where(h => h.Identificador.StartsWith(tipoAtual + "-"))
                .AsEnumerable() // <- aqui também
                .OrderBy(h => int.Parse(h.Identificador.Replace(tipoAtual + "-", string.Empty)))
                .FirstOrDefault();

            if (primeiro != null)
                return primeiro;
        }

        return null;
    }

    public List<Hino> Pesquisar(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return new List<Hino>();
        }

        // Normalizar o texto de busca (remover acentos e converter para minúsculas)
        var textoNormalizado = TextNormalizer.Normalizar(texto);
        var palavras = textoNormalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (palavras.Length == 0)
        {
            return new List<Hino>();
        }

        // Buscar todos os hinos e processar em memória para permitir normalização
        var todosHinos = _context.Hinos.ToList();

        // Filtrar hinos que contenham pelo menos uma das palavras
        var hinosFiltrados = todosHinos.Where(hino =>
        {
            var letraNormalizada = TextNormalizer.Normalizar(hino.Letra);
            return palavras.Any(palavra => letraNormalizada.Contains(palavra) ) ;
        }).ToList();

        return hinosFiltrados;
    }

    public void Add(Hino hinos)
    {
        _context.Hinos.Add(hinos);
        _context.SaveChanges();
    }

    public void AddRange(List<Hino> hinos)
    {
        _context.Hinos.AddRange(hinos);
        _context.SaveChanges();
    }

    public void Update(Hino hinos)
    {
        _context.Hinos.Update(hinos);
        _context.SaveChanges();
    }



 public Hino? GetById(int id)
{
    return _context.Hinos.Find(id);
}

    public Hino? GetByIdentificador(string identificador)
    {
        return _context.Hinos.FirstOrDefault(h => h.Identificador.ToUpper().Replace("-", "") == identificador.ToUpper().Replace("-", "" ));
    }
}