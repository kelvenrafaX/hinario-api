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

    public List<Hinos> GetAll()
    {
        return _context.Hinos.ToList();
    }

    public void Add(Hinos hinos)
    {
        _context.Hinos.Add(hinos);
        _context.SaveChanges();
    }
}