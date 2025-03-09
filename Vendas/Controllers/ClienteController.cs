using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendas.Data;
using Vendas.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace Vendas.Controllers
{
    public class ClienteController : Controller
    {
        private readonly VendasDBContext _context;

        public ClienteController(VendasDBContext context)
        {
            _context = context;
        }

        // GET: Cliente - Listar com pesquisa pelo nome
        public async Task<IActionResult> Index(string searchString)
        {
            var clientes = from c in _context.Clientes select c;
            if (!string.IsNullOrEmpty(searchString))
            {
                clientes = clientes.Where(c => c.nmCliente.Contains(searchString));
            }
            return View(await clientes.ToListAsync());
        }

        // GET: Cliente/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cliente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("idCliente,nmCliente,Cidade")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Cliente/Edit/x
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Cliente/Edit/x
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("idCliente,nmCliente,Cidade")] Cliente cliente)
        {
            if (id != cliente.idCliente)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Clientes.Any(e => e.idCliente == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Cliente/Delete/x
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.idCliente == id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Cliente/Delete/x
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Ação para carregar clientes a partir da API
        public async Task<IActionResult> LoadFromApi()
        {
            string apiUrl = "https://camposdealer.dev/Sites/TesteAPI/cliente";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    string json = content.Trim();

                    if (json.StartsWith("<"))
                    {
                        try
                        {
                            var xml = XElement.Parse(json);
                            json = xml.Value;
                        }
                        catch (System.Exception ex)
                        {
                            throw new System.Exception("Erro ao processar XML: " + ex.Message, ex);
                        }
                    }

                    if (json.StartsWith("\"") && json.EndsWith("\""))
                    {
                        json = JsonConvert.DeserializeObject<string>(json);
                    }

                    var clientes = JsonConvert.DeserializeObject<List<Cliente>>(json);
                    foreach (var cli in clientes)
                    {
                        cli.idCliente = 0;
                        if (!_context.Clientes.Any(c => c.nmCliente == cli.nmCliente))
                        {
                            _context.Clientes.Add(cli);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
