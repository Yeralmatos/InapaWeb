using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InapaWeb.Data;
using InapaWeb.Models;

namespace InapaWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }


        private bool EsAdministrador()
        {
            return HttpContext.Session.GetString("RolUsuario")
                   == "Administrador";
        }

        private IActionResult RedirigirAlLogin()
        {
            return RedirectToAction("Login", "Acceso");
        }


        public IActionResult Index()
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            ViewBag.NombreUsuario =
                HttpContext.Session.GetString("NombreUsuario")
                ?? "Administrador";

            ViewBag.TotalUsuarios =
                _context.Usuarios.Count();

            ViewBag.TotalContratos =
                _context.Contratos.Count();

            ViewBag.TotalAverias =
                _context.Averias.Count();

            ViewBag.TotalReclamaciones =
                _context.Reclamaciones.Count();

            ViewBag.TotalFacturas =
                _context.Facturas.Count();

            ViewBag.TotalSolicitudes =
                _context.SolicitudesServicio
                    .Count(s => s.Estado == "Pendiente");

            ViewBag.TotalSolicitudesContrato =
                _context.SolicitudesContrato
                    .Count(s => s.Estado == "Pendiente");

            ViewBag.SolicitudesRecientes =
                _context.SolicitudesServicio
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .Where(s => s.Estado == "Pendiente")
                    .OrderByDescending(s => s.FechaSolicitud)
                    .Take(5)
                    .ToList();

            /*
             * Estas solicitudes se muestran únicamente
             * como información administrativa.
             *
             * La asignación será realizada por el
             * Coordinador Técnico.
             */
            ViewBag.SolicitudesParaAsignar =
                _context.SolicitudesServicio
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
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var usuarios =
                _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

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
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            return View(new Usuario
            {
                Estado = "Activo",
                OrigenRegistro = "Oficina",
                DebeCambiarClave = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearUsuario(Usuario usuario)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            PrepararUsuarioInterno(usuario);

            if (string.IsNullOrWhiteSpace(usuario.NombreUsuario) ||
                string.IsNullOrWhiteSpace(usuario.Correo) ||
                string.IsNullOrWhiteSpace(usuario.Contrasena) ||
                string.IsNullOrWhiteSpace(usuario.Rol))
            {
                ViewBag.Error =
                    "Debe completar todos los campos obligatorios.";

                return View(usuario);
            }

            usuario.NombreUsuario =
                usuario.NombreUsuario.Trim();

            usuario.Correo =
                usuario.Correo.Trim().ToLowerInvariant();

            usuario.Rol =
                usuario.Rol.Trim();

            string contrasenaTemporal =
                usuario.Contrasena.Trim();

            if (contrasenaTemporal.Length < 8)
            {
                ViewBag.Error =
                    "La contraseña debe tener al menos 8 caracteres.";

                return View(usuario);
            }

            bool correoExiste =
                _context.Usuarios.Any(u =>
                    u.Correo.ToLower() ==
                    usuario.Correo.ToLower());

            if (correoExiste)
            {
                ViewBag.Error =
                    "Ya existe un usuario registrado con ese correo.";

                return View(usuario);
            }

            string[] rolesPermitidos =
            {
                "Técnico",
                "Coordinador Técnico",
                "Cajero",
                "Supervisor",
                "AtencionCliente"
            };

            if (!rolesPermitidos.Contains(usuario.Rol))
            {
                ViewBag.Error =
                    "El rol seleccionado no es válido.";

                return View(usuario);
            }

            /*
             * El Administrador crea solamente usuarios internos.
             *
             * Los clientes registrados presencialmente serán
             * creados desde Atención al Cliente.
             */
            usuario.Estado = "Activo";
            usuario.OrigenRegistro = "Oficina";
            usuario.DebeCambiarClave = false;

            usuario.Contrasena =
                _passwordHasher.HashPassword(
                    usuario,
                    contrasenaTemporal
                );

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            TempData["MensajeUsuario"] =
                "Usuario interno creado correctamente.";

            return RedirectToAction(nameof(Usuarios));
        }

        private static void PrepararUsuarioInterno(
            Usuario usuario)
        {
            usuario.Estado = "Activo";
            usuario.OrigenRegistro = "Oficina";
            usuario.DebeCambiarClave = false;
        }


        public IActionResult AprobarUsuario(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var usuario =
                _context.Usuarios
                    .FirstOrDefault(u =>
                        u.IdUsuario == id);

            if (usuario == null)
            {
                TempData["ErrorUsuario"] =
                    "El usuario seleccionado no existe.";

                return RedirectToAction(nameof(Usuarios));
            }

            usuario.Estado = "Activo";

            var cliente =
                _context.Clientes
                    .FirstOrDefault(c =>
                        c.IdUsuario == id);

            if (cliente != null)
            {
                cliente.EstadoCliente = "Activo";
            }

            _context.SaveChanges();

            TempData["MensajeUsuario"] =
                "Usuario aprobado correctamente.";

            return RedirectToAction(nameof(Usuarios));
        }



        public IActionResult RechazarUsuario(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var usuario =
                _context.Usuarios
                    .FirstOrDefault(u =>
                        u.IdUsuario == id);

            if (usuario == null)
            {
                TempData["ErrorUsuario"] =
                    "El usuario seleccionado no existe.";

                return RedirectToAction(nameof(Usuarios));
            }

            usuario.Estado = "Rechazado";

            var cliente =
                _context.Clientes
                    .FirstOrDefault(c =>
                        c.IdUsuario == id);

            if (cliente != null)
            {
                cliente.EstadoCliente = "Rechazado";
            }

            _context.SaveChanges();

            TempData["MensajeUsuario"] =
                "Usuario rechazado correctamente.";

            return RedirectToAction(nameof(Usuarios));
        }



        public IActionResult CambiarEstado(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            int? usuarioActualId =
                HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioActualId == id)
            {
                TempData["ErrorUsuario"] =
                    "No puede cambiar el estado de su propia cuenta.";

                return RedirectToAction(nameof(Usuarios));
            }

            var usuario =
                _context.Usuarios
                    .FirstOrDefault(u =>
                        u.IdUsuario == id);

            if (usuario == null)
            {
                TempData["ErrorUsuario"] =
                    "El usuario seleccionado no existe.";

                return RedirectToAction(nameof(Usuarios));
            }

            usuario.Estado =
                usuario.Estado == "Activo"
                    ? "Inactivo"
                    : "Activo";

            var cliente =
                _context.Clientes
                    .FirstOrDefault(c =>
                        c.IdUsuario == id);

            if (cliente != null)
            {
                cliente.EstadoCliente =
                    usuario.Estado;
            }

            _context.SaveChanges();

            TempData["MensajeUsuario"] =
                "Estado actualizado correctamente.";

            return RedirectToAction(nameof(Usuarios));
        }


        public IActionResult DetalleUsuario(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var usuario =
                _context.Usuarios
                    .FirstOrDefault(u =>
                        u.IdUsuario == id);

            if (usuario == null)
            {
                TempData["ErrorUsuario"] =
                    "El usuario seleccionado no existe.";

                return RedirectToAction(nameof(Usuarios));
            }

            ViewBag.Cliente =
                _context.Clientes
                    .Include(c => c.Tarifa)
                    .FirstOrDefault(c =>
                        c.IdUsuario == id);

            return View(usuario);
        }



        public IActionResult EliminarUsuario(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            int? usuarioActualId =
                HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioActualId == id)
            {
                TempData["ErrorUsuario"] =
                    "No puede eliminar su propia cuenta.";

                return RedirectToAction(nameof(Usuarios));
            }

            var usuario =
                _context.Usuarios
                    .FirstOrDefault(u =>
                        u.IdUsuario == id);

            if (usuario == null)
            {
                TempData["ErrorUsuario"] =
                    "El usuario seleccionado no existe.";

                return RedirectToAction(nameof(Usuarios));
            }

            var cliente =
                _context.Clientes
                    .FirstOrDefault(c =>
                        c.IdUsuario == id);

            if (cliente != null)
            {
                bool tieneSolicitudesServicio =
                    _context.SolicitudesServicio.Any(s =>
                        s.IdCliente == cliente.IdCliente);

                bool tieneSolicitudesContrato =
                    _context.SolicitudesContrato.Any(s =>
                        s.IdCliente == cliente.IdCliente);

                bool tieneContratos =
                    _context.Contratos.Any(c =>
                        c.IdCliente == cliente.IdCliente);

                if (tieneSolicitudesServicio ||
                    tieneSolicitudesContrato ||
                    tieneContratos)
                {
                    cliente.EstadoCliente = "Inactivo";
                    usuario.Estado = "Inactivo";

                    _context.SaveChanges();

                    TempData["MensajeUsuario"] =
                        "El usuario posee información relacionada y fue desactivado.";

                    return RedirectToAction(nameof(Usuarios));
                }

                _context.Clientes.Remove(cliente);
            }

            bool tieneAsignaciones =
                _context.AsignacionesTecnicos.Any(a =>
                    a.IdTecnico == id);

            if (tieneAsignaciones)
            {
                usuario.Estado = "Inactivo";

                _context.SaveChanges();

                TempData["MensajeUsuario"] =
                    "El usuario posee trabajos relacionados y fue desactivado.";

                return RedirectToAction(nameof(Usuarios));
            }

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            TempData["MensajeUsuario"] =
                "Usuario eliminado correctamente.";

            return RedirectToAction(nameof(Usuarios));
        }


        public IActionResult Solicitudes(
            string? buscar,
            string estado = "Pendiente")
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var solicitudes =
                _context.SolicitudesServicio
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado) &&
                estado != "Todos")
            {
                solicitudes =
                    solicitudes.Where(s =>
                        s.Estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                solicitudes = solicitudes.Where(s =>
                    s.Cliente.Usuario.NombreUsuario.Contains(buscar) ||
                    s.TipoSolicitud.Contains(buscar) ||
                    s.Estado.Contains(buscar));
            }

            ViewBag.Buscar = buscar;
            ViewBag.EstadoSeleccionado = estado;

            ViewBag.TotalPendientes =
                _context.SolicitudesServicio
                    .Count(s => s.Estado == "Pendiente");

            ViewBag.TotalAprobadas =
                _context.SolicitudesServicio
                    .Count(s => s.Estado == "Aprobada");

            ViewBag.TotalAsignadas =
                _context.SolicitudesServicio
                    .Count(s => s.Estado == "Asignada");

            return View(
                solicitudes
                    .OrderByDescending(s => s.FechaSolicitud)
                    .ToList()
            );
        }



        public IActionResult AprobarSolicitud(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var solicitud =
                _context.SolicitudesServicio
                    .FirstOrDefault(s =>
                        s.IdSolicitud == id);

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
                "Solicitud aprobada. Pendiente de coordinación técnica.";

            _context.SaveChanges();

            TempData["MensajeSolicitud"] =
                "Solicitud aprobada correctamente. El Coordinador Técnico podrá realizar la asignación.";

            return RedirectToAction(
                nameof(Solicitudes),
                new { estado = "Aprobada" }
            );
        }



        public IActionResult RechazarSolicitud(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var solicitud =
                _context.SolicitudesServicio
                    .FirstOrDefault(s =>
                        s.IdSolicitud == id);

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
                "Solicitud rechazada correctamente.";

            return RedirectToAction(nameof(Solicitudes));
        }



        public IActionResult DetalleSolicitud(int id)
        {
            if (!EsAdministrador())
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
                TempData["ErrorSolicitud"] =
                    "La solicitud seleccionada no existe.";

                return RedirectToAction(nameof(Solicitudes));
            }

            return View(solicitud);
        }



        public IActionResult SolicitudesContrato(
            string? buscar)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var solicitudes =
                _context.SolicitudesContrato
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

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


        public IActionResult AprobarSolicitudContrato(
            int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var solicitudContrato =
                _context.SolicitudesContrato
                    .Include(s => s.Cliente)
                    .FirstOrDefault(s =>
                        s.IdSolicitudContrato == id);

            if (solicitudContrato == null)
            {
                TempData["ErrorSolicitudContrato"] =
                    "La solicitud de contrato no existe.";

                return RedirectToAction(
                    nameof(SolicitudesContrato)
                );
            }

            if (solicitudContrato.Estado != "Pendiente")
            {
                TempData["ErrorSolicitudContrato"] =
                    "La solicitud de contrato ya fue procesada.";

                return RedirectToAction(
                    nameof(SolicitudesContrato)
                );
            }

            bool yaExisteSolicitudServicio =
                _context.SolicitudesServicio.Any(s =>
                    s.IdCliente ==
                    solicitudContrato.IdCliente &&
                    s.TipoSolicitud.Contains(
                        $"SC-{solicitudContrato.IdSolicitudContrato}") &&
                    s.Estado != "Rechazada");

            if (yaExisteSolicitudServicio)
            {
                TempData["ErrorSolicitudContrato"] =
                    "Esta solicitud ya generó un proceso de servicio.";

                return RedirectToAction(
                    nameof(SolicitudesContrato)
                );
            }

            using var transaccion =
                _context.Database.BeginTransaction();

            try
            {
                solicitudContrato.Estado = "Aprobada";

                solicitudContrato.ObservacionAdministrador =
                    "Solicitud aprobada. Pendiente de levantamiento técnico.";

                string descripcion =
                    $"Solicitud de contrato SC-{solicitudContrato.IdSolicitudContrato}. " +
                    $"Servicio: {solicitudContrato.TipoServicio}. " +
                    $"Dirección: {solicitudContrato.DireccionServicio}.";

                if (!string.IsNullOrWhiteSpace(
                    solicitudContrato.ObservacionCliente))
                {
                    descripcion +=
                        $" Observación del cliente: " +
                        solicitudContrato.ObservacionCliente;
                }

                var solicitudServicio =
                    new SolicitudServicio
                    {
                        IdCliente =
                            solicitudContrato.IdCliente,

                        TipoSolicitud =
                            $"Contrato SC-{solicitudContrato.IdSolicitudContrato}",

                        Descripcion = descripcion,

                        FechaSolicitud = DateTime.Now,

                        Estado = "Aprobada",

                        ObservacionAdministrador =
                            "Solicitud aprobada. Pendiente de coordinación técnica para el levantamiento."
                    };

                _context.SolicitudesServicio
                    .Add(solicitudServicio);

                _context.SaveChanges();

                transaccion.Commit();

                TempData["MensajeSolicitudContrato"] =
                    "Solicitud aprobada. Quedó disponible para el Coordinador Técnico.";

                return RedirectToAction(
                    nameof(SolicitudesContrato)
                );
            }
            catch (Exception)
            {
                transaccion.Rollback();

                TempData["ErrorSolicitudContrato"] =
                    "No fue posible aprobar la solicitud de contrato.";

                return RedirectToAction(
                    nameof(SolicitudesContrato)
                );
            }
        }



        public IActionResult RechazarSolicitudContrato(
            int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var solicitud =
                _context.SolicitudesContrato
                    .FirstOrDefault(s =>
                        s.IdSolicitudContrato == id);

            if (solicitud == null)
            {
                TempData["ErrorSolicitudContrato"] =
                    "La solicitud de contrato no existe.";

                return RedirectToAction(
                    nameof(SolicitudesContrato)
                );
            }

            if (solicitud.Estado != "Pendiente")
            {
                TempData["ErrorSolicitudContrato"] =
                    "La solicitud ya fue procesada anteriormente.";

                return RedirectToAction(
                    nameof(SolicitudesContrato)
                );
            }

            solicitud.Estado = "Rechazada";

            solicitud.ObservacionAdministrador =
                "Solicitud rechazada por el administrador.";

            _context.SaveChanges();

            TempData["MensajeSolicitudContrato"] =
                "Solicitud de contrato rechazada.";

            return RedirectToAction(
                nameof(SolicitudesContrato)
            );
        }



        public IActionResult ResultadoLevantamiento(
            int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var trabajo =
                _context.AsignacionesTecnicos
                    .Include(a => a.SolicitudServicio)
                        .ThenInclude(s => s.Cliente)
                            .ThenInclude(c => c.Usuario)
                    .Include(a => a.Tecnico)
                    .FirstOrDefault(a =>
                        a.IdSolicitud == id &&
                        a.TipoTrabajo == "Levantamiento");

            if (trabajo == null)
            {
                TempData["ErrorSolicitud"] =
                    "No se encontró el levantamiento solicitado.";

                return RedirectToAction(nameof(Solicitudes));
            }

            return View(trabajo);
        }


        public IActionResult CategorizarCliente(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var trabajo =
                _context.AsignacionesTecnicos
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
                TempData["ErrorSolicitud"] =
                    "El levantamiento no existe o todavía no ha finalizado.";

                return RedirectToAction(nameof(Solicitudes));
            }

            ViewBag.Trabajo = trabajo;

            ViewBag.Tarifas =
                _context.Tarifas
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
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            if (idTarifa <= 0)
            {
                TempData["ErrorSolicitud"] =
                    "Debe seleccionar una tarifa.";

                return RedirectToAction(
                    nameof(CategorizarCliente),
                    new { id = idSolicitud }
                );
            }

            var solicitud =
                _context.SolicitudesServicio
                    .Include(s => s.Cliente)
                    .FirstOrDefault(s =>
                        s.IdSolicitud == idSolicitud);

            if (solicitud == null ||
                solicitud.Estado !=
                "Levantamiento Finalizado")
            {
                TempData["ErrorSolicitud"] =
                    "La solicitud no está disponible para categorización.";

                return RedirectToAction(nameof(Solicitudes));
            }

            var tarifa =
                _context.Tarifas
                    .FirstOrDefault(t =>
                        t.IdTarifa == idTarifa);

            if (tarifa == null)
            {
                TempData["ErrorSolicitud"] =
                    "La tarifa seleccionada no existe.";

                return RedirectToAction(
                    nameof(CategorizarCliente),
                    new { id = idSolicitud }
                );
            }

            solicitud.Cliente.IdTarifa =
                tarifa.IdTarifa;

            solicitud.Estado =
                "Cliente Categorizado";

            solicitud.ObservacionAdministrador =
                $"Cliente categorizado con la tarifa: {tarifa.Descripcion}.";

            _context.SaveChanges();

            TempData["MensajeSolicitud"] =
                "Cliente categorizado correctamente.";

            return RedirectToAction(nameof(Solicitudes));
        }



        public IActionResult GenerarContrato(int id)
        {
            if (!EsAdministrador())
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

            if (solicitud == null ||
                solicitud.Estado !=
                "Cliente Categorizado")
            {
                TempData["ErrorContrato"] =
                    "La solicitud no está disponible para generar contrato.";

                return RedirectToAction(nameof(Solicitudes));
            }

            bool existeContrato =
                _context.Contratos.Any(c =>
                    c.IdCliente ==
                    solicitud.IdCliente &&
                    (
                        c.Estado == "Activo" ||
                        c.Estado == "Pendiente"
                    ));

            if (existeContrato)
            {
                TempData["ErrorContrato"] =
                    "El cliente ya posee un contrato activo o pendiente.";

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
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var solicitud =
                _context.SolicitudesServicio
                    .Include(s => s.Cliente)
                    .FirstOrDefault(s =>
                        s.IdSolicitud == idSolicitud);

            if (solicitud == null ||
                solicitud.Estado !=
                "Cliente Categorizado")
            {
                TempData["ErrorContrato"] =
                    "La solicitud no está disponible para generar contrato.";

                return RedirectToAction(nameof(Solicitudes));
            }

            if (solicitud.Cliente.IdTarifa == null)
            {
                TempData["ErrorContrato"] =
                    "El cliente debe tener una tarifa asignada.";

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

            bool existeContrato =
                _context.Contratos.Any(c =>
                    c.IdCliente ==
                    solicitud.IdCliente &&
                    (
                        c.Estado == "Activo" ||
                        c.Estado == "Pendiente"
                    ));

            if (existeContrato)
            {
                TempData["ErrorContrato"] =
                    "El cliente ya posee un contrato activo o pendiente.";

                return RedirectToAction(nameof(Solicitudes));
            }

            var contrato =
                new Contrato
                {
                    IdCliente =
                        solicitud.IdCliente,

                    FechaInicio =
                        fechaInicio,

                    FechaFin =
                        fechaFin,

                    Estado =
                        "Pendiente"
                };

            _context.Contratos.Add(contrato);

            solicitud.Estado =
                "Contrato Generado";

            solicitud.ObservacionAdministrador =
                "Contrato generado y pendiente de aprobación.";

            var solicitudContrato =
                _context.SolicitudesContrato
                    .Where(s =>
                        s.IdCliente ==
                        solicitud.IdCliente &&
                        s.Estado == "Aprobada")
                    .OrderByDescending(s =>
                        s.FechaSolicitud)
                    .FirstOrDefault();

            if (solicitudContrato != null)
            {
                solicitudContrato.Estado =
                    "Contrato Generado";

                solicitudContrato
                    .ObservacionAdministrador =
                    "El contrato fue generado y está pendiente de aprobación.";
            }

            _context.SaveChanges();

            TempData["MensajeContrato"] =
                "Contrato generado correctamente.";

            return RedirectToAction(nameof(Contratos));
        }



        public IActionResult Contratos(string? buscar)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var contratos =
                _context.Contratos
                    .Include(c => c.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .Include(c => c.Cliente)
                        .ThenInclude(c => c.Tarifa)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

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
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var contrato =
                _context.Contratos
                    .Include(c => c.Cliente)
                    .FirstOrDefault(c =>
                        c.IdContrato == id);

            if (contrato == null ||
                contrato.Estado != "Pendiente")
            {
                TempData["ErrorContrato"] =
                    "El contrato no existe o ya fue procesado.";

                return RedirectToAction(nameof(Contratos));
            }

            contrato.Estado = "Activo";

            var solicitud =
                _context.SolicitudesServicio
                    .Where(s =>
                        s.IdCliente ==
                        contrato.IdCliente &&
                        s.Estado == "Contrato Generado")
                    .OrderByDescending(s =>
                        s.FechaSolicitud)
                    .FirstOrDefault();

            if (solicitud != null)
            {
                solicitud.Estado =
                    "Contrato Aprobado";

                solicitud.ObservacionAdministrador =
                    "Contrato aprobado. Pendiente de coordinación técnica para la instalación.";
            }

            var solicitudContrato =
                _context.SolicitudesContrato
                    .Where(s =>
                        s.IdCliente ==
                        contrato.IdCliente &&
                        s.Estado ==
                        "Contrato Generado")
                    .OrderByDescending(s =>
                        s.FechaSolicitud)
                    .FirstOrDefault();

            if (solicitudContrato != null)
            {
                solicitudContrato.Estado =
                    "Finalizada";

                solicitudContrato
                    .ObservacionAdministrador =
                    "Contrato generado y aprobado correctamente.";
            }

            _context.SaveChanges();

            TempData["MensajeContrato"] =
                "Contrato aprobado correctamente.";

            return RedirectToAction(nameof(Contratos));
        }



        public IActionResult RechazarContrato(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }

            var contrato =
                _context.Contratos
                    .FirstOrDefault(c =>
                        c.IdContrato == id);

            if (contrato == null ||
                contrato.Estado != "Pendiente")
            {
                TempData["ErrorContrato"] =
                    "El contrato no existe o ya fue procesado.";

                return RedirectToAction(nameof(Contratos));
            }

            contrato.Estado = "Rechazado";


            var solicitud =
                _context.SolicitudesServicio
                    .Where(s =>
                        s.IdCliente ==
                        contrato.IdCliente &&
                        s.Estado ==
                        "Contrato Generado")
                    .OrderByDescending(s =>
                        s.FechaSolicitud)
                    .FirstOrDefault();


            if (solicitud != null)
            {
                solicitud.Estado =
                    "Contrato Rechazado";

                solicitud.ObservacionAdministrador =
                    "El contrato fue rechazado por el administrador.";
            }



            var solicitudContrato =
                _context.SolicitudesContrato
                    .Where(s =>
                        s.IdCliente ==
                        contrato.IdCliente &&
                        s.Estado ==
                        "Contrato Generado")
                    .OrderByDescending(s =>
                        s.FechaSolicitud)
                    .FirstOrDefault();


            if (solicitudContrato != null)
            {
                solicitudContrato.Estado =
                    "Contrato Rechazado";

                solicitudContrato.ObservacionAdministrador =
                    "El contrato generado fue rechazado.";
            }


            _context.SaveChanges();


            TempData["MensajeContrato"] =
                "Contrato rechazado correctamente.";


            return RedirectToAction(nameof(Contratos));
        }



        // ==========================================
        // MÓDULO DE RECLAMACIONES
        // ==========================================

        public IActionResult Reclamaciones()
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }


            ViewBag.NombreUsuario =
                HttpContext.Session.GetString("NombreUsuario")
                ?? "Administrador";


            ViewBag.TotalReclamaciones =
                _context.Reclamaciones.Count();


            ViewBag.ReclamacionesPendientes =
                _context.Reclamaciones
                    .Count(r => r.Estado == "Pendiente");


            ViewBag.ReclamacionesProceso =
                _context.Reclamaciones
                    .Count(r => r.Estado == "En Proceso");


            ViewBag.ReclamacionesFinalizadas =
                _context.Reclamaciones
                    .Count(r => r.Estado == "Finalizada");


            return View();
        }


        // ==========================================
        // RECLAMACIONES INDIVIDUALES
        // ==========================================

        public IActionResult Individual(string? buscar)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }


            var reclamaciones = _context.Reclamaciones
                .Include(r => r.Cliente)
                    .ThenInclude(c => c.Usuario)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                reclamaciones = reclamaciones.Where(r =>
                    r.Cliente.Usuario.NombreUsuario.Contains(buscar) ||
                    r.Descripcion.Contains(buscar) ||
                    r.Estado.Contains(buscar));
            }


            ViewBag.Buscar = buscar;


            return View(
                reclamaciones
                    .OrderByDescending(r => r.IdReclamacion)
                    .ToList()
            );
        }



        // ==========================================
        // DETALLE DE RECLAMACIÓN
        // ==========================================

        public IActionResult DetalleReclamacion(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirAlLogin();
            }


            var reclamacion = _context.Reclamaciones
                .Include(r => r.Cliente)
                    .ThenInclude(c => c.Usuario)
                .FirstOrDefault(r =>
                    r.IdReclamacion == id);


            if (reclamacion == null)
            {
                TempData["ErrorReclamacion"] =
                    "La reclamación no existe.";

                return RedirectToAction(nameof(Individual));
            }


            return View(reclamacion);
        }
    }
}
