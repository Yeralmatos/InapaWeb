using Microsoft.AspNetCore.Mvc;
using InapaWeb.Data;
using InapaWeb.Models;

namespace InapaWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccesoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == correo && u.Estado == "Activo");

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos o cuenta no activa.";
                return View();
            }

            if (usuario.Contrasena != contrasena)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View();
            }

            HttpContext.Session.SetInt32("UsuarioId", usuario.IdUsuario);
            HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
            HttpContext.Session.SetString("RolUsuario", usuario.Rol);

            if (usuario.Rol == "Administrador")
                return RedirectToAction("Index", "Admin");

            if (usuario.Rol == "Cliente")
                return RedirectToAction("Index", "Cliente");

            if (usuario.Rol == "Técnico")
                return RedirectToAction("Index", "Tecnico");

            if (usuario.Rol == "Cajero")
                return RedirectToAction("Index", "Cajero");

            if (usuario.Rol == "Supervisor")
                return RedirectToAction("Index", "Supervisor");

            if (usuario.Rol == "AtencionCliente")
                return RedirectToAction("Index", "AtencionCliente");

            ViewBag.Error = "El rol del usuario no tiene acceso configurado.";
            return View();
        }

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
            string sector,
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
                ViewBag.Error = "Debe completar todos los campos obligatorios.";
                return View();
            }

            if (_context.Usuarios.Any(u => u.Correo == correo))
            {
                ViewBag.Error = "Ya existe una cuenta registrada con ese correo.";
                return View();
            }

            if (_context.Clientes.Any(c => c.CedulaPasaporteRnc == cedulaPasaporteRnc))
            {
                ViewBag.Error = "Ya existe un cliente registrado con esa cédula, pasaporte o RNC.";
                return View();
            }

            using var transaccion = _context.Database.BeginTransaction();

            try
            {
                var usuario = new Usuario
                {
                    NombreUsuario = nombreUsuario,
                    Correo = correo,
                    Contrasena = contrasena,
                    Rol = "Cliente",
                    Estado = "Pendiente"
                };

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                var cliente = new Cliente
                {
                    IdUsuario = usuario.IdUsuario,
                    CedulaPasaporteRnc = cedulaPasaporteRnc,
                    Telefono = telefono,
                    Provincia = provincia,
                    Municipio = municipio,
                    Sector = sector ?? "",
                    Direccion = direccion,
                    FechaRegistro = DateTime.Now,
                    EstadoCliente = "Pendiente"
                };

                _context.Clientes.Add(cliente);
                _context.SaveChanges();

                transaccion.Commit();

                ViewBag.Mensaje = "Registro completado. Su cuenta está pendiente de aprobación por un administrador.";
                return View();
            }
            catch (Exception ex)
            {
                transaccion.Rollback();

                var errorReal = ex.InnerException != null
                    ? ex.InnerException.Message
                    : ex.Message;

                ViewBag.Error = "Error al registrar: " + errorReal;
                return View();
            }
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Acceso");
        }
    }
}