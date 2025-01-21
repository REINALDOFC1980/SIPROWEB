using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace SIPROWEB.Controllers
{
    public class RelatorioController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrlrpt;
        public RelatorioController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrlrpt = configuration.GetValue<string>("BaseApiUrlRpt");
        }
        public async Task<IActionResult> EmitirRPT(string rpt, string[] paramtros)
        {
            try
            {
                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
                queryString["rpt"] = rpt;

                for (int i = 0; i < paramtros.Length; i++)
                {
                    queryString[$"paramtros[{i}]"] = Uri.UnescapeDataString(paramtros[i]);

                }

                string apiUrl = $"{_baseApiUrlrpt}rpt?{queryString}";

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {

                    var stream = await response.Content.ReadAsStreamAsync();

                    byte[] content;
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        content = memoryStream.ToArray();
                    }

                    return File(content, "application/pdf", "Report.pdf");
                }
                else
                {
                    // Se a resposta não for bem-sucedida, exibir uma mensagem de erro
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    return Content($"Error: {response.StatusCode} - {errorMessage}");
                }
            }
            catch (Exception ex)
            {

                return Content($"Exception: {ex.Message}");
            }
        }

    }
}
