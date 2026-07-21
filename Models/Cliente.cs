using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;

        public int? IdTarifa { get; set; }

        [ForeignKey("IdTarifa")]
        public Tarifa? Tarifa { get; set; }

        [Required]
        [StringLength(20)]
        public string CedulaPasaporteRnc { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Provincia { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Municipio { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Sector { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string EstadoCliente { get; set; } = "Activo";
    }
}
