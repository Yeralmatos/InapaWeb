using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InapaWeb.Data;
using InapaWeb.Models;

namespace InapaWeb.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClienteController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool EsCliente()
        {
            return HttpContext.Session.GetString("RolUsuario") == "Cliente";
        }

        private int? ObtenerIdUsuario()
        {
            return HttpContext.Session.GetInt32("UsuarioId");
        }

        private Cliente? ObtenerClienteActual()
        {
            int? idUsuario = ObtenerIdUsuario();

            if (idUsuario == null)
            {
                return null;
            }

            return _context.Clientes
                .FirstOrDefault(c => c.IdUsuario == idUsuario.Value);
        }
         

        public IActionResult Index()
        {
            if (!EsCliente())
            {
                return RedirectToAction("Login", "Acceso");
            }

            ViewBag.NombreUsuario =
                HttpContext.Session.GetString("NombreUsuario") ?? "Cliente";

            var cliente = ObtenerClienteActual();

            if (cliente == null)
            {
                ViewBag.TotalSolicitudes = 0;
                ViewBag.TotalFacturasPendientes = 0;
                ViewBag.TotalAverias = 0;
                ViewBag.TotalReclamaciones = 0;

                return View();
            }

            ViewBag.TotalSolicitudes = _context.SolicitudesServicio
                .Count(s => s.IdCliente == cliente.IdCliente);

            ViewBag.TotalFacturasPendientes = _context.Facturas
                .Include(f => f.Contrato)
                .Count(f =>
                    f.Contrato != null &&
                    f.Contrato.IdCliente == cliente.IdCliente &&
                    f.Estado == "Pendiente");

            ViewBag.TotalAverias = _context.Averias
                .Count(a =>
                    a.IdCliente == cliente.IdCliente &&
                    a.Estado != "Resuelta");

            ViewBag.TotalReclamaciones = _context.Reclamaciones
                .Count(r =>
                    r.IdCliente == cliente.IdCliente &&
                    r.Estado != "Cerrada");

            return View();
        }
         

        [HttpGet]
        public IActionResult NuevaSolicitud()
        {
            if (!EsCliente())
            {
                return RedirectToAction("Login", "Acceso");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NuevaSolicitud(SolicitudServicio solicitud)
        {
            if (!EsCliente())
            {
                return RedirectToAction("Login", "Acceso");
            }

            var cliente = ObtenerClienteActual();

            if (cliente == null)
            {
                ViewBag.Error = "No se encontró el perfil del cliente.";
                return View(solicitud);
            }

            if (string.IsNullOrWhiteSpace(solicitud.TipoSolicitud) ||
                string.IsNullOrWhiteSpace(solicitud.Descripcion))
            {
                ViewBag.Error =
                    "Debe completar el tipo y la descripción de la solicitud.";

                return View(solicitud);
            }

            var nuevaSolicitud = new SolicitudServicio
            {
                IdCliente = cliente.IdCliente,
                TipoSolicitud = solicitud.TipoSolicitud.Trim(),
                Descripcion = solicitud.Descripcion.Trim(),
                FechaSolicitud = DateTime.Now,
                Estado = "Pendiente",
                ObservacionAdministrador = null
            };

            _context.SolicitudesServicio.Add(nuevaSolicitud);
            _context.SaveChanges();

            TempData["Mensaje"] =
                "La solicitud fue enviada correctamente.";

            return RedirectToAction(nameof(MisSolicitudes));
        }

        [HttpGet]
        public IActionResult MisSolicitudes()
        {
            if (!EsCliente())
            {
                return RedirectToAction("Login", "Acceso");
            }

            var cliente = ObtenerClienteActual();

            if (cliente == null)
            {
                return View(new List<SolicitudServicio>());
            }

            var solicitudes = _context.SolicitudesServicio
                .Where(s => s.IdCliente == cliente.IdCliente)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToList();

            return View(solicitudes);
        }

  
        [HttpGet]
        public IActionResult MiContrato()
        {
            if (!EsCliente())
            {
                return RedirectToAction("Login", "Acceso");
            }

            var cliente = ObtenerClienteActual();

            if (cliente == null)
            {
                TempData["Error"] =
                    "No se encontró el perfil del cliente.";

                return RedirectToAction(nameof(Index));
            }

             
            var contrato = _context.Contratos
                .Include(c => c.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(c => c.Cliente)
                    .ThenInclude(c => c.Tarifa)
                .Where(c => c.IdCliente == cliente.IdCliente)
                .OrderByDescending(c => c.IdContrato)
                .FirstOrDefault();

             
            bool tieneSolicitudContrato = _context.SolicitudesServicio
                .Any(s =>
                    s.IdCliente == cliente.IdCliente &&
                    s.TipoSolicitud == "Solicitud de contrato" &&
                    s.Estado != "Rechazada" &&
                    s.Estado != "Contrato Aprobado");

            ViewBag.TieneSolicitudContrato = tieneSolicitudContrato;

            return View(contrato);
        }

        

        [HttpGet]
        public IActionResult SolicitarContrato()
        {
            if (!EsCliente())
            {
                return RedirectToAction("Login", "Acceso");
            }

            var cliente = ObtenerClienteActual();

            if (cliente == null)
            {
                TempData["Error"] =
                    "No se encontró el perfil del cliente.";

                return RedirectToAction(nameof(Index));
            }

            bool tieneContrato = _context.Contratos
     .Any(c =>
         c.IdCliente == cliente.IdCliente &&
         (c.EstadoContrato == "Activo" ||
          c.EstadoContrato == "Pendiente"));
            if (tieneContrato)
            {
                TempData["Error"] =
                    "Ya tienes un contrato activo o pendiente de aprobación.";

                return RedirectToAction(nameof(MiContrato));
            }

            bool tieneSolicitudEnProceso = _context.SolicitudesServicio
                .Any(s =>
                    s.IdCliente == cliente.IdCliente &&
                    s.TipoSolicitud == "Solicitud de contrato" &&
                    s.Estado != "Rechazada" &&
                    s.Estado != "Contrato Aprobado");

            if (tieneSolicitudEnProceso)
            {
                TempData["Error"] =
                    "Ya tienes una solicitud de contrato en proceso.";

                return RedirectToAction(nameof(MisSolicitudes));
            }

            string direccionCompleta = string.Join(
                ", ",
                new[]
                {
                    cliente.Direccion,

                    string.IsNullOrWhiteSpace(cliente.Sector)
                        ? null
                        : $"Sector {cliente.Sector}",

                    cliente.Municipio,
                    cliente.Provincia
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
            );

            var modelo = new SolicitudContrato
            {
                DireccionServicio = direccionCompleta
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SolicitarContrato(SolicitudContrato solicitud)
        {
            if (!EsCliente())
            {
                return RedirectToAction("Login", "Acceso");
            }

            var cliente = ObtenerClienteActual();

            if (cliente == null)
            {
                TempData["Error"] =
                    "No se encontró el perfil del cliente.";

                return RedirectToAction(nameof(Index));
            }

            bool tieneContrato = _context.Contratos
                .Any(c =>
                    c.IdCliente == cliente.IdCliente &&
                    (c.Estado == "Activo" ||
                     c.Estado == "Pendiente"));

            if (tieneContrato)
            {
                TempData["Error"] =
                    "Ya tienes un contrato activo o pendiente de aprobación.";

                return RedirectToAction(nameof(MiContrato));
            }

            bool tieneSolicitudEnProceso = _context.SolicitudesServicio
                .Any(s =>
                    s.IdCliente == cliente.IdCliente &&
                    s.TipoSolicitud == "Solicitud de contrato" &&
                    s.Estado != "Rechazada" &&
                    s.Estado != "Contrato Aprobado");

            if (tieneSolicitudEnProceso)
            {
                TempData["Error"] =
                    "Ya tienes una solicitud de contrato en proceso.";

                return RedirectToAction(nameof(MisSolicitudes));
            }

            if (string.IsNullOrWhiteSpace(solicitud.TipoServicio) ||
                string.IsNullOrWhiteSpace(solicitud.DireccionServicio))
            {
                ViewBag.Error =
                    "Debe seleccionar el tipo de servicio y escribir la dirección.";

                return View(solicitud);
            }

            string descripcion =
                $"Tipo de servicio: {solicitud.TipoServicio.Trim()}. " +
                $"Dirección del servicio: " +
                $"{solicitud.DireccionServicio.Trim()}.";

            if (!string.IsNullOrWhiteSpace(
                solicitud.ObservacionCliente))
            {
                descripcion +=
                    $" Observación: " +
                    $"{solicitud.ObservacionCliente.Trim()}.";
            }

            var nuevaSolicitud = new SolicitudServicio
            {
                IdCliente = cliente.IdCliente,
                TipoSolicitud = "Solicitud de contrato",
                Descripcion = descripcion,
                FechaSolicitud = DateTime.Now,
                Estado = "Pendiente",
                ObservacionAdministrador = null
            };

          
            _context.SolicitudesServicio.Add(nuevaSolicitud);
            _context.SaveChanges();

            TempData["Mensaje"] =
                "La solicitud de contrato fue enviada correctamente.";

            return RedirectToAction(nameof(MisSolicitudes));
        }
    }
}
