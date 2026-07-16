using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class HistorialServicio
    {
        [Key]
        public int IdServicio { get; set; }

        [Required]
        public int IdAveria { get; set; }

        [ForeignKey("IdAveria")]
        public Averia Averia { get; set; }

        [Required]
        public int IdTecnico { get; set; }

        [Required]
        public string Descripcion { get; set; }
    }
}