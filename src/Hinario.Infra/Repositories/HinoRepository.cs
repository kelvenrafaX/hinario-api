using Hinario.Infra.Context;
using Hinario.Domain.Interfaces;
using Hinario.Domain.Models;
using Hinario.Utils;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Hinario.Infra.Reporitories;

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