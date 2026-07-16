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

        [ForeignKey("IdSolicitud")]
        public SolicitudServicio SolicitudServicio { get; set; }

        [Required]
        public int IdTecnico { get; set; }

        [ForeignKey("IdTecnico")]
        public Usuario Tecnico { get; set; }

        [Required]
        [StringLength(100)]
        public string TipoTrabajo { get; set; }

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        public DateTime? FechaFinalizacion { get; set; }

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Asignado";

        [StringLength(300)]
        public string? Observacion { get; set; }

        [StringLength(800)]
        public string? Resultado { get; set; }
    }
}