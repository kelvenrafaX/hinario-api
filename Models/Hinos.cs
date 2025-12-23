using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinhaPrimeiraApi.Models
{
    [Table("hinos")]
    public class Hinos
    {
        [Key]
        [Column("id")]
        public int Id {get ; private set;}
        [Column("letra")]
        public string Letra {get ; private set;}
        [Column("titulo")]
        public string Titulo {get ; private set;}

        public Hinos(string letra, string titulo)
        {
            Letra = letra ?? throw new ArgumentNullException(nameof(letra));
            Titulo = titulo;
        }
    }
}