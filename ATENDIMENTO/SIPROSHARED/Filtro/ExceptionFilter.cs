using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using SIPROSHARED.Models;
using SIRPOEXCEPTIONS.ExceptionBase;
using System.Net;

namespace SIPROSHARED.Filtro
{
    public class ExceptionFilter : IExceptionFilter
    {

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is SiproException)
                HandleProjectException(context);
            else
            {
                ThrowUnknowException(context);
            }
        }

        private void HandleProjectException(ExceptionContext context)
        {
            if (context.Exception is ErrorOnValidationException)
            {
                var exception = context.Exception as ErrorOnValidationException;

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new BadRequestObjectResult(new ResponseErrorJson(exception.ErrosMessages));
            }
        }


        private void ThrowUnknowException(ExceptionContext context)
        {
            context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                controller = "Home",
                action = "InternalServerError"
            }));

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.ExceptionHandled = true; // Marca a exceção como tratada.
        }

    }
}
