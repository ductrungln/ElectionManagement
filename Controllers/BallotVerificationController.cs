using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ElectionManagement.Models;
using ElectionManagement.Data;
using ElectionManagement.Services;

namespace ElectionManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BallotVerificationController : ControllerBase
    {
        private readonly ElectionDbContext _context;
        private readonly BallotImportService5Candidates _ballotImportService5;
        private readonly BallotImportService7Candidates _ballotImportService7;

        public BallotVerificationController(ElectionDbContext context, BallotImportService5Candidates ballotImportService5, BallotImportService7Candidates ballotImportService7)
        {
            _context = context;
            _ballotImportService5 = ballotImportService5;
            _ballotImportService7 = ballotImportService7;
        }

        // GET: api/ballotverification
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BallotVerification>>> GetAll([FromQuery] string level)
        {
            var query = _context.BallotVerifications.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(b => b.Level == level);
            var data = await query
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
            return Ok(data);
        }

        // GET: api/ballotverification/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BallotVerification>> GetById(int id)
        {
            var data = await _context.BallotVerifications.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Không tìm thấy dữ liệu" });
            return Ok(data);
        }

        // POST: api/ballotverification
        [HttpPost]
        public async Task<ActionResult<BallotVerification>> Create(BallotVerification request, [FromQuery] string level)
        {
            Console.WriteLine($"[CREATE DEBUG] Create() called - level={level}, districtName={request?.DistrictName}");
            
            if (request == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            level = level ?? "";
            var data = new BallotVerification
            {
                Level = level,
                DistrictName = request.DistrictName,
                IssuedBallots = request.IssuedBallots,
                ReceivedBallots = request.ReceivedBallots,
                ValidBallots = request.ValidBallots,
                InvalidBallots = request.InvalidBallots,
                BallotType1Count = request.BallotType1Count,
                BallotType1Votes = request.BallotType1Votes,
                BallotType2Count = request.BallotType2Count,
                BallotType2Votes = request.BallotType2Votes,
                BallotType3Count = request.BallotType3Count,
                BallotType3Votes = request.BallotType3Votes,
                BallotType4Count = request.BallotType4Count,
                BallotType4Votes = request.BallotType4Votes,
                Candidate1Votes = request.Candidate1Votes,
                Candidate2Votes = request.Candidate2Votes,
                Candidate3Votes = request.Candidate3Votes,
                Candidate4Votes = request.Candidate4Votes,
                Candidate5Votes = request.Candidate5Votes,
                Candidate6Votes = request.Candidate6Votes,
                Candidate7Votes = request.Candidate7Votes,
                Candidate1Name = request.Candidate1Name,
                Candidate2Name = request.Candidate2Name,
                Candidate3Name = request.Candidate3Name,
                Candidate4Name = request.Candidate4Name,
                Candidate5Name = request.Candidate5Name,
                Candidate6Name = request.Candidate6Name,
                Candidate7Name = request.Candidate7Name,
                TotalCandidates = request.TotalCandidates,
                CreatedDate = DateTime.Now
            };

            Console.WriteLine($"[CREATE DEBUG] About to add to DbSet - candidate6Votes={data.Candidate6Votes}, candidate7Votes={data.Candidate7Votes}");
            _context.BallotVerifications.Add(data);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[CREATE DEBUG] SaveChangesAsync completed - Id={data.Id}");

            return CreatedAtAction(nameof(GetById), new { id = data.Id }, data);
        }

        // PUT: api/ballotverification/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BallotVerification request)
        {
            Console.WriteLine($"[UPDATE DEBUG] Update() called - id={id}, districtName={request?.DistrictName}");
            
            var data = await _context.BallotVerifications.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Không tìm thấy dữ liệu" });

            data.DistrictName = request.DistrictName;
            data.IssuedBallots = request.IssuedBallots;
            data.ReceivedBallots = request.ReceivedBallots;
            data.ValidBallots = request.ValidBallots;
            data.InvalidBallots = request.InvalidBallots;
            data.BallotType1Count = request.BallotType1Count;
            data.BallotType1Votes = request.BallotType1Votes;
            data.BallotType2Count = request.BallotType2Count;
            data.BallotType2Votes = request.BallotType2Votes;
            data.BallotType3Count = request.BallotType3Count;
            data.BallotType3Votes = request.BallotType3Votes;
            data.BallotType4Count = request.BallotType4Count;
            data.BallotType4Votes = request.BallotType4Votes;
            data.Candidate1Votes = request.Candidate1Votes;
            data.Candidate2Votes = request.Candidate2Votes;
            data.Candidate3Votes = request.Candidate3Votes;
            data.Candidate4Votes = request.Candidate4Votes;
            data.Candidate5Votes = request.Candidate5Votes;
            data.Candidate6Votes = request.Candidate6Votes;
            data.Candidate7Votes = request.Candidate7Votes;
            data.Candidate1Name = request.Candidate1Name;
            data.Candidate2Name = request.Candidate2Name;
            data.Candidate3Name = request.Candidate3Name;
            data.Candidate4Name = request.Candidate4Name;
            data.Candidate5Name = request.Candidate5Name;
            data.Candidate6Name = request.Candidate6Name;
            data.Candidate7Name = request.Candidate7Name;
            data.TotalCandidates = request.TotalCandidates;
            data.UpdatedDate = DateTime.Now;

            Console.WriteLine($"[UPDATE DEBUG] About to update - candidate6Votes={data.Candidate6Votes}, candidate7Votes={data.Candidate7Votes}");
            _context.BallotVerifications.Update(data);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[UPDATE DEBUG] SaveChangesAsync completed");

            return Ok(new { message = "Cập nhật thành công", data });
        }

        // DELETE: api/ballotverification/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.BallotVerifications.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Không tìm thấy dữ liệu" });

            _context.BallotVerifications.Remove(data);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa thành công" });
        }

        // DELETE: api/ballotverification/clear-all
        [HttpDelete("clear-all")]
        public async Task<IActionResult> ClearAll([FromQuery] string level)
        {
            var query = _context.BallotVerifications.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(b => b.Level == level);
            var allData = await query.ToListAsync();
            _context.BallotVerifications.RemoveRange(allData);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa tất cả dữ liệu thành công" });
        }

        // POST: api/ballotverification/import-ballot
        [HttpPost("import-ballot")]
        public async Task<IActionResult> ImportBallotFile(IFormFile file, [FromQuery] string level)
        {
            Console.WriteLine($"[BACKEND DEBUG] ImportBallotFile() called - level={level}, fileName={file?.FileName}");
            
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file Excel" });

            // Kiểm tra file extension
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { message = "Chỉ chấp nhận file Excel (.xlsx, .xls)" });

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    // Use level parameter to determine which service to use
                    // Provincial level (tỉnh) uses 7 candidates
                    // District level (xã) uses 5 candidates
                    level = level ?? "Xã";
                    bool isProvincialLevel = !string.IsNullOrEmpty(level) && 
                                           level.ToLower().Contains("tinh");
                    
                    stream.Position = 0;
                    BallotImportResult result;
                    
                    if (isProvincialLevel)
                    {
                        // Provincial level: use 7-candidate service
                        result = await _ballotImportService7.ImportBallotDataAsync(stream);
                    }
                    else
                    {
                        // District level: use 5-candidate service
                        result = await _ballotImportService5.ImportBallotDataAsync(stream);
                    }
                    
                    if (!result.Success)
                    {
                        return BadRequest(new { 
                            message = result.Message,
                            errorDetails = result.ErrorDetails
                        });
                    }

                    // Don't save to database here - just return the parsed data
                    // User will fill in form and save manually using Create() endpoint
                    var ballotVerification = new BallotVerification
                    {
                        Level = level,
                        DistrictName = "", // User will fill this in
                        IssuedBallots = result.IssuedBallots,
                        ReceivedBallots = result.ReceivedBallots,
                        ValidBallots = result.ValidBallots,
                        InvalidBallots = result.InvalidBallots,
                        BallotType1Count = result.BallotType1Count,
                        BallotType1Votes = result.BallotType1Votes,
                        BallotType2Count = result.BallotType2Count,
                        BallotType2Votes = result.BallotType2Votes,
                        BallotType3Count = result.BallotType3Count,
                        BallotType3Votes = result.BallotType3Votes,
                        BallotType4Count = result.BallotType4Count,
                        BallotType4Votes = result.BallotType4Votes,
                        Candidate1Votes = result.Candidate1Votes,
                        Candidate2Votes = result.Candidate2Votes,
                        Candidate3Votes = result.Candidate3Votes,
                        Candidate4Votes = result.Candidate4Votes,
                        Candidate5Votes = result.Candidate5Votes,
                        Candidate6Votes = result.Candidate6Votes,
                        Candidate7Votes = result.Candidate7Votes,
                        Candidate1Name = result.Candidate1Name ?? "",
                        Candidate2Name = result.Candidate2Name ?? "",
                        Candidate3Name = result.Candidate3Name ?? "",
                        Candidate4Name = result.Candidate4Name ?? "",
                        Candidate5Name = result.Candidate5Name ?? "",
                        Candidate6Name = result.Candidate6Name ?? "",
                        Candidate7Name = result.Candidate7Name ?? "",
                        TotalCandidates = result.TotalCandidates
                    };

                    Console.WriteLine($"[IMPORT DEBUG] Returning parsed data only (not saving) - will be saved by user via save button");
                    
                    return Ok(new { 
                        message = "Nhập dữ liệu thành công",
                        data = ballotVerification
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Lỗi khi xử lý file",
                    errorDetails = ex.Message
                });
            }
        }

        private int DetectTotalCandidates(Stream stream)
        {
            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) return 5; // Default to 5
                    
                    // Read candidate names from row 4 (D4:J4)
                    var candidates = new[] {
                        worksheet.Cells[4, 4].Value?.ToString() ?? "",
                        worksheet.Cells[4, 5].Value?.ToString() ?? "",
                        worksheet.Cells[4, 6].Value?.ToString() ?? "",
                        worksheet.Cells[4, 7].Value?.ToString() ?? "",
                        worksheet.Cells[4, 8].Value?.ToString() ?? "",
                        worksheet.Cells[4, 9].Value?.ToString() ?? "",
                        worksheet.Cells[4, 10].Value?.ToString() ?? ""
                    };
                    
                    // Count non-empty candidates from the end
                    int count = 0;
                    for (int i = candidates.Length - 1; i >= 0; i--)
                    {
                        if (!string.IsNullOrEmpty(candidates[i]) && 
                            !candidates[i].Equals("-") && 
                            !candidates[i].Equals("―"))
                        {
                            count = i + 1;
                            break;
                        }
                    }
                    
                    return count > 0 ? count : 5; // Default to 5 if can't determine
                }
            }
            catch
            {
                return 5; // Default to 5 on error
            }
        }
    }
}
