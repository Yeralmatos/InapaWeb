using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class Contrato
    {
        [Key]
        public int IdContrato { get; set; }


        // ============================
        // RELACIÓN CON CLIENTE
        // ============================

        [Required]
        public int IdCliente { get; set; }

        [ForeignKey("IdCliente")]
        public Cliente Cliente { get; set; } = null!;



        // ============================
        // DATOS DEL CONTRATO
        // ============================

        [Required]
        [StringLength(30)]
        public string NumeroContrato { get; set; } = string.Empty;


        [Required]
        [StringLength(50)]
        public string TipoContrato { get; set; } = "Residencial";
        // Residencial - Comercial - Industrial - Gubernamental


        [Required]
        [StringLength(30)]
        public string EstadoContrato { get; set; } = "Pendiente";
        // Pendiente - Aprobado - Activo - Suspendido - Cancelado



        // ============================
        // FECHAS DEL PROCESO
        // ============================

        [Required]
        public DateTime FechaSolicitud { get; set; }


        public DateTime? FechaAprobacion { get; set; }


        public DateTime? FechaInstalacion { get; set; }


        [Required]
        public DateTime FechaInicioServicio { get; set; }


        public DateTime? FechaVencimiento { get; set; }



        // ============================
        // DATOS DEL TITULAR (COPIA DEL CLIENTE)
        // ============================

        [Required]
        [StringLength(150)]
        public string NombreTitular { get; set; } = string.Empty;


        [StringLength(20)]
        public string DocumentoTitular { get; set; } = string.Empty;
        // Cédula o RNC


        [StringLength(20)]
        public string TelefonoTitular { get; set; } = string.Empty;



        // ============================
        // DIRECCIÓN DEL SERVICIO
        // ============================

        [Required]
        [StringLength(250)]
        public string DireccionServicio { get; set; } = string.Empty;


        [StringLength(100)]
        public string Sector { get; set; } = string.Empty;


        [StringLength(100)]
        public string Municipio { get; set; } = string.Empty;


        [StringLength(100)]
        public string Provincia { get; set; } = string.Empty;



        // ============================
        // INFORMACIÓN DEL SERVICIO
        // ============================

        [Required]
        [StringLength(50)]
        public string TipoServicio { get; set; } = "Agua Potable";


        [StringLength(50)]
        public string CategoriaServicio { get; set; } = "Residencial";


        [StringLength(50)]
        public string NumeroMedidor { get; set; } = string.Empty;


        public int? IdTarifa { get; set; }

        [ForeignKey("IdTarifa")]
        public Tarifa? Tarifa { get; set; }



        // ============================
        // APROBACIÓN Y CONTROL
        // ============================

        public int? UsuarioAprobacionId { get; set; }


        [StringLength(100)]
        public string UsuarioAprobador { get; set; } = string.Empty;


        public DateTime FechaRegistro { get; set; } = DateTime.Now;


        public DateTime? FechaActualizacion { get; set; }



        // ============================
        // CANCELACIÓN / SUSPENSIÓN
        // ============================

        [StringLength(500)]
        public string MotivoCancelacion { get; set; } = string.Empty;



        // ============================
        // OBSERVACIONES
        // ============================

        [StringLength(1000)]
        public string Observaciones { get; set; } = string.Empty;
    }
}