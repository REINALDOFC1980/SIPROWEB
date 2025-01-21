using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SIPROSHARED.Models;
using System.DirectoryServices.Protocols;
using System.Net.Http;
using System.Text;

namespace SIPROSHARED.API
{
    public class Detran
    {
    
        private readonly HttpClient _httpClient;

        public Detran(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ResultGetCnhPorCpf> CnhCpfAsync(string cpf)
        {
            // URL fornecida para o serviço RENACH
            string baseUrl = "https://transonline2.salvador.ba.gov.br/transalvador_service/api/renach/cpf";
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

            // Monta a URL completa com os parâmetros
            string url = $"{baseUrl}?token={token}&cpf={cpf}";

            ResultGetCnhPorCpf cnhItem = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Faz a requisição GET para a URL
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Verifica se a resposta foi bem-sucedida
                    if (response.IsSuccessStatusCode)
                    {
                        // Lê o conteúdo da resposta como string
                        string json = await response.Content.ReadAsStringAsync();

                        // Desserializa o JSON para o objeto ApiResponse
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(json);

                        if (apiResponse != null && apiResponse.isOk && apiResponse.items != null && apiResponse.items.Count > 0)
                        {
                            // Pega o primeiro item da lista de CNHs
                            cnhItem = apiResponse.items[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Se houver uma exceção, você pode definir o cnhItem com valores indicativos de erro, se necessário
                // ou então apenas logar o erro para tratamento posterior
                Console.WriteLine($"Erro ao conectar com o serviço RENACH: {ex.Message}");
            }

            return cnhItem;
        }


    }
}
