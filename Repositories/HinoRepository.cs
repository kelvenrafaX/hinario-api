using MinhaPrimeiraApi.Context;
using MinhaPrimeiraApi.Interfaces;
using MinhaPrimeiraApi.Models;
using MinhaPrimeiraApi.Utils;

public class HinoRepository : IHinoRepository
{
    private readonly HinarioApiContext _context;

    public HinoRepository(HinarioApiContext context)
    {
        _context = context;
    }

    public List<Hino> GetAll()
    {
        return _context.Hinos.ToList();
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
            return palavras.Any(palavra => letraNormalizada.Contains(palavra));
        }).ToList();

        return hinosFiltrados;
    }

    public void Add(Hino hinos)
    {
        _context.Hinos.Add(hinos);
        _context.SaveChanges();
    }
    public void Update(Hino hinos)
    {
        _context.Hinos.Update(hinos);
        _context.SaveChanges();
    }

    public object GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Hino? GetByIdentificador(string identificador)
    {
        return _context.Hinos.FirstOrDefault(h => h.Identificador == identificador);
    }
}