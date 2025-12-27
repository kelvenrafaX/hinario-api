using MinhaPrimeiraApi.Models;

namespace MinhaPrimeiraApi.Interfaces
{
    public interface IHinoRepository
    {
        List<Hino> GetAll();
        void Add(Hino hinos);
        void Update(Hino hinos);
        object GetById(int id);
    }
}