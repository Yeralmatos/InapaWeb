using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class Factura
    {
        [Key]
        public int IdFactura { get; set; }

        [Required]
        public int IdContrato { get; set; }

        [ForeignKey("IdContrato")]
        public Contrato Contrato { get; set; }

        [Required]
        public int IdTarifa { get; set; }

        [ForeignKey("IdTarifa")]
        public Tarifa Tarifa { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoTotal { get; set; }

        [Required]
        public DateTime FechaEmision { get; set; } = DateTime.Now;

        [Required]
        public DateTime FechaVencimiento { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";
    }
}
