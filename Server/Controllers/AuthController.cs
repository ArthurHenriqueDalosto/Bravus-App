using BravusApp.Server.Data;
using BravusApp.Server.Services;
using BravusApp.Shared.Models;
using BravusApp.Shared.RequestModels;
using BravusApp.Shared.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BravusApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Cpf) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(RequestResponse<string>.Fail("CPF e senha são obrigatórios"));

            var response = await _authService.Login(request.Cpf, request.Password);

            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }
    }
}
