
using System.ComponentModel.DataAnnotations;

namespace InapaWeb.Models
{
    public class RecursoMaterial
    {
        [Key]
        public int IdRecursoMaterial { get; set; }

        [Required(ErrorMessage = "Debe indicar el nombre del material.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Descripcion { get; set; }

        [StringLength(50)]
        public string UnidadMedida { get; set; } = "Unidad";

        public bool Activo { get; set; } = true;
    }
}
