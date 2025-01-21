using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.Models;
using SIPROSHARED.Filtro;

using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace SIPROWEB.Controllers
{
  
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult InternalServerError(string cpf )
        {
            return View();
        }


        public IActionResult ChamarDistribuicao()
        {
            return RedirectToAction("Index", "Distribuicao", new { area = "" }, null); // Ajuste conforme necessário
        }

        [AutorizacaoTokenAttribute("ADM", "ATENDIMENTO_1")]
        public IActionResult Index()
        {
            return View();
        }

        [AutorizacaoTokenAttribute("ADM", "ATENDIMENTO_1","DISTRIBUICAO_1", "JULGAMENTO_1","INSTRUCAO_1")]
        public IActionResult Apresentacao()
        {
            return View();
        }
        public IActionResult Agendados()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
           
            return View();
        }
    }
}
