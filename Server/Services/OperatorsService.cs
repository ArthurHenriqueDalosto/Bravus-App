using BCrypt.Net;
using BravusApp.Server.Data;
using BravusApp.Shared.Models;
using BravusApp.Shared.RequestModels;
using BravusApp.Shared.ResponseModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BravusApp.Server.Services
{
    public interface IOperatorService
    {
        Task<RequestResponse<bool>> AddOperator(AddOperatorRequest model);
        Task<RequestResponse<bool>> ChangePassword(ChangePasswordRequest model);
        Task<RequestResponse<bool>> ChangeStatus(int id);
        Task<RequestResponse<List<OperatorsResponse>>> GetOperators();
    }

    public class OperatorService : IOperatorService
    {
        private readonly AppDbContext _context;

        public OperatorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RequestResponse<bool>> AddOperator(AddOperatorRequest model)
        {
            try
            {
                if (String.IsNullOrEmpty(model.OperatorName) && String.IsNullOrEmpty(model.OperatorName))
                    return RequestResponse<bool>.Fail("Nome e CPF precisam ser preenchidos");

                var result = _context.Operators.Where(x => x.Cpf == model.Cpf).FirstOrDefault();

                if (result != null)
                    return RequestResponse<bool>.Fail("Já existe um operador registrado com este cpf");


                _context.Add(new Operator()
                {
                    Cpf = model.Cpf,
                    Name = model.OperatorName,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    PswdChanged = false,
                    Password = BCrypt.Net.BCrypt.HashPassword("123"),
                });
                _context.SaveChanges();
                return RequestResponse<bool>.Ok(true);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RequestResponse<bool>.Fail("Erro ao Cadastrar operador");
            }
        }

        public async Task<RequestResponse<bool>> ChangePassword(ChangePasswordRequest model)
        {
            try
            {
                var user = _context.Operators.Where(x => x.Id == model.id).FirstOrDefault();

                if (user == null)
                    return RequestResponse<bool>.Fail("Usuário não encontrado");

                if (model.isReset)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword("123");
                    user.PswdChanged = false;
                }
                else
                {
                    if (String.IsNullOrEmpty(model.newPswd))
                        return RequestResponse<bool>.Fail("Preencha a nova senha!");

                    user.Password = BCrypt.Net.BCrypt.HashPassword(model.newPswd);
                    user.PswdChanged = true;
                }
                _context.SaveChanges();

                return RequestResponse<bool>.Ok(true);

            }
            catch (Exception ex)
            {
                return RequestResponse<bool>.Fail("Erro ao atualizar senha");
            }
        }

        public async Task<RequestResponse<bool>> ChangeStatus(int id)
        {
            try
            {
                var model = _context.Operators.Where(x => x.Id == id).FirstOrDefault();
                if (model != null)
                {
                    model.IsActive = !model.IsActive;
                    _context.SaveChanges();
                    return RequestResponse<bool>.Ok(true);
                }
                else
                    return RequestResponse<bool>.Fail("Usuário não encontrado");
            }
            catch (Exception ex)
            {
                return RequestResponse<bool>.Fail("Erro Atualizar status");
            }
        }

        public async Task<RequestResponse<List<OperatorsResponse>>> GetOperators()
        {
            var response = new List<OperatorsResponse>();
            try
            {
                var result = _context.Operators.ToList();
                foreach (var resultItem in result)
                {
                    response.Add(new OperatorsResponse()
                    {
                        Id = resultItem.Id,
                        Name = resultItem.Name,
                    });
                }
                return RequestResponse<List<OperatorsResponse>>.Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return RequestResponse<List<OperatorsResponse>>.Fail("Erro ao Cadastrar operador");
            }
        }
    }
}
