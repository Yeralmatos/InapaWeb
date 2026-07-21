using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class Averia
    {
        [Key]
        public int IdAveria { get; set; }

        [Required]
        public int IdCliente { get; set; }

        [ForeignKey(nameof(IdCliente))]
        public Cliente? Cliente { get; set; }

        [Required]
        [StringLength(30)]
        public string TipoAveria { get; set; } = "Residencial";

        [Required]
        [StringLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [Required]
        [StringLength(800, MinimumLength = 10)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string GradoAveria { get; set; } = "Menor";

        [Required]
        [StringLength(20)]
        public string Prioridad { get; set; } = "Media";

        [Required]
        [StringLength(40)]
        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaReporte { get; set; } = DateTime.Now;

        public DateTime? FechaAsignacion { get; set; }

        public DateTime? FechaAtencion { get; set; }

        public DateTime? FechaFinalizacion { get; set; }

        public DateTime? FechaCierre { get; set; }

        [Required]
        [StringLength(500)]
        public string DireccionAveria { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TipoAsignacion { get; set; } = "Automatica";

        public bool RequiereValidacionCoordinador { get; set; } = false;

        public int? IdTecnico { get; set; }

        [ForeignKey(nameof(IdTecnico))]
        public Usuario? Tecnico { get; set; }

        public int? IdCoordinador { get; set; }

        [ForeignKey(nameof(IdCoordinador))]
        public Usuario? Coordinador { get; set; }

        [StringLength(500)]
        public string? ObservacionAdministrador { get; set; }

        [StringLength(500)]
        public string? ObservacionCierre { get; set; }

        public SolucionAveria? SolucionAveria { get; set; }
    }
}
