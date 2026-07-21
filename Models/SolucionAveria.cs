using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class SolucionAveria
    {
        [Key]
        public int IdSolucionAveria { get; set; }


        [Required]
        public int IdAveria { get; set; }

        [ForeignKey(nameof(IdAveria))]
        public Averia? Averia { get; set; }


        [Required]
        public int IdTecnico { get; set; }

        [ForeignKey(nameof(IdTecnico))]
        public Usuario? Tecnico { get; set; }

        // =====================================================
        // FECHAS DE LA SOLUCIÓN
        // =====================================================

        public DateTime FechaInicio { get; set; } = DateTime.Now;

        public DateTime? FechaSolucion { get; set; }



        [Required(ErrorMessage = "Debe escribir el detalle de la solución.")]
        [StringLength(
            1500,
            MinimumLength = 10,
            ErrorMessage = "El detalle debe tener entre 10 y 1500 caracteres."
        )]
        public string DetalleSolucion { get; set; } = string.Empty;


        [StringLength(800)]
        public string? ObservacionesTecnico { get; set; }



        [Required]
        [StringLength(40)]
        public string EstadoSolucion { get; set; } = "En Proceso";



        [StringLength(500)]
        public string? EvidenciaImagen1 { get; set; }

        [StringLength(500)]
        public string? EvidenciaImagen2 { get; set; }

        [StringLength(500)]
        public string? EvidenciaImagen3 { get; set; }

        [StringLength(500)]
        public string? EvidenciaImagen4 { get; set; }

        [StringLength(500)]
        public string? EvidenciaImagen5 { get; set; }



        [StringLength(1000)]
        public string? RecursosMaterialesUtilizados { get; set; }

        [StringLength(1000)]
        public string? RecursosHumanosUtilizados { get; set; }



        public DateTime? FechaValidacion { get; set; }

        public int? IdCoordinadorValidador { get; set; }

        [ForeignKey(nameof(IdCoordinadorValidador))]
        public Usuario? CoordinadorValidador { get; set; }

        [StringLength(800)]
        public string? ObservacionCoordinador { get; set; }

        public bool ValidadaPorCoordinador { get; set; } = false;
    }
}
