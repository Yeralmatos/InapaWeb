using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class SolucionRecursoHumano
    {
        [Key]
        public int IdSolucionRecursoHumano { get; set; }



        [Required]
        public int IdSolucionAveria { get; set; }

        [ForeignKey(nameof(IdSolucionAveria))]
        public SolucionAveria? SolucionAveria { get; set; }


        [Required]
        public int IdUsuario { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }



        [Required(ErrorMessage = "Debe indicar la función del recurso humano.")]
        [StringLength(100)]
        public string Funcion { get; set; } = string.Empty;

         
 

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        public DateTime? FechaFinalizacion { get; set; }



        [StringLength(500)]
        public string? Observacion { get; set; }



        public bool Activo { get; set; } = true;
    }
}
