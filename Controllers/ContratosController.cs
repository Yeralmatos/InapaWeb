using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InapaWeb.Data;
using InapaWeb.Models;

namespace InapaWeb.Controllers
{
    public class ContratosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContratosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // LISTADO DE CONTRATOS
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var contratos = await _context.Contratos
                .Include(c => c.Cliente)
                .ThenInclude(c => c.Usuario)
                .Include(c => c.Tarifa)
                .ToListAsync();

            return View(contratos);
        }

        // ==========================================
        // FORMULARIO NUEVO CONTRATO
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCombos();
            return View();
        }

        // ==========================================
        // GUARDAR NUEVO CONTRATO
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contrato contrato)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombos();
                return View(contrato);
            }

            contrato.FechaRegistro = DateTime.Now;

            contrato.FechaSolicitud = DateTime.Now;

            contrato.EstadoContrato = "Pendiente";

            if (string.IsNullOrEmpty(contrato.NumeroContrato))
            {
                contrato.NumeroContrato =
                    "CTR-" + DateTime.Now.Year + "-" +
                    Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            }


            _context.Contratos.Add(contrato);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Contrato registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CARGAR LISTAS
        // ==========================================
        private async Task CargarCombos()
        {
            ViewBag.Clientes = await _context.Clientes
                .Include(c => c.Usuario)
                .ToListAsync();

            ViewBag.Tarifas = await _context.Tarifas
                .OrderBy(t => t.Descripcion)
                .ToListAsync();
        }
    }
}