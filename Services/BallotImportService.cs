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

                    // Phiếu hợp lệ: Lấy từ ô C32 (Row 32, Column C = 3)
                    var validCellVal = worksheet.Cells[32, 3].Value?.ToString() ?? "";
                    if (int.TryParse(validCellVal, out int validNum))
                    {
                        validBallots = validNum;
                        Console.WriteLine($"Phiếu hợp lệ (Valid) from C32: {validBallots}");
                    }
                    else
                    {
                        // Fallback: Tìm từ row 4
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            var cellText = worksheet.Cells[4, col].Value?.ToString() ?? "";
                            if (cellText.Contains("hợp lệ") && !cellText.Contains("không"))
                            {
                                var nextVal = worksheet.Cells[4, col + 1].Value?.ToString() ?? "";
                                if (int.TryParse(nextVal, out int num))
                                {
                                    validBallots = num;
                                    Console.WriteLine($"Phiếu hợp lệ (Valid) from search: {validBallots}");
                                    break;
                                }
                            }
                        }
                    }

                    // Phiếu không hợp lệ: Lấy từ ô C31 (Row 31, Column C = 3)
                    var invalidCellVal = worksheet.Cells[31, 3].Value?.ToString() ?? "";
                    if (int.TryParse(invalidCellVal, out int invalidNum))
                    {
                        invalidBallots = invalidNum;
                        Console.WriteLine($"Phiếu không hợp lệ (Invalid) from C31: {invalidBallots}");
                    }
                    else
                    {
                        // Fallback: Tìm từ row 5
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            var cellText = worksheet.Cells[5, col].Value?.ToString() ?? "";
                            if (cellText.Contains("không hợp lệ"))
                            {
                                var nextVal = worksheet.Cells[5, col + 1].Value?.ToString() ?? "";
                                if (int.TryParse(nextVal, out int num))
                                {
                                    invalidBallots = num;
                                    Console.WriteLine($"Phiếu không hợp lệ (Invalid) from search: {invalidBallots}");
                                    break;
                                }
                            }
                        }
                    }

                    // === KIỂM TRA: Phiếu phát ra phải bằng phiếu thu vào ===
                    // [SKIP THIS CHECK - Allow mismatch between issued and received ballots]
                    Console.WriteLine($"Kiểm tra: Phiếu phát ra ({issuedBallots}) vs Phiếu thu vào ({receivedBallots})");
                    //if (issuedBallots != receivedBallots)
                    //{
                    //    result.Success = false;
                    //    result.Message = $"Lỗi: Số phiếu phát ra ({issuedBallots}) không bằng số phiếu thu vào ({receivedBallots})";
                    //    result.ErrorDetails = $"Vui lòng kiểm tra lại dữ liệu trong file Excel. Mismatch: {Math.Abs(issuedBallots - receivedBallots)} phiếu";
                    //    Console.WriteLine($"❌ {result.Message}");
                    //    return result;
                    //}

                    // === PARSE BALLOT TYPES FROM SPECIFIC RANGES IN COLUMN C ===
                    // Phiếu bầu 1: Sum of C26 to C30
                    Console.WriteLine("\n===== CALCULATING BALLOT TYPES =====");
                    Console.WriteLine("Calculating Phiếu bầu 1 from C26:C30");
                    for (int row = 26; row <= 30; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "";
                        if (int.TryParse(cellVal, out int val))
                        {
                            ballotType1Count += val;
                            Console.WriteLine($"  C{row}: {val}");
                        }
                    }
                    ballotType1Votes = ballotType1Count * 1; // Multiply by 1
                    Console.WriteLine($"Phiếu bầu 1: Count={ballotType1Count}, Votes={ballotType1Votes}");

                    // Phiếu bầu 2: Sum of C16 to C25
                    Console.WriteLine("Calculating Phiếu bầu 2 from C16:C25");
                    for (int row = 16; row <= 25; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "";
                        if (int.TryParse(cellVal, out int val))
                        {
                            ballotType2Count += val;
                            Console.WriteLine($"  C{row}: {val}");
                        }
                    }
                    ballotType2Votes = ballotType2Count * 2; // Multiply by 2
                    Console.WriteLine($"Phiếu bầu 2: Count={ballotType2Count}, Votes={ballotType2Votes}");

                    // Phiếu bầu 3: Sum of C6 to C15
                    Console.WriteLine("Calculating Phiếu bầu 3 from C6:C15");
                    for (int row = 6; row <= 15; row++)
                    {
                        var cellVal = worksheet.Cells[row, 3].Value?.ToString() ?? "";
                        if (int.TryParse(cellVal, out int val))
                        {
                            ballotType3Count += val;
                            Console.WriteLine($"  C{row}: {val}");
                        }
                    }
                    ballotType3Votes = ballotType3Count * 3; // Multiply by 3
                    Console.WriteLine($"Phiếu bầu 3: Count={ballotType3Count}, Votes={ballotType3Votes}");

                    // Calculate total weighted votes
                    int totalWeightedVotes = ballotType1Votes + ballotType2Votes + ballotType3Votes;
                    Console.WriteLine($"\n===== TOTAL WEIGHTED VOTES CALCULATION =====");
                    Console.WriteLine($"Phiếu loại 1: {ballotType1Count} × 1 = {ballotType1Votes}");
                    Console.WriteLine($"Phiếu loại 2: {ballotType2Count} × 2 = {ballotType2Votes}");
                    Console.WriteLine($"Phiếu loại 3: {ballotType3Count} × 3 = {ballotType3Votes}");
                    Console.WriteLine($"TOTAL WEIGHTED VOTES: {totalWeightedVotes}");

                    // === READ CANDIDATE VOTES FROM ROW 32 (C32 and D32:H32) ===
                    // C32 = Total valid ballots (phiếu hợp lệ)
                    // D32:H32 = Crossed-out votes for each candidate (phiếu gạch)
                    // Formula: candidate votes = C32 - D32:H32
                    
                    Console.WriteLine($"\n===== READING CANDIDATE VOTES FROM ROW 32 =====");
                    Console.WriteLine($"C32 (Phiếu hợp lệ): {validBallots}");
                    Console.WriteLine($"D32:H32 (Phiếu gạch từng người):");

                    // Read D32 (Column 4 = Column D) - Crossed-out votes for person 1
                    var d32Val = worksheet.Cells[32, 4].Value?.ToString() ?? "0";
                    int d32CrossedOut = 0;
                    if (int.TryParse(d32Val, out int d32Votes))
                        d32CrossedOut = d32Votes;
                    candidate1Total = validBallots - d32CrossedOut;
                    Console.WriteLine($"  D32: {d32CrossedOut} → Person 1: {validBallots} - {d32CrossedOut} = {candidate1Total}");

                    // Read E32 (Column 5 = Column E) - Crossed-out votes for person 2
                    var e32Val = worksheet.Cells[32, 5].Value?.ToString() ?? "0";
                    int e32CrossedOut = 0;
                    if (int.TryParse(e32Val, out int e32Votes))
                        e32CrossedOut = e32Votes;
                    candidate2Total = validBallots - e32CrossedOut;
                    Console.WriteLine($"  E32: {e32CrossedOut} → Person 2: {validBallots} - {e32CrossedOut} = {candidate2Total}");

                    // Read F32 (Column 6 = Column F) - Crossed-out votes for person 3
                    var f32Val = worksheet.Cells[32, 6].Value?.ToString() ?? "0";
                    int f32CrossedOut = 0;
                    if (int.TryParse(f32Val, out int f32Votes))
                        f32CrossedOut = f32Votes;
                    candidate3Total = validBallots - f32CrossedOut;
                    Console.WriteLine($"  F32: {f32CrossedOut} → Person 3: {validBallots} - {f32CrossedOut} = {candidate3Total}");

                    // Read G32 (Column 7 = Column G) - Crossed-out votes for person 4
                    var g32Val = worksheet.Cells[32, 7].Value?.ToString() ?? "0";
                    int g32CrossedOut = 0;
                    if (int.TryParse(g32Val, out int g32Votes))
                        g32CrossedOut = g32Votes;
                    candidate4Total = validBallots - g32CrossedOut;
                    Console.WriteLine($"  G32: {g32CrossedOut} → Person 4: {validBallots} - {g32CrossedOut} = {candidate4Total}");

                    // Read H32 (Column 8 = Column H) - Crossed-out votes for person 5
                    var h32Val = worksheet.Cells[32, 8].Value?.ToString() ?? "0";
                    int h32CrossedOut = 0;
                    if (int.TryParse(h32Val, out int h32Votes))
                        h32CrossedOut = h32Votes;
                    candidate5Total = validBallots - h32CrossedOut;
                    Console.WriteLine($"  H32: {h32CrossedOut} → Person 5: {validBallots} - {h32CrossedOut} = {candidate5Total}");

                    int totalCandidateVotes = candidate1Total + candidate2Total + candidate3Total + candidate4Total + candidate5Total;
                    Console.WriteLine($"\n===== TOTAL CANDIDATE VOTES CALCULATION =====");
                    Console.WriteLine($"Person 1: {candidate1Total}");
                    Console.WriteLine($"Person 2: {candidate2Total}");
                    Console.WriteLine($"Person 3: {candidate3Total}");
                    Console.WriteLine($"Person 4: {candidate4Total}");
                    Console.WriteLine($"Person 5: {candidate5Total}");
                    Console.WriteLine($"TOTAL: {candidate1Total} + {candidate2Total} + {candidate3Total} + {candidate4Total} + {candidate5Total} = {totalCandidateVotes}");

                    // === VALIDATION: Check if total candidate votes matches total weighted votes ===
                    Console.WriteLine($"\n===== VALIDATION CHECK =====");
                    Console.WriteLine($"Tổng bầu (từ phiếu loại 1-3): {totalWeightedVotes}");
                    Console.WriteLine($"Tổng phiếu (từ C32 - gạch): {totalCandidateVotes}");
                    
                    if (totalWeightedVotes != totalCandidateVotes)
                    {
                        int diff = Math.Abs(totalWeightedVotes - totalCandidateVotes);
                        Console.WriteLine($"❌ SỰ KHÔNG KHỚP: Sai lệch {diff} phiếu");
                        result.Success = false;
                        result.Message = $"Lỗi: Tổng bầu ({totalWeightedVotes}) không khớp với số phiếu ({totalCandidateVotes}). Sai lệch: {diff} phiếu";
                        result.ErrorDetails = $"Vui lòng kiểm tra dữ liệu trong file Excel.\n" +
                            $"Tổng bầu từ phiếu loại 1-3: {totalWeightedVotes}\n" +
                            $"Tổng phiếu được tính từ C32 - gạch phiếu: {totalCandidateVotes}\n" +
                            $"Chi tiết:\n" +
                            $"- Phiếu loại 1: {ballotType1Count} × 1 = {ballotType1Votes}\n" +
                            $"- Phiếu loại 2: {ballotType2Count} × 2 = {ballotType2Votes}\n" +
                            $"- Phiếu loại 3: {ballotType3Count} × 3 = {ballotType3Votes}\n" +
                            $"- C32: {validBallots}\n" +
                            $"- Gạch phiếu (D32:H32): {d32CrossedOut}, {e32CrossedOut}, {f32CrossedOut}, {g32CrossedOut}, {h32CrossedOut}";
                        Console.WriteLine($"❌ {result.Message}");
                        return result;
                    }
                    else
                    {
                        Console.WriteLine($"✅ Validation passed: Tổng bầu = Số phiếu = {totalCandidateVotes}");
                    }

                    // === PARSE CANDIDATE NAMES FROM ROW 4 (D4, E4, F4, G4, H4) ===
                    Console.WriteLine($"===== READING CANDIDATE NAMES FROM ROW 4 (D4:H4) =====");
                    
                    // Read candidate names directly from D4 to H4
                    var candidate1NameVal = worksheet.Cells[4, 4].Value?.ToString() ?? ""; // D4
                    var candidate2NameVal = worksheet.Cells[4, 5].Value?.ToString() ?? ""; // E4
                    var candidate3NameVal = worksheet.Cells[4, 6].Value?.ToString() ?? ""; // F4
                    var candidate4NameVal = worksheet.Cells[4, 7].Value?.ToString() ?? ""; // G4
                    var candidate5NameVal = worksheet.Cells[4, 8].Value?.ToString() ?? ""; // H4
                    
                    Console.WriteLine($"D4 (Candidate 1): '{candidate1NameVal}'");
                    Console.WriteLine($"E4 (Candidate 2): '{candidate2NameVal}'");
                    Console.WriteLine($"F4 (Candidate 3): '{candidate3NameVal}'");
                    Console.WriteLine($"G4 (Candidate 4): '{candidate4NameVal}'");
                    Console.WriteLine($"H4 (Candidate 5): '{candidate5NameVal}'");
                    
                    if (!string.IsNullOrEmpty(candidate1NameVal) && !candidate1NameVal.Equals("-") && !candidate1NameVal.Equals("―"))
                    {
                        result.Candidate1Name = candidate1NameVal;
                        Console.WriteLine($"Set Candidate1Name = '{candidate1NameVal}'");
                    }
                    else
                    {
                        result.Candidate1Name = "Ứng cử viên số 1";
                    }
                    
                    if (!string.IsNullOrEmpty(candidate2NameVal) && !candidate2NameVal.Equals("-") && !candidate2NameVal.Equals("―"))
                    {
                        result.Candidate2Name = candidate2NameVal;
                        Console.WriteLine($"Set Candidate2Name = '{candidate2NameVal}'");
                    }
                    else
                    {
                        result.Candidate2Name = "Ứng cử viên số 2";
                    }
                    
                    if (!string.IsNullOrEmpty(candidate3NameVal) && !candidate3NameVal.Equals("-") && !candidate3NameVal.Equals("―"))
                    {
                        result.Candidate3Name = candidate3NameVal;
                        Console.WriteLine($"Set Candidate3Name = '{candidate3NameVal}'");
                    }
                    else
                    {
                        result.Candidate3Name = "Ứng cử viên số 3";
                    }
                    
                    if (!string.IsNullOrEmpty(candidate4NameVal) && !candidate4NameVal.Equals("-") && !candidate4NameVal.Equals("―"))
                    {
                        result.Candidate4Name = candidate4NameVal;
                        Console.WriteLine($"Set Candidate4Name = '{candidate4NameVal}'");
                    }
                    else
                    {
                        result.Candidate4Name = "Ứng cử viên số 4";
                    }
                    
                    if (!string.IsNullOrEmpty(candidate5NameVal) && !candidate5NameVal.Equals("-") && !candidate5NameVal.Equals("―"))
                    {
                        result.Candidate5Name = candidate5NameVal;
                        Console.WriteLine($"Set Candidate5Name = '{candidate5NameVal}'");
                    }
                    else
                    {
                        result.Candidate5Name = "Ứng cử viên số 5";
                    }

                    // Calculate total ballots
                    int totalBallots = ballotType1Count + ballotType2Count + ballotType3Count;

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
                    result.Candidate1Votes = candidate1Total;
                    result.Candidate2Votes = candidate2Total;
                    result.Candidate3Votes = candidate3Total;
                    result.Candidate4Votes = candidate4Total;
                    result.Candidate5Votes = candidate5Total;
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
    }
}
