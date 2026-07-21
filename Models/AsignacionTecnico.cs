using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class AsignacionTecnico
    {
        [Key]
        public int IdAsignacion { get; set; }

        [Required]
        public int IdSolicitud { get; set; }

        [ForeignKey(nameof(IdSolicitud))]
        public SolicitudServicio SolicitudServicio { get; set; } = null!;

        [Required]
        public int IdTecnico { get; set; }

        [ForeignKey(nameof(IdTecnico))]
        public Usuario Tecnico { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string TipoTrabajo { get; set; } = string.Empty;

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        public DateTime? FechaFinalizacion { get; set; }

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Asignado";

        [StringLength(300)]
        public string? Observacion { get; set; }

        [StringLength(800)]
        public string? Resultado { get; set; }

         

        [StringLength(500)]
        public string? EvidenciaImagen1 { get; set; }

        [StringLength(500)]
        public string? EvidenciaImagen2 { get; set; }

        [StringLength(500)]
        public string? EvidenciaImagen3 { get; set; }
    }
}
