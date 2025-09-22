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
    [Authorize]
    public class OperatorsController : ControllerBase
    {
        private readonly IOperatorService _operatorService;

        public OperatorsController(IOperatorService operatorService)
        {
            _operatorService = operatorService;
        }

        [HttpGet("get")]
        public async Task<RequestResponse<List<OperatorsResponse>>> GetOperators()
        {
            var response = await _operatorService.GetOperators();
            return response; 
        }

        [HttpPost("add")]
        public async Task<RequestResponse<bool>> AddOperator(AddOperatorRequest model)
        {
            var response = await _operatorService.AddOperator(model);
            return response;
        }

        [HttpPost("change-pswd")]
        public async Task<RequestResponse<bool>> ChangePassword(ChangePasswordRequest model)
        {
            var response = await _operatorService.ChangePassword(model);
            return response;
        }

        [HttpPost("change-status")]
        public async Task<RequestResponse<bool>> AddOperator(int id)
        {
            var response = await _operatorService.ChangeStatus(id);
            return response;
        }
    }
}
