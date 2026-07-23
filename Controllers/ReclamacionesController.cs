using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InapaWeb.Data;
using InapaWeb.Models;

namespace InapaWeb.Controllers
{
    public class ReclamacionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReclamacionesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ================================
        // MÓDULO PRINCIPAL
        // ================================
        public async Task<IActionResult> Index()
        {
            var reclamaciones = await _context.Reclamaciones
                .Include(r => r.Cliente)
                .ThenInclude(c => c.Usuario)
                .ToListAsync();

            return View(reclamaciones);
        }



        // ================================
        // FORMULARIO RECLAMACIÓN INDIVIDUAL
        // ================================
        public async Task<IActionResult> Individual()
        {
            int? idUsuario =
                HttpContext.Session.GetInt32("UsuarioId");


            if (idUsuario == null)
            {
                return RedirectToAction(
                    "Login",
                    "Acceso"
                );
            }


            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(
                    c => c.IdUsuario == idUsuario.Value
                );


            if (cliente == null)
            {
                return NotFound(
                    "No existe cliente asociado a este usuario."
                );
            }



            var reclamacion = new Reclamacion
            {
                IdCliente = cliente.IdCliente,
                TipoReclamacion = "Individual",
                Estado = "Pendiente",
                FechaRegistro = DateTime.Now
            };



            ViewBag.NombreCliente =
                cliente.Usuario?.NombreUsuario
                ?? "Cliente";



            return View(reclamacion);
        }





        // ================================
        // GUARDAR RECLAMACIÓN
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            Reclamacion reclamacion)
        {

            int? idUsuario =
                HttpContext.Session.GetInt32("UsuarioId");


            if (idUsuario == null)
            {
                return RedirectToAction(
                    "Login",
                    "Acceso"
                );
            }



            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(
                    c => c.IdUsuario == idUsuario.Value
                );



            if (cliente == null)
            {
                return NotFound(
                    "Cliente no encontrado."
                );
            }



            // El cliente siempre será el usuario conectado
            reclamacion.IdCliente =
                cliente.IdCliente;


            reclamacion.Estado =
                "Pendiente";


            reclamacion.FechaRegistro =
                DateTime.Now;



            if (ModelState.IsValid)
            {

                _context.Reclamaciones.Add(
                    reclamacion
                );


                await _context.SaveChangesAsync();



                return RedirectToAction(
                    nameof(Seguimiento)
                );
            }



            ViewBag.NombreCliente =
                cliente.Usuario?.NombreUsuario
                ?? "Cliente";



            return View(
                "Individual",
                reclamacion
            );
        }





        // ================================
        // RECLAMACIÓN COLECTIVA
        // ================================
        public IActionResult Colectiva()
        {
            return View();
        }



        // ================================
        // RECLAMACIÓN VIRTUAL
        // ================================
        public IActionResult RegistrarVirtual()
        {
            return View();
        }



        // ================================
        // RECLAMACIÓN PRESENCIAL
        // ================================
        public IActionResult RegistrarPresencial()
        {
            return View();
        }



        // ================================
        // RECLAMACIÓN VIVIENDA
        // ================================
        public IActionResult ReclamacionVivienda()
        {
            return View();
        }



        // ================================
        // RECLAMACIÓN SECTOR
        // ================================
        public IActionResult ReclamacionSector()
        {
            return View();
        }





        // ================================
        // SEGUIMIENTO CLIENTE
        // ================================
        public async Task<IActionResult> Seguimiento()
        {

            int? idUsuario =
                HttpContext.Session.GetInt32("UsuarioId");



            if (idUsuario == null)
            {
                return RedirectToAction(
                    "Login",
                    "Acceso"
                );
            }



            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(
                    c => c.IdUsuario == idUsuario.Value
                );



            if (cliente == null)
            {
                return NotFound();
            }



            var reclamaciones =
                await _context.Reclamaciones
                .Where(
                    r => r.IdCliente == cliente.IdCliente
                )
                .OrderByDescending(
                    r => r.FechaRegistro
                )
                .ToListAsync();



            return View(reclamaciones);
        }





        // ================================
        // DETALLE RECLAMACIÓN
        // ================================
        public async Task<IActionResult> Detalle(int id)
        {

            var reclamacion =
                await _context.Reclamaciones
                .Include(r => r.Cliente)
                .ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(
                    r => r.IdReclamacion == id
                );



            if (reclamacion == null)
            {
                return NotFound();
            }



            return View(reclamacion);
        }





        // ================================
        // ATENCIÓN
        // ================================
        public IActionResult Atencion()
        {
            return View();
        }





        // ================================
        // CERRAR
        // ================================
        public IActionResult Cerrar()
        {
            return View();
        }

    }
}