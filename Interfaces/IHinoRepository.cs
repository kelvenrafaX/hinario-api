using MinhaPrimeiraApi.Models;

namespace MinhaPrimeiraApi.Interfaces
{
    public interface IHinoRepository
    {
        List<Hino> GetAll();
        List<Hino> Pesquisar(string texto);
        void Add(Hino hinos);
        void Update(Hino hinos);
        object GetById(int id);
    }
}