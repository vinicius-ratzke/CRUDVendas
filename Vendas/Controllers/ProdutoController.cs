using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendas.Data;
using Vendas.Models;
using System.Net.Http;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Vendas.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly VendasDBContext _context;

        public ProdutoController(VendasDBContext context)
        {
            _context = context;
        }

        // GET: Produto
        public async Task<IActionResult> Index(string searchString)
        {
            var produtos = from p in _context.Produtos select p;
            if (!string.IsNullOrEmpty(searchString))
            {
                produtos = produtos.Where(p => p.DscProduto.Contains(searchString));
            }
            return View(await produtos.ToListAsync());
        }

        // GET: Produto/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Produto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProduto,DscProduto,VlrUnitario")] Produto produto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
        }

        // GET: Produto/Edit/x
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound();

            return View(produto);
        }

        // POST: Produto/Edit/x
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProduto,DscProduto,VlrUnitario")] Produto produto)
        {
            if (id != produto.IdProduto)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Produtos.Any(e => e.IdProduto == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(produto);
        }

        // GET: Produto/Delete/x
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(m => m.IdProduto == id);
            if (produto == null)
                return NotFound();

            return View(produto);
        }

        // POST: Produto/Delete/x
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Ação para carregar produtos a partir do endpoint da API
        public async Task<IActionResult> LoadFromApi()
        {
            string apiUrl = "https://camposdealer.dev/Sites/TesteAPI/produto";
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

                    var produtos = JsonConvert.DeserializeObject<List<Produto>>(json);
                    foreach (var prod in produtos)
                    {

                        prod.IdProduto = 0;
                        if (!_context.Produtos.Any(p => p.DscProduto == prod.DscProduto))
                        {
                            _context.Produtos.Add(prod);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
