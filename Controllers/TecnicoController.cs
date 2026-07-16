using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InapaWeb.Data;
using InapaWeb.Models;

namespace InapaWeb.Controllers
{
    public class TecnicoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TecnicoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("RolUsuario") != "Técnico")
                return RedirectToAction("Login", "Acceso");

            ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");

            return View();
        }

        public IActionResult TrabajosAsignados(string? estado)
        {
            if (HttpContext.Session.GetString("RolUsuario") != "Técnico")
            {
                return RedirectToAction("Login", "Acceso");
            }

            int? idTecnico =
                HttpContext.Session.GetInt32("UsuarioId");

            if (idTecnico == null)
            {
                return RedirectToAction("Login", "Acceso");
            }

            var trabajos = _context.AsignacionesTecnicos
                .Include(a => a.SolicitudServicio)
                    .ThenInclude(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                .Where(a => a.IdTecnico == idTecnico.Value)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                trabajos = trabajos.Where(a => a.Estado == estado);
            }

            ViewBag.EstadoSeleccionado =
                string.IsNullOrWhiteSpace(estado)
                    ? "Todos"
                    : estado;

            ViewBag.TotalTrabajos = _context.AsignacionesTecnicos
                .Count(a => a.IdTecnico == idTecnico.Value);

            ViewBag.TotalAsignados = _context.AsignacionesTecnicos
                .Count(a =>
                    a.IdTecnico == idTecnico.Value &&
                    a.Estado == "Asignado");

            ViewBag.TotalEnProceso = _context.AsignacionesTecnicos
                .Count(a =>
                    a.IdTecnico == idTecnico.Value &&
                    a.Estado == "En Proceso");

            ViewBag.TotalFinalizados = _context.AsignacionesTecnicos
                .Count(a =>
                    a.IdTecnico == idTecnico.Value &&
                    a.Estado == "Finalizado");

            return View(
                trabajos
                    .OrderByDescending(a => a.FechaAsignacion)
                    .ToList()
            );
        }
    }

}

