using BravusApp.Client.Pages;
using BravusApp.Server.Data;
using BravusApp.Shared.Enums;
using BravusApp.Shared.Models;
using BravusApp.Shared.RequestModels;
using BravusApp.Shared.ResponseModels;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using System.Collections.Generic;
using System.Globalization;
using static MudBlazor.CategoryTypes;

namespace BravusApp.Server.Services
{
    public interface IDutiesService
    {
        Task<RequestResponse<List<DutiesResponse>>> GetDuties(int month, int year);
        Task<RequestResponse<bool>> AddDuty(AddDutyRequest model);
        Task<byte[]> ExportAsync(int year, int month, CancellationToken ct = default);
    }

    public class DutiesService : IDutiesService
    {
        private readonly AppDbContext _context;

        public DutiesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportAsync(int year, int month, CancellationToken ct = default)
        {
            var pt = new CultureInfo("pt-BR");
            var operators = (_context.Operators.ToList())
                .OrderBy(o => o.Name, StringComparer.Create(pt, true))
                .ToList();

            var duties = await GetDuties(month, year);

            var map = duties.Data
                .Where(d => d.Date.Year == year && d.Date.Month == month)
                .GroupBy(d => (d.OperatorId, d.Date))
                .ToDictionary(
                    g => g.Key,
                    g => ResolveDuty(g.Select(x => x.DutyType))
                );

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Escala");

            int diasNoMes = DateTime.DaysInMonth(year, month);
            int splitDay = Math.Min(15, diasNoMes); // 1–15 e 16–fim

            // cores
            var cHeader = XLColor.FromHtml("#111C2D");
            var cHeaderFont = XLColor.White;
            var cWeekend = XLColor.FromHtml("#E7EEF6");
            var cGrid = XLColor.FromHtml("#CBD5E1");
            var cLegendBorder = XLColor.FromHtml("#64748B");

            // --- título (cobre a largura do maior bloco) ---
            int part1Cols = 2 + splitDay;                    // Nº + VOLUNTÁRIO + dias 1..split
            int part2Cols = 2 + (diasNoMes - splitDay);      // Nº + VOLUNTÁRIO + dias split+1..fim
            int maxCols = Math.Max(part1Cols, part2Cols);

            ws.Cell(1, 1).Value =
                $"AGENDAMENTO DOS PLANTÕES DE {pt.TextInfo.ToTitleCase(pt.DateTimeFormat.GetMonthName(month).ToUpper())} DE {year}";
            ws.Range(1, 1, 1, maxCols).Merge().Style
                .Font.SetBold().Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            int currentTop = 3; // primeira grade começa na linha 3

            // ----- função para desenhar cada metade -----
            int DrawTable(int startDay, int endDay, int startRow)
            {
                // cabeçalho
                int headerRow = startRow;
                ws.Cell(headerRow, 1).Value = "Nº";
                ws.Cell(headerRow, 2).Value = "VOLUNTÁRIO";

                ws.Range(headerRow, 1, headerRow, 2).Style
                    .Fill.SetBackgroundColor(cHeader)
                    .Font.SetBold().Font.SetFontColor(cHeaderFont)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetOutsideBorderColor(cGrid);

                int colOffset = 2;
                for (int d = startDay; d <= endDay; d++)
                {
                    var data = new DateOnly(year, month, d);
                    int col = colOffset + (d - startDay) + 1;

                    var head = ws.Cell(headerRow, col);
                    head.Value = $"{d}\n{data.ToString("ddd", pt).ToUpper()}";
                    head.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                        .Alignment.SetWrapText(true)
                                        .Font.SetBold()
                                        .Fill.SetBackgroundColor(cHeader)
                                        .Font.SetFontColor(cHeaderFont)
                                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                                        .Border.SetOutsideBorderColor(cGrid);

                    ws.Column(col).Width = 5.2;
                }
                ws.Column(1).Width = 4.0;   // Nº
                ws.Column(2).Width = 28.0;  // VOLUNTÁRIO

                // dados
                int row = headerRow + 1;
                int index = 1;
                foreach (var op in operators)
                {
                    // número e nome
                    ws.Cell(row, 1).Value = index++;
                    ws.Cell(row, 2).Value = op.Name;

                    ws.Range(row, 1, row, 2).Style
                        .Font.SetBold()
                        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                        .Border.SetOutsideBorderColor(cGrid)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                    ws.Cell(row, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                    // dias
                    for (int d = startDay; d <= endDay; d++)
                    {
                        var data = new DateOnly(year, month, d);
                        int col = colOffset + (d - startDay) + 1;

                        string sigla = GetSigla(map.TryGetValue((op.Id, data), out var t) ? t : DutyType.None);

                        var cell = ws.Cell(row, col);
                        cell.Value = sigla;
                        cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                                            .Border.SetOutsideBorderColor(cGrid);

                        if (data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                            cell.Style.Fill.SetBackgroundColor(cWeekend);
                    }

                    row++;
                }

                // bordas internas da grade
                ws.Range(headerRow, 1, row - 1, colOffset + (endDay - startDay + 1))
                  .Style.Border.SetInsideBorder(XLBorderStyleValues.Thin)
                               .Border.SetInsideBorderColor(cGrid);

                return row; // próxima linha livre após a tabela
            }

            // desenha 1ª metade (1..splitDay) e 2ª metade (splitDay+1..diasNoMes)
            int afterPart1 = DrawTable(1, splitDay, currentTop);
            currentTop = afterPart1 + 2;
            int afterPart2 = DrawTable(splitDay + 1, diasNoMes, currentTop);

            // --- legenda (abaixo da 2ª metade) ---
            int legendTop = afterPart2 + 2;
            ws.Cell(legendTop, 1).Value = "SIGLAS DA PREVISÃO DE PLANTÃO";
            ws.Cell(legendTop, 1).Style.Font.SetBold();

            var legend = new (string Sigla, string Descricao)[]
            {
                ("SV/24", "SERVIÇO TURNO 08h – 08h"),
                ("SVD",   "SERVIÇO TURNO DIA 08h – 20h"),
                ("SVN",   "SERVIÇO TURNO NOITE 20h – 08h"),
                ("TD",    "SOMENTE TARDE 13h – 20h"),
            };

            int lr = legendTop + 1;
            foreach (var item in legend)
            {
                ws.Cell(lr, 1).Value = item.Sigla;
                ws.Cell(lr, 2).Value = item.Descricao;
                ws.Range(lr, 1, lr, 2).Style
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                    .Border.SetOutsideBorderColor(cLegendBorder);
                lr++;
            }

            var dataPt = DateOnly.FromDateTime(DateTime.Today).ToString("dd 'de' MMMM 'de' yyyy", pt);

            ws.PageSetup.Footer.Right.AddText($"Estância Velha, {dataPt}");

            // impressão
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.FitToPages(1, 2); // tenta caber em até 2 páginas, se necessário

            ws.SheetView.FreezeColumns(2); // fixa Nº e VOLUNTÁRIO na rolagem
            ws.Rows().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static DutyType ResolveDuty(IEnumerable<DutyType> types)
        {
            // remove duplicados exatos
            var set = types.Distinct().ToList();

            // 2) dia + noite => 24h
            if (set.Contains(DutyType.SV24)) return DutyType.SV24;  // SV/24
            if (set.Contains(DutyType.SVD) && set.Contains(DutyType.SVN))
                return DutyType.SV24;

            // 3) ordem de prioridade “operacional”
            if (set.Contains(DutyType.SVD)) return DutyType.SVD;           // SVD
            if (set.Contains(DutyType.SVN)) return DutyType.SVD;         // SVN
            if (set.Contains(DutyType.TD)) return DutyType.TD; // TD

            return DutyType.None;
        }

        private static string GetSigla(DutyType t) => t switch
        {
            DutyType.SV24 => "SV/24",
            DutyType.SVD => "SVD",
            DutyType.SVN => "SVN",
            DutyType.TD => "TD",
            _ => ""
        };

        public async Task<RequestResponse<List<DutiesResponse>>> GetDuties(int month, int year)
        {
            var response = new List<DutiesResponse>();
            try
            {
                var result = _context.Duties.AsNoTracking().Include(x => x.Operator).Where(x => x.Date.Month == month && x.Date.Year == year).ToList();
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

                _context.Add(duty);
                _context.SaveChanges();

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
