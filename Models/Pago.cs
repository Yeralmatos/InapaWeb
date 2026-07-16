using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public int IdFactura { get; set; }

        [ForeignKey("IdFactura")]
        public Factura Factura { get; set; }

        [Required]
        public decimal MontoPagado { get; set; }

        [Required]
        public DateTime FechaPago { get; set; }
    }
}