using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using ElectionManagement.Models;

namespace ElectionManagement.Services
{
    public class BallotImportService5Candidates
    {
        /// <summary>
        /// Import ballot data for 5 candidates with 3 ballot types
        /// Type 3 (Choose 3): Rows 6-15 - Cross out 2, select 3 people - C(5,2)=10 combinations
        /// Type 2 (Choose 2): Rows 16-25 - Cross out 3, select 2 people - C(5,3)=10 combinations
        /// Type 1 (Choose 1): Rows 26-30 - Cross out 4, select 1 person - C(5,4)=5 combinations
        /// Summary rows: 31-32 (Type 1), 54-55 (Type 2), 77-78 (Type 3)
        /// </summary>
        public async Task<BallotImportResult> ImportBallotDataAsync(Stream fileStream)
        {
            var result = new BallotImportResult();

            try
            {
                if (fileStream == null || fileStream.Length == 0)
                {
                    result.Success = false;
                    result.Message = "File không hợp lệ";
                    result.ErrorDetails = "File trống hoặc không tồn tại";
                    return result;
                }

                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    
                    if (worksheet == null)
                    {
                        result.Success = false;
                        result.Message = "File Excel không có sheet";
                        return result;
                    }

                    // Initialize variables
                    int issuedBallots = 0;
                    int receivedBallots = 0;
                    int validBallots = 0;
                    int invalidBallots = 0;
                    int ballotType1Count = 0;
                    int ballotType2Count = 0;
                    int ballotType3Count = 0;
                    int ballotType1Votes = 0;
                    int ballotType2Votes = 0;
                    int ballotType3Votes = 0;
                    int candidate1Total = 0;
                    int candidate2Total = 0;
                    int candidate3Total = 0;
                    int candidate4Total = 0;
                    int candidate5Total = 0;

                    // === PARSE METADATA ===
                    // Phiếu phát ra: Tìm từ row 2, bất kỳ cột nào
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        var cellText = worksheet.Cells[2, col].Value?.ToString() ?? "";
                        if (cellText.Contains("Phiếu phát ra") || cellText.Contains("phát ra"))
                        {
                            var nextCellVal = worksheet.Cells[2, col + 1].Value?.ToString() ?? "";
                            if (int.TryParse(nextCellVal, out int num))
                            {
                                issuedBallots = num;
                                Console.WriteLine($"[5-CAND] Phiếu phát ra (Issued): {issuedBallots}");
                                break;
                            }
                        }
                    }

                    // Phiếu thu vào: Lấy từ ô H3 (Row 3, Column H = 8)
                    var receivedCellVal = worksheet.Cells[3, 8].Value?.ToString() ?? "";
                    if (int.TryParse(receivedCellVal, out int receivedNum))
                    {
                        receivedBallots = receivedNum;
                        Console.WriteLine($"[5-CAND] Phiếu thu vào (Received) from H3: {receivedBallots}");
                    }

                    // === READ CANDIDATE NAMES FROM ROW 4 ===
                    Console.WriteLine($"\n===== [5-CAND] READING CANDIDATE NAMES FROM ROW 4 =====");
                    
                    var candidate1NameVal = worksheet.Cells[4, 4].Value?.ToString() ?? "";
                    var candidate2NameVal = worksheet.Cells[4, 5].Value?.ToString() ?? "";
                    var candidate3NameVal = worksheet.Cells[4, 6].Value?.ToString() ?? "";
                    var candidate4NameVal = worksheet.Cells[4, 7].Value?.ToString() ?? "";
                    var candidate5NameVal = worksheet.Cells[4, 8].Value?.ToString() ?? "";
                    
                    Console.WriteLine($"  D4 (Cand 1): '{candidate1NameVal}'");
                    Console.WriteLine($"  E4 (Cand 2): '{candidate2NameVal}'");
                    Console.WriteLine($"  F4 (Cand 3): '{candidate3NameVal}'");
                    Console.WriteLine($"  G4 (Cand 4): '{candidate4NameVal}'");
                    Console.WriteLine($"  H4 (Cand 5): '{candidate5NameVal}'");

                    // ===== READ 3 BALLOT TYPES for 5 CANDIDATES =====
                    // Count ballots from data rows
                    Console.WriteLine($"\n===== [5-CAND] READING 3 BALLOT TYPES FROM DATA ROWS =====");
                    
                    // Type 3 (Choose 3, Cross 2): Rows 6-15 (10 pairs like 1-2, 1-3, etc.)
                    Console.WriteLine($"\n[5-CAND] Type 3 (Choose 3, Cross 2): Rows 6-15");
                    ballotType3Count = 0;
                    for (int row = 6; row <= 15; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "0";
                        if (int.TryParse(cellVal, out int count))
                            ballotType3Count += count;
                    }
                    ballotType3Votes = ballotType3Count * 3;
                    Console.WriteLine($"  Total: {ballotType3Count} phiếu × 3 votes = {ballotType3Votes}");
                    
                    // Type 2 (Choose 2, Cross 3): Rows 16-25 (10 triples like 1-2-3, 1-2-4, etc.)
                    Console.WriteLine($"\n[5-CAND] Type 2 (Choose 2, Cross 3): Rows 16-25");
                    ballotType2Count = 0;
                    for (int row = 16; row <= 25; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "0";
                        if (int.TryParse(cellVal, out int count))
                            ballotType2Count += count;
                    }
                    ballotType2Votes = ballotType2Count * 2;
                    Console.WriteLine($"  Total: {ballotType2Count} phiếu × 2 votes = {ballotType2Votes}");
                    
                    // Type 1 (Choose 1, Cross 4): Rows 26-30 (5 quadruples like 1-2-3-4, 1-2-3-5, etc.)
                    Console.WriteLine($"\n[5-CAND] Type 1 (Choose 1, Cross 4): Rows 26-30");
                    ballotType1Count = 0;
                    for (int row = 26; row <= 30; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "0";
                        if (int.TryParse(cellVal, out int count))
                            ballotType1Count += count;
                    }
                    ballotType1Votes = ballotType1Count * 1;
                    Console.WriteLine($"  Total: {ballotType1Count} phiếu × 1 vote = {ballotType1Votes}");
                    
                    // === READ INVALID BALLOTS FROM C31 ===
                    var invalidBallotsVal = worksheet.Cells[31, 3].Value?.ToString() ?? "0";
                    if (int.TryParse(invalidBallotsVal, out int invalidNum))
                    {
                        invalidBallots = invalidNum;
                    }
                    
                    // Phiếu hợp lệ = Phiếu thu vào - Phiếu không hợp lệ
                    validBallots = receivedBallots - invalidBallots;
                    Console.WriteLine($"[5-CAND] Phiếu hợp lệ (Valid) = {receivedBallots} - {invalidBallots} = {validBallots}");
                    
                    int totalWeightedVotes = ballotType1Votes + ballotType2Votes + ballotType3Votes;
                    Console.WriteLine($"\n===== [5-CAND] TOTAL BALLOT SUMMARY =====");
                    Console.WriteLine($"Tổng phiếu không hợp lệ (C31): {invalidBallots}");
                    Console.WriteLine($"Tổng phiếu hợp lệ: {validBallots}");
                    Console.WriteLine($"Tổng weighted votes: {totalWeightedVotes}");

                    // === READ CANDIDATE VOTES FROM SUMMARY ROWS ===
                    // Summary rows contain CROSSED-OUT counts for each candidate per ballot type
                    // Formula: votes = (valid_ballots - crossed_out_count) × multiplier
                    Console.WriteLine($"\n===== [5-CAND] READING CANDIDATE VOTES FROM SUMMARY ROWS =====");
                    
                    // Type 1: Row 32 (summary), multiplier = 1
                    // C32 = valid ballots, D32:H32 = crossed-out counts
                    Console.WriteLine($"\n[5-CAND] Type 1 (Row 32) - multiplier 1:");
                    var type1ValidStr = worksheet.Cells[32, 3].Value?.ToString() ?? "0";
                    int type1Valid = 0;
                    if (int.TryParse(type1ValidStr, out int v1))
                        type1Valid = v1;
                    Console.WriteLine($"  Valid ballots (C32): {type1Valid}");
                    
                    for (int cand = 1; cand <= 5; cand++)
                    {
                        int col = 3 + cand; // Column D=4, E=5, F=6, G=7, H=8
                        var cellVal = worksheet.Cells[32, col].Value?.ToString() ?? "0";
                        int crossedOutCount = 0;
                        if (int.TryParse(cellVal, out int count))
                            crossedOutCount = count;
                        
                        int votesReceived = (type1Valid - crossedOutCount) * 1;
                        
                        switch (cand)
                        {
                            case 1: candidate1Total += votesReceived; break;
                            case 2: candidate2Total += votesReceived; break;
                            case 3: candidate3Total += votesReceived; break;
                            case 4: candidate4Total += votesReceived; break;
                            case 5: candidate5Total += votesReceived; break;
                        }
                        
                        Console.WriteLine($"  Person {cand}: ({type1Valid} - {crossedOutCount}) × 1 = {votesReceived} votes");
                    }
                    
                    // Type 2: Row 55 (summary), multiplier = 2
                    // C55 = valid ballots, D55:H55 = crossed-out counts
                    Console.WriteLine($"\n[5-CAND] Type 2 (Row 55) - multiplier 2:");
                    var type2ValidStr = worksheet.Cells[55, 3].Value?.ToString() ?? "0";
                    int type2Valid = 0;
                    if (int.TryParse(type2ValidStr, out int v2))
                        type2Valid = v2;
                    Console.WriteLine($"  Valid ballots (C55): {type2Valid}");
                    
                    for (int cand = 1; cand <= 5; cand++)
                    {
                        int col = 3 + cand;
                        var cellVal = worksheet.Cells[55, col].Value?.ToString() ?? "0";
                        int crossedOutCount = 0;
                        if (int.TryParse(cellVal, out int count))
                            crossedOutCount = count;
                        
                        int votesReceived = (type2Valid - crossedOutCount) * 2;
                        
                        switch (cand)
                        {
                            case 1: candidate1Total += votesReceived; break;
                            case 2: candidate2Total += votesReceived; break;
                            case 3: candidate3Total += votesReceived; break;
                            case 4: candidate4Total += votesReceived; break;
                            case 5: candidate5Total += votesReceived; break;
                        }
                        
                        Console.WriteLine($"  Person {cand}: ({type2Valid} - {crossedOutCount}) × 2 = {votesReceived} votes");
                    }
                    
                    // Type 3: Row 78 (summary), multiplier = 3
                    // C78 = valid ballots, D78:H78 = crossed-out counts
                    Console.WriteLine($"\n[5-CAND] Type 3 (Row 78) - multiplier 3:");
                    var type3ValidStr = worksheet.Cells[78, 3].Value?.ToString() ?? "0";
                    int type3Valid = 0;
                    if (int.TryParse(type3ValidStr, out int v3))
                        type3Valid = v3;
                    Console.WriteLine($"  Valid ballots (C78): {type3Valid}");
                    
                    for (int cand = 1; cand <= 5; cand++)
                    {
                        int col = 3 + cand;
                        var cellVal = worksheet.Cells[78, col].Value?.ToString() ?? "0";
                        int crossedOutCount = 0;
                        if (int.TryParse(cellVal, out int count))
                            crossedOutCount = count;
                        
                        int votesReceived = (type3Valid - crossedOutCount) * 3;
                        
                        switch (cand)
                        {
                            case 1: candidate1Total += votesReceived; break;
                            case 2: candidate2Total += votesReceived; break;
                            case 3: candidate3Total += votesReceived; break;
                            case 4: candidate4Total += votesReceived; break;
                            case 5: candidate5Total += votesReceived; break;
                        }
                        
                        Console.WriteLine($"  Person {cand}: ({type3Valid} - {crossedOutCount}) × 3 = {votesReceived} votes");
                    }
                    
                    Console.WriteLine($"\n===== [5-CAND] TOTAL CANDIDATE VOTES =====");
                    Console.WriteLine($"Person 1: {candidate1Total}");
                    Console.WriteLine($"Person 2: {candidate2Total}");
                    Console.WriteLine($"Person 3: {candidate3Total}");
                    Console.WriteLine($"Person 4: {candidate4Total}");
                    Console.WriteLine($"Person 5: {candidate5Total}");
                    Console.WriteLine($"TOTAL WEIGHTED VOTES: {candidate1Total + candidate2Total + candidate3Total + candidate4Total + candidate5Total}");

                    // === BUILD RESULT ===
                    int totalBallots = ballotType1Count + ballotType2Count + ballotType3Count;

                    result.TotalBallots = totalBallots;
                    result.IssuedBallots = issuedBallots > 0 ? issuedBallots : totalBallots;
                    result.ReceivedBallots = receivedBallots > 0 ? receivedBallots : totalBallots;
                    result.ValidBallots = validBallots > 0 ? validBallots : totalBallots;
                    result.InvalidBallots = invalidBallots;
                    result.BallotType1Count = ballotType1Count;
                    result.BallotType1Votes = ballotType1Votes;
                    result.BallotType2Count = ballotType2Count;
                    result.BallotType2Votes = ballotType2Votes;
                    result.BallotType3Count = ballotType3Count;
                    result.BallotType3Votes = ballotType3Votes;
                    result.BallotType4Count = 0;
                    result.BallotType4Votes = 0;
                    result.Candidate1Votes = candidate1Total;
                    result.Candidate2Votes = candidate2Total;
                    result.Candidate3Votes = candidate3Total;
                    result.Candidate4Votes = candidate4Total;
                    result.Candidate5Votes = candidate5Total;
                    result.Candidate6Votes = 0;
                    result.Candidate7Votes = 0;
                    result.Candidate8Votes = 0;
                    result.Candidate1Name = candidate1NameVal;
                    result.Candidate2Name = candidate2NameVal;
                    result.Candidate3Name = candidate3NameVal;
                    result.Candidate4Name = candidate4NameVal;
                    result.Candidate5Name = candidate5NameVal;
                    result.Candidate6Name = "";
                    result.Candidate7Name = "";
                    result.Candidate8Name = "";
                    result.TotalCandidates = 5;
                    result.Success = true;
                    result.Message = $"[5-CAND] Import thành công! Tổng phiếu: {totalBallots}, Hợp lệ: {validBallots}";

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "[5-CAND] Lỗi khi xử lý file Excel";
                result.ErrorDetails = ex.Message;
                return result;
            }
        }
    }
}
