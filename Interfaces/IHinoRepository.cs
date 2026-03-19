using MinhaPrimeiraApi.Models;

namespace MinhaPrimeiraApi.Interfaces
{
    public interface IHinoRepository
    {
        List<Hino> GetAll();
        List<Hino> Pesquisar(string texto);
        void Add(Hino hinos);
        void AddRange(List<Hino> hinos);
        void Update(Hino hinos);
        Hino? GetById(int id);
        Hino? GetByIdentificador(string identificador);
        Hino? ObterProximoPorTipoENumeroAsync(string tipo, int numero);
    }
}