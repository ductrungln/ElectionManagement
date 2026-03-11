using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectionManagement.Data;
using ElectionManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectionManagement.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummary> GetSummary(string level = null);
    }

    public class DashboardSummary
    {
        public int TotalDistricts { get; set; }
        public int TotalVoters { get; set; }
        public int VotedCount { get; set; }
        public decimal TurnoutPercentage { get; set; }
        public List<CandidateResult> CandidateResults { get; set; } = new();
    }

    public class CandidateResult
    {
        public int CandidateNumber { get; set; }
        public int TotalVotes { get; set; }
        public decimal Percentage { get; set; }
    }

    public class DashboardService : IDashboardService
    {
        private readonly ElectionDbContext _context;

        public DashboardService(ElectionDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummary> GetSummary(string level = null)
        {
            var query = _context.ElectionResults.AsQueryable();
            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(r => r.Level == level);
            var results = await query.ToListAsync();

            if (!results.Any())
            {
                return new DashboardSummary();
            }

            var summary = new DashboardSummary
            {
                TotalDistricts = results.Count,
                TotalVoters = results.Sum(r => r.TongCuTri),
                VotedCount = results.Sum(r => r.PhieuThuVe),
            };

            summary.TurnoutPercentage = summary.TotalVoters > 0 
                ? Math.Round((decimal)summary.VotedCount / summary.TotalVoters * 100, 2)
                : 0;

            var totalValidVotes = results.Sum(r => r.PhieuHopLe);

            summary.CandidateResults = new List<CandidateResult>
            {
                new CandidateResult 
                { 
                    CandidateNumber = 1, 
                    TotalVotes = results.Sum(r => r.UngCuVien1),
                    Percentage = totalValidVotes > 0 ? Math.Round((decimal)results.Sum(r => r.UngCuVien1) / totalValidVotes * 100, 2) : 0
                },
                new CandidateResult 
                { 
                    CandidateNumber = 2, 
                    TotalVotes = results.Sum(r => r.UngCuVien2),
                    Percentage = totalValidVotes > 0 ? Math.Round((decimal)results.Sum(r => r.UngCuVien2) / totalValidVotes * 100, 2) : 0
                },
                new CandidateResult 
                { 
                    CandidateNumber = 3, 
                    TotalVotes = results.Sum(r => r.UngCuVien3),
                    Percentage = totalValidVotes > 0 ? Math.Round((decimal)results.Sum(r => r.UngCuVien3) / totalValidVotes * 100, 2) : 0
                },
                new CandidateResult 
                { 
                    CandidateNumber = 4, 
                    TotalVotes = results.Sum(r => r.UngCuVien4),
                    Percentage = totalValidVotes > 0 ? Math.Round((decimal)results.Sum(r => r.UngCuVien4) / totalValidVotes * 100, 2) : 0
                },
                new CandidateResult 
                { 
                    CandidateNumber = 5, 
                    TotalVotes = results.Sum(r => r.UngCuVien5),
                    Percentage = totalValidVotes > 0 ? Math.Round((decimal)results.Sum(r => r.UngCuVien5) / totalValidVotes * 100, 2) : 0
                }
            };

            return summary;
        }
    }
}
