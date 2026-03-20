using MinhaPrimeiraApi.Context;
using MinhaPrimeiraApi.Interfaces;
using MinhaPrimeiraApi.Models;
using MinhaPrimeiraApi.Utils;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

    public Hino? ObterAnteriorPorTipoENumeroAsync(string tipo, int numero)
    {
        // 1. Tenta o hino anterior do mesmo tipo
        var anterior = _context.Hinos
            .Where(h => h.Identificador.StartsWith(tipo + "-"))
            .AsEnumerable() // <- traz para memória aqui
            .Where(h => int.Parse(h.Identificador.Replace(tipo + "-", string.Empty)) < numero)
            .OrderByDescending(h => int.Parse(h.Identificador.Replace(tipo + "-", string.Empty)))
            .FirstOrDefault();

        if (anterior != null)
            return anterior;

        // 2. Tenta siglas anteriores
        var index = ordemTipos.IndexOf(tipo);
        for (int i = index - 1; i >= 0; i--)
        {
            var tipoAtual = ordemTipos[i];
            var ultimo = _context.Hinos
                .Where(h => h.Identificador.StartsWith(tipoAtual + "-"))
                .AsEnumerable() // <- aqui também
                .OrderByDescending(h => int.Parse(h.Identificador.Replace(tipoAtual + "-", string.Empty)))
                .FirstOrDefault();

            if (ultimo != null)
                return ultimo;
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

        // Monta query no formato "palavra1 | palavra2 | palavra3" (OR entre termos)
        var tsQuery = string.Join(" | ", palavras);

        return _context.Hinos
        .AsNoTracking()
        .Where(h => h.LetraIdx.Matches(EF.Functions.ToTsQuery("portuguese", tsQuery)))
        .Select(h => new Hino
        {
            Id = h.Id,
            Identificador = h.Identificador,
            Titulo = h.Titulo,
            Letra = h.Letra
        })
        .ToList();
    }

    private static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var leftBody = new ReplaceParameterVisitor(left.Parameters[0], param).Visit(left.Body)!;
        var rightBody = new ReplaceParameterVisitor(right.Parameters[0], param).Visit(right.Body)!;
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(leftBody, rightBody), param);
    }

    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _from ? _to : base.VisitParameter(node);
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