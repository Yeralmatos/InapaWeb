using System.ComponentModel.DataAnnotations;

namespace InapaWeb.Models
{
    public class Tarifa
    {
        [Key]
        public int IdTarifa { get; set; }

        [Required]
        [StringLength(100)]
        public string Descripcion { get; set; }

        [Required]
        public decimal Monto { get; set; }
    }
}