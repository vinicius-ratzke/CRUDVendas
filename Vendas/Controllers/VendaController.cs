using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendas.Data;
using Vendas.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Vendas.Controllers
{
    public class VendaController : Controller
    {
        private readonly VendasDBContext _context;

        public VendaController(VendasDBContext context)
        {
            _context = context;
        }

        // GET: Venda - Lista e pesquisa (por nome do Cliente ou descrição do Produto)
        public async Task<IActionResult> Index(string searchString)
        {
            var vendas = _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Produto)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                vendas = vendas.Where(v => v.Cliente.nmCliente.Contains(searchString) ||
                                           v.Produto.DscProduto.Contains(searchString));
            }

            return View(await vendas.ToListAsync());
        }

        // GET: Venda/Create
        public IActionResult Create()
        {
            ViewData["ClienteList"] = new SelectList(_context.Clientes, "idCliente", "nmCliente");
            ViewData["ProdutoList"] = new SelectList(_context.Produtos, "IdProduto", "DscProduto");
            return View();
        }

        // POST: Venda/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("idVenda,idCliente,idProduto,qtdVenda,vlrUnitarioVenda,dthVenda")] Venda venda)
        {
            if (ModelState.IsValid)
            {
                // Calcula o valor total da venda
                venda.vlrTotalVenda = venda.vlrUnitarioVenda * venda.qtdVenda;
                _context.Add(venda);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteList"] = new SelectList(_context.Clientes, "idCliente", "nmCliente", venda.idCliente);
            ViewData["ProdutoList"] = new SelectList(_context.Produtos, "IdProduto", "DscProduto", venda.idProduto);
            return View(venda);
        }

        // GET: Venda/Edit/x
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var venda = await _context.Vendas.FindAsync(id);
            if (venda == null)
                return NotFound();

            ViewData["ClienteList"] = new SelectList(_context.Clientes, "idCliente", "nmCliente", venda.idCliente);
            ViewData["ProdutoList"] = new SelectList(_context.Produtos, "IdProduto", "DscProduto", venda.idProduto);
            return View(venda);
        }

        // POST: Venda/Edit/x
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("idVenda,idCliente,idProduto,qtdVenda,vlrUnitarioVenda,dthVenda")] Venda venda)
        {
            if (id != venda.idVenda)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    venda.vlrTotalVenda = venda.vlrUnitarioVenda * venda.qtdVenda;
                    _context.Update(venda);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendaExists(venda.idVenda))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteList"] = new SelectList(_context.Clientes, "idCliente", "nmCliente", venda.idCliente);
            ViewData["ProdutoList"] = new SelectList(_context.Produtos, "IdProduto", "DscProduto", venda.idProduto);
            return View(venda);
        }

        // GET: Venda/Delete/x
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var venda = await _context.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Produto)
                .FirstOrDefaultAsync(m => m.idVenda == id);
            if (venda == null)
                return NotFound();

            return View(venda);
        }

        // POST: Venda/Delete/x
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venda = await _context.Vendas.FindAsync(id);
            _context.Vendas.Remove(venda);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Ação para carregar vendas a partir do endpoint da API
        public async Task<IActionResult> LoadFromApi()
        {
            string apiUrl = "https://camposdealer.dev/Sites/TesteAPI/venda";
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
                        catch (Exception ex)
                        {
                            throw new Exception("Erro ao processar XML: " + ex.Message, ex);
                        }
                    }

                    if (json.StartsWith("\"") && json.EndsWith("\""))
                    {
                        json = JsonConvert.DeserializeObject<string>(json);
                    }

                    var vendas = JsonConvert.DeserializeObject<List<Venda>>(json);
                    foreach (var venda in vendas)
                    {
                        venda.idVenda = 0;
                        venda.vlrTotalVenda = venda.vlrUnitarioVenda * venda.qtdVenda;
                        _context.Vendas.Add(venda);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VendaExists(int id)
        {
            return _context.Vendas.Any(e => e.idVenda == id);
        }
    }
}
