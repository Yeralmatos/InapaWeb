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


        [Required]
        [StringLength(30)]
        public string EstadoContrato { get; set; } = "Pendiente";

        // ==================================================
        // COMPATIBILIDAD CON CÓDIGO ANTERIOR DEL SISTEMA
        // ==================================================

        [NotMapped]
        public string Estado
        {
            get { return EstadoContrato; }
            set { EstadoContrato = value; }
        }


        // ============================
        // FECHAS DEL PROCESO
        // ============================

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;


        public DateTime? FechaAprobacion { get; set; }


        public DateTime? FechaInstalacion { get; set; }


        [Required]
        public DateTime FechaInicioServicio { get; set; }


        public DateTime? FechaVencimiento { get; set; }



        // ============================
        // DATOS DEL TITULAR
        // ============================

        [Required]
        [StringLength(150)]
        public string NombreTitular { get; set; } = string.Empty;


        [StringLength(20)]
        public string DocumentoTitular { get; set; } = string.Empty;


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



        // ============================
        // RELACIÓN TARIFA
        // ============================

        [Required]
        public int IdTarifa { get; set; }


        [ForeignKey("IdTarifa")]
        public Tarifa Tarifa { get; set; } = null!;



        // ============================
        // CONTROL ADMINISTRATIVO
        // ============================

        public int? UsuarioAprobacionId { get; set; }


        [StringLength(100)]
        public string UsuarioAprobador { get; set; } = string.Empty;


        public DateTime FechaRegistro { get; set; } = DateTime.Now;


        public DateTime? FechaActualizacion { get; set; }



        // ============================
        // CANCELACIÓN
        // ============================

        [StringLength(500)]
        public string MotivoCancelacion { get; set; } = string.Empty;



        // ============================
        // OBSERVACIONES
        // ============================

        [StringLength(1000)]
        public string Observaciones { get; set; } = string.Empty;



        // ============================
        // COMPATIBILIDAD FECHAS
        // ============================

        [NotMapped]
        public DateTime? FechaInicio
        {
            get => FechaInicioServicio;
            set
            {
                if (value.HasValue)
                    FechaInicioServicio = value.Value;
            }
        }


        [NotMapped]
        public DateTime? FechaFin
        {
            get => FechaVencimiento;
            set => FechaVencimiento = value;
        }
    }
}