using BravusApp.Server.Data;
using BravusApp.Server.Services;
using BravusApp.Shared.Models;
using BravusApp.Shared.RequestModels;
using BravusApp.Shared.ResponseModels;
using DocumentFormat.OpenXml.Office2016.Excel;
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
    public class DutiesController : ControllerBase
    {
        private readonly IDutiesService _dutiesService;

        public DutiesController(IDutiesService dutiesService)
        {
            _dutiesService = dutiesService;
        }

        [HttpGet("get")]
        public async Task<RequestResponse<List<DutiesResponse>>> GetDuties([FromQuery] int month, [FromQuery] int year)
        {
            var response = await _dutiesService.GetDuties(month, year);
            return response;
        }

        [HttpPost("add")]
        public async Task<RequestResponse<bool>> AddDuty(AddDutyRequest request)
        {
            var response = await _dutiesService.AddDuty(request);
            return response;
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
        {
            var bytes = await _dutiesService.ExportAsync(year, month, ct);
            var fileName = $"Escala_{year:D4}_{month:D2}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
