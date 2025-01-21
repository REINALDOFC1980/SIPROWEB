using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace SIPROSHARED.Filtro
{
    public class AutorizacaoTokenAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AutorizacaoTokenAttribute(params string[] roles)
        {
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;

            // Obtém o token JWT da sessão
            var token = session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Autenticacao",
                    action = "Login"
                }));
                return; // Adiciona um return aqui para garantir que o código não continue
            }

            try
            {
                // Configurações para validar o token JWT
                var jwtSettings = httpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("Jwt");
                var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];

                // Configuração de validação do token JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                // Valida o token JWT e obtém o principal
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var userRoles = principal.FindFirst(ClaimTypes.Role)?.Value;
                var userNome = principal.FindFirst(ClaimTypes.Name)?.Value;
                var userMatrix = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                session.SetString("UserRoles", userRoles);
                session.SetString("UserNome", userNome);
                session.SetString("UserMatrix", userMatrix);

                // Verifica se o usuário possui pelo menos uma das roles necessárias
                bool hasRequiredRole = _roles.Any(r => userRoles != null && userRoles.Contains(r));

                if (!hasRequiredRole)
                {
                    context.Result = new ViewResult
                    {
                        ViewName = "Unauthorized"
                    };
                    return;
                }

                httpContext.Items["UserRoles"] = userRoles;
                httpContext.Items["UserNome"] = userNome;
                httpContext.Items["UserMatrix"] = userMatrix;
            }
            catch (SecurityTokenValidationException)
            {
                // Caso haja erro na validação do token
                context.Result = new UnauthorizedResult();
                return;
            }
            catch (Exception)
            {
                // Outros erros
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }
        }
    }
}
