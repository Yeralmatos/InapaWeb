using System;
using System.Linq;
using System.Threading.Tasks;
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

        // GET: Contratos
        public async Task<IActionResult> Index()
        {
            var contratos = await _context.Contratos
                .Include(c => c.Tarifa)
                .ToListAsync();

            return View(contratos);
        }
        // GET: Contratos
[HttpGet]
public async Task<IActionResult> Index(string buscar, string estado)
{
    // 1. Iniciar la consulta incluyendo la Tarifa y el Cliente con su Usuario
    var query = _context.Contratos
        .Include(c => c.Tarifa)
        .Include(c => c.Cliente)
            .ThenInclude(cli => cli.Usuario)
        .AsQueryable();

    // 2. Filtro por búsqueda (Número de contrato, Cédula o Nombre del titular)
    if (!string.IsNullOrEmpty(buscar))
    {
        query = query.Where(c => c.NumeroContrato.Contains(buscar) || 
                                 c.NombreTitular.Contains(buscar) || 
                                 c.DocumentoTitular.Contains(buscar));
    }

    // 3. Filtro por Estado (Activo, Pendiente, Suspendido, etc.)
    if (!string.IsNullOrEmpty(estado))
    {
        query = query.Where(c => c.EstadoContrato == estado);
    }

    
    var contratos = await query.OrderByDescending(c => c.FechaRegistro).ToListAsync();

    // Mantener los valores de búsqueda en la vista
    ViewData["BuscarActual"] = buscar;
    ViewData["EstadoActual"] = estado;

    return View(contratos);
}

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarCombos();
            return View();
        }

        // POST: Contratos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contrato contrato)
        {
            if (ModelState.IsValid)
            {
                contrato.FechaRegistro = DateTime.Now;
                contrato.FechaSolicitud = DateTime.Now;
                contrato.EstadoContrato = "Pendiente";

                // Asignar código único si viene vacío
                if (string.IsNullOrEmpty(contrato.NumeroContrato))
                {
                    contrato.NumeroContrato = "CT-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                }

                _context.Contratos.Add(contrato);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Contrato registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Si hay error en validación, recargar los combos antes de devolver la vista
            await CargarCombos();
            return View(contrato);
        }

        // Carga de combos auxiliares para la vista
        private async Task CargarCombos()
        {
            ViewBag.Clientes = await _context.Clientes
                .Include(c => c.Usuario)
                .ToListAsync();

            ViewBag.Tarifas = await _context.Tarifas.ToListAsync();
        }
        // GET: Contratos/Detalle/5
[HttpGet]
public async Task<IActionResult> Detalle(int? id)
{
    if (id == null)
    {
        return NotFound();
    }

    var contrato = await _context.Contratos
        .Include(c => c.Tarifa)
        .Include(c => c.Cliente)
            .ThenInclude(cli => cli.Usuario)
        .FirstOrDefaultAsync(m => m.IdContrato == id);

    if (contrato == null)
    {
        return NotFound();
    }

    return View(contrato);
}
    }
}