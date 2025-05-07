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

         // Pensar em uma solução para as Actions para tratamento de erros
        [HttpGet]
        public async Task<IActionResult> Publicacao() 
        {
                PublicacaoModel publicacaoModel = new PublicacaoModel();

                publicacaoModel = await Buscar_Qtd_Processo();
                ViewBag.LoteGerado = await Buscar_Lotes();

                return View(publicacaoModel);       

        }

        [HttpGet]
        public async Task<PublicacaoModel> Buscar_Qtd_Processo()
        {
            var apiUrl = $"{_baseApiUrl}publicacao/quantidade-processo/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.NoContent)
                return new PublicacaoModel();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<PublicacaoModel>();
                return result ?? new PublicacaoModel(); // Protege contra retorno null
            }

            ViewBag.MensagemErro = $"Erro ao buscar os lotes: {response.StatusCode}";

            return new PublicacaoModel();
        }

        [HttpGet]
        public async Task<List<PublicacaoModel>> Buscar_Lotes()
        {
            ViewBag.LoteGerado = new List<PublicacaoModel>();

            var apiUrl = $"{_baseApiUrl}publicacao/buscar-lotes/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.NoContent)
                return new List<PublicacaoModel>();

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<PublicacaoModel>>();
                ViewBag.LoteGerado = result ?? new List<PublicacaoModel>();
                return ViewBag.LoteGerado;
            }

            ViewBag.MensagemErro = $"Erro ao buscar os lotes: {response.StatusCode}";
            return new List<PublicacaoModel>();
        }

        [HttpPost]
        public async Task<IActionResult> GerarLote()
        {
            var apiUrl = $"{_baseApiUrl}publicacao/gerar-lote/{userMatrix}";
            var response = await _httpClient.PostAsync(apiUrl, null);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            var publicacaoModel = await Buscar_Qtd_Processo();
            ViewBag.LoteGerado = await Buscar_Lotes();

            return PartialView("_Qtd_Publicar", publicacaoModel);
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarPublicacao(PublicacaoModel publicacaoModel)
        {
            try
            {
                publicacaoModel.prt_usu_publicacao = userMatrix;
       
                var apiUrl = $"{_baseApiUrl}publicacao/atualizar-publicacao";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, publicacaoModel);
      

                // Verifica erros tratados
                var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
                if (resultadoErro != null)
                    return resultadoErro;

                publicacaoModel = await Buscar_Qtd_Processo();
                ViewBag.LoteGerado = await Buscar_Lotes();

                return PartialView("_Qtd_Publicar", publicacaoModel);
            }
            catch (Exception ex)
            {
                return PartialView("_ErrorPartialView");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Buscar_Lote(string lote)
        {
          
            PublicacaoModel publicacaoModel = new PublicacaoModel();
            var valor = lote?.Replace("/", "") ?? "";

            var apiUrl = $"{_baseApiUrl}publicacao/buscar-lote/{valor}";
            var response = await _httpClient.GetAsync(apiUrl);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                publicacaoModel = await response.Content.ReadFromJsonAsync<PublicacaoModel>();
                publicacaoModel.prt_publicacao_dom = userMatrix;
                return Json(new { error = false, publicacaoModel });
            }

            return Json(new { error = false, publicacaoModel });
        }

        [HttpPost]
        public async Task<IActionResult> ExcluirLote( string lote)
        {
            var valor = lote?.Replace("/", "") ?? "";

            PublicacaoModel publicacaoModel = new PublicacaoModel();

            var apiUrl = $"{_baseApiUrl}publicacao/excluir-lote/{valor}";
            var response = await _httpClient.PutAsJsonAsync(apiUrl, valor);


            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            publicacaoModel = await Buscar_Qtd_Processo();
            ViewBag.LoteGerado = await Buscar_Lotes();

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
