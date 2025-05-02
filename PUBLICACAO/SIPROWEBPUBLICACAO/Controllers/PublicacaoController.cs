using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDPUBLICACAO.Model;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using SIPROWEBPUBLICACAO.Helper;

namespace SIPROWEBPUBLICACAO.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "JULGAMENTO_1")]
    public class PublicacaoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
        private readonly string? _baseSipApiUrl;
        private string? userMatrix;

        public PublicacaoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("PublicacaoApiUrl");
            _baseSipApiUrl = configuration.GetValue<string>("BaseSipApiUrl");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            userMatrix = HttpContext.Items["UserMatrix"] as string;
            base.OnActionExecuting(context);
        }

        private async Task<JsonResult> HandleErrorResponse(HttpResponseMessage response)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            var errorData = JsonConvert.DeserializeObject<ErrorResponseModel>(errorResponse);
            var errorMessage = errorData?.Errors?.FirstOrDefault() ?? "Erro ao processar sua solicitação.";
            TempData["ErroMessage"] = errorMessage;
            return Json(new { error = "BadRequest", message = errorMessage });
        }

        public async Task<PublicacaoModel> BuscarQtdPublicar(string usuario)
        {
            try
            {
                PublicacaoModel publicacaoModel = new PublicacaoModel();

                string apiUrl = $"{_baseApiUrl}publicacao/quantidade-processo/{userMatrix}";
                var response = await _httpClient.GetAsync(apiUrl); // Aguarda a resposta


                if (response.StatusCode == HttpStatusCode.OK)
                    publicacaoModel = await response.Content.ReadFromJsonAsync<PublicacaoModel>();

                return publicacaoModel;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        [HttpGet]
        public async Task<IActionResult> Publicacao() // Torna o método assíncrono
        {
            try
            {

                PublicacaoModel publicacaoModel = new PublicacaoModel();

                publicacaoModel = await Buscar_Qtd_Processo(userMatrix);

                ViewBag.LoteGerado = await Buscar_Lotes(userMatrix);

                return View(publicacaoModel);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        [HttpPost]
        public async Task<PartialViewResult> GerarLote()
        {

            PublicacaoModel publicacaoModel = new PublicacaoModel();

            //Gerando Lote
            string apiUrl = $"{_baseApiUrl}publicacao/gerar-lote/{userMatrix}";
            var response = await _httpClient.PostAsync(apiUrl, null);

            if (!response.IsSuccessStatusCode)
                return PartialView("_ErrorPartialView");

            publicacaoModel = await Buscar_Qtd_Processo(userMatrix);

            ViewBag.LoteGerado = await Buscar_Lotes(userMatrix);

            return PartialView("_Qtd_Publicar", publicacaoModel);
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarPublicacao(PublicacaoModel publicacaoModel)
        {
            publicacaoModel.prt_usu_publicacao = userMatrix;

            var apiUrl = $"{_baseApiUrl}publicacao/atualizar-publicacao";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, publicacaoModel);

            //tratamento de erro
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return Json(new { error = true });

                else if (response.StatusCode == HttpStatusCode.BadRequest)
                    return await HandleErrorResponse(response);

                else if (response.StatusCode == HttpStatusCode.NoContent)
                    return await HandleErrorResponse(response);

            }

            ViewBag.LoteGerado = Buscar_Lotes(userMatrix);
            return PartialView("_Qtd_Publicar");
        }

        [HttpGet]
        public async Task<List<PublicacaoModel>> Buscar_Lotes(string usuario)
        {

            //Buscando os lotes gerados!
            ViewBag.LoteGerado = new List<PublicacaoModel>();
            var apiUrl = $"{_baseApiUrl}publicacao/buscar-lotes/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<PublicacaoModel> result = await response.Content.ReadFromJsonAsync<List<PublicacaoModel>>();
                return result;
            }

            return new List<PublicacaoModel>();

        }

        public async Task<IActionResult> Buscar_Lote(string lote)
        {
            try
            {
                PublicacaoModel publicacaoModel = new PublicacaoModel();

                var valor = lote?.Replace("/", "") ?? "";

                var apiUrl = $"{_baseApiUrl}publicacao/buscar-lote/{valor}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    publicacaoModel = await response.Content.ReadFromJsonAsync<PublicacaoModel>();

                    publicacaoModel.prt_publicacao_dom = userMatrix;


                    return Json(new { error = false, publicacaoModel });
                }

                return Json(new { error = true, message = "Lote não encontrado" });
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        [HttpGet]
        public async Task<PublicacaoModel> Buscar_Qtd_Processo(string usuario)
        {

            //Buscando os novos valores
            var apiUrl = $"{_baseApiUrl}publicacao/quantidade-processo/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<PublicacaoModel>();

            return new PublicacaoModel();

        }



        [HttpPost]
        public async Task<IActionResult> ExcluirLote(string lote)
        {
            var valor = lote?.Replace("/", "") ?? "";

            PublicacaoModel publicacaoModel = new PublicacaoModel();

            var apiUrl = $"{_baseApiUrl}publicacao/excluir-lote/{valor}";
            var response = await _httpClient.PutAsJsonAsync(apiUrl, valor);

            if (!response.IsSuccessStatusCode)
                return Json(new { error = true, message = "Erro ao excluir o Lote." });

            publicacaoModel = await Buscar_Qtd_Processo(userMatrix);

            return PartialView("_Qtd_Publicar", publicacaoModel);
        }


        [HttpGet]
        public async Task<List<PublicacaoDOMModel>> GerarDOM(string lote)
        {

            var valor = lote?.Replace("/", "") ?? "";
            ViewBag.LoteGerado = new List<PublicacaoDOMModel>();


            var apiUrl = $"{_baseApiUrl}publicacao/gerar-dom/{valor}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<PublicacaoDOMModel> result = await response.Content.ReadFromJsonAsync<List<PublicacaoDOMModel>>();
                return result;
            }

            return new List<PublicacaoDOMModel>();

        }



        
        public async Task<IActionResult> GerarWord(string lote)
        {
            var valor = lote?.Replace("/", "") ?? "";

            List<PublicacaoDOMModel> lista = await GerarDOM(lote);

            if (lista == null || !lista.Any())
                return NoContent();

            //var lista = resultado.Select(r => new string[]
            //{
            //    r.PES_Nome ?? "",       // Solicitante
            //    r.PRT_NUMERO ?? "",     // Processo
            //    r.PRT_AIT ?? "",        // AIT
            //    r.PRT_RESULTADO ?? ""   // Resultado
            //}).ToList();

            string nomeArquivo = "Relatorio.docx";
            using (MemoryStream memStream = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = new Body(
                        new SectionProperties(
                            new PageSize { Width = 11906, Height = 16838 },
                            new PageMargin { Top = 1440, Right = 1440, Bottom = 1440, Left = 1440 }
                        )
                    );

                    RunProperties fonteNegrito10 = new RunProperties(new Bold(), new RunFonts { Ascii = "Arial", HighAnsi = "Arial", EastAsia = "Arial", ComplexScript = "Arial" }, new FontSize { Val = "20" });
                    RunProperties fonte8 = new RunProperties(new RunFonts { Ascii = "Arial", HighAnsi = "Arial", EastAsia = "Arial", ComplexScript = "Arial" }, new FontSize { Val = "16" });
                    RunProperties fonteNegrito8 = new RunProperties(new Bold(), new RunFonts { Ascii = "Arial", HighAnsi = "Arial", EastAsia = "Arial", ComplexScript = "Arial" }, new FontSize { Val = "16" });

                    // Título
                    body.Append(new Paragraph(
                        new ParagraphProperties(new Justification { Val = JustificationValues.Left }, new Indentation { Left = "720" }),
                        new Run(fonteNegrito10.CloneNode(true), new Text("Relatório de Protocolos"))
                    ));

                    // Subtítulo
                    string loteTexto = string.IsNullOrWhiteSpace(lote) ? "NÃO INFORMADO" : lote.ToUpper();
                    body.Append(new Paragraph(
                        new ParagraphProperties(new Justification { Val = JustificationValues.Left }, new Indentation { Left = "720" }),
                        new Run(fonteNegrito10.CloneNode(true), new Text("LOTE PUBLICAÇÃO (" + loteTexto + ")"))
                    ));

                    // Remover esse espaçamento para deixar mais compacto

                    Table tabela = new Table(
                        new TableProperties(
                            new TableStyle { Val = "TableGrid" },
                            new TableWidth { Width = "7600", Type = TableWidthUnitValues.Dxa },
                            new TableJustification { Val = TableRowAlignmentValues.Center },
                            new TableBorders(
                                new TopBorder { Val = BorderValues.Single, Size = 8 },
                                new BottomBorder { Val = BorderValues.Single, Size = 8 },
                                new LeftBorder { Val = BorderValues.Single, Size = 8 },
                                new RightBorder { Val = BorderValues.Single, Size = 8 },
                                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 8 },
                                new InsideVerticalBorder { Val = BorderValues.Single, Size = 8 }
                            )
                        )
                    );

                    // Cabeçalho
                    TableRow header = new TableRow();
                    header.Append(WordHelper.CreateCell("Solicitante", true, 4800, 16, true));
                    header.Append(WordHelper.CreateCell("Processo", true, 1600, 16, true));
                    header.Append(WordHelper.CreateCell("AIT", true, 1500, 16, true));
                    header.Append(WordHelper.CreateCell("Resultado", true, 1700, 16, true));
                    tabela.Append(header);

                    foreach (var item in lista)
                    {
                        TableRow row = new TableRow();
                        row.Append(WordHelper.CreateCell(item.PES_Nome, false, 4800, 16, true));
                        row.Append(WordHelper.CreateCell(item.PRT_NUMERO, false, 1600, 16, true));
                        row.Append(WordHelper.CreateCell(item.PRT_AIT, false, 1500, 16, true));
                        row.Append(WordHelper.CreateCell(item.PRT_RESULTADO, false, 1700, 16, true));

                        tabela.Append(row);
                    }

                    body.Append(tabela);
                    body.Append(new Paragraph(new Run(new Text(" "))));
                    body.Append(new Paragraph(new Run(new Text(" "))));

                    // Rodapé

                    body.Append(new Paragraph(
                        new ParagraphProperties(
                            new Justification { Val = JustificationValues.Center },
                            new SpacingBetweenLines { After = "500" } // Aumenta o espaço após a data
                        ),
                        new Run(fonte8.CloneNode(true), new Text("Salvador, " + DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy")))
                    ));



                    body.Append(new Paragraph(
                        new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                        new Run(fonteNegrito8.CloneNode(true), new Text("FABRIZZIO M.MARTINEZ")),
                        new Break(), // quebra de linha sem criar novo parágrafo
                        new Run(fonte8.CloneNode(true), new Text("Superintendente Executivo"))
                      ));


                    mainPart.Document.Append(body);
                    mainPart.Document.Save();
                }

                memStream.Position = 0;
                return File(memStream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nomeArquivo);
            }
        }


    }
}
