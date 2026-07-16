using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InapaWeb.Data;
using InapaWeb.Models;

namespace InapaWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // PANEL PRINCIPAL
        // =========================================================

        public IActionResult Index()
        {
            ViewBag.NombreUsuario =
                HttpContext.Session.GetString("NombreUsuario")
                ?? "Administrador";

            ViewBag.TotalUsuarios = _context.Usuarios.Count();

            ViewBag.TotalContratos = _context.Contratos.Count();

            ViewBag.TotalAverias = _context.Averias.Count();

            ViewBag.TotalReclamaciones = _context.Reclamaciones.Count();

            ViewBag.TotalFacturas = _context.Facturas.Count();

            // IMPORTANTE:
            // Solo cuenta solicitudes de servicio que estén pendientes.
            ViewBag.TotalSolicitudes = _context.SolicitudesServicio
                .Count(s => s.Estado == "Pendiente");

            // Solo cuenta solicitudes de contrato pendientes.
            ViewBag.TotalSolicitudesContrato = _context.SolicitudesContrato
                .Count(s => s.Estado == "Pendiente");

            // Solicitudes pendientes para mostrarlas en el panel.
            ViewBag.SolicitudesRecientes = _context.SolicitudesServicio
                .Include(s => s.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Where(s => s.Estado == "Pendiente")
                .OrderByDescending(s => s.FechaSolicitud)
                .Take(5)
                .ToList();

            // Solicitudes aprobadas que todavía necesitan técnico.
            ViewBag.SolicitudesParaAsignar = _context.SolicitudesServicio
                .Include(s => s.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Where(s =>
                    s.Estado == "Aprobada" &&
                    !_context.AsignacionesTecnicos.Any(a =>
                        a.IdSolicitud == s.IdSolicitud &&
                        a.Estado != "Finalizado"))
                .OrderByDescending(s => s.FechaSolicitud)
                .Take(5)
                .ToList();

            return View();
        }

 

        public IActionResult Usuarios(string? buscar)
        {
            var usuarios = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                usuarios = usuarios.Where(u =>
                    u.NombreUsuario.Contains(buscar) ||
                    u.Correo.Contains(buscar) ||
                    u.Rol.Contains(buscar) ||
                    u.Estado.Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            return View(
                usuarios
                    .OrderByDescending(u => u.IdUsuario)
                    .ToList()
            );
        }
     
        [HttpGet]
        public IActionResult CrearUsuario()
        {
            return View(new Usuario());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearUsuario(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.NombreUsuario) ||
                string.IsNullOrWhiteSpace(usuario.Correo) ||
                string.IsNullOrWhiteSpace(usuario.Contrasena) ||
                string.IsNullOrWhiteSpace(usuario.Rol))
            {
                ViewBag.Error =
                    "Debe completar todos los campos obligatorios.";

                return View(usuario);
            }

            usuario.NombreUsuario = usuario.NombreUsuario.Trim();
            usuario.Correo = usuario.Correo.Trim().ToLower();
            usuario.Rol = usuario.Rol.Trim();

            bool correoExiste = _context.Usuarios
                .Any(u => u.Correo.ToLower() == usuario.Correo);

            if (correoExiste)
            {
                ViewBag.Error =
                    "Ya existe un usuario registrado con ese correo.";

                return View(usuario);
            }

            usuario.Estado = string.IsNullOrWhiteSpace(usuario.Estado)
                ? "Activo"
                : usuario.Estado;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            TempData["MensajeUsuario"] =
                "El usuario fue creado correctamente.";

            return RedirectToAction(nameof(Usuarios));
        }



        public IActionResult AprobarUsuario(int id)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario != null)
            {
                usuario.Estado = "Activo";

                var cliente = _context.Clientes
                    .FirstOrDefault(c => c.IdUsuario == id);

                if (cliente != null)
                {
                    cliente.EstadoCliente = "Activo";
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Usuarios));
        }

        public IActionResult RechazarUsuario(int id)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario != null)
            {
                usuario.Estado = "Rechazado";

                var cliente = _context.Clientes
                    .FirstOrDefault(c => c.IdUsuario == id);

                if (cliente != null)
                {
                    cliente.EstadoCliente = "Rechazado";
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Usuarios));
        }

        public IActionResult CambiarEstado(int id)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario != null)
            {
                usuario.Estado =
                    usuario.Estado == "Activo"
                        ? "Inactivo"
                        : "Activo";

                var cliente = _context.Clientes
                    .FirstOrDefault(c => c.IdUsuario == id);

                if (cliente != null)
                {
                    cliente.EstadoCliente = usuario.Estado;
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Usuarios));
        }

        public IActionResult DetalleUsuario(int id)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return RedirectToAction(nameof(Usuarios));
            }

            ViewBag.Cliente = _context.Clientes
                .Include(c => c.Tarifa)
                .FirstOrDefault(c => c.IdUsuario == id);

            return View(usuario);
        }

        public IActionResult EliminarUsuario(int id)
        {
            var cliente = _context.Clientes
                .FirstOrDefault(c => c.IdUsuario == id);

            if (cliente != null)
            {
                bool tieneSolicitudesServicio =
                    _context.SolicitudesServicio
                        .Any(s => s.IdCliente == cliente.IdCliente);

                bool tieneSolicitudesContrato =
                    _context.SolicitudesContrato
                        .Any(s => s.IdCliente == cliente.IdCliente);

                bool tieneContratos =
                    _context.Contratos
                        .Any(c => c.IdCliente == cliente.IdCliente);

                if (tieneSolicitudesServicio ||
                    tieneSolicitudesContrato ||
                    tieneContratos)
                {
                    cliente.EstadoCliente = "Inactivo";

                    var usuarioRelacionado = _context.Usuarios
                        .FirstOrDefault(u => u.IdUsuario == id);

                    if (usuarioRelacionado != null)
                    {
                        usuarioRelacionado.Estado = "Inactivo";
                    }

                    _context.SaveChanges();

                    return RedirectToAction(nameof(Usuarios));
                }

                _context.Clientes.Remove(cliente);
            }

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Usuarios));
        }

        // =========================================================
        // SOLICITUDES DE SERVICIO
        // =========================================================

        public IActionResult Solicitudes(
            string? buscar,
            string estado = "Pendiente")
        {
            var solicitudes = _context.SolicitudesServicio
                .Include(s => s.Cliente)
                    .ThenInclude(c => c.Usuario)
                .AsQueryable();

            // Por defecto muestra solamente las pendientes.
            // Puede usar estado=Todos para mostrar todas.
            if (!string.IsNullOrWhiteSpace(estado) &&
                estado != "Todos")
            {
                solicitudes = solicitudes
                    .Where(s => s.Estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                solicitudes = solicitudes.Where(s =>
                    s.Cliente.Usuario.NombreUsuario.Contains(buscar) ||
                    s.TipoSolicitud.Contains(buscar) ||
                    s.Estado.Contains(buscar));
            }

            ViewBag.Buscar = buscar;
            ViewBag.EstadoSeleccionado = estado;

            ViewBag.TotalPendientes = _context.SolicitudesServicio
                .Count(s => s.Estado == "Pendiente");

            ViewBag.TotalAprobadas = _context.SolicitudesServicio
                .Count(s => s.Estado == "Aprobada");

            ViewBag.TotalAsignadas = _context.SolicitudesServicio
                .Count(s => s.Estado == "Asignada");

            return View(
                solicitudes
                    .OrderByDescending(s => s.FechaSolicitud)
                    .ToList()
            );
        }

        public IActionResult AprobarSolicitud(int id)
        {
            var solicitud = _context.SolicitudesServicio
                .FirstOrDefault(s => s.IdSolicitud == id);

            if (solicitud == null)
            {
                TempData["ErrorSolicitud"] =
                    "La solicitud seleccionada no existe.";

                return RedirectToAction(nameof(Solicitudes));
            }

            if (solicitud.Estado != "Pendiente")
            {
                TempData["ErrorSolicitud"] =
                    "La solicitud ya fue procesada anteriormente.";

                return RedirectToAction(nameof(Solicitudes));
            }

            solicitud.Estado = "Aprobada";

            solicitud.ObservacionAdministrador =
                "Solicitud aprobada. Pendiente de asignación de técnico.";

            _context.SaveChanges();

            TempData["MensajeSolicitud"] =
                "Solicitud aprobada correctamente. Ahora debe asignar un técnico.";

            // Después de aprobar, abre directamente la pantalla
            // para asignar el técnico.
            return RedirectToAction(
                nameof(AsignarTecnico),
                new { id = solicitud.IdSolicitud }
            );
        }

        public IActionResult RechazarSolicitud(int id)
        {
            var solicitud = _context.SolicitudesServicio
                .FirstOrDefault(s => s.IdSolicitud == id);

            if (solicitud == null)
            {
                TempData["ErrorSolicitud"] =
                    "La solicitud seleccionada no existe.";

                return RedirectToAction(nameof(Solicitudes));
            }

            if (solicitud.Estado != "Pendiente")
            {
                TempData["ErrorSolicitud"] =
                    "La solicitud ya fue procesada anteriormente.";

                return RedirectToAction(nameof(Solicitudes));
            }

            solicitud.Estado = "Rechazada";

            solicitud.ObservacionAdministrador =
                "Solicitud rechazada por el administrador.";

            _context.SaveChanges();

            TempData["MensajeSolicitud"] =
                "La solicitud fue rechazada correctamente.";

            return RedirectToAction(nameof(Solicitudes));
        }

        public IActionResult DetalleSolicitud(int id)
        {
            var solicitud = _context.SolicitudesServicio
                .Include(s => s.Cliente)
                    .ThenInclude(c => c.Usuario)
                .FirstOrDefault(s => s.IdSolicitud == id);

            if (solicitud == null)
            {
                return RedirectToAction(nameof(Solicitudes));
            }

            return View(solicitud);
        }

        // =========================================================
        // SOLICITUDES DE CONTRATO
        // =========================================================

        public IActionResult SolicitudesContrato(string? buscar)
        {
            var solicitudes = _context.SolicitudesContrato
                .Include(s => s.Cliente)
                    .ThenInclude(c => c.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                solicitudes = solicitudes.Where(s =>
                    s.Cliente.Usuario.NombreUsuario.Contains(buscar) ||
                    s.Cliente.Usuario.Correo.Contains(buscar) ||
                    s.TipoServicio.Contains(buscar) ||
                    s.DireccionServicio.Contains(buscar) ||
                    s.Estado.Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            return View(
                solicitudes
                    .OrderByDescending(s => s.FechaSolicitud)
                    .ToList()
            );
        }

        public IActionResult AprobarSolicitudContrato(int id)
        {
            var solicitudContrato = _context.SolicitudesContrato
                .Include(s => s.Cliente)
                .FirstOrDefault(s =>
                    s.IdSolicitudContrato == id);

            if (solicitudContrato == null)
            {
                TempData["ErrorSolicitudContrato"] =
                    "La solicitud de contrato no existe.";

                return RedirectToAction(nameof(SolicitudesContrato));
            }

            if (solicitudContrato.Estado != "Pendiente")
            {
                TempData["ErrorSolicitudContrato"] =
                    "La solicitud de contrato ya fue procesada.";

                return RedirectToAction(nameof(SolicitudesContrato));
            }

            bool yaExisteSolicitudServicio =
                _context.SolicitudesServicio.Any(s =>
                    s.IdCliente == solicitudContrato.IdCliente &&
                    s.TipoSolicitud.Contains(
                        $"SC-{solicitudContrato.IdSolicitudContrato}") &&
                    s.Estado != "Rechazada");

            if (yaExisteSolicitudServicio)
            {
                TempData["ErrorSolicitudContrato"] =
                    "Esta solicitud ya generó un proceso de servicio.";

                return RedirectToAction(nameof(SolicitudesContrato));
            }

            using var transaccion =
                _context.Database.BeginTransaction();

            try
            {
                solicitudContrato.Estado = "Aprobada";

                solicitudContrato.ObservacionAdministrador =
                    "Solicitud de contrato aprobada. Pendiente de levantamiento técnico.";

                string descripcion =
                    $"Solicitud de contrato SC-{solicitudContrato.IdSolicitudContrato}. " +
                    $"Servicio: {solicitudContrato.TipoServicio}. " +
                    $"Dirección: {solicitudContrato.DireccionServicio}.";

                if (!string.IsNullOrWhiteSpace(
                    solicitudContrato.ObservacionCliente))
                {
                    descripcion +=
                        $" Observación del cliente: " +
                        $"{solicitudContrato.ObservacionCliente}";
                }

                var solicitudServicio = new SolicitudServicio
                {
                    IdCliente = solicitudContrato.IdCliente,

                    TipoSolicitud =
                        $"Contrato SC-{solicitudContrato.IdSolicitudContrato}",

                    Descripcion = descripcion,

                    FechaSolicitud = DateTime.Now,

                    // La solicitud ya fue aprobada.
                    // No debe aparecer como pendiente.
                    Estado = "Aprobada",

                    ObservacionAdministrador =
                        "Solicitud aprobada. Pendiente de asignación de técnico para levantamiento."
                };

                _context.SolicitudesServicio.Add(solicitudServicio);

                _context.SaveChanges();

                transaccion.Commit();

                TempData["MensajeSolicitudContrato"] =
                    "Solicitud aprobada. Ahora debe asignar un técnico.";

                // Envía directamente a asignar el técnico.
                return RedirectToAction(
                    nameof(AsignarTecnico),
                    new { id = solicitudServicio.IdSolicitud }
                );
            }
            catch (Exception)
            {
                transaccion.Rollback();

                TempData["ErrorSolicitudContrato"] =
                    "No fue posible aprobar la solicitud de contrato.";

                return RedirectToAction(nameof(SolicitudesContrato));
            }
        }

        public IActionResult RechazarSolicitudContrato(int id)
        {
            var solicitud = _context.SolicitudesContrato
                .FirstOrDefault(s =>
                    s.IdSolicitudContrato == id);

            if (solicitud == null)
            {
                TempData["ErrorSolicitudContrato"] =
                    "La solicitud de contrato no existe.";

                return RedirectToAction(nameof(SolicitudesContrato));
            }

            if (solicitud.Estado != "Pendiente")
            {
                TempData["ErrorSolicitudContrato"] =
                    "La solicitud ya fue procesada anteriormente.";

                return RedirectToAction(nameof(SolicitudesContrato));
            }

            solicitud.Estado = "Rechazada";

            solicitud.ObservacionAdministrador =
                "Solicitud de contrato rechazada por el administrador.";

            _context.SaveChanges();

            TempData["MensajeSolicitudContrato"] =
                "La solicitud de contrato fue rechazada.";

            return RedirectToAction(nameof(SolicitudesContrato));
        }

        // =========================================================
        // LEVANTAMIENTOS Y ASIGNACIÓN TÉCNICA
        // =========================================================

        public IActionResult ResultadoLevantamiento(int id)
        {
            var trabajo = _context.AsignacionesTecnicos
                .Include(a => a.SolicitudServicio)
                    .ThenInclude(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                .Include(a => a.Tecnico)
                .FirstOrDefault(a =>
                    a.IdSolicitud == id &&
                    a.TipoTrabajo == "Levantamiento");

            if (trabajo == null)
            {
                return RedirectToAction(nameof(Solicitudes));
            }

            return View(trabajo);
        }

        public IActionResult AsignarTecnico(int id)
        {
            var solicitud = _context.SolicitudesServicio
                .Include(s => s.Cliente)
                    .ThenInclude(c => c.Usuario)
                .FirstOrDefault(s => s.IdSolicitud == id);

            if (solicitud == null)
            {
                TempData["ErrorSolicitud"] =
                    "La solicitud seleccionada no existe.";

                return RedirectToAction(nameof(Solicitudes));
            }

            if (solicitud.Estado != "Aprobada")
            {
                TempData["ErrorSolicitud"] =
                    "Solo puede asignarse un técnico a una solicitud aprobada.";

                return RedirectToAction(
                    nameof(Solicitudes),
                    new { estado = "Aprobada" }
                );
            }

            bool yaTieneAsignacion =
                _context.AsignacionesTecnicos.Any(a =>
                    a.IdSolicitud == id &&
                    a.Estado != "Finalizado");

            if (yaTieneAsignacion)
            {
                TempData["ErrorSolicitud"] =
                    "Esta solicitud ya tiene un técnico asignado.";

                return RedirectToAction(
                    nameof(Solicitudes),
                    new { estado = "Asignada" }
                );
            }

            ViewBag.Solicitud = solicitud;

            ViewBag.Tecnicos = _context.Usuarios
                .Where(u =>
                    u.Rol == "Técnico" &&
                    u.Estado == "Activo")
                .OrderBy(u => u.NombreUsuario)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AsignarTecnico(
            int idSolicitud,
            int idTecnico,
            string? observacion)
        {
            var solicitud = _context.SolicitudesServicio
                .FirstOrDefault(s =>
                    s.IdSolicitud == idSolicitud);

            if (solicitud == null)
            {
                TempData["ErrorSolicitud"] =
                    "La solicitud seleccionada no existe.";

                return RedirectToAction(nameof(Solicitudes));
            }

            if (solicitud.Estado != "Aprobada")
            {
                TempData["ErrorSolicitud"] =
                    "La solicitud no está disponible para asignación.";

                return RedirectToAction(nameof(Solicitudes));
            }

            if (idTecnico <= 0)
            {
                TempData["ErrorAsignacion"] =
                    "Debe seleccionar un técnico.";

                return RedirectToAction(
                    nameof(AsignarTecnico),
                    new { id = idSolicitud }
                );
            }

            var tecnico = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == idTecnico &&
                    u.Rol == "Técnico" &&
                    u.Estado == "Activo");

            if (tecnico == null)
            {
                TempData["ErrorAsignacion"] =
                    "El técnico seleccionado no existe o no está activo.";

                return RedirectToAction(
                    nameof(AsignarTecnico),
                    new { id = idSolicitud }
                );
            }

            bool yaAsignada =
                _context.AsignacionesTecnicos.Any(a =>
                    a.IdSolicitud == idSolicitud &&
                    a.Estado != "Finalizado");

            if (yaAsignada)
            {
                TempData["ErrorSolicitud"] =
                    "Esta solicitud ya tiene un técnico asignado.";

                return RedirectToAction(
                    nameof(Solicitudes),
                    new { estado = "Asignada" }
                );
            }

            using var transaccion =
                _context.Database.BeginTransaction();

            try
            {
                var asignacion = new AsignacionTecnico
                {
                    IdSolicitud = idSolicitud,
                    IdTecnico = idTecnico,
                    TipoTrabajo = "Levantamiento",
                    FechaAsignacion = DateTime.Now,
                    Estado = "Asignado",
                    Observacion = observacion
                };

                _context.AsignacionesTecnicos.Add(asignacion);

                solicitud.Estado = "Asignada";

                solicitud.ObservacionAdministrador =
                    $"Solicitud asignada al técnico {tecnico.NombreUsuario} " +
                    $"para realizar el levantamiento.";

                _context.SaveChanges();

                transaccion.Commit();

                TempData["MensajeSolicitud"] =
                    "El técnico fue asignado correctamente.";

                return RedirectToAction(
                    nameof(Solicitudes),
                    new { estado = "Asignada" }
                );
            }
            catch (Exception)
            {
                transaccion.Rollback();

                TempData["ErrorAsignacion"] =
                    "No fue posible asignar el técnico.";

                return RedirectToAction(
                    nameof(AsignarTecnico),
                    new { id = idSolicitud }
                );
            }
        }

        // =========================================================
        // CATEGORIZACIÓN
        // =========================================================

        public IActionResult CategorizarCliente(int id)
        {
            var trabajo = _context.AsignacionesTecnicos
                .Include(a => a.SolicitudServicio)
                    .ThenInclude(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                .Include(a => a.Tecnico)
                .FirstOrDefault(a =>
                    a.IdSolicitud == id &&
                    a.TipoTrabajo == "Levantamiento" &&
                    a.Estado == "Finalizado");

            if (trabajo == null)
            {
                return RedirectToAction(nameof(Solicitudes));
            }

            ViewBag.Trabajo = trabajo;

            ViewBag.Tarifas = _context.Tarifas
                .OrderBy(t => t.Descripcion)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CategorizarCliente(
            int idSolicitud,
            int idTarifa)
        {
            if (idTarifa == 0)
            {
                return RedirectToAction(
                    nameof(CategorizarCliente),
                    new { id = idSolicitud }
                );
            }

            var solicitud = _context.SolicitudesServicio
                .Include(s => s.Cliente)
                .FirstOrDefault(s =>
                    s.IdSolicitud == idSolicitud);

            if (solicitud == null ||
                solicitud.Estado != "Levantamiento Finalizado")
            {
                return RedirectToAction(nameof(Solicitudes));
            }

            var tarifa = _context.Tarifas
                .FirstOrDefault(t => t.IdTarifa == idTarifa);

            if (tarifa == null)
            {
                return RedirectToAction(
                    nameof(CategorizarCliente),
                    new { id = idSolicitud }
                );
            }

            solicitud.Cliente.IdTarifa = idTarifa;

            solicitud.Estado = "Cliente Categorizado";

            solicitud.ObservacionAdministrador =
                $"Cliente categorizado con la tarifa: {tarifa.Descripcion}.";

            _context.SaveChanges();

            return RedirectToAction(nameof(Solicitudes));
        }

        // =========================================================
        // GENERACIÓN DEL CONTRATO
        // =========================================================

        public IActionResult GenerarContrato(int id)
        {
            var solicitud = _context.SolicitudesServicio
                .Include(s => s.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(s => s.Cliente)
                    .ThenInclude(c => c.Tarifa)
                .FirstOrDefault(s =>
                    s.IdSolicitud == id);

            if (solicitud == null ||
                solicitud.Estado != "Cliente Categorizado")
            {
                return RedirectToAction(nameof(Solicitudes));
            }

            bool existeContrato = _context.Contratos.Any(c =>
                c.IdCliente == solicitud.IdCliente &&
                (c.Estado == "Activo" ||
                 c.Estado == "Pendiente"));

            if (existeContrato)
            {
                return RedirectToAction(nameof(Solicitudes));
            }

            ViewBag.Solicitud = solicitud;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerarContrato(
            int idSolicitud,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var solicitud = _context.SolicitudesServicio
                .Include(s => s.Cliente)
                .FirstOrDefault(s =>
                    s.IdSolicitud == idSolicitud);

            if (solicitud == null ||
                solicitud.Estado != "Cliente Categorizado")
            {
                return RedirectToAction(nameof(Solicitudes));
            }

            if (solicitud.Cliente.IdTarifa == null)
            {
                return RedirectToAction(
                    nameof(CategorizarCliente),
                    new { id = idSolicitud }
                );
            }

            if (fechaInicio == default)
            {
                fechaInicio = DateTime.Today;
            }

            if (fechaFin == default ||
                fechaFin <= fechaInicio)
            {
                TempData["ErrorContrato"] =
                    "La fecha de fin debe ser posterior a la fecha de inicio.";

                return RedirectToAction(
                    nameof(GenerarContrato),
                    new { id = idSolicitud }
                );
            }

            bool existeContrato = _context.Contratos.Any(c =>
                c.IdCliente == solicitud.IdCliente &&
                (c.Estado == "Activo" ||
                 c.Estado == "Pendiente"));

            if (existeContrato)
            {
                return RedirectToAction(nameof(Solicitudes));
            }

            var contrato = new Contrato
            {
                IdCliente = solicitud.IdCliente,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Estado = "Pendiente"
            };

            _context.Contratos.Add(contrato);

            solicitud.Estado = "Contrato Generado";

            solicitud.ObservacionAdministrador =
                "Contrato generado y pendiente de aprobación.";

            var solicitudContrato =
                _context.SolicitudesContrato
                    .Where(s =>
                        s.IdCliente == solicitud.IdCliente &&
                        s.Estado == "Aprobada")
                    .OrderByDescending(s => s.FechaSolicitud)
                    .FirstOrDefault();

            if (solicitudContrato != null)
            {
                solicitudContrato.Estado = "Contrato Generado";

                solicitudContrato.ObservacionAdministrador =
                    "El contrato fue generado y está pendiente de aprobación.";
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Contratos));
        }

        // =========================================================
        // CONTRATOS
        // =========================================================

        public IActionResult Contratos(string? buscar)
        {
            var contratos = _context.Contratos
                .Include(c => c.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(c => c.Cliente)
                    .ThenInclude(c => c.Tarifa)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                contratos = contratos.Where(c =>
                    c.Cliente.Usuario.NombreUsuario.Contains(buscar) ||
                    c.Cliente.Usuario.Correo.Contains(buscar) ||
                    c.Estado.Contains(buscar) ||
                    c.IdContrato.ToString().Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            return View(
                contratos
                    .OrderByDescending(c => c.IdContrato)
                    .ToList()
            );
        }

        public IActionResult AprobarContrato(int id)
        {
            var contrato = _context.Contratos
                .Include(c => c.Cliente)
                .FirstOrDefault(c =>
                    c.IdContrato == id);

            if (contrato == null ||
                contrato.Estado != "Pendiente")
            {
                return RedirectToAction(nameof(Contratos));
            }

            contrato.Estado = "Activo";

            var solicitud = _context.SolicitudesServicio
                .Where(s =>
                    s.IdCliente == contrato.IdCliente &&
                    s.Estado == "Contrato Generado")
                .OrderByDescending(s => s.FechaSolicitud)
                .FirstOrDefault();

            if (solicitud != null)
            {
                solicitud.Estado = "Contrato Aprobado";

                solicitud.ObservacionAdministrador =
                    "Contrato aprobado. Pendiente de asignación para instalación.";
            }

            var solicitudContrato =
                _context.SolicitudesContrato
                    .Where(s =>
                        s.IdCliente == contrato.IdCliente &&
                        s.Estado == "Contrato Generado")
                    .OrderByDescending(s => s.FechaSolicitud)
                    .FirstOrDefault();

            if (solicitudContrato != null)
            {
                solicitudContrato.Estado = "Finalizada";

                solicitudContrato.ObservacionAdministrador =
                    "Contrato generado y aprobado correctamente.";
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Contratos));
        }

        public IActionResult RechazarContrato(int id)
        {
            var contrato = _context.Contratos
                .FirstOrDefault(c =>
                    c.IdContrato == id);

            if (contrato == null ||
                contrato.Estado != "Pendiente")
            {
                return RedirectToAction(nameof(Contratos));
            }

            contrato.Estado = "Rechazado";

            var solicitud = _context.SolicitudesServicio
                .Where(s =>
                    s.IdCliente == contrato.IdCliente &&
                    s.Estado == "Contrato Generado")
                .OrderByDescending(s => s.FechaSolicitud)
                .FirstOrDefault();

            if (solicitud != null)
            {
                solicitud.Estado = "Contrato Rechazado";

                solicitud.ObservacionAdministrador =
                    "El contrato fue rechazado por el administrador.";
            }

            var solicitudContrato =
                _context.SolicitudesContrato
                    .Where(s =>
                        s.IdCliente == contrato.IdCliente &&
                        s.Estado == "Contrato Generado")
                    .OrderByDescending(s => s.FechaSolicitud)
                    .FirstOrDefault();

            if (solicitudContrato != null)
            {
                solicitudContrato.Estado = "Contrato Rechazado";

                solicitudContrato.ObservacionAdministrador =
                    "El contrato generado fue rechazado.";
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Contratos));
        }
    }
}
