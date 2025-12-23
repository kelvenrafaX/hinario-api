using MinhaPrimeiraApi.Models;

namespace MinhaPrimeiraApi.Interfaces
{
    public interface IHinoRepository
    {
        List<Hinos> GetAll();
        void Add(Hinos hinos);
    }
}