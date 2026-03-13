using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using ElectionManagement.Models;

namespace ElectionManagement.Services
{
    public class BallotImportService7Candidates
    {
        /// <summary>
        /// Import ballot data for 7 candidates with 4 ballot types
        /// Type 1 (Choose 1): Row 31-32 - Cross out 6, select 1 person
        /// Type 2 (Choose 2): Row 54-55 - Cross out 5, select 2 people
        /// Type 3 (Choose 3): Row 77-78 - Cross out 4, select 3 people
        /// Type 4 (Choose 4): Row 104-105 - Cross out 3, select 4 people
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
                    int ballotType4Count = 0;
                    int ballotType1Votes = 0;
                    int ballotType2Votes = 0;
                    int ballotType3Votes = 0;
                    int ballotType4Votes = 0;
                    int candidate1Total = 0;
                    int candidate2Total = 0;
                    int candidate3Total = 0;
                    int candidate4Total = 0;
                    int candidate5Total = 0;
                    int candidate6Total = 0;
                    int candidate7Total = 0;

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
                                Console.WriteLine($"[7-CAND] Phiếu phát ra (Issued): {issuedBallots}");
                                break;
                            }
                        }
                    }

                    // Phiếu thu vào: Lấy từ ô J3 (Row 3, Column J = 10)
                    var receivedCellVal = worksheet.Cells[3, 10].Value?.ToString() ?? "";
                    Console.WriteLine($"[7-CAND] DEBUG - J3 raw value: '{receivedCellVal}'");
                    if (int.TryParse(receivedCellVal, out int receivedNum))
                    {
                        receivedBallots = receivedNum;
                        Console.WriteLine($"[7-CAND] Phiếu thu vào (Received) from J3: {receivedBallots}");
                    }
                    else
                    {
                        Console.WriteLine($"[7-CAND] WARN: Không parse được J3, giá trị: '{receivedCellVal}'");
                    }

                    // === READ CANDIDATE NAMES FROM ROW 4 ===
                    Console.WriteLine($"\n===== [7-CAND] READING CANDIDATE NAMES FROM ROW 4 =====");
                    
                    var candidate1NameVal = worksheet.Cells[4, 4].Value?.ToString() ?? "";
                    var candidate2NameVal = worksheet.Cells[4, 5].Value?.ToString() ?? "";
                    var candidate3NameVal = worksheet.Cells[4, 6].Value?.ToString() ?? "";
                    var candidate4NameVal = worksheet.Cells[4, 7].Value?.ToString() ?? "";
                    var candidate5NameVal = worksheet.Cells[4, 8].Value?.ToString() ?? "";
                    var candidate6NameVal = worksheet.Cells[4, 9].Value?.ToString() ?? "";
                    var candidate7NameVal = worksheet.Cells[4, 10].Value?.ToString() ?? "";
                    
                    Console.WriteLine($"  D4 (Cand 1): '{candidate1NameVal}'");
                    Console.WriteLine($"  E4 (Cand 2): '{candidate2NameVal}'");
                    Console.WriteLine($"  F4 (Cand 3): '{candidate3NameVal}'");
                    Console.WriteLine($"  G4 (Cand 4): '{candidate4NameVal}'");
                    Console.WriteLine($"  H4 (Cand 5): '{candidate5NameVal}'");
                    Console.WriteLine($"  I4 (Cand 6): '{candidate6NameVal}'");
                    Console.WriteLine($"  J4 (Cand 7): '{candidate7NameVal}'");

                    // ===== READ 4 BALLOT TYPES for 7 CANDIDATES =====
                    Console.WriteLine($"\n===== [7-CAND] READING 4 BALLOT TYPES =====");
                    
                    // Type 1: Sum(C103:C97) - Sum từ C97 đến C103
                    ballotType1Count = 0;
                    for (int row = 97; row <= 103; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "0";
                        if (int.TryParse(cellVal, out int val))
                            ballotType1Count += val;
                    }
                    ballotType1Votes = ballotType1Count * 1;
                    Console.WriteLine($"[7-CAND] Type 1 (Sum C97:C103): {ballotType1Count} phiếu × 1 vote = {ballotType1Votes}");
                    
                    // Type 2: Sum(C96:C76) - Sum từ C76 đến C96
                    ballotType2Count = 0;
                    for (int row = 76; row <= 96; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "0";
                        if (int.TryParse(cellVal, out int val))
                            ballotType2Count += val;
                    }
                    ballotType2Votes = ballotType2Count * 2;
                    Console.WriteLine($"[7-CAND] Type 2 (Sum C76:C96): {ballotType2Count} phiếu × 2 votes = {ballotType2Votes}");
                    
                    // Type 3: Sum(C75:C41) - Sum từ C41 đến C75
                    ballotType3Count = 0;
                    for (int row = 41; row <= 75; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "0";
                        if (int.TryParse(cellVal, out int val))
                            ballotType3Count += val;
                    }
                    ballotType3Votes = ballotType3Count * 3;
                    Console.WriteLine($"[7-CAND] Type 3 (Sum C41:C75): {ballotType3Count} phiếu × 3 votes = {ballotType3Votes}");
                    
                    // Type 4: Sum(C40:C6) - Sum từ C6 đến C40
                    ballotType4Count = 0;
                    for (int row = 6; row <= 40; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "0";
                        if (int.TryParse(cellVal, out int val))
                            ballotType4Count += val;
                    }
                    ballotType4Votes = ballotType4Count * 4;
                    Console.WriteLine($"[7-CAND] Type 4 (Sum C6:C40): {ballotType4Count} phiếu × 4 votes = {ballotType4Votes}");
                    
                    // Phiếu không hợp lệ: Lấy từ ô C104 (Row 104, Column C = 3)
                    var invalidBallotsCellVal = worksheet.Cells[104, 3].Value?.ToString() ?? "0";
                    Console.WriteLine($"[7-CAND] DEBUG - C104 raw value: '{invalidBallotsCellVal}'");
                    int.TryParse(invalidBallotsCellVal, out int cellInvalidBallots);
                    invalidBallots = cellInvalidBallots;
                    Console.WriteLine($"[7-CAND] Phiếu không hợp lệ (Invalid) from C104: {invalidBallots}");
                    
                    // Phiếu hợp lệ = Phiếu thu vào - Phiếu không hợp lệ
                    validBallots = receivedBallots - invalidBallots;
                    Console.WriteLine($"[7-CAND] Phiếu hợp lệ (Valid) = {receivedBallots} - {invalidBallots} = {validBallots}");
                    
                    int totalWeightedVotes = ballotType1Votes + ballotType2Votes + ballotType3Votes + ballotType4Votes;
                    
                    Console.WriteLine($"\n===== [7-CAND] TOTAL BALLOT SUMMARY =====");
                    Console.WriteLine($"Tổng phiếu hợp lệ: {validBallots}");
                    Console.WriteLine($"Tổng phiếu không hợp lệ: {invalidBallots}");
                    Console.WriteLine($"Tổng weighted votes: {totalWeightedVotes}");

                    // === READ CANDIDATE VOTES FROM ROW 105 ===
                    // Formula: Candidate N votes = C105 - (D105, E105, F105, ... based on column)
                    Console.WriteLine($"\n===== [7-CAND] READING CANDIDATE VOTES FROM ROW 105 =====");
                    
                    // Total valid ballots (C105 = SUM(C6:C103))
                    int totalValidVotes = 0;
                    for (int row = 6; row <= 103; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "0";
                        if (int.TryParse(cellVal, out int val))
                            totalValidVotes += val;
                    }
                    Console.WriteLine($"[7-CAND] Total Valid Votes (C6:C103): {totalValidVotes}");
                    
                    // For each candidate, calculate: Total Valid - Not Selected For This Candidate
                    int[] candidateNotSelected = new int[7];
                    for (int cand = 1; cand <= 7; cand++)
                    {
                        int col = 3 + cand; // D=4, E=5, F=6, G=7, H=8, I=9, J=10
                        // Sum of "not selected" for this candidate (D6:D103, E6:E103, etc.)
                        int notSelected = 0;
                        for (int row = 6; row <= 103; row++)
                        {
                            var cellVal = worksheet.Cells[row, col].Value?.ToString() ?? "0";
                            if (int.TryParse(cellVal, out int val))
                                notSelected += val;
                        }
                        candidateNotSelected[cand - 1] = notSelected;
                        
                        // Debug log for candidate 6, 7
                        if (cand == 6 || cand == 7)
                        {
                            Console.WriteLine($"[7-CAND DEBUG] Candidate {cand} (Col {(char)(64+col)}):");
                            // Print first 10 and last 10 values
                            string firstVals = "";
                            string lastVals = "";
                            for (int row = 6; row <= 15; row++)
                            {
                                firstVals += worksheet.Cells[row, col].Value?.ToString() ?? "0" + ", ";
                            }
                            for (int row = 94; row <= 103; row++)
                            {
                                lastVals += worksheet.Cells[row, col].Value?.ToString() ?? "0" + ", ";
                            }
                            Console.WriteLine($"  Rows 6-15: {firstVals}");
                            Console.WriteLine($"  Rows 94-103: {lastVals}");
                            Console.WriteLine($"  Total 'Not Selected': {notSelected}");
                        }
                    }
                    
                    // Calculate candidate votes
                    candidate1Total = totalValidVotes - candidateNotSelected[0];
                    candidate2Total = totalValidVotes - candidateNotSelected[1];
                    candidate3Total = totalValidVotes - candidateNotSelected[2];
                    candidate4Total = totalValidVotes - candidateNotSelected[3];
                    candidate5Total = totalValidVotes - candidateNotSelected[4];
                    candidate6Total = totalValidVotes - candidateNotSelected[5];
                    candidate7Total = totalValidVotes - candidateNotSelected[6];
                    
                    Console.WriteLine($"[7-CAND] Candidate votes calculated:");
                    Console.WriteLine($"  Cand 1: {totalValidVotes} - {candidateNotSelected[0]} = {candidate1Total}");
                    Console.WriteLine($"  Cand 2: {totalValidVotes} - {candidateNotSelected[1]} = {candidate2Total}");
                    Console.WriteLine($"  Cand 3: {totalValidVotes} - {candidateNotSelected[2]} = {candidate3Total}");
                    Console.WriteLine($"  Cand 4: {totalValidVotes} - {candidateNotSelected[3]} = {candidate4Total}");
                    Console.WriteLine($"  Cand 5: {totalValidVotes} - {candidateNotSelected[4]} = {candidate5Total}");
                    Console.WriteLine($"  Cand 6: {totalValidVotes} - {candidateNotSelected[5]} = {candidate6Total}");
                    Console.WriteLine($"  Cand 7: {totalValidVotes} - {candidateNotSelected[6]} = {candidate7Total}");
                    
                    Console.WriteLine($"\n===== [7-CAND] TOTAL CANDIDATE VOTES =====");
                    Console.WriteLine($"Person 1: {candidate1Total}");
                    Console.WriteLine($"Person 2: {candidate2Total}");
                    Console.WriteLine($"Person 3: {candidate3Total}");
                    Console.WriteLine($"Person 4: {candidate4Total}");
                    Console.WriteLine($"Person 5: {candidate5Total}");
                    Console.WriteLine($"Person 6: {candidate6Total}");
                    Console.WriteLine($"Person 7: {candidate7Total}");
                    Console.WriteLine($"TOTAL WEIGHTED VOTES: {candidate1Total + candidate2Total + candidate3Total + candidate4Total + candidate5Total + candidate6Total + candidate7Total}");

                    // === BUILD RESULT ===
                    int totalBallots = ballotType1Count + ballotType2Count + ballotType3Count + ballotType4Count;

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
                    result.BallotType4Count = ballotType4Count;
                    result.BallotType4Votes = ballotType4Votes;
                    result.Candidate1Votes = candidate1Total;
                    result.Candidate2Votes = candidate2Total;
                    result.Candidate3Votes = candidate3Total;
                    result.Candidate4Votes = candidate4Total;
                    result.Candidate5Votes = candidate5Total;
                    result.Candidate6Votes = candidate6Total;
                    result.Candidate7Votes = candidate7Total;
                    result.Candidate8Votes = 0;
                    result.Candidate1Name = candidate1NameVal;
                    result.Candidate2Name = candidate2NameVal;
                    result.Candidate3Name = candidate3NameVal;
                    result.Candidate4Name = candidate4NameVal;
                    result.Candidate5Name = candidate5NameVal;
                    result.Candidate6Name = candidate6NameVal;
                    result.Candidate7Name = candidate7NameVal;
                    result.Candidate8Name = "";
                    result.TotalCandidates = 7;
                    result.Success = true;
                    result.Message = $"[7-CAND] Import thành công! Tổng phiếu: {totalBallots}, Hợp lệ: {validBallots}, Không hợp lệ: {invalidBallots}";

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "[7-CAND] Lỗi khi xử lý file Excel";
                result.ErrorDetails = ex.Message;
                return result;
            }
        }
    }
}
