using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class SolicitudContrato
    {
        [Key]
        public int IdSolicitudContrato { get; set; }

        [Required]
        public int IdCliente { get; set; }

        [ForeignKey(nameof(IdCliente))]
        public Cliente Cliente { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TipoServicio { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DireccionServicio { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ObservacionCliente { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";

        [StringLength(500)]
        public string? ObservacionAdministrador { get; set; }
    }
}
