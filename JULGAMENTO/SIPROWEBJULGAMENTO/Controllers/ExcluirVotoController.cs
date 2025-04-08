using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.Filtro;

namespace SIPROWEBJULGAMENTO.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "JULGAMENTO_1")]
    public class ExcluirVotoController : Controller
    {
        public IActionResult ExcluirVoto()
        {
            return View();
        }
    }
}
