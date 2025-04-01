using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.Filtro;

namespace SIPROWEBPUBLICACAO.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "JULGAMENTO_1")]
    public class PublicacaoController : Controller
    {
        public IActionResult Publicacao()
        {
            return View();
        }
    }
}
