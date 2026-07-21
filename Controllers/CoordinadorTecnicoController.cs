using InapaWeb.Data;
using InapaWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InapaWeb.Controllers
{
    public class CoordinadorTecnicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinadorTecnicoController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // PANEL PRINCIPAL
        // =========================================================

        public IActionResult Index()
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            int? usuarioId =
                HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirigirAlLogin();
            }

            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == usuarioId.Value);

            if (usuario == null)
            {
                HttpContext.Session.Clear();

                return RedirigirAlLogin();
            }

            ViewBag.NombreUsuario =
                usuario.NombreUsuario;

            ViewBag.DebeCambiarClave =
                usuario.DebeCambiarClave;

            // =====================================================
            // CONTADORES DEL PANEL
            // =====================================================

            int totalPendientesAsignacion =
                _context.SolicitudesServicio
                    .Count(s =>
                        s.Estado == "Aprobada" &&
                        !_context.AsignacionesTecnicos.Any(a =>
                            a.IdSolicitud == s.IdSolicitud &&
                            a.Estado != "Finalizado"));

            int totalAsignadas =
                _context.AsignacionesTecnicos
                    .Count(a =>
                        a.Estado == "Asignado" ||
                        a.Estado == "En proceso" ||
                        a.Estado == "En Proceso");

            int totalLevantamientosFinalizados =
                _context.AsignacionesTecnicos
                    .Count(a =>
                        a.Estado == "Finalizado");

            int totalTecnicosActivos =
                _context.Usuarios
                    .Count(u =>
                        u.Rol == "Técnico" &&
                        u.Estado == "Activo");

            ViewBag.TotalPendientesAsignacion =
                totalPendientesAsignacion;

            ViewBag.TotalAsignadas =
                totalAsignadas;

            ViewBag.TotalLevantamientosFinalizados =
                totalLevantamientosFinalizados;

            ViewBag.TotalTecnicosActivos =
                totalTecnicosActivos;

            // =====================================================
            // NOTIFICACIONES
            // =====================================================

            int totalAveriasNuevas =
                _context.Averias.Count(a =>
                    a.Estado == "Pendiente" ||
                    a.Estado == "Reportada" ||
                    a.Estado == "Nueva");

            int totalReclamacionesNuevas =
                _context.Reclamaciones.Count(r =>
                    r.Estado == "Pendiente" ||
                    r.Estado == "Reportada" ||
                    r.Estado == "Nueva");

            int totalSolicitudesNotificacion =
                totalPendientesAsignacion;

            int totalFinalizadosNotificacion =
                _context.AsignacionesTecnicos.Count(a =>
                    a.Estado == "Finalizado");

            int totalNotificaciones =
                totalAveriasNuevas +
                totalReclamacionesNuevas +
                totalSolicitudesNotificacion +
                totalFinalizadosNotificacion +
                (usuario.DebeCambiarClave ? 1 : 0);

            ViewBag.TotalAveriasNuevas =
                totalAveriasNuevas;

            ViewBag.TotalReclamacionesNuevas =
                totalReclamacionesNuevas;

            ViewBag.TotalSolicitudesNotificacion =
                totalSolicitudesNotificacion;

            ViewBag.TotalFinalizadosNotificacion =
                totalFinalizadosNotificacion;

            ViewBag.TotalNotificaciones =
                totalNotificaciones;

            // =====================================================
            // SOLICITUDES PENDIENTES RECIENTES
            // =====================================================

            ViewBag.SolicitudesPendientes =
                _context.SolicitudesServicio
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .Where(s =>
                        s.Estado == "Aprobada" &&
                        !_context.AsignacionesTecnicos.Any(a =>
                            a.IdSolicitud == s.IdSolicitud &&
                            a.Estado != "Finalizado"))
                    .OrderByDescending(s =>
                        s.FechaSolicitud)
                    .Take(5)
                    .ToList();

            // =====================================================
            // TRABAJOS ACTIVOS RECIENTES
            // =====================================================

            ViewBag.TrabajosActivos =
                _context.AsignacionesTecnicos
                    .Include(a => a.Tecnico)
                    .Include(a => a.SolicitudServicio)
                        .ThenInclude(s => s.Cliente)
                            .ThenInclude(c => c.Usuario)
                    .Where(a =>
                        a.Estado == "Asignado" ||
                        a.Estado == "En proceso" ||
                        a.Estado == "En Proceso")
                    .OrderByDescending(a =>
                        a.FechaAsignacion)
                    .Take(5)
                    .ToList();

            return View();
        }

        // =========================================================
        // SOLICITUDES PENDIENTES
        // =========================================================

        public IActionResult SolicitudesPendientes(
            string? buscar)
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            var consulta =
                _context.SolicitudesServicio
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .Where(s =>
                        s.Estado == "Aprobada" &&
                        !_context.AsignacionesTecnicos.Any(a =>
                            a.IdSolicitud == s.IdSolicitud &&
                            a.Estado != "Finalizado"))
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(s =>
                    s.Cliente.Usuario.NombreUsuario.Contains(buscar) ||
                    s.TipoSolicitud.Contains(buscar) ||
                    s.Descripcion.Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            var solicitudes = consulta
                .OrderByDescending(s =>
                    s.FechaSolicitud)
                .ToList();

            return View(solicitudes);
        }

        // =========================================================
        // DETALLE DE SOLICITUD
        // =========================================================

        public IActionResult DetalleSolicitud(int id)
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            var solicitud =
                _context.SolicitudesServicio
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Tarifa)
                    .FirstOrDefault(s =>
                        s.IdSolicitud == id);

            if (solicitud == null)
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud no fue encontrada.";

                return RedirectToAction(
                    nameof(SolicitudesPendientes));
            }

            return View(solicitud);
        }

        // =========================================================
        // ASIGNAR TÉCNICO - GET
        // =========================================================

        [HttpGet]
        public IActionResult AsignarTecnico(int id)
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            var solicitud =
                _context.SolicitudesServicio
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .FirstOrDefault(s =>
                        s.IdSolicitud == id);

            if (solicitud == null)
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud no fue encontrada.";

                return RedirectToAction(
                    nameof(SolicitudesPendientes));
            }

            if (solicitud.Estado != "Aprobada")
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud no está disponible para asignación.";

                return RedirectToAction(
                    nameof(SolicitudesPendientes));
            }

            bool yaAsignada =
                _context.AsignacionesTecnicos.Any(a =>
                    a.IdSolicitud == id &&
                    a.Estado != "Finalizado");

            if (yaAsignada)
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud ya tiene un técnico asignado.";

                return RedirectToAction(
                    nameof(TrabajosAsignados));
            }

            var tecnicos =
                _context.Usuarios
                    .Where(u =>
                        u.Rol == "Técnico" &&
                        u.Estado == "Activo")
                    .OrderBy(u =>
                        u.NombreUsuario)
                    .ToList();

            ViewBag.Tecnicos = tecnicos;

            return View(solicitud);
        }

        // =========================================================
        // ASIGNAR TÉCNICO - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AsignarTecnico(
            int idSolicitud,
            int idTecnico,
            string? observacion)
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            var solicitud =
                _context.SolicitudesServicio
                    .FirstOrDefault(s =>
                        s.IdSolicitud == idSolicitud);

            if (solicitud == null)
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud no fue encontrada.";

                return RedirectToAction(
                    nameof(SolicitudesPendientes));
            }

            if (solicitud.Estado != "Aprobada")
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud no está disponible para asignación.";

                return RedirectToAction(
                    nameof(SolicitudesPendientes));
            }

            if (idTecnico <= 0)
            {
                TempData["ErrorCoordinador"] =
                    "Debe seleccionar un técnico.";

                return RedirectToAction(
                    nameof(AsignarTecnico),
                    new { id = idSolicitud });
            }

            bool yaAsignada =
                _context.AsignacionesTecnicos.Any(a =>
                    a.IdSolicitud == idSolicitud &&
                    a.Estado != "Finalizado");

            if (yaAsignada)
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud ya tiene un técnico asignado.";

                return RedirectToAction(
                    nameof(TrabajosAsignados));
            }

            var tecnico =
                _context.Usuarios
                    .FirstOrDefault(u =>
                        u.IdUsuario == idTecnico &&
                        u.Rol == "Técnico" &&
                        u.Estado == "Activo");

            if (tecnico == null)
            {
                TempData["ErrorCoordinador"] =
                    "El técnico seleccionado no está disponible.";

                return RedirectToAction(
                    nameof(AsignarTecnico),
                    new { id = idSolicitud });
            }

            using var transaccion =
                _context.Database.BeginTransaction();

            try
            {
                var asignacion =
                    new AsignacionTecnico
                    {
                        IdSolicitud = idSolicitud,
                        IdTecnico = tecnico.IdUsuario,
                        TipoTrabajo = "Levantamiento",
                        FechaAsignacion = DateTime.Now,
                        Estado = "Asignado",

                        Observacion =
                            string.IsNullOrWhiteSpace(observacion)
                                ? null
                                : observacion.Trim()
                    };

                _context.AsignacionesTecnicos.Add(asignacion);

                solicitud.Estado = "Asignada";

                solicitud.ObservacionAdministrador =
                    $"Solicitud asignada al técnico {tecnico.NombreUsuario}.";

                _context.SaveChanges();

                transaccion.Commit();

                TempData["MensajeCoordinador"] =
                    $"La solicitud fue asignada a {tecnico.NombreUsuario}.";

                return RedirectToAction(
                    nameof(TrabajosAsignados));
            }
            catch (Exception ex)
            {
                transaccion.Rollback();

                TempData["ErrorCoordinador"] =
                    "No se pudo realizar la asignación. " +
                    ObtenerMensajeError(ex);

                return RedirectToAction(
                    nameof(AsignarTecnico),
                    new { id = idSolicitud });
            }
        }

        // =========================================================
        // ASIGNACIÓN AUTOMÁTICA
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AsignarAutomaticamente(
            int idSolicitud)
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            var solicitud =
                _context.SolicitudesServicio
                    .FirstOrDefault(s =>
                        s.IdSolicitud == idSolicitud);

            if (solicitud == null)
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud no fue encontrada.";

                return RedirectToAction(
                    nameof(SolicitudesPendientes));
            }

            if (solicitud.Estado != "Aprobada")
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud no está disponible para asignación.";

                return RedirectToAction(
                    nameof(SolicitudesPendientes));
            }

            bool yaAsignada =
                _context.AsignacionesTecnicos.Any(a =>
                    a.IdSolicitud == idSolicitud &&
                    a.Estado != "Finalizado");

            if (yaAsignada)
            {
                TempData["ErrorCoordinador"] =
                    "La solicitud ya tiene un técnico asignado.";

                return RedirectToAction(
                    nameof(TrabajosAsignados));
            }

            var tecnico =
                _context.Usuarios
                    .Where(u =>
                        u.Rol == "Técnico" &&
                        u.Estado == "Activo")
                    .Select(u => new
                    {
                        Usuario = u,

                        CantidadTrabajos =
                            _context.AsignacionesTecnicos.Count(a =>
                                a.IdTecnico == u.IdUsuario &&
                                (
                                    a.Estado == "Asignado" ||
                                    a.Estado == "En proceso" ||
                                    a.Estado == "En Proceso"
                                ))
                    })
                    .OrderBy(x =>
                        x.CantidadTrabajos)
                    .ThenBy(x =>
                        x.Usuario.NombreUsuario)
                    .Select(x =>
                        x.Usuario)
                    .FirstOrDefault();

            if (tecnico == null)
            {
                TempData["ErrorCoordinador"] =
                    "No existen técnicos activos disponibles.";

                return RedirectToAction(
                    nameof(SolicitudesPendientes));
            }

            using var transaccion =
                _context.Database.BeginTransaction();

            try
            {
                var asignacion =
                    new AsignacionTecnico
                    {
                        IdSolicitud = idSolicitud,
                        IdTecnico = tecnico.IdUsuario,
                        TipoTrabajo = "Levantamiento",
                        FechaAsignacion = DateTime.Now,
                        Estado = "Asignado",

                        Observacion =
                            "Asignación automática realizada según la menor carga de trabajo."
                    };

                _context.AsignacionesTecnicos.Add(asignacion);

                solicitud.Estado = "Asignada";

                solicitud.ObservacionAdministrador =
                    $"Solicitud asignada automáticamente al técnico {tecnico.NombreUsuario}.";

                _context.SaveChanges();

                transaccion.Commit();

                TempData["MensajeCoordinador"] =
                    $"La solicitud fue asignada automáticamente a {tecnico.NombreUsuario}.";

                return RedirectToAction(
                    nameof(TrabajosAsignados));
            }
            catch (Exception ex)
            {
                transaccion.Rollback();

                TempData["ErrorCoordinador"] =
                    "No se pudo realizar la asignación automática. " +
                    ObtenerMensajeError(ex);

                return RedirectToAction(
                    nameof(SolicitudesPendientes));
            }
        }

        // =========================================================
        // TRABAJOS ASIGNADOS
        // =========================================================

        public IActionResult TrabajosAsignados(
            string estado = "Todos",
            string? buscar = null)
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            var consulta =
                _context.AsignacionesTecnicos
                    .Include(a => a.Tecnico)
                    .Include(a => a.SolicitudServicio)
                        .ThenInclude(s => s.Cliente)
                            .ThenInclude(c => c.Usuario)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado) &&
                estado != "Todos")
            {
                consulta = consulta.Where(a =>
                    a.Estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(a =>
                    a.Tecnico.NombreUsuario.Contains(buscar) ||

                    a.SolicitudServicio
                        .Cliente
                        .Usuario
                        .NombreUsuario
                        .Contains(buscar) ||

                    a.SolicitudServicio
                        .TipoSolicitud
                        .Contains(buscar) ||

                    a.Estado.Contains(buscar));
            }

            ViewBag.Estado = estado;
            ViewBag.EstadoSeleccionado = estado;
            ViewBag.Buscar = buscar;

            var asignaciones = consulta
                .OrderByDescending(a =>
                    a.FechaAsignacion)
                .ToList();

            return View(asignaciones);
        }

        // =========================================================
        // DETALLE DE ASIGNACIÓN
        // =========================================================

        public IActionResult DetalleAsignacion(int id)
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            var asignacion =
                _context.AsignacionesTecnicos
                    .Include(a => a.Tecnico)
                    .Include(a => a.SolicitudServicio)
                        .ThenInclude(s => s.Cliente)
                            .ThenInclude(c => c.Usuario)
                    .FirstOrDefault(a =>
                        a.IdAsignacion == id);

            if (asignacion == null)
            {
                TempData["ErrorCoordinador"] =
                    "La asignación no fue encontrada.";

                return RedirectToAction(
                    nameof(TrabajosAsignados));
            }

            return View(asignacion);
        }

        // =========================================================
        // REASIGNAR TÉCNICO - GET
        // =========================================================

        [HttpGet]
        public IActionResult ReasignarTecnico(int id)
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            var asignacion =
                _context.AsignacionesTecnicos
                    .Include(a => a.Tecnico)
                    .Include(a => a.SolicitudServicio)
                        .ThenInclude(s => s.Cliente)
                            .ThenInclude(c => c.Usuario)
                    .FirstOrDefault(a =>
                        a.IdAsignacion == id);

            if (asignacion == null)
            {
                TempData["ErrorCoordinador"] =
                    "La asignación no fue encontrada.";

                return RedirectToAction(
                    nameof(TrabajosAsignados));
            }

            if (asignacion.Estado == "Finalizado")
            {
                TempData["ErrorCoordinador"] =
                    "No se puede reasignar un trabajo finalizado.";

                return RedirectToAction(
                    nameof(DetalleAsignacion),
                    new { id });
            }

            var tecnicos =
                _context.Usuarios
                    .Where(u =>
                        u.Rol == "Técnico" &&
                        u.Estado == "Activo" &&
                        u.IdUsuario != asignacion.IdTecnico)
                    .OrderBy(u =>
                        u.NombreUsuario)
                    .ToList();

            ViewBag.Tecnicos = tecnicos;

            return View(asignacion);
        }

        // =========================================================
        // REASIGNAR TÉCNICO - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReasignarTecnico(
            int idAsignacion,
            int idTecnico,
            string? observacion)
        {
            if (!EsCoordinador())
            {
                return RedirigirAlLogin();
            }

            var asignacion =
                _context.AsignacionesTecnicos
                    .Include(a =>
                        a.SolicitudServicio)
                    .FirstOrDefault(a =>
                        a.IdAsignacion == idAsignacion);

            if (asignacion == null)
            {
                TempData["ErrorCoordinador"] =
                    "La asignación no fue encontrada.";

                return RedirectToAction(
                    nameof(TrabajosAsignados));
            }

            if (asignacion.Estado == "Finalizado")
            {
                TempData["ErrorCoordinador"] =
                    "No se puede reasignar un trabajo finalizado.";

                return RedirectToAction(
                    nameof(DetalleAsignacion),
                    new { id = idAsignacion });
            }

            if (idTecnico <= 0)
            {
                TempData["ErrorCoordinador"] =
                    "Debe seleccionar un técnico.";

                return RedirectToAction(
                    nameof(ReasignarTecnico),
                    new { id = idAsignacion });
            }

            var tecnico =
                _context.Usuarios
                    .FirstOrDefault(u =>
                        u.IdUsuario == idTecnico &&
                        u.Rol == "Técnico" &&
                        u.Estado == "Activo");

            if (tecnico == null)
            {
                TempData["ErrorCoordinador"] =
                    "El técnico seleccionado no está disponible.";

                return RedirectToAction(
                    nameof(ReasignarTecnico),
                    new { id = idAsignacion });
            }

            asignacion.IdTecnico =
                tecnico.IdUsuario;

            asignacion.FechaAsignacion =
                DateTime.Now;

            asignacion.Estado =
                "Asignado";

            asignacion.FechaFinalizacion =
                null;

            asignacion.Resultado =
                null;

            asignacion.Observacion =
                string.IsNullOrWhiteSpace(observacion)
                    ? $"Trabajo reasignado a {tecnico.NombreUsuario}."
                    : observacion.Trim();

            asignacion.SolicitudServicio.Estado =
                "Asignada";

            asignacion.SolicitudServicio
                .ObservacionAdministrador =
                $"Trabajo reasignado al técnico {tecnico.NombreUsuario}.";

            _context.SaveChanges();

            TempData["MensajeCoordinador"] =
                $"El trabajo fue reasignado a {tecnico.NombreUsuario}.";

            return RedirectToAction(
                nameof(DetalleAsignacion),
                new { id = idAsignacion });
        }

        // =========================================================
        // VALIDAR COORDINADOR
        // =========================================================

        private bool EsCoordinador()
        {
            string? rol =
                HttpContext.Session
                    .GetString("RolUsuario");

            return rol == "Coordinador Técnico";
        }

        // =========================================================
        // REDIRECCIÓN AL LOGIN
        // =========================================================

        private RedirectToActionResult RedirigirAlLogin()
        {
            return RedirectToAction(
                "Login",
                "Acceso");
        }

        // =========================================================
        // EXTRAER ERROR
        // =========================================================

        private static string ObtenerMensajeError(
            Exception ex)
        {
            return ex.InnerException?.Message
                   ?? ex.Message;
        }
    }
}
