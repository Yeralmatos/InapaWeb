using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class Reclamacion
    {
        [Key]
        public int IdReclamacion { get; set; }


        [Required]
        public int IdCliente { get; set; }


        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }



        [Required]
        public string Descripcion { get; set; } = null!;



        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";



        [Required]
        [StringLength(50)]
        public string TipoReclamacion { get; set; } = "Individual";



        // Técnico asignado
        public int? IdTecnico { get; set; }


        [ForeignKey("IdTecnico")]
        public Usuario? Tecnico { get; set; }



        // Diagnóstico realizado por técnico
        public string? DiagnosticoTecnico { get; set; }



        // Solución aplicada
        public string? SolucionAplicada { get; set; }



        // Evidencias (rutas de archivos o imágenes)
        public string? Evidencias { get; set; }



        // Observación del supervisor
        public string? ObservacionSupervisor { get; set; }



        // Fechas de control
        public DateTime FechaRegistro { get; set; } = DateTime.Now;


        public DateTime? FechaCierre { get; set; }
    }
}