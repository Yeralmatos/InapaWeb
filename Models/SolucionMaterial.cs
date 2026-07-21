using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InapaWeb.Models
{
    public class SolucionMaterial
    {
        [Key]
        public int IdSolucionMaterial { get; set; }

     
        [Required]
        public int IdSolucionAveria { get; set; }

        [ForeignKey(nameof(IdSolucionAveria))]
        public SolucionAveria? SolucionAveria { get; set; }
 

        [Required]
        public int IdRecursoMaterial { get; set; }

        [ForeignKey(nameof(IdRecursoMaterial))]
        public RecursoMaterial? RecursoMaterial { get; set; }

        

        [Required(ErrorMessage = "Debe indicar la cantidad utilizada.")]
        [Range(
            0.01,
            100000,
            ErrorMessage = "La cantidad debe ser mayor que cero."
        )]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cantidad { get; set; }
 

        [Required(ErrorMessage = "Debe indicar la unidad de medida.")]
        [StringLength(50)]
        public string UnidadMedida { get; set; } = "Unidad";
 

        [StringLength(300)]
        public string? Observacion { get; set; }
    }
}
