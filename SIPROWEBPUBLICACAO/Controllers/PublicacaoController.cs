using Microsoft.AspNetCore.Mvc;

namespace SIPROWEBPUBLICACAO.Controllers
{
    public class PublicacaoController : Controller
    {
        public IActionResult Publicacao()
        {
            return View();
        }
    }
}
