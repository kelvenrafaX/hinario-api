using Hinario.Domain.Interfaces;
using Hinario.Domain.Models;
using Hinario.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Hinario.Infra.Repositories
{
    public class RepertorioRepository(HinarioApiContext context) : IRepertorioRepository
    {
        public List<Repertorio> GetAll() =>
            context.Repertorios
                .Include(r => r.Itens)
                    .ThenInclude(i => i.Hino)
                .OrderByDescending(r => r.Data)
                .ToList();

        public Repertorio? GetById(int id) =>
            context.Repertorios
                .Include(r => r.Itens)
                    .ThenInclude(i => i.Hino)
                .FirstOrDefault(r => r.Id == id);

        public Repertorio? GetAtivo() =>
            context.Repertorios
                .Include(r => r.Itens.OrderBy(i => i.Ordem))
                    .ThenInclude(i => i.Hino)
                .FirstOrDefault(r => r.Ativo);

        public (List<Repertorio> Items, int Total) GetAtivos(int pagina, int tamanhoPagina, string ordenacao)
        {
            var total = context.Repertorios.Count(r => r.Ativo);

            var query = context.Repertorios
                .Include(r => r.Itens.OrderBy(i => i.Ordem))
                    .ThenInclude(i => i.Hino)
                .Where(r => r.Ativo);

            IQueryable<Repertorio> ordenado = ordenacao.ToLower() switch
            {
                "data_asc"  => query.OrderBy(r => r.Data),
                "nome"      => query.OrderBy(r => r.Nome),
                "nome_desc" => query.OrderByDescending(r => r.Nome),
                _           => query.OrderByDescending(r => r.Data),
            };

            var items = ordenado
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToList();

            return (items, total);
        }

        public void Add(Repertorio repertorio)
        {
            context.Repertorios.Add(repertorio);
            context.SaveChanges();
        }

        public void Update(Repertorio repertorio)
        {
            context.Repertorios.Update(repertorio);
            context.SaveChanges();
        }

        public void Delete(Repertorio repertorio)
        {
            context.Repertorios.Remove(repertorio);
            context.SaveChanges();
        }

        public void DesativarTodos()
        {
            context.Repertorios
                .Where(r => r.Ativo)
                .ExecuteUpdate(s => s.SetProperty(r => r.Ativo, false));
        }
    }
}
