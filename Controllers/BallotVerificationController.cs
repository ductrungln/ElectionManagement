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
        private readonly BallotImportService _ballotImportService;

        public BallotVerificationController(ElectionDbContext context, BallotImportService ballotImportService)
        {
            _context = context;
            _ballotImportService = ballotImportService;
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
                Candidate1Votes = request.Candidate1Votes,
                Candidate2Votes = request.Candidate2Votes,
                Candidate3Votes = request.Candidate3Votes,
                Candidate4Votes = request.Candidate4Votes,
                Candidate5Votes = request.Candidate5Votes,
                Candidate1Name = request.Candidate1Name,
                Candidate2Name = request.Candidate2Name,
                Candidate3Name = request.Candidate3Name,
                Candidate4Name = request.Candidate4Name,
                Candidate5Name = request.Candidate5Name,
                CreatedDate = DateTime.Now
            };

            _context.BallotVerifications.Add(data);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = data.Id }, data);
        }

        // PUT: api/ballotverification/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BallotVerification request)
        {
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
            data.Candidate1Votes = request.Candidate1Votes;
            data.Candidate2Votes = request.Candidate2Votes;
            data.Candidate3Votes = request.Candidate3Votes;
            data.Candidate4Votes = request.Candidate4Votes;
            data.Candidate5Votes = request.Candidate5Votes;
            data.Candidate1Name = request.Candidate1Name;
            data.Candidate2Name = request.Candidate2Name;
            data.Candidate3Name = request.Candidate3Name;
            data.Candidate4Name = request.Candidate4Name;
            data.Candidate5Name = request.Candidate5Name;
            data.UpdatedDate = DateTime.Now;

            _context.BallotVerifications.Update(data);
            await _context.SaveChangesAsync();

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
        public async Task<IActionResult> ImportBallotFile(IFormFile file)
        {
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
                    var result = await _ballotImportService.ImportBallotDataAsync(stream);
                    
                    if (!result.Success)
                    {
                        return BadRequest(new { 
                            message = result.Message,
                            errorDetails = result.ErrorDetails
                        });
                    }

                    return Ok(new { 
                        message = result.Message,
                        data = result
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
    }
}
