using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SIPROSHARED.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Filtro
{
    public static class ApiErrorHandler
    {
        public static async Task<IActionResult> TratarErrosHttpResponse(HttpResponseMessage response, IUrlHelper urlHelper)
        {
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                return new ObjectResult(new
                {
                    error = true,
                    message = "Erro interno no servidor.",
                    redirectTo = urlHelper.Action("InternalServerError", "Home")
                })
                { StatusCode = (int)response.StatusCode };
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ObjectResult(new
                {
                    error = true,
                    message = "Recurso não encontrado."
                })
                { StatusCode = (int)response.StatusCode };
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                var errorData = JsonConvert.DeserializeObject<ErrorResponseModel>(errorResponse);
                var errorMessage = errorData?.Errors?.FirstOrDefault() ?? "Requisição inválida.";

                return new ObjectResult(new
                {
                    error = true,
                    message = errorMessage
                    //redirectTo = urlHelper.Action("BadRequest", "Home", new { msg = errorMessage })
                })
                { StatusCode = (int)response.StatusCode };
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new ObjectResult(new
                {
                    error = true,
                    message = "Você não está autorizado. Faça login novamente.",
                    redirectTo = urlHelper.Action("Login", "Conta")
                })
                { StatusCode = (int)response.StatusCode };
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return new ObjectResult(new
                {
                    error = true,
                    message = "Você não tem permissão para acessar este recurso."
                })
                { StatusCode = (int)response.StatusCode };
            }
            else if (response.StatusCode == HttpStatusCode.RequestTimeout)
            {
                return new ObjectResult(new
                {
                    error = true,
                    message = "A solicitação expirou. Tente novamente."
                })
                { StatusCode = (int)response.StatusCode };
            }
            else if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 600)
            {
                return new ObjectResult(new
                {
                    error = true,
                    message = $"Erro ao processar a solicitação: {response.StatusCode}"
                })
                { StatusCode = (int)response.StatusCode };
            }

            return null;
        }
    }
}
