using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectionManagement.Data;
using ElectionManagement.Models;
using ElectionManagement.Services;

namespace ElectionManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElectionController : ControllerBase
    {
        private readonly ElectionDbContext _context;
        private readonly IExcelService _excelService;
        private readonly IDashboardService _dashboardService;

        public ElectionController(ElectionDbContext context, IExcelService excelService, IDashboardService dashboardService)
        {
            _context = context;
            _excelService = excelService;
            _dashboardService = dashboardService;
        }

        // GET: api/election/results
        [HttpGet("results")]
        public async Task<IActionResult> GetResults([FromQuery] string level)
        {
            Console.WriteLine($"[DEBUG] GetResults called with level={level ?? "null"}");
            var query = _context.ElectionResults.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level))
            {
                Console.WriteLine($"[DEBUG] Applying filter: level == {level}");
                query = query.Where(r => r.Level == level);
            }
            var results = await query.OrderBy(r => r.Stt).ToListAsync();
            Console.WriteLine($"[DEBUG] GetResults returning {results.Count} records");
            if (results.Count > 0)
            {
                Console.WriteLine($"[DEBUG] First result: KhuVuc={results[0].KhuVuc}, Level={results[0].Level}");
            }
            return Ok(results);
        }

        // GET: api/election/progress
        [HttpGet("progress")]
        public async Task<IActionResult> GetProgress([FromQuery] string level)
        {
            var query = _context.ElectionProgresses.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(p => p.Level == level);
            }
            var progress = await query.OrderBy(p => p.Stt).ToListAsync();
            return Ok(progress);
        }

        // POST: api/election/import-results
        [HttpPost("import-results")]
        public async Task<IActionResult> ImportResults([FromForm] IFormFile file, [FromForm] string level)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không được để trống");

            if (!file.FileName.EndsWith(".xlsx"))
                return BadRequest("Chỉ chấp nhận file .xlsx");

            level = level ?? "";
            using (var stream = file.OpenReadStream())
            {
                var result = await _excelService.ImportElectionResults(stream, file.FileName, level);
                return Ok(result);
            }
        }

        // POST: api/election/import-progress
        [HttpPost("import-progress")]
        public async Task<IActionResult> ImportProgress([FromForm] IFormFile file, [FromForm] string level)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không được để trống");

            if (!file.FileName.EndsWith(".xlsx"))
                return BadRequest("Chỉ chấp nhận file .xlsx");

            level = level ?? "";
            using (var stream = file.OpenReadStream())
            {
                var result = await _excelService.ImportElectionProgress(stream, file.FileName, level);
                return Ok(result);
            }
        }

        // GET: api/election/export-comprehensive
        [HttpGet("export-comprehensive")]
        public async Task<IActionResult> ExportComprehensive([FromQuery] string level)
        {
            try
            {
                var resultsQuery = _context.ElectionResults.AsQueryable();
                var progressQuery = _context.ElectionProgresses.AsQueryable();
                
                if (!string.IsNullOrWhiteSpace(level))
                {
                    resultsQuery = resultsQuery.Where(r => r.Level == level);
                    progressQuery = progressQuery.Where(p => p.Level == level);
                }
                
                var results = await resultsQuery.OrderBy(r => r.Stt).ToListAsync();
                var progress = await progressQuery.OrderBy(p => p.Stt).ToListAsync();
                
                // Allow export even with no data to show form structure
                var fileBytes = await _excelService.ExportComprehensiveElectionResults(results, progress, level ?? "");
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SoLieuBauCu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi xuất file: {ex.Message}" });
            }
        }

        // POST: api/election/export-official-form (with page data)
        [HttpPost("export-official-form")]
        public async Task<IActionResult> ExportOfficialFormPost([FromBody] ExportOfficialFormRequest request)
        {
            try
            {
                Console.WriteLine("[DEBUG] ExportOfficialFormPost called");
                if (request == null)
                {
                    Console.WriteLine("[ERROR] Request is NULL!");
                    return BadRequest(new { message = "Request body is null" });
                }
                Console.WriteLine($"[DEBUG] Level: {request.Level}");
                Console.WriteLine($"[DEBUG] Table rows: {request.TableData?.Count ?? 0}");
                Console.WriteLine($"[DEBUG] Footer data: {request.FooterData?.Count ?? 0} items");
                
                // Convert page data to ElectionResult objects
                var results = new List<ElectionResult>();
                
                if (request.TableData != null)
                {
                    int rowNum = 1;
                    foreach (var rowData in request.TableData)
                    {
                        if (rowData == null || rowData.Count < 7) continue;
                        
                        try
                        {
                            // Parse all values first to avoid variable naming conflicts
                            // For XA/QUOCHOI level: 21 columns (A-U)
                            // A-L (0-11): basic info (Number, Ward, District, Voters, etc.)
                            // M-U (12-20): M=Bầu03, N=Bầu02, O=Bầu01, P=UCV1, Q=UCV2, R=UCV3, S=UCV4, T=UCV5, U=Cộng
                            int tongCuTri = int.TryParse(rowData[3], out int v1) ? v1 : 0;
                            int phieuPhatRa = rowData.Count > 6 ? (int.TryParse(rowData[6], out int v2) ? v2 : 0) : 0;
                            int phieuThuVe = rowData.Count > 7 ? (int.TryParse(rowData[7], out int v3) ? v3 : 0) : 0;
                            int phieuKhongHopLe = rowData.Count > 8 ? (int.TryParse(rowData[8], out int v4) ? v4 : 0) : 0;
                            int phieuHopLe = rowData.Count > 10 ? (int.TryParse(rowData[10], out int v5) ? v5 : 0) : 0;
                            // For XA/QUOCHOI: no Bầu 04, M(12)=Bầu03, N(13)=Bầu02, O(14)=Bầu01, P(15)=UCV1, Q(16)=UCV2, R(17)=UCV3, S(18)=UCV4, T(19)=UCV5
                            int phieuBau04 = 0;  // No Bầu 04 for XA/QUOCHOI levels
                            int phieuBau03 = rowData.Count > 12 ? (int.TryParse(rowData[12], out int v6) ? v6 : 0) : 0;
                            int phieuBau02 = rowData.Count > 13 ? (int.TryParse(rowData[13], out int v7) ? v7 : 0) : 0;
                            int phieuBau01 = rowData.Count > 14 ? (int.TryParse(rowData[14], out int v8) ? v8 : 0) : 0;
                            int ungCuVien1 = rowData.Count > 15 ? (int.TryParse(rowData[15], out int v9) ? v9 : 0) : 0;
                            int ungCuVien2 = rowData.Count > 16 ? (int.TryParse(rowData[16], out int v10) ? v10 : 0) : 0;
                            int ungCuVien3 = rowData.Count > 17 ? (int.TryParse(rowData[17], out int v11) ? v11 : 0) : 0;
                            int ungCuVien4 = rowData.Count > 18 ? (int.TryParse(rowData[18], out int v12) ? v12 : 0) : 0;
                            int ungCuVien5 = rowData.Count > 19 ? (int.TryParse(rowData[19], out int v13) ? v13 : 0) : 0;
                            int ungCuVien6 = 0;  // No UCV 6 for XA/QUOCHOI levels (only 5 candidates)
                            int ungCuVien7 = 0;  // No UCV 7 for XA/QUOCHOI levels (only 5 candidates)
                            
                            var result = new ElectionResult
                            {
                                Stt = rowNum,
                                Level = request.Level ?? "xa",
                                KhuVuc = rowData.Count > 2 ? rowData[2] : "",
                                To = rowData.Count > 1 ? rowData[1] : "",
                                TongCuTri = tongCuTri,
                                PhieuPhatRa = phieuPhatRa,
                                PhieuThuVe = phieuThuVe,
                                PhieuKhongHopLe = phieuKhongHopLe,
                                PhieuHopLe = phieuHopLe,
                                PhieuBau04 = phieuBau04,
                                PhieuBau03 = phieuBau03,
                                PhieuBau02 = phieuBau02,
                                PhieuBau01 = phieuBau01,
                                UngCuVien1 = ungCuVien1,
                                UngCuVien2 = ungCuVien2,
                                UngCuVien3 = ungCuVien3,
                                UngCuVien4 = ungCuVien4,
                                UngCuVien5 = ungCuVien5,
                                UngCuVien6 = ungCuVien6,
                                UngCuVien7 = ungCuVien7,
                            };
                            
                            results.Add(result);
                            rowNum++;
                            
                            Console.WriteLine($"[DEBUG] Converted row {rowNum-1}: KhuVuc={result.KhuVuc}, Candidates=[{result.UngCuVien1},{result.UngCuVien2},{result.UngCuVien3},{result.UngCuVien4},{result.UngCuVien5}]");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Failed to convert row: {ex.Message}");
                        }
                    }
                }
                
                Console.WriteLine($"[DEBUG] Converted {results.Count} rows to ElectionResult objects");
                
                // Use the existing export method with converted data (keeps same Excel structure)
                var fileBytes = await _excelService.ExportOfficialBallotVerificationForm(results, request.Level ?? "xa", "", request.FooterData);
                
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"BangTongHopKiemPhieu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ExportOfficialFormPost failed: {ex}");
                return BadRequest(new { message = $"Lỗi xuất file: {ex.Message}" });
            }
        }

        // GET: api/election/export-official-form (backward compatibility)
        [HttpGet("export-official-form")]
        public async Task<IActionResult> ExportOfficialForm([FromQuery] string level, [FromQuery] string area = "")
        {
            try
            {
                var query = _context.ElectionResults.AsQueryable();
                if (!string.IsNullOrWhiteSpace(level)) 
                    query = query.Where(r => r.Level == level);
                
                var results = await query.OrderBy(r => r.Stt).ToListAsync();
                
                // Allow export even with no data to show form structure
                var fileBytes = await _excelService.ExportOfficialBallotVerificationForm(results, level ?? "", area);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BangTongHopKiemPhieu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Lỗi xuất file: {ex.Message}" });
            }
        }

        // GET: api/election/export-results
        [HttpGet("export-results")]
        public async Task<IActionResult> ExportResults([FromQuery] string level)
        {
            var query = _context.ElectionResults.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level)) query = query.Where(r => r.Level == level);
            var data = await query.ToListAsync();
            var fileBytes = await _excelService.ExportElectionResults(data);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "KetQuaBauCu.xlsx");
        }

        // GET: api/election/export-progress
        [HttpGet("export-progress")]
        public async Task<IActionResult> ExportProgress([FromQuery] string level)
        {
            var query = _context.ElectionProgresses.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level)) query = query.Where(p => p.Level == level);
            var data = await query.ToListAsync();
            
            // Calculate summary for 10 groups
            var groupedData = new Dictionary<int, dynamic>();
            
            // Initialize groups
            for (int i = 1; i <= 10; i++)
            {
                groupedData[i] = new
                {
                    GroupNum = i,
                    TongCuTri = 0,
                    TongCuTriDiBau = 0,
                    Gio8 = 0,
                    Gio10 = 0,
                    Gio12 = 0,
                    Gio14 = 0,
                    Gio16 = 0
                };
            }
            
            // Group data by unit number (DonVi)
            foreach (var item in data)
            {
                // Extract group number from donVi only (e.g., "Đơn vị 1" -> 1)
                var match = System.Text.RegularExpressions.Regex.Match(item.DonVi ?? "", @"(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int toNum) && toNum >= 1 && toNum <= 10)
                {
                    var current = groupedData[toNum];
                    groupedData[toNum] = new
                    {
                        GroupNum = toNum,
                        TongCuTri = current.TongCuTri + (item.TongCuTri),
                        TongCuTriDiBau = current.TongCuTriDiBau + (item.TongCuTriDiBau),
                        Gio8 = current.Gio8 + (item.Gio8),
                        Gio10 = current.Gio10 + (item.Gio10),
                        Gio12 = current.Gio12 + (item.Gio12),
                        Gio14 = current.Gio14 + (item.Gio14),
                        Gio16 = current.Gio16 + (item.Gio16)
                    };
                }
            }
            
            var summaryData = groupedData.OrderByDescending(x => x.Key).Select(x => x.Value).ToList();
            
            var fileBytes = await _excelService.ExportProgressSummary(summaryData);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TongHopTienDo.xlsx");
        }

        // DELETE: api/election/results/clear
        [HttpDelete("results/clear")]
        public async Task<IActionResult> ClearResults([FromQuery] string level)
        {
            try
            {
                var query = _context.ElectionResults.AsQueryable();
                if (!string.IsNullOrWhiteSpace(level))
                    query = query.Where(r => r.Level == level);
                _context.ElectionResults.RemoveRange(query);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã xóa dữ liệu kết quả bầu cử" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/election/progress/clear
        [HttpDelete("progress/clear")]
        public async Task<IActionResult> ClearProgress([FromQuery] string level)
        {
            try
            {
                var query = _context.ElectionProgresses.AsQueryable();
                if (!string.IsNullOrWhiteSpace(level))
                    query = query.Where(p => p.Level == level);
                _context.ElectionProgresses.RemoveRange(query);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã xóa dữ liệu tiến độ bầu cử" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/election/import-logs/clear
        [HttpDelete("import-logs/clear")]
        public async Task<IActionResult> ClearImportLogs([FromQuery] string level)
        {
            try
            {
                var query = _context.ImportLogs.AsQueryable();
                if (!string.IsNullOrWhiteSpace(level))
                    query = query.Where(l => l.Level == level);
                _context.ImportLogs.RemoveRange(query);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã xóa lịch sử import" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/election/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] string level)
        {
            var summary = await _dashboardService.GetSummary(level);
            return Ok(summary);
        }

        // GET: api/election/import-logs
        [HttpGet("import-logs")]
        public async Task<IActionResult> GetImportLogs([FromQuery] string level)
        {
            var query = _context.ImportLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(l => l.Level == level);
            var logs = await query.OrderByDescending(l => l.Id).ToListAsync();
            return Ok(logs);
        }

        // DELETE: api/election/results/{id}
        [HttpDelete("results/{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.ElectionResults.FindAsync(id);
            if (result == null)
                return NotFound();

            _context.ElectionResults.Remove(result);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // CLEAR: api/election/results/clear-all
        [HttpDelete("results/clear-all")]
        public async Task<IActionResult> ClearAllResults([FromQuery] string level)
        {
            var query = _context.ElectionResults.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level)) query = query.Where(r => r.Level == level);
            _context.ElectionResults.RemoveRange(query);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa kết quả bầu cử thành công" });
        }

        // DELETE: api/election/progress/{id}
        [HttpDelete("progress/{id}")]
        public async Task<IActionResult> DeleteProgress(int id)
        {
            var progress = await _context.ElectionProgresses.FindAsync(id);
            if (progress == null)
                return NotFound();

            _context.ElectionProgresses.Remove(progress);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // CLEAR: api/election/progress/clear-all
        [HttpDelete("progress/clear-all")]
        public async Task<IActionResult> ClearAllProgress([FromQuery] string level)
        {
            var query = _context.ElectionProgresses.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level)) query = query.Where(p => p.Level == level);
            _context.ElectionProgresses.RemoveRange(query);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa tiến độ bầu cử thành công" });
        }

        // CLEAR: api/election/logs/clear-all
        [HttpDelete("logs/clear-all")]
        public async Task<IActionResult> ClearAllLogs([FromQuery] string level)
        {
            var query = _context.ImportLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level)) query = query.Where(l => l.Level == level);
            _context.ImportLogs.RemoveRange(query);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa lịch sử import thành công" });
        }

        // DELETE: api/election/clear-all-data (Xóa tất cả dữ liệu bầu cử)
        [HttpDelete("clear-all-data")]
        public async Task<IActionResult> ClearAllData()
        {
            try
            {
                // Delete all tables
                _context.ElectionResults.RemoveRange(_context.ElectionResults);
                _context.ElectionProgresses.RemoveRange(_context.ElectionProgresses);
                _context.ImportLogs.RemoveRange(_context.ImportLogs);
                _context.BallotVerifications.RemoveRange(_context.BallotVerifications);
                
                // Delete other election-related entities if they exist
                // Add more DbSets if needed based on your database schema
                
                await _context.SaveChangesAsync();
                
                Console.WriteLine("[DELETE-ALL] Đã xóa tất cả dữ liệu bầu cử thành công");
                return Ok(new { message = "✅ Đã xóa tất cả dữ liệu bầu cử thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DELETE-ALL] Lỗi: {ex.Message}");
                return StatusCode(500, new { message = "❌ Lỗi khi xóa dữ liệu: " + ex.Message });
            }
        }
    }
}
