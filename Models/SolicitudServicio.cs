using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class SolicitudServicio
    {
        [Key]
        public int IdSolicitud { get; set; }

        [Required]
        public int IdCliente { get; set; }

        [ForeignKey(nameof(IdCliente))]
        public Cliente Cliente { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string TipoSolicitud { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";

        [StringLength(300)]
        public string? ObservacionAdministrador { get; set; }
    }
}
