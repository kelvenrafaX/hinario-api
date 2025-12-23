using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinhaPrimeiraApi.Models
{
    [Table("hinos")]
    public class Hinos
    {
        [Key]
        [Column("id")]
        public int Id {get ; set;}
        [Column("letra")]
        public string Letra {get ; set;}
        [Column("titulo")]
        public string Titulo {get ; set;}

        public Hinos(string letra, string titulo)
        {
            Letra = letra ?? throw new ArgumentNullException(nameof(letra));
            Titulo = titulo;
        }
    }
}