
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class Reclamacion
    {
        [Key]
        public int IdReclamacion { get; set; }


        // Cliente que registra la reclamación
        [Required]
        public int IdCliente { get; set; }


        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }



        // Tipo de reclamación:
        // Individual, Colectiva, Técnica, Comercial
        [Required]
        [StringLength(50)]
        public string TipoReclamacion { get; set; } = "Individual";



        // Descripción del problema
        [Required]
        public string Descripcion { get; set; } = null!;



        // Estado de la reclamación
        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";



        // Fecha de registro
        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;



        // Evidencias adjuntas
        // Fotos, documentos o archivos
        public string? Evidencias { get; set; }



        // ==============================
        // INFORMACIÓN TÉCNICA
        // ==============================


        // Técnico asignado
        public int? IdTecnico { get; set; }


        [ForeignKey("IdTecnico")]
        public Usuario? Tecnico { get; set; }



        // Diagnóstico realizado
        public string? DiagnosticoTecnico { get; set; }



        // Solución aplicada
        public string? SolucionAplicada { get; set; }



        // Observación del supervisor
        public string? ObservacionSupervisor { get; set; }



        // Fecha de cierre
        public DateTime? FechaCierre { get; set; }



        // ==============================
        // INFORMACIÓN DEL PROBLEMA
        // ==============================


        // Dirección donde ocurre el problema
        [StringLength(500)]
        public string? Direccion { get; set; }



        // Categoría del reclamo
        // Ejemplo:
        // Fuga, Facturación, Falta de agua
        [StringLength(100)]
        public string? Categoria { get; set; }



        // Prioridad técnica
        // Baja, Media, Alta, Urgente
        [StringLength(20)]
        public string? Prioridad { get; set; }



        // ==============================
        // INFORMACIÓN COMERCIAL
        // ==============================


        // Número de contrato del cliente
        [StringLength(50)]
        public string? NumeroContrato { get; set; }



        // Número de factura relacionada
        [StringLength(50)]
        public string? NumeroFactura { get; set; }



        // ==============================
        // INFORMACIÓN COLECTIVA
        // ==============================


        // Barrio o sector afectado
        [StringLength(100)]
        public string? Sector { get; set; }



        // Cantidad aproximada de afectados
        public int? CantidadAfectados { get; set; }

    }
}
