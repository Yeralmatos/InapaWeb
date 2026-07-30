using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }


        // ============================
        // USUARIO RELACIONADO
        // ============================

        [Required]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;



        // ============================
        // TARIFA DEL CLIENTE
        // ============================

        public int? IdTarifa { get; set; }

        [ForeignKey("IdTarifa")]
        public Tarifa? Tarifa { get; set; }



        // ============================
        // DATOS DEL CLIENTE
        // ============================

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



        // ============================
        // CONTROL DEL CLIENTE
        // ============================

        public DateTime FechaRegistro { get; set; } = DateTime.Now;


        [Required]
        [StringLength(20)]
        public string EstadoCliente { get; set; } = "Activo";



        // ============================
        // CONTRATOS DEL CLIENTE
        // ============================

        public ICollection<Contrato> Contratos { get; set; }
            = new List<Contrato>();
    }
}