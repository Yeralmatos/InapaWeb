using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using InapaWeb.Data;
using InapaWeb.Models;

namespace InapaWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AccesoController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        // =========================================================
        // INICIO DE SESIÓN
        // =========================================================

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error =
                    "Debe ingresar el correo y la contraseña.";

                return View();
            }

            correo = correo.Trim().ToLowerInvariant();

            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.Correo.ToLower() == correo &&
                    u.Estado == "Activo");

            if (usuario == null)
            {
                ViewBag.Error =
                    "Correo o contraseña incorrectos o cuenta no activa.";

                return View();
            }

            bool contrasenaCorrecta =
                VerificarContrasena(usuario, contrasena);

            if (!contrasenaCorrecta)
            {
                ViewBag.Error =
                    "Correo o contraseña incorrectos.";

                return View();
            }

            HttpContext.Session.SetInt32(
                "UsuarioId",
                usuario.IdUsuario
            );

            HttpContext.Session.SetString(
                "NombreUsuario",
                usuario.NombreUsuario
            );

            HttpContext.Session.SetString(
                "RolUsuario",
                usuario.Rol
            );

            /*
             * El cambio obligatorio se aplica únicamente cuando:
             *
             * 1. Es un cliente.
             * 2. Fue registrado presencialmente en una oficina.
             * 3. Todavía conserva la contraseña temporal.
             */
            if (usuario.Rol == "Cliente" &&
                usuario.OrigenRegistro == "Oficina" &&
                usuario.DebeCambiarClave)
            {
                return RedirectToAction("CambiarClave");
            }

            return RedirigirSegunRol(usuario.Rol);
        }

        // =========================================================
        // CAMBIO OBLIGATORIO DE CONTRASEÑA
        // =========================================================

        public IActionResult CambiarClave()
        {
            int? idUsuario =
                HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Login");
            }

            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == idUsuario.Value);

            if (usuario == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login");
            }

            bool requiereCambio =
                usuario.Rol == "Cliente" &&
                usuario.OrigenRegistro == "Oficina" &&
                usuario.DebeCambiarClave;

            if (!requiereCambio)
            {
                return RedirigirSegunRol(usuario.Rol);
            }

            ViewBag.NombreUsuario =
                usuario.NombreUsuario;

            return View();
        }

        [HttpPost]
        public IActionResult CambiarClave(
            string nuevaContrasena,
            string confirmarContrasena)
        {
            int? idUsuario =
                HttpContext.Session.GetInt32("UsuarioId");

            if (idUsuario == null)
            {
                return RedirectToAction("Login");
            }

            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == idUsuario.Value);

            if (usuario == null)
            {
                HttpContext.Session.Clear();

                return RedirectToAction("Login");
            }

            ViewBag.NombreUsuario =
                usuario.NombreUsuario;

            bool requiereCambio =
                usuario.Rol == "Cliente" &&
                usuario.OrigenRegistro == "Oficina" &&
                usuario.DebeCambiarClave;

            if (!requiereCambio)
            {
                return RedirigirSegunRol(usuario.Rol);
            }

            if (string.IsNullOrWhiteSpace(nuevaContrasena) ||
                string.IsNullOrWhiteSpace(confirmarContrasena))
            {
                ViewBag.Error =
                    "Debe completar los dos campos de contraseña.";

                return View();
            }

            if (nuevaContrasena.Length < 8)
            {
                ViewBag.Error =
                    "La contraseña debe tener al menos 8 caracteres.";

                return View();
            }

            if (nuevaContrasena != confirmarContrasena)
            {
                ViewBag.Error =
                    "Las contraseñas no coinciden.";

                return View();
            }

            if (VerificarContrasena(
                usuario,
                nuevaContrasena))
            {
                ViewBag.Error =
                    "La nueva contraseña debe ser diferente a la contraseña temporal.";

                return View();
            }

            usuario.Contrasena =
                _passwordHasher.HashPassword(
                    usuario,
                    nuevaContrasena
                );

            usuario.DebeCambiarClave = false;

            _context.SaveChanges();

            TempData["Mensaje"] =
                "Contraseña cambiada correctamente.";

            return RedirigirSegunRol(usuario.Rol);
        }

        // =========================================================
        // REGISTRO VIRTUAL DEL CLIENTE
        // =========================================================

        public IActionResult RegistroCliente()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegistroCliente(
            string nombreUsuario,
            string correo,
            string contrasena,
            string cedulaPasaporteRnc,
            string telefono,
            string provincia,
            string municipio,
            string? sector,
            string direccion)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena) ||
                string.IsNullOrWhiteSpace(cedulaPasaporteRnc) ||
                string.IsNullOrWhiteSpace(telefono) ||
                string.IsNullOrWhiteSpace(provincia) ||
                string.IsNullOrWhiteSpace(municipio) ||
                string.IsNullOrWhiteSpace(direccion))
            {
                ViewBag.Error =
                    "Debe completar todos los campos obligatorios.";

                return View();
            }

            nombreUsuario =
                nombreUsuario.Trim();

            correo =
                correo.Trim().ToLowerInvariant();

            cedulaPasaporteRnc =
                cedulaPasaporteRnc.Trim();

            telefono =
                telefono.Trim();

            provincia =
                provincia.Trim();

            municipio =
                municipio.Trim();

            direccion =
                direccion.Trim();

            sector =
                sector?.Trim();

            if (contrasena.Length < 8)
            {
                ViewBag.Error =
                    "La contraseña debe tener al menos 8 caracteres.";

                return View();
            }

            if (_context.Usuarios.Any(u =>
                u.Correo.ToLower() == correo))
            {
                ViewBag.Error =
                    "Ya existe una cuenta registrada con ese correo.";

                return View();
            }

            if (_context.Clientes.Any(c =>
                c.CedulaPasaporteRnc ==
                cedulaPasaporteRnc))
            {
                ViewBag.Error =
                    "Ya existe un cliente registrado con esa cédula, pasaporte o RNC.";

                return View();
            }

            using var transaccion =
                _context.Database.BeginTransaction();

            try
            {
                var usuario = new Usuario
                {
                    NombreUsuario = nombreUsuario,
                    Correo = correo,
                    Rol = "Cliente",
                    Estado = "Pendiente",

                    // Registro realizado por el ciudadano
                    OrigenRegistro = "Virtual",

                    // No utiliza contraseña temporal
                    DebeCambiarClave = false
                };

                usuario.Contrasena =
                    _passwordHasher.HashPassword(
                        usuario,
                        contrasena
                    );

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                var cliente = new Cliente
                {
                    IdUsuario = usuario.IdUsuario,

                    CedulaPasaporteRnc =
                        cedulaPasaporteRnc,

                    Telefono = telefono,
                    Provincia = provincia,
                    Municipio = municipio,
                    Sector = sector,
                    Direccion = direccion,

                    FechaRegistro = DateTime.Now,

                    // Esperará aprobación administrativa
                    EstadoCliente = "Pendiente"
                };

                _context.Clientes.Add(cliente);
                _context.SaveChanges();

                transaccion.Commit();

                ViewBag.Mensaje =
                    "Registro completado. Su cuenta está pendiente de aprobación por un administrador.";

                ModelState.Clear();

                return View();
            }
            catch (Exception ex)
            {
                transaccion.Rollback();

                string errorReal =
                    ex.InnerException?.Message
                    ?? ex.Message;

                ViewBag.Error =
                    "Error al registrar: " +
                    errorReal;

                return View();
            }
        }

        // =========================================================
        // CERRAR SESIÓN
        // =========================================================

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login",
                "Acceso"
            );
        }

        // =========================================================
        // VERIFICAR CONTRASEÑA
        // =========================================================

        private bool VerificarContrasena(
            Usuario usuario,
            string contrasenaIngresada)
        {
            try
            {
                PasswordVerificationResult resultado =
                    _passwordHasher.VerifyHashedPassword(
                        usuario,
                        usuario.Contrasena,
                        contrasenaIngresada
                    );

                if (resultado ==
                        PasswordVerificationResult.Success ||
                    resultado ==
                        PasswordVerificationResult.SuccessRehashNeeded)
                {
                    if (resultado ==
                        PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        usuario.Contrasena =
                            _passwordHasher.HashPassword(
                                usuario,
                                contrasenaIngresada
                            );

                        _context.SaveChanges();
                    }

                    return true;
                }
            }
            catch
            {
                /*
                 * Compatibilidad temporal con cuentas antiguas
                 * que todavía guardan la contraseña sin cifrar.
                 */
            }

            if (usuario.Contrasena ==
                contrasenaIngresada)
            {
                /*
                 * La contraseña antigua es correcta.
                 * Se cifra automáticamente para mejorar
                 * la seguridad de la cuenta.
                 */
                usuario.Contrasena =
                    _passwordHasher.HashPassword(
                        usuario,
                        contrasenaIngresada
                    );

                _context.SaveChanges();

                return true;
            }

            return false;
        }

        // =========================================================
        // REDIRECCIÓN SEGÚN EL ROL
        // =========================================================

        private IActionResult RedirigirSegunRol(
            string rol)
        {
            if (rol == "Administrador")
            {
                return RedirectToAction(
                    "Index",
                    "Admin"
                );
            }

            if (rol == "Cliente")
            {
                return RedirectToAction(
                    "Index",
                    "Cliente"
                );
            }

            if (rol == "Técnico")
            {
                return RedirectToAction(
                    "Index",
                    "Tecnico"
                );
            }

            if (rol == "Coordinador Técnico")
            {
                return RedirectToAction(
                    "Index",
                    "CoordinadorTecnico"
                );
            }

            if (rol == "Cajero")
            {
                return RedirectToAction(
                    "Index",
                    "Cajero"
                );
            }

            if (rol == "Supervisor")
            {
                return RedirectToAction(
                    "Index",
                    "Supervisor"
                );
            }

            if (rol == "AtencionCliente")
            {
                return RedirectToAction(
                    "Index",
                    "AtencionCliente"
                );
            }

            HttpContext.Session.Clear();

            TempData["ErrorLogin"] =
                "El rol del usuario no tiene acceso configurado.";

            return RedirectToAction("Login");
        }
    }
}
