using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SIPRORELATORIO.Controllers
{
    [RoutePrefix("sipro/relatorio")]
    public class CrystalReportController : ApiController
    {
        [HttpGet]
        [Route("rpt")]
        public HttpResponseMessage EmitirRelatorio(string rpt, [FromUri] string[] paramtros)
        {
            try
            {
                // Validar se os parâmetros foram enviados
                if (paramtros == null || paramtros.Length == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Nenhum parâmetro foi enviado.");
                }

                // Processar o relatório com os parâmetros recebidos
                CrystalReport report = new CrystalReport();
                return report.Act_EmitirRelatorio(rpt, paramtros);
            }
            catch (Exception ex)
            {
                // Retornar erro em caso de exceção
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


    }
}
