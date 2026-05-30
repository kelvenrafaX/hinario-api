using Hinario.Domain.Models;

namespace Hinario.Domain.Dtos
{
    public class RepertoriosAtivosPaginadosDto
    {
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
        public int Total { get; set; }
        public int TotalPaginas { get; set; }
        public List<Repertorio> Repertorios { get; set; } = [];
    }

    public class CriarRepertorioDto
    {
        public string Nome { get; set; } = string.Empty;
        public DateOnly? Data { get; set; }
    }

    public class AtualizarRepertorioDto
    {
        public string? Nome { get; set; }
        public DateOnly? Data { get; set; }
    }

    public class AdicionarHinoRepertorioDto
    {
        public int HinoId { get; set; }
    }

    public class ReordenarHinosDto
    {
        /// <summary>
        /// IDs dos RepertorioItens na nova ordem desejada.
        /// </summary>
        public List<int> ItemIds { get; set; } = [];
    }
}
