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
    public class DutiesController : ControllerBase
    {
        private readonly IDutiesService _dutiesService;

        public DutiesController(IDutiesService dutiesService)
        {
            _dutiesService = dutiesService;
        }

        [HttpGet("get")]
        public async Task<RequestResponse<List<DutiesResponse>>> GetDuties([FromQuery] int month)
        {
            var response = await _dutiesService.GetDuties(month);
            return response;
        }

        [HttpPost("add")]
        public async Task<RequestResponse<bool>> AddDuty(AddDutyRequest request)
        {
            var response = await _dutiesService.AddDuty(request);
            return response;
        }
    }
}
