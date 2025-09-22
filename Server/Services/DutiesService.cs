using BravusApp.Server.Data;
using BravusApp.Shared.Enums;
using BravusApp.Shared.Models;
using BravusApp.Shared.RequestModels;
using BravusApp.Shared.ResponseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using System.Collections.Generic;
using static MudBlazor.CategoryTypes;

namespace BravusApp.Server.Services
{
    public interface IDutiesService
    {
        Task<RequestResponse<List<DutiesResponse>>> GetDuties(int month);
        Task<RequestResponse<bool>> AddDuty(AddDutyRequest model);
    }

    public class DutiesService : IDutiesService
    {
        private readonly AppDbContext _context;

        public DutiesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RequestResponse<List<DutiesResponse>>> GetDuties(int month)
        {
            var response = new List<DutiesResponse>();
            try
            {
                var result = _context.Duties.AsNoTracking().Include(x => x.Operator).Where(x => x.Date.Month == month).ToList();
                foreach (var item in result)
                {
                    response.Add(new DutiesResponse()
                    {
                        Id = item.Id,
                        OperatorId = item.OperatorId,
                        Date = DateOnly.FromDateTime(item.Date),
                        DutyType = (DutyType)item.DutyType,
                        OperatorName = item.Operator.Name
                    });
                }
                return RequestResponse<List<DutiesResponse>>.Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new RequestResponse<List<DutiesResponse>> { };
        }

        public async Task<RequestResponse<bool>> AddDuty(AddDutyRequest model)
        {
            if (model == null)
                return RequestResponse<bool>.Fail("Dados inválidos");

            try
            {
                var duty = new Duty
                {
                    OperatorId = model.OperatorId,
                    DutyType = (int)model.DutyType,
                    Date = model.Date.ToDateTime(TimeOnly.MinValue),
                    CreatedAt = DateTime.UtcNow,
                };

                _context.Duties.Add(duty);
                await _context.SaveChangesAsync();

                return RequestResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar duty: {ex.Message}");
                return RequestResponse<bool>.Fail("Erro interno ao salvar escala.");
            }
        }
    }
}
