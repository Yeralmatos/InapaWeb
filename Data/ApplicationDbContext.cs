using Microsoft.EntityFrameworkCore;
using InapaWeb.Models;

namespace InapaWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SolicitudServicio> SolicitudesServicio { get; set; }

        public DbSet<SolicitudContrato> SolicitudesContrato { get; set; }

        public DbSet<AsignacionTecnico> AsignacionesTecnicos { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Contrato> Contratos { get; set; }

        public DbSet<Tarifa> Tarifas { get; set; }

        public DbSet<Factura> Facturas { get; set; }

        public DbSet<Pago> Pagos { get; set; }

        public DbSet<Averia> Averias { get; set; }

        public DbSet<Reclamacion> Reclamaciones { get; set; }

        public DbSet<Traslado> Traslados { get; set; }

        public DbSet<Notificacion> Notificaciones { get; set; }

        public DbSet<HistorialServicio> HistorialServicios { get; set; }
    }
}
