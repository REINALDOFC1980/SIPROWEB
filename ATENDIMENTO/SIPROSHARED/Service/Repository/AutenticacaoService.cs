using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHARED.Service.IRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Service.Repository
{
    public class AutenticacaoService : IAutenticacaoService
    {
        private readonly DapperContext _context;
        private readonly IConfiguration  _configuration;


        public AutenticacaoService(DapperContext context, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AutenticacaoModel> Autenticacao(string Usu_Login, string Usu_Senha)
        {
            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { Usu_Login, Usu_Senha };
                    var command = await connection.QueryFirstOrDefaultAsync<AutenticacaoModel>(
                        "Stb_Autenticacao",
                        parametros,
                        commandType: CommandType.StoredProcedure
                    );
                    return command;
                }
            }
            catch (Exception ex)
            {
                // Melhor manipulação de exceções: log ou tratar adequadamente
                throw new Exception("Erro ao obter autenticação de usuário.", ex);
            }
        }

        public string GerearToken(AutenticacaoModel autenticacaoModel)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, autenticacaoModel.Usu_Nome),
            new Claim(ClaimTypes.Role, autenticacaoModel.Usu_Role),
            new Claim(ClaimTypes.NameIdentifier, autenticacaoModel.Usu_Usu_Matrix)
            };

            var tokenOptions = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Expiração do token (1 hora)
                signingCredentials: signingCredentials);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return token;
        }



    }
}
