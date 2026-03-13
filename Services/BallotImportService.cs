using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using ElectionManagement.Models;

namespace ElectionManagement.Services
{
    public class BallotImportService
    {
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
                    int candidate8Total = 0;

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
                                Console.WriteLine($"Phiếu phát ra (Issued): {issuedBallots}");
                                break;
                            }
                        }
                    }

                    // Phiếu thu vào: Lấy từ ô H3 (Row 3, Column H = 8)
                    var receivedCellVal = worksheet.Cells[3, 8].Value?.ToString() ?? "";
                    if (int.TryParse(receivedCellVal, out int receivedNum))
                    {
                        receivedBallots = receivedNum;
                        Console.WriteLine($"Phiếu thu vào (Received) from H3: {receivedBallots}");
                    }
                    else
                    {
                        // Fallback: Tìm từ row 3 nếu H3 không có dữ liệu
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            var cellText = worksheet.Cells[3, col].Value?.ToString() ?? "";
                            if (cellText.Contains("thu vào"))
                            {
                                var nextVal = worksheet.Cells[3, col + 1].Value?.ToString() ?? "";
                                if (int.TryParse(nextVal, out int num))
                                {
                                    receivedBallots = num;
                                    Console.WriteLine($"Phiếu thu vào (Received) from search: {receivedBallots}");
                                    break;
                                }
                            }
                        }
                    }

                    // === FIRST: READ CANDIDATE NAMES FROM ROW 4 AND DETERMINE TOTAL CANDIDATES ===
                    Console.WriteLine($"\n===== READING CANDIDATE NAMES FROM ROW 4 (D4:K4) =====");
                    
                    var candidate1NameVal = worksheet.Cells[4, 4].Value?.ToString() ?? "";
                    var candidate2NameVal = worksheet.Cells[4, 5].Value?.ToString() ?? "";
                    var candidate3NameVal = worksheet.Cells[4, 6].Value?.ToString() ?? "";
                    var candidate4NameVal = worksheet.Cells[4, 7].Value?.ToString() ?? "";
                    var candidate5NameVal = worksheet.Cells[4, 8].Value?.ToString() ?? "";
                    var candidate6NameVal = worksheet.Cells[4, 9].Value?.ToString() ?? "";
                    var candidate7NameVal = worksheet.Cells[4, 10].Value?.ToString() ?? "";
                    var candidate8NameVal = worksheet.Cells[4, 11].Value?.ToString() ?? "";
                    
                    // DEBUG: Print all candidate names read from row 4
                    Console.WriteLine($"  D4 (Cand 1): '{candidate1NameVal}'");
                    Console.WriteLine($"  E4 (Cand 2): '{candidate2NameVal}'");
                    Console.WriteLine($"  F4 (Cand 3): '{candidate3NameVal}'");
                    Console.WriteLine($"  G4 (Cand 4): '{candidate4NameVal}'");
                    Console.WriteLine($"  H4 (Cand 5): '{candidate5NameVal}'");
                    Console.WriteLine($"  I4 (Cand 6): '{candidate6NameVal}'");
                    Console.WriteLine($"  J4 (Cand 7): '{candidate7NameVal}'");
                    Console.WriteLine($"  K4 (Cand 8): '{candidate8NameVal}'");
                    
                    // Determine total candidates from row 4
                    int totalCandidates = 0;
                    var excelCandidateValues = new[] {
                        candidate1NameVal, candidate2NameVal, candidate3NameVal, candidate4NameVal,
                        candidate5NameVal, candidate6NameVal, candidate7NameVal, candidate8NameVal
                    };
                    
                    for (int i = excelCandidateValues.Length - 1; i >= 0; i--)
                    {
                        if (!string.IsNullOrEmpty(excelCandidateValues[i]) && 
                            !excelCandidateValues[i].Equals("-") && 
                            !excelCandidateValues[i].Equals("―"))
                        {
                            totalCandidates = i + 1;
                            break;
                        }
                    }
                    if (totalCandidates == 0) totalCandidates = 5;
                    
                    Console.WriteLine($"✓ Total candidates determined: {totalCandidates}");

                    // === BALLOT READING LOGIC: DIFFERENT FOR 5 vs 7 CANDIDATES ===
                    int readColumn = (totalCandidates == 7) ? 4 : 3;  // Col D=4 for 7-person, Col C=3 for 5-person
                    
                    if (totalCandidates == 7)
                    {
                        Console.WriteLine($"\n===== READING 4 BALLOT TYPES (For 7 candidates) =====");
                        
                        // DEBUG: Check raw cell values before parsing
                        Console.WriteLine($"\n===== DEBUG: Raw cell values from rows 31-32, 54-55, 77-78, 104-105 =====");
                        for (int row = 31; row <= 32; row++)
                        {
                            Console.WriteLine($"Row {row}: C={worksheet.Cells[row, 3].Value} | D={worksheet.Cells[row, 4].Value}");
                        }
                        for (int row = 54; row <= 55; row++)
                        {
                            Console.WriteLine($"Row {row}: C={worksheet.Cells[row, 3].Value} | D={worksheet.Cells[row, 4].Value}");
                        }
                        for (int row = 77; row <= 78; row++)
                        {
                            Console.WriteLine($"Row {row}: C={worksheet.Cells[row, 3].Value} | D={worksheet.Cells[row, 4].Value}");
                        }
                        for (int row = 104; row <= 105; row++)
                        {
                            Console.WriteLine($"Row {row}: C={worksheet.Cells[row, 3].Value} | D={worksheet.Cells[row, 4].Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"\n===== READING BALLOT DATA (For 5 candidates) =====");
                    }
                    
                    // Type 1: Gạch 6 lấy 1 (chọn 1 người)
                    var type1InvalidVal = worksheet.Cells[31, readColumn].Value?.ToString() ?? "0";
                    var type1ValidVal = worksheet.Cells[32, readColumn].Value?.ToString() ?? "0";
                    int.TryParse(type1InvalidVal, out int type1Invalid);
                    int.TryParse(type1ValidVal, out int type1Valid);
                    ballotType1Count = type1Valid;
                    ballotType1Votes = ballotType1Count * 1;
                    Console.WriteLine($"Type 1: Invalid={type1Invalid}, Valid={type1Valid} → {ballotType1Count} phiếu × 1 vote = {ballotType1Votes}");
                    
                    int type2Invalid = 0, type2Valid = 0, type3Invalid = 0, type3Valid = 0, type4Invalid = 0, type4Valid = 0;
                    
                    // Type 2, 3, 4 only for 7-candidate case
                    if (totalCandidates == 7)
                    {
                        // Type 2: Gạch 5 lấy 2 (chọn 2 người)
                        var type2InvalidVal = worksheet.Cells[54, readColumn].Value?.ToString() ?? "0";
                        var type2ValidVal = worksheet.Cells[55, readColumn].Value?.ToString() ?? "0";
                        int.TryParse(type2InvalidVal, out type2Invalid);
                        int.TryParse(type2ValidVal, out type2Valid);
                        ballotType2Count = type2Valid;
                        ballotType2Votes = ballotType2Count * 2;
                        Console.WriteLine($"Type 2: Invalid={type2Invalid}, Valid={type2Valid} → {ballotType2Count} phiếu × 2 votes = {ballotType2Votes}");
                        
                        // Type 3: Gạch 4 lấy 3 (chọn 3 người)
                        var type3InvalidVal = worksheet.Cells[77, readColumn].Value?.ToString() ?? "0";
                        var type3ValidVal = worksheet.Cells[78, readColumn].Value?.ToString() ?? "0";
                        int.TryParse(type3InvalidVal, out type3Invalid);
                        int.TryParse(type3ValidVal, out type3Valid);
                        ballotType3Count = type3Valid;
                        ballotType3Votes = ballotType3Count * 3;
                        Console.WriteLine($"Type 3: Invalid={type3Invalid}, Valid={type3Valid} → {ballotType3Count} phiếu × 3 votes = {ballotType3Votes}");
                        
                        // Type 4: Gạch 3 lấy 4 (chọn 4 người)
                        var type4InvalidVal = worksheet.Cells[104, readColumn].Value?.ToString() ?? "0";
                        var type4ValidVal = worksheet.Cells[105, readColumn].Value?.ToString() ?? "0";
                        int.TryParse(type4InvalidVal, out type4Invalid);
                        int.TryParse(type4ValidVal, out type4Valid);
                        ballotType4Count = type4Valid;
                        ballotType4Votes = ballotType4Count * 4;
                        Console.WriteLine($"Type 4: Invalid={type4Invalid}, Valid={type4Valid} → {ballotType4Count} phiếu × 4 votes = {ballotType4Votes}");
                    }
                    else
                    {
                        // For 5-candidate: No Type 2, 3, 4
                        ballotType2Count = 0;
                        ballotType2Votes = 0;
                        ballotType3Count = 0;
                        ballotType3Votes = 0;
                        ballotType4Count = 0;
                        ballotType4Votes = 0;
                    }
                    
                    validBallots = ballotType1Count + ballotType2Count + ballotType3Count + ballotType4Count;
                    int totalInvalidBallots = type1Invalid;
                    
                    // For 7-candidate case, add other invalid counts
                    if (totalCandidates == 7)
                    {
                        // Need to re-declare these for the scope if not already calculated
                        // They were already calculated above in the if block
                        // This calculation is already done above
                    }
                    
                    invalidBallots = totalInvalidBallots;
                    int totalWeightedVotes = ballotType1Votes + ballotType2Votes + ballotType3Votes + ballotType4Votes;
                    
                    Console.WriteLine($"\n===== TOTAL BALLOT SUMMARY =====");
                    Console.WriteLine($"Tổng phiếu hợp lệ: {validBallots}");
                    Console.WriteLine($"Tổng phiếu không hợp lệ: {invalidBallots}");
                    Console.WriteLine($"Tổng weighted votes: {totalWeightedVotes}");

                    // === READ CANDIDATE VOTES FROM CORRESPONDING ROWS ===
                    // For each ballot type, read from its corresponding row
                    // Type 1: Row 32, Type 2: Row 55, Type 3: Row 78, Type 4: Row 105
                    
                    Console.WriteLine($"\n===== READING CANDIDATE VOTES FROM CORRESPONDING ROWS =====");
                    
                    // Initialize candidate totals
                    candidate1Total = 0;
                    candidate2Total = 0;
                    candidate3Total = 0;
                    candidate4Total = 0;
                    candidate5Total = 0;
                    candidate6Total = 0;
                    candidate7Total = 0;
                    candidate8Total = 0;
                    
                    if (totalCandidates == 7)
                    {
                        // Define vote multiplier for each type (7-candidate case)
                        int[] typeVoteMultiplier = new int[] { 0, 1, 2, 3, 4 }; // type 1=1 vote, type 2=2 votes, etc
                        int[] typeRows = new int[] { 0, 32, 55, 78, 105 };      // row for each type
                        
                        for (int type = 1; type <= 4; type++)
                        {
                            int dataRow = typeRows[type];
                            int multiplier = typeVoteMultiplier[type];
                            
                            Console.WriteLine($"\nType {type} (Row {dataRow}) - multiplier {multiplier}:");
                            
                            int typeCount = 0;
                            switch (type)
                            {
                                case 1: typeCount = ballotType1Count; break;
                                case 2: typeCount = ballotType2Count; break;
                                case 3: typeCount = ballotType3Count; break;
                                case 4: typeCount = ballotType4Count; break;
                            }
                            
                            // Read votes for each candidate
                            for (int cand = 1; cand <= totalCandidates; cand++)
                            {
                                int col = 3 + cand; // Column D=4 for person 1, E=5 for person 2, etc.
                                var cellVal = worksheet.Cells[dataRow, col].Value?.ToString() ?? "0";
                                int crossedOut = 0;
                                if (int.TryParse(cellVal, out int votes))
                                    crossedOut = votes;
                                
                                // Votes for this type = typeCount * multiplier
                                int typeVotes = typeCount * multiplier;
                                
                                // Add to candidate's total
                                switch (cand)
                                {
                                    case 1: candidate1Total += typeVotes; break;
                                    case 2: candidate2Total += typeVotes; break;
                                    case 3: candidate3Total += typeVotes; break;
                                    case 4: candidate4Total += typeVotes; break;
                                    case 5: candidate5Total += typeVotes; break;
                                    case 6: candidate6Total += typeVotes; break;
                                    case 7: candidate7Total += typeVotes; break;
                                    case 8: candidate8Total += typeVotes; break;
                                }
                                
                                Console.WriteLine($"  Person {cand}: {typeCount} ballot(s) × {multiplier} = {typeVotes} votes");
                            }
                        }
                    }
                    else
                    {
                        // 5-candidate case: Only Type 1 from row 32
                        Console.WriteLine($"\nType 1 (Row 32) - multiplier 1:");
                        
                        // Read votes for each candidate from row 32
                        for (int cand = 1; cand <= totalCandidates; cand++)
                        {
                            int col = 3 + cand;
                            var cellVal = worksheet.Cells[32, col].Value?.ToString() ?? "0";
                            int crossedOut = 0;
                            if (int.TryParse(cellVal, out int votes))
                                crossedOut = votes;
                            
                            int typeVotes = ballotType1Count * 1;
                            
                            switch (cand)
                            {
                                case 1: candidate1Total += typeVotes; break;
                                case 2: candidate2Total += typeVotes; break;
                                case 3: candidate3Total += typeVotes; break;
                                case 4: candidate4Total += typeVotes; break;
                                case 5: candidate5Total += typeVotes; break;
                            }
                            
                            Console.WriteLine($"  Person {cand}: {ballotType1Count} ballot(s) × 1 = {typeVotes} votes");
                        }
                    }
                    
                    Console.WriteLine($"\n===== TOTAL CANDIDATE VOTES CALCULATION =====");
                    Console.WriteLine($"Person 1: {candidate1Total}");
                    Console.WriteLine($"Person 2: {candidate2Total}");
                    Console.WriteLine($"Person 3: {candidate3Total}");
                    Console.WriteLine($"Person 4: {candidate4Total}");
                    Console.WriteLine($"Person 5: {candidate5Total}");
                    Console.WriteLine($"Person 6: {candidate6Total}");
                    Console.WriteLine($"Person 7: {candidate7Total}");
                    Console.WriteLine($"TOTAL WEIGHTED VOTES: {candidate1Total + candidate2Total + candidate3Total + candidate4Total + candidate5Total + candidate6Total + candidate7Total}");

                    // === ASSIGN CANDIDATE NAMES TO RESULT ===
                    Console.WriteLine($"\n===== ASSIGNING CANDIDATE NAMES =====");

                    // Calculate total ballots
                    int totalBallots = ballotType1Count + ballotType2Count + ballotType3Count + ballotType4Count;

                    // Build result
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
                    result.Candidate8Votes = candidate8Total;
                    result.Candidate1Name = candidate1NameVal;
                    result.Candidate2Name = candidate2NameVal;
                    result.Candidate3Name = candidate3NameVal;
                    result.Candidate4Name = candidate4NameVal;
                    result.Candidate5Name = candidate5NameVal;
                    result.Candidate6Name = candidate6NameVal;
                    result.Candidate7Name = candidate7NameVal;
                    result.Candidate8Name = candidate8NameVal;
                    result.TotalCandidates = totalCandidates;
                    result.Success = true;
                    result.Message = $"Import thành công! Tổng phiếu: {totalBallots}, Hợp lệ: {validBallots}, Không hợp lệ: {invalidBallots}";

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Lỗi khi xử lý file Excel";
                result.ErrorDetails = ex.Message;
                return result;
            }
        }

        // Helper method to convert column number to letter
        private string ConvertToLetter(int columnNumber)
        {
            // Column 4 = D, 5 = E, etc.
            return ((char)('A' + columnNumber - 1)).ToString();
        }
    }
}
