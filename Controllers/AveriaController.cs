using InapaWeb.Data;
using InapaWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InapaWeb.Controllers
{
    public class AveriaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AveriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            string rolUsuario =
                HttpContext.Session.GetString("RolUsuario") ?? string.Empty;

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Acceso");
            }

            IQueryable<Averia> consulta = _context.Averias
                .Include(a => a.Cliente)
                .Include(a => a.Tecnico)
                .Include(a => a.Coordinador)
                .AsNoTracking();

            if (rolUsuario.Equals(
                    "Cliente",
                    StringComparison.OrdinalIgnoreCase))
            {
                Cliente? cliente = await _context.Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        c => c.IdUsuario == usuarioId.Value
                    );

                if (cliente == null)
                {
                    TempData["Error"] =
                        "No existe un cliente asociado a este usuario.";

                    return RedirectToAction("Index", "Cliente");
                }

                consulta = consulta.Where(
                    a => a.IdCliente == cliente.IdCliente
                );
            }
            else if (
                rolUsuario.Equals(
                    "Técnico",
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                rolUsuario.Equals(
                    "Tecnico",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                consulta = consulta.Where(
                    a => a.IdTecnico == usuarioId.Value
                );
            }
            else if (
                !rolUsuario.Equals(
                    "Administrador",
                    StringComparison.OrdinalIgnoreCase
                )
                &&
                !rolUsuario.Equals(
                    "Coordinador",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return RedirectToAction("Login", "Acceso");
            }

            var averias = await consulta
                .OrderByDescending(a => a.FechaReporte)
                .ToListAsync();

            return View(averias);
        }

        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            string rolUsuario =
                HttpContext.Session.GetString("RolUsuario") ?? string.Empty;

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Acceso");
            }

            if (!rolUsuario.Equals(
                    "Cliente",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] =
                    "Solo los clientes pueden reportar averías.";

                return RedirectToAction("Index");
            }

            Cliente? cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.IdUsuario == usuarioId.Value
                );

            if (cliente == null)
            {
                TempData["Error"] =
                    "No existe un cliente asociado a este usuario.";

                return RedirectToAction("Index", "Cliente");
            }

            Averia averia = new Averia
            {
                IdCliente = cliente.IdCliente,
                TipoAveria = "Residencial",
                GradoAveria = "Menor",
                Prioridad = "Media",
                Estado = "Pendiente",
                TipoAsignacion = "Automatica",
                FechaReporte = DateTime.Now
            };

            return View(averia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Averia averia)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            string rolUsuario =
                HttpContext.Session.GetString("RolUsuario") ?? string.Empty;

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Acceso");
            }

            if (!rolUsuario.Equals(
                    "Cliente",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            Cliente? cliente = await _context.Clientes
                .FirstOrDefaultAsync(
                    c => c.IdUsuario == usuarioId.Value
                );

            if (cliente == null)
            {
                TempData["Error"] =
                    "No existe un cliente asociado a este usuario.";

                return RedirectToAction("Index", "Cliente");
            }

            ModelState.Remove(nameof(Averia.IdCliente));
            ModelState.Remove(nameof(Averia.Estado));
            ModelState.Remove(nameof(Averia.Prioridad));
            ModelState.Remove(nameof(Averia.TipoAsignacion));

            if (!ModelState.IsValid)
            {
                averia.IdCliente = cliente.IdCliente;
                return View(averia);
            }

            averia.IdAveria = 0;
            averia.IdCliente = cliente.IdCliente;
            averia.FechaReporte = DateTime.Now;
            averia.FechaAsignacion = null;
            averia.FechaAtencion = null;
            averia.FechaFinalizacion = null;
            averia.FechaCierre = null;
            averia.IdTecnico = null;
            averia.IdCoordinador = null;
            averia.ObservacionAdministrador = null;
            averia.ObservacionCierre = null;

            ConfigurarPrioridad(averia);

            bool asignacionAutomatica =
                averia.TipoAveria.Equals(
                    "Residencial",
                    StringComparison.OrdinalIgnoreCase
                )
                &&
                (
                    averia.GradoAveria.Equals(
                        "Menor",
                        StringComparison.OrdinalIgnoreCase
                    )
                    ||
                    averia.GradoAveria.Equals(
                        "Moderada",
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (asignacionAutomatica)
            {
                averia.TipoAsignacion = "Automatica";
                averia.RequiereValidacionCoordinador = false;

                Usuario? tecnico = await BuscarTecnicoDisponible();

                if (tecnico != null)
                {
                    averia.IdTecnico = tecnico.IdUsuario;
                    averia.FechaAsignacion = DateTime.Now;
                    averia.Estado = "Asignada";
                }
                else
                {
                    averia.Estado = "Pendiente";
                }
            }
            else
            {
                averia.TipoAsignacion = "Manual";
                averia.RequiereValidacionCoordinador = true;
                averia.Estado = "Pendiente de Coordinador";
            }

            _context.Averias.Add(averia);
            await _context.SaveChangesAsync();

            if (averia.IdTecnico.HasValue)
            {
                TempData["Exito"] =
                    "La avería fue registrada y asignada automáticamente.";
            }
            else if (averia.TipoAsignacion == "Manual")
            {
                TempData["Exito"] =
                    "La avería fue registrada y enviada al coordinador.";
            }
            else
            {
                TempData["Exito"] =
                    "La avería fue registrada. Actualmente no hay técnicos disponibles.";
            }

            return RedirectToAction(nameof(Detalle), new
            {
                id = averia.IdAveria
            });
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            string rolUsuario =
                HttpContext.Session.GetString("RolUsuario") ?? string.Empty;

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Acceso");
            }

            Averia? averia = await _context.Averias
                .Include(a => a.Cliente)
                .Include(a => a.Tecnico)
                .Include(a => a.Coordinador)
                .Include(a => a.SolucionAveria)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAveria == id);

            if (averia == null)
            {
                return NotFound();
            }

            if (rolUsuario.Equals(
                    "Cliente",
                    StringComparison.OrdinalIgnoreCase))
            {
                Cliente? cliente = await _context.Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        c => c.IdUsuario == usuarioId.Value
                    );

                if (cliente == null ||
                    averia.IdCliente != cliente.IdCliente)
                {
                    return Forbid();
                }
            }

            if (
                rolUsuario.Equals(
                    "Técnico",
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                rolUsuario.Equals(
                    "Tecnico",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                if (averia.IdTecnico != usuarioId.Value)
                {
                    return Forbid();
                }
            }

            return View(averia);
        }

        private async Task<Usuario?> BuscarTecnicoDisponible()
        {
            string[] estadosActivos =
            {
                "Asignada",
                "En Proceso",
                "Pendiente de Recursos"
            };

            Usuario? tecnico = await _context.Usuarios
                .Where(u =>
                    (
                        u.Rol == "Técnico"
                        ||
                        u.Rol == "Tecnico"
                    )
                    &&
                    u.Estado == "Activo"
                )
                .Select(u => new
                {
                    Usuario = u,

                    TrabajosActivos = _context.Averias.Count(a =>
                        a.IdTecnico == u.IdUsuario
                        &&
                        estadosActivos.Contains(a.Estado)
                    ),

                    UltimaAsignacion = _context.Averias
                        .Where(a => a.IdTecnico == u.IdUsuario)
                        .Max(a => (DateTime?)a.FechaAsignacion)
                })
                .OrderBy(x => x.TrabajosActivos)
                .ThenBy(x => x.UltimaAsignacion ?? DateTime.MinValue)
                .Select(x => x.Usuario)
                .FirstOrDefaultAsync();

            return tecnico;
        }

        private static void ConfigurarPrioridad(Averia averia)
        {
            if (averia.GradoAveria.Equals(
                    "Crítica",
                    StringComparison.OrdinalIgnoreCase)
                ||
                averia.GradoAveria.Equals(
                    "Critica",
                    StringComparison.OrdinalIgnoreCase))
            {
                averia.Prioridad = "Crítica";
                return;
            }

            if (averia.GradoAveria.Equals(
                    "Mayor",
                    StringComparison.OrdinalIgnoreCase))
            {
                averia.Prioridad = "Alta";
                return;
            }

            if (averia.GradoAveria.Equals(
                    "Moderada",
                    StringComparison.OrdinalIgnoreCase))
            {
                averia.Prioridad = "Media";
                return;
            }

            averia.Prioridad = "Baja";
        }
    }
}
