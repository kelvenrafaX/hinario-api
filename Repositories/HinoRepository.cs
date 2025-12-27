using MinhaPrimeiraApi.Context;
using MinhaPrimeiraApi.Interfaces;
using MinhaPrimeiraApi.Models;

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
        return _context.Hinos.Where(hino => hino.Letra.Contains(texto)).ToList();
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
}