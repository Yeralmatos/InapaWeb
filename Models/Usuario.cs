using System.ComponentModel.DataAnnotations;

namespace InapaWeb.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Contrasena { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Rol { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";

        public bool DebeCambiarClave { get; set; } = false;

        [Required]
        [StringLength(20)]
        public string OrigenRegistro { get; set; } = "Virtual";
    }
}
