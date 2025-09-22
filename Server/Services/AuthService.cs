using BravusApp.Server.Data;
using BravusApp.Shared.Models;
using BravusApp.Shared.ResponseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BravusApp.Server.Services
{
    public interface IAuthService
    {
        Task<RequestResponse<string>> Login(string cpf,  string password);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<RequestResponse<string>> Login(string cpf, string password)
        {
            var op = await _context.Operators.FirstOrDefaultAsync(x => x.Cpf == cpf);

            if (op == null || !BCrypt.Net.BCrypt.Verify(password, op.Password))
                return RequestResponse<string>.Fail("CPF ou senha inválidos");
            
            var token = GenerateJwtToken(op);
            if (String.IsNullOrEmpty(token))
                return RequestResponse<string>.Fail("Erro ao Gerar Token");

            return RequestResponse<string>.Ok(token, "Login realizado com sucesso");
        }

        private string GenerateJwtToken(Operator op)
        {
            var key = Encoding.ASCII.GetBytes(_config["JwtKey"] ?? "super_secret_key_bravus_2025");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", op.Id.ToString()),
                    new Claim(ClaimTypes.Name, op.Name),
            }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
