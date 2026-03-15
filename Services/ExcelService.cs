using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.EntityFrameworkCore;
using ElectionManagement.Models;
using ElectionManagement.Data;

namespace ElectionManagement.Services
{
    public interface IExcelService
    {
        Task<ImportResult> ImportElectionResults(Stream fileStream, string fileName, string level);
        Task<ImportResult> ImportElectionProgress(Stream fileStream, string fileName, string level);
        Task<byte[]> ExportElectionResults(List<ElectionResult> data);
        Task<byte[]> ExportElectionProgress(List<ElectionProgress> data);
        Task<byte[]> ExportProgressSummary(List<dynamic> summaryData);
        Task<byte[]> ExportComprehensiveElectionResults(List<ElectionResult> results, List<ElectionProgress> progress, string level);
        Task<byte[]> ExportOfficialBallotVerificationForm(List<ElectionResult> results, string level, string area = "", List<string> footerData = null);
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<dynamic> PreviewData { get; set; } = new();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
    }

    public class ExcelService : IExcelService
    {
        private readonly ElectionDbContext _context;

        public ExcelService(ElectionDbContext context)
        {
            _context = context;
            // EPPlus 5.x không yêu cầu license
        }

        // Helper method để parse int an toàn
        private int SafeParseInt(object value)
        {
            if (value == null) return 0;
            
            string strValue = value.ToString().Trim();
            if (string.IsNullOrEmpty(strValue)) return 0;
            
            // Handle NaN and other formula errors
            if (strValue.Equals("NaN", StringComparison.OrdinalIgnoreCase)) return 0;
            if (strValue.Equals("#VALUE!", StringComparison.OrdinalIgnoreCase)) return 0;
            if (strValue.Equals("#N/A", StringComparison.OrdinalIgnoreCase)) return 0;
            
            if (int.TryParse(strValue, out int result))
                return result;
            
            return 0; // Không throw, just return 0 for invalid values
        }

        // Helper method để parse decimal an toàn
        private decimal SafeParseDecimal(object value)
        {
            if (value == null) return 0;
            
            string strValue = value.ToString().Trim();
            if (string.IsNullOrEmpty(strValue)) return 0;
            
            // Handle NaN and other formula errors
            if (strValue.Equals("NaN", StringComparison.OrdinalIgnoreCase)) return 0;
            if (strValue.Equals("#VALUE!", StringComparison.OrdinalIgnoreCase)) return 0;
            if (strValue.Equals("#N/A", StringComparison.OrdinalIgnoreCase)) return 0;
            
            // Remove % sign if present
            if (strValue.EndsWith("%"))
                strValue = strValue.Substring(0, strValue.Length - 1).Trim();
            
            if (decimal.TryParse(strValue, out decimal result))
                return result;
            
            return 0;
        }

        // Helper method để kiểm tra xem dòng có phải là dữ liệu hợp lệ
        private bool IsValidDataRow(ExcelWorksheet worksheet, int row)
        {
            try
            {
                // Lấy giá trị từ cột STT (cột 1)
                var sttValue = worksheet.Cells[row, 1].Value;
                if (sttValue == null) return false;
                
                string strStt = sttValue.ToString().Trim();
                if (string.IsNullOrEmpty(strStt)) return false;
                
                // Bỏ qua dòng header (chứa "STT" text)
                if (strStt.Equals("STT", StringComparison.OrdinalIgnoreCase)) return false;
                
                // Bỏ qua dòng chứa emoji, dấu gạch ngang, hoặc text không phải số
                if (strStt.Contains("—") || strStt.Contains("-") || strStt.Contains("⚠") || 
                    strStt.Contains("📢") || strStt.Contains("🏆") || strStt.Contains("📌") || 
                    strStt.Contains("🔒") || strStt.Contains("[") || !char.IsDigit(strStt[0]))
                    return false;
                
                // Parse STT để kiểm tra xem là số hợp lệ không
                if (!int.TryParse(strStt, out int sttNum) || sttNum <= 0)
                    return false;
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ImportResult> ImportElectionResults(Stream fileStream, string fileName, string level)
        {
            Console.WriteLine($"[DEBUG] ImportElectionResults started, fileName={fileName}, level={level}");
            Console.WriteLine($"[DEBUG] Connection string: {_context.Database.GetDbConnection().ConnectionString}");

            var result = new ImportResult();
            var errors = new List<string>();

            try
            {
                // make sure db is reachable/created before reading file
                await _context.Database.EnsureCreatedAsync();
                Console.WriteLine("[DEBUG] Database EnsureCreatedAsync completed");

                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    if (rowCount < 2)
                    {
                        result.Success = false;
                        result.Message = "File Excel rỗng hoặc không có dữ liệu";
                        return result;
                    }

                    result.TotalRows = rowCount - 2;
                    var records = new List<ElectionResult>();

                    // Data starts from row 3 (skip header rows 1-2)
                    for (int row = 3; row <= rowCount; row++)
                    {
                        // Skip rows that are not valid data (empty rows, summary rows)
                        if (!IsValidDataRow(worksheet, row))
                            continue;

                        try
                        {
                            // Read group name from cell A1 if available
                            var toName = worksheet.Cells[1, 1].Value?.ToString() ?? "";
                            
                            var ungCuVien1 = SafeParseInt(worksheet.Cells[row, 4].Value);
                            var ungCuVien2 = SafeParseInt(worksheet.Cells[row, 5].Value);
                            var ungCuVien3 = SafeParseInt(worksheet.Cells[row, 6].Value);
                            var ungCuVien4 = SafeParseInt(worksheet.Cells[row, 7].Value);
                            var ungCuVien5 = SafeParseInt(worksheet.Cells[row, 8].Value);
                            
                            // Read PhieuKhongHopLe from C31 (row 31, column 3)
                            int phieuKhongHopLe = SafeParseInt(worksheet.Cells[31, 3].Value);
                            
                            Console.WriteLine($"[DEBUG] Row {row} - PhieuKhongHopLe from C31: {phieuKhongHopLe}");
                            Console.WriteLine($"[DEBUG] Before: UC1={ungCuVien1}, UC2={ungCuVien2}, UC3={ungCuVien3}, UC4={ungCuVien4}, UC5={ungCuVien5}");
                            
                            // Todos 5 ứng cử viên cùng bị trừ phiếu không hợp lệ
                            ungCuVien1 = Math.Max(0, ungCuVien1 - phieuKhongHopLe);
                            ungCuVien2 = Math.Max(0, ungCuVien2 - phieuKhongHopLe);
                            ungCuVien3 = Math.Max(0, ungCuVien3 - phieuKhongHopLe);
                            ungCuVien4 = Math.Max(0, ungCuVien4 - phieuKhongHopLe);
                            ungCuVien5 = Math.Max(0, ungCuVien5 - phieuKhongHopLe);
                            
                            Console.WriteLine($"[DEBUG] After: UC1={ungCuVien1}, UC2={ungCuVien2}, UC3={ungCuVien3}, UC4={ungCuVien4}, UC5={ungCuVien5}");
                            
                            // Calculate PhieuHopLe from remaining valid votes
                            int phieuHopLe = ungCuVien1 + ungCuVien2 + ungCuVien3 + ungCuVien4 + ungCuVien5;
                            
                            var record = new ElectionResult
                            {
                                Level = level,
                                Stt = SafeParseInt(worksheet.Cells[row, 1].Value),
                                KhuVuc = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                                To = toName,
                                TongCuTri = SafeParseInt(worksheet.Cells[row, 3].Value),
                                PhieuPhatRa = 0,
                                PhieuThuVe = 0,
                                PhieuHopLe = phieuHopLe,  // Phiếu hợp lệ = tổng phiếu sau khi trừ phiếu không hợp lệ
                                PhieuKhongHopLe = phieuKhongHopLe,  // Phiếu không hợp lệ = từ file
                                UngCuVien1 = ungCuVien1,
                                UngCuVien2 = ungCuVien2,
                                UngCuVien3 = ungCuVien3,
                                UngCuVien4 = ungCuVien4,
                                UngCuVien5 = ungCuVien5,
                                UngCuVien6 = 0,
                                UngCuVien7 = 0,
                                UngCuVien8 = 0
                            };

                            // Validate data
                            if (record.TongCuTri < 0)
                                errors.Add($"Dòng {row}: ⚠️ Không được có số âm");
                            
                            // Warning: check if PhieuHopLe matches TongCuTri (they should be related)
                            if (record.PhieuHopLe > 0 && (record.TongCuTri == 0))
                                errors.Add($"Dòng {row}: ⚠️ Cảnh báo: Có {record.PhieuHopLe} phiếu bầu nhưng Tổng Cử Tri = 0");

                            records.Add(record);
                            result.PreviewData.Add(record);
                            result.ValidRows++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex.Message);
                        }
                    }

                    result.Errors = errors;

                    if (result.ValidRows == 0)
                    {
                        result.Success = false;
                        result.Message = "Không có dữ liệu hợp lệ để import";
                        return result;
                    }

                    // Import to database
                    try
                    {
                        // Merge logic: if KhuVuc name exists, add new values to old ones
                        foreach (var newRecord in records)
                        {
                            var existingRecord = await _context.ElectionResults
                                .FirstOrDefaultAsync(r => r.Level == level && r.KhuVuc == newRecord.KhuVuc && r.To == newRecord.To);

                            if (existingRecord != null)
                            {
                                // Merge data: add new values to existing values
                                existingRecord.TongCuTri += newRecord.TongCuTri;
                                existingRecord.PhieuPhatRa += newRecord.PhieuPhatRa;
                                existingRecord.PhieuThuVe += newRecord.PhieuThuVe;
                                existingRecord.PhieuHopLe += newRecord.PhieuHopLe;
                                existingRecord.PhieuKhongHopLe += newRecord.PhieuKhongHopLe;
                                existingRecord.UngCuVien1 += newRecord.UngCuVien1;
                                existingRecord.UngCuVien2 += newRecord.UngCuVien2;
                                existingRecord.UngCuVien3 += newRecord.UngCuVien3;
                                existingRecord.UngCuVien4 += newRecord.UngCuVien4;
                                existingRecord.UngCuVien5 += newRecord.UngCuVien5;
                                existingRecord.UngCuVien6 += newRecord.UngCuVien6;
                                existingRecord.UngCuVien7 += newRecord.UngCuVien7;
                                existingRecord.UngCuVien8 += newRecord.UngCuVien8;
                                _context.ElectionResults.Update(existingRecord);
                            }
                            else
                            {
                                // New record, just add it
                                _context.ElectionResults.Add(newRecord);
                            }
                        }

                        await _context.SaveChangesAsync();
                        
                        // DEBUG: Check what was saved
                        var savedRecords = await _context.ElectionResults
                            .Where(r => r.Level == level)
                            .ToListAsync();
                        Console.WriteLine($"[DEBUG] After SaveChangesAsync: {savedRecords.Count} records saved with level={level}");
                        foreach (var rec in savedRecords.Take(3))
                        {
                            Console.WriteLine($"[DEBUG]   Record: KhuVuc={rec.KhuVuc}, To={rec.To}, Level={rec.Level}");
                        }

                        // Log
                        var log = new ImportLog
                        {
                            FileName = fileName,
                            ImportType = "ElectionResults",
                            Level = level,
                            TotalRows = result.TotalRows,
                            SuccessRows = result.ValidRows,
                            ErrorRows = errors.Count,
                            Errors = string.Join("; ", errors)
                        };
                        _context.ImportLogs.Add(log);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception dbEx)
                    {
                        string msg = dbEx.InnerException?.Message ?? dbEx.Message;
                        if (msg.Contains("cannot find the file", StringComparison.OrdinalIgnoreCase)
                            || msg.Contains("Cannot open database", StringComparison.OrdinalIgnoreCase))
                        {
                            msg = "Không thể kết nối tới cơ sở dữ liệu. Hãy đảm bảo SQL Server/LocalDB đang chạy và chuỗi kết nối đúng.";
                        }
                        result.Success = false;
                        result.Message = msg;
                        result.Errors.Add(msg);
                        return result;
                    }

                    result.Success = true;
                    result.Message = $"Import thành công {result.ValidRows} dòng";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<ImportResult> ImportElectionProgress(Stream fileStream, string fileName, string level)
        {
            Console.WriteLine($"[DEBUG] ImportElectionProgress started, fileName={fileName}, level={level}");
            Console.WriteLine($"[DEBUG] Connection string: {_context.Database.GetDbConnection().ConnectionString}");

            // ensure database exists before doing anything else
            try
            {
                await _context.Database.EnsureCreatedAsync();
                Console.WriteLine("[DEBUG] Database EnsureCreatedAsync completed (import progress start)");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] EnsureCreatedAsync at method start failed: " + ex);
            }

            var result = new ImportResult();
            var errors = new List<string>();

            try
            {
                using (var package = new ExcelPackage(fileStream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    result.TotalRows = rowCount - 1;
                    var records = new List<ElectionProgress>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        // Skip rows that are not valid data (headers, metadata, empty rows)
                        if (!IsValidDataRow(worksheet, row))
                            continue;

                        try
                        {
                            // build base record from fixed columns
                            var record = new ElectionProgress
                            {
                                Level = level,
                                Stt = SafeParseInt(worksheet.Cells[row, 1].Value),
                                TenKhuVuc = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                                DonVi = worksheet.Cells[row, 3].Value?.ToString() ?? "",
                                TongCuTri = SafeParseInt(worksheet.Cells[row, 4].Value)
                            };

                            // read any additional numeric columns as time values
                            int maxCol = worksheet.Dimension?.Columns ?? 4;
                            int timeTotal = 0;
                            // reset all hourly fields
                            record.Gio8 = record.Gio10 = record.Gio12 = record.Gio14 = record.Gio16 = record.Gio19 = 0;
                            for (int col = 5; col <= maxCol; col++)
                            {
                                int val = SafeParseInt(worksheet.Cells[row, col].Value);
                                timeTotal += val;
                                switch (col - 5)
                                {
                                    case 0: record.Gio8 = val; break;
                                    case 1: record.Gio10 = val; break;
                                    case 2: record.Gio12 = val; break;
                                    case 3: record.Gio14 = val; break;
                                    case 4: record.Gio16 = val; break;
                                    case 5: record.Gio19 = val; break;
                                    default: break; // ignore extras
                                }
                            }

                            record.TongCuTriDiBau = timeTotal;
                            // compute rate if possible
                            record.TiLe = record.TongCuTri > 0 ? (decimal)timeTotal / record.TongCuTri * 100 : 0;

                            if (record.TongCuTri < 0)
                                throw new Exception($"Dòng {row}: Không được có số âm");

                            records.Add(record);
                            result.PreviewData.Add(record);
                            result.ValidRows++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex.Message);
                        }
                    }

                    result.Errors = errors;

                    if (result.ValidRows == 0)
                    {
                        result.Success = false;
                        result.Message = "Không có dữ liệu hợp lệ để import";
                        return result;
                    }

                    try
                    {
                        // ensure database exists so SaveChanges won't fail with file-not-found
                        await _context.Database.EnsureCreatedAsync();
                        Console.WriteLine("[DEBUG] Database EnsureCreatedAsync completed");

                        // Get all existing records
                        var existingRecords = await _context.ElectionProgresses.Where(p => p.Level == level).ToListAsync();
                        Console.WriteLine($"[DEBUG] Found {existingRecords.Count} existing records in database");
                        
                        // Smart merge: extract area and unit numbers, then overwrite
                        foreach (var newRecord in records)
                        {
                            // Extract area number from TenKhuVuc (e.g., "Khu vực bỏ phiếu số 1" -> 1)
                            int? newAreaNum = ExtractAreaNumber(newRecord.TenKhuVuc);
                            
                            // Extract unit number from DonVi (e.g., "Đơn vị 1" -> 1)
                            int? newUnitNum = ExtractUnitNumber(newRecord.DonVi);
                            
                            Console.WriteLine($"[DEBUG] Processing record: TenKhuVuc='{newRecord.TenKhuVuc}' (areaNum={newAreaNum}), DonVi='{newRecord.DonVi}' (unitNum={newUnitNum})");
                            
                            // Try to find existing record by matching area and unit numbers
                            var existing = existingRecords
                                .FirstOrDefault(p => 
                                    ExtractAreaNumber(p.TenKhuVuc) == newAreaNum && 
                                    ExtractUnitNumber(p.DonVi) == newUnitNum &&
                                    newAreaNum.HasValue && newUnitNum.HasValue);

                            if (existing != null)
                            {
                                // Update existing record with new values (completely overwrite)
                                existing.TenKhuVuc = newRecord.TenKhuVuc;
                                existing.DonVi = newRecord.DonVi;
                                existing.TongCuTri = newRecord.TongCuTri;
                                existing.Gio8 = newRecord.Gio8;
                                existing.Gio10 = newRecord.Gio10;
                                existing.Gio12 = newRecord.Gio12;
                                existing.Gio14 = newRecord.Gio14;
                                existing.Gio16 = newRecord.Gio16;
                                existing.Gio19 = newRecord.Gio19;
                                existing.TongCuTriDiBau = newRecord.TongCuTriDiBau;
                                existing.TiLe = newRecord.TiLe;
                                
                                _context.ElectionProgresses.Update(existing);
                                Console.WriteLine($"[DEBUG] Updated existing record ID={existing.Id}");
                            }
                            else
                            {
                                // Add new record - assign next STT
                                int maxStt = existingRecords.Any() ? existingRecords.Max(p => p.Stt) : 0;
                                newRecord.Stt = maxStt + 1;
                                existingRecords.Add(newRecord); // Add to collection for next iteration
                                _context.ElectionProgresses.Add(newRecord);
                                Console.WriteLine($"[DEBUG] Added new record with STT={newRecord.Stt}");
                            }
                        }

                        // Save changes
                        await _context.SaveChangesAsync();
                        Console.WriteLine("[DEBUG] Database save completed successfully");

                        var log = new ImportLog
                        {
                            FileName = fileName,
                            ImportType = "ElectionProgress",
                            Level = level,
                            TotalRows = result.TotalRows,
                            SuccessRows = result.ValidRows,
                            ErrorRows = errors.Count,
                            Errors = string.Join("; ", errors)
                        };
                        _context.ImportLogs.Add(log);
                        await _context.SaveChangesAsync();
                        Console.WriteLine("[DEBUG] Import log saved");
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"[ERROR] exception in ImportElectionProgress DB save:");
                        Console.WriteLine(dbEx.ToString());
                        string msg = dbEx.InnerException?.Message ?? dbEx.Message;
                        if (msg.Contains("cannot find the file", StringComparison.OrdinalIgnoreCase)
                            || msg.Contains("Cannot open database", StringComparison.OrdinalIgnoreCase))
                        {
                            msg = "Không thể kết nối tới cơ sở dữ liệu. Hãy đảm bảo SQL Server/LocalDB đang chạy và chuỗi kết nối đúng.";
                        }
                        result.Success = false;
                        result.Message = msg;
                        result.Errors.Add(msg);
                        return result;
                    }

                    result.Success = true;
                    result.Message = $"Import thành công {result.ValidRows} dòng";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<byte[]> ExportElectionResults(List<ElectionResult> data)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Kết quả bầu cử");

                // Headers
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Khu vực";
                worksheet.Cells[1, 3].Value = "Tổng cử tri";
                worksheet.Cells[1, 4].Value = "Phiếu phát ra";
                worksheet.Cells[1, 5].Value = "Phiếu thu về";
                worksheet.Cells[1, 6].Value = "Phiếu hợp lệ";
                worksheet.Cells[1, 7].Value = "Phiếu không hợp lệ";
                worksheet.Cells[1, 8].Value = "Ứng cử viên 1";
                worksheet.Cells[1, 9].Value = "Ứng cử viên 2";
                worksheet.Cells[1, 10].Value = "Ứng cử viên 3";
                worksheet.Cells[1, 11].Value = "Ứng cử viên 4";
                worksheet.Cells[1, 12].Value = "Ứng cử viên 5";

                // Data
                for (int i = 0; i < data.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = data[i].Stt;
                    worksheet.Cells[row, 2].Value = data[i].KhuVuc;
                    worksheet.Cells[row, 3].Value = data[i].TongCuTri;
                    worksheet.Cells[row, 4].Value = data[i].PhieuPhatRa;
                    worksheet.Cells[row, 5].Value = data[i].PhieuThuVe;
                    worksheet.Cells[row, 6].Value = data[i].PhieuHopLe;
                    worksheet.Cells[row, 7].Value = data[i].PhieuKhongHopLe;
                    worksheet.Cells[row, 8].Value = data[i].UngCuVien1;
                    worksheet.Cells[row, 9].Value = data[i].UngCuVien2;
                    worksheet.Cells[row, 10].Value = data[i].UngCuVien3;
                    worksheet.Cells[row, 11].Value = data[i].UngCuVien4;
                    worksheet.Cells[row, 12].Value = data[i].UngCuVien5;
                }

                worksheet.Cells.AutoFitColumns();
                return await Task.FromResult(package.GetAsByteArray());
            }
        }

        public async Task<byte[]> ExportProgressSummary(List<dynamic> summaryData)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tổng hợp tiến độ");

                // Headers
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Tổ";
                worksheet.Cells[1, 3].Value = "Tổng số cử tri";
                worksheet.Cells[1, 4].Value = "Tổng số cử tri đi bầu";
                worksheet.Cells[1, 5].Value = "Tỷ lệ";
                worksheet.Cells[1, 6].Value = "9:00";
                worksheet.Cells[1, 7].Value = "11:30";
                worksheet.Cells[1, 8].Value = "15:00";
                worksheet.Cells[1, 9].Value = "19:00";
                worksheet.Cells[1, 10].Value = "22:00";

                // Data
                for (int i = 0; i < summaryData.Count; i++)
                {
                    var row = i + 2;
                    var item = summaryData[i];
                    
                    int groupNum = item.GroupNum;
                    int tongCuTri = item.TongCuTri;
                    int tongCuTriDiBau = item.TongCuTriDiBau;
                    decimal tiLe = tongCuTri > 0 ? ((decimal)tongCuTriDiBau / tongCuTri) * 100 : 0;
                    
                    worksheet.Cells[row, 1].Value = i + 1;
                    worksheet.Cells[row, 2].Value = $"Tổ {groupNum}";
                    worksheet.Cells[row, 3].Value = tongCuTri;
                    worksheet.Cells[row, 4].Value = tongCuTriDiBau;
                    worksheet.Cells[row, 5].Value = tiLe.ToString("F1");
                    worksheet.Cells[row, 6].Value = item.Gio8;
                    worksheet.Cells[row, 7].Value = item.Gio10;
                    worksheet.Cells[row, 8].Value = item.Gio12;
                    worksheet.Cells[row, 9].Value = item.Gio14;
                    worksheet.Cells[row, 10].Value = item.Gio16;
                }

                worksheet.Cells.AutoFitColumns();
                return await Task.FromResult(package.GetAsByteArray());
            }
        }

        public async Task<byte[]> ExportElectionProgress(List<ElectionProgress> data)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tiến độ bầu cử");

                // Headers
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Tên khu vực";
                worksheet.Cells[1, 3].Value = "Đơn vị";
                worksheet.Cells[1, 4].Value = "Tổng cử tri";
                worksheet.Cells[1, 5].Value = "9:00";
                worksheet.Cells[1, 6].Value = "11:30";
                worksheet.Cells[1, 7].Value = "15:00";
                worksheet.Cells[1, 8].Value = "19:00";
                worksheet.Cells[1, 9].Value = "22:00";
                worksheet.Cells[1, 10].Value = "Tổng cử tri đi bầu";
                worksheet.Cells[1, 11].Value = "Tỉ lệ (%)";

                // Data
                for (int i = 0; i < data.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = data[i].Stt;
                    worksheet.Cells[row, 2].Value = data[i].TenKhuVuc;
                    worksheet.Cells[row, 3].Value = data[i].DonVi;
                    worksheet.Cells[row, 4].Value = data[i].TongCuTri;
                    worksheet.Cells[row, 5].Value = data[i].Gio8;
                    worksheet.Cells[row, 6].Value = data[i].Gio10;
                    worksheet.Cells[row, 7].Value = data[i].Gio12;
                    worksheet.Cells[row, 8].Value = data[i].Gio14;
                    worksheet.Cells[row, 9].Value = data[i].Gio16;
                    worksheet.Cells[row, 10].Value = data[i].TongCuTriDiBau;
                    worksheet.Cells[row, 11].Value = data[i].TiLe;
                }

                worksheet.Cells.AutoFitColumns();
                return await Task.FromResult(package.GetAsByteArray());
            }
        }

        // Helper method to extract area number from TenKhuVuc
        private int? ExtractAreaNumber(string tenKhuVuc)
        {
            if (string.IsNullOrWhiteSpace(tenKhuVuc))
                return null;
            
            // Try to match "số X" pattern (e.g., "Khu vực bỏ phiếu số 1" -> 1)
            var match = System.Text.RegularExpressions.Regex.Match(tenKhuVuc, @"số\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int areaNum))
                return areaNum;
            
            return null;
        }

        // Helper method to extract unit number from DonVi
        private int? ExtractUnitNumber(string donVi)
        {
            if (string.IsNullOrWhiteSpace(donVi))
                return null;
            
            // Try to match any number pattern (e.g., "Đơn vị 1" -> 1)
            var match = System.Text.RegularExpressions.Regex.Match(donVi, @"(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int unitNum))
                return unitNum;
            
            return null;
        }

        // Comprehensive export with all tables as shown in the UI
        public async Task<byte[]> ExportComprehensiveElectionResults(List<ElectionResult> results, List<ElectionProgress> progress, string level)
        {
            using (var package = new ExcelPackage())
            {
                // Sheet 1: Summary by Unit (SỐ LIỆU ĐƠN VỊ BẦU CỬ)
                var summarySheet = package.Workbook.Worksheets.Add("Tổng hợp đơn vị");
                CreateSummarySheet(summarySheet, results, progress, level);

                // Sheet 2: Detail by District (CHI TIẾT THEO KHU VỰC)
                var detailSheet = package.Workbook.Worksheets.Add("Chi tiết theo khu vực");
                CreateDetailSheet(detailSheet, results);

                // Sheet 3: Results by Candidate (KẾT QUẢ THEO ỨNG VIÊN)
                var candidateSheet = package.Workbook.Worksheets.Add("Kết quả theo ứng viên");
                CreateCandidateResultsSheet(candidateSheet, results);

                // Sheet 4: Summary by Unit - Winners & Losers (KẾT QUẢ TRÚNG CỬ - THẤT CỬ THEO TỔ)
                var summaryByUnitSheet = package.Workbook.Worksheets.Add("Kết quả trúng/thất cử");
                CreateWinnersLosersSheet(summaryByUnitSheet, results);

                return await Task.FromResult(package.GetAsByteArray());
            }
        }

        private void CreateSummarySheet(ExcelWorksheet ws, List<ElectionResult> results, List<ElectionProgress> progress, string level)
        {
            int row = 1;
            
            // Title
            ws.Cells[row, 1].Value = "SỐ LIỆU ĐƠN VỊ BẦU CỬ";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            row += 2;

            // Summary statistics
            if (results.Count > 0)
            {
                int totalVoters = results.Sum(r => r.TongCuTri);
                int votesReceived = results.Sum(r => r.PhieuThuVe);
                int validVotes = results.Sum(r => r.PhieuHopLe);
                int invalidVotes = results.Sum(r => r.PhieuKhongHopLe);
                decimal turnoutRate = totalVoters > 0 ? ((decimal)votesReceived / totalVoters) * 100 : 0;

                // Add statistics in pairs
                ws.Cells[row, 1].Value = "Tổng số cử tri";
                ws.Cells[row, 2].Value = totalVoters;
                ws.Cells[row, 3].Value = "Tổng số phiếu bầu";
                ws.Cells[row, 4].Value = validVotes + invalidVotes;
                row++;

                ws.Cells[row, 1].Value = "Phiếu thu về";
                ws.Cells[row, 2].Value = votesReceived;
                ws.Cells[row, 3].Value = "Tỉ lệ cử tri đi bầu";
                ws.Cells[row, 4].Value = Math.Round(turnoutRate, 2) + "%";
                row++;

                ws.Cells[row, 1].Value = "Phiếu hợp lệ";
                ws.Cells[row, 2].Value = validVotes;
                ws.Cells[row, 3].Value = "Phiếu không hợp lệ";
                ws.Cells[row, 4].Value = invalidVotes;
                row++;

                // Summary candidate votes
                ws.Cells[row, 1].Value = "Ứng cử viên 1";
                ws.Cells[row, 2].Value = results.Sum(r => r.UngCuVien1);
                ws.Cells[row, 3].Value = "Ứng cử viên 2";
                ws.Cells[row, 4].Value = results.Sum(r => r.UngCuVien2);
                row++;

                ws.Cells[row, 1].Value = "Ứng cử viên 3";
                ws.Cells[row, 2].Value = results.Sum(r => r.UngCuVien3);
                ws.Cells[row, 3].Value = "Ứng cử viên 4";
                ws.Cells[row, 4].Value = results.Sum(r => r.UngCuVien4);
                row++;

                ws.Cells[row, 1].Value = "Ứng cử viên 5";
                ws.Cells[row, 2].Value = results.Sum(r => r.UngCuVien5);
                row++;
            }

            ws.Cells.AutoFitColumns();
        }

        private void CreateDetailSheet(ExcelWorksheet ws, List<ElectionResult> results)
        {
            int row = 1;
            
            // Title
            ws.Cells[row, 1].Value = "CHI TIẾT THEO KHU VỰC";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            row += 2;

            // Headers
            var headers = new[] { "STT", "Khu vực", "Tổng cử tri", "Phiếu phát ra", "Phiếu thu về", 
                "Phiếu hợp lệ", "Phiếu không hợp lệ", "Ứng cử viên 1", "Ứng cử viên 2", 
                "Ứng cử viên 3", "Ứng cử viên 4", "Ứng cử viên 5" };
            
            for (int col = 0; col < headers.Length; col++)
            {
                ws.Cells[row, col + 1].Value = headers[col];
                ws.Cells[row, col + 1].Style.Font.Bold = true;
                ws.Cells[row, col + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[row, col + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            row++;

            // Data rows
            foreach (var result in results.OrderBy(r => r.Stt))
            {
                ws.Cells[row, 1].Value = result.Stt;
                ws.Cells[row, 2].Value = result.KhuVuc;
                ws.Cells[row, 3].Value = result.TongCuTri;
                ws.Cells[row, 4].Value = result.PhieuPhatRa;
                ws.Cells[row, 5].Value = result.PhieuThuVe;
                ws.Cells[row, 6].Value = result.PhieuHopLe;
                ws.Cells[row, 7].Value = result.PhieuKhongHopLe;
                ws.Cells[row, 8].Value = result.UngCuVien1;
                ws.Cells[row, 9].Value = result.UngCuVien2;
                ws.Cells[row, 10].Value = result.UngCuVien3;
                ws.Cells[row, 11].Value = result.UngCuVien4;
                ws.Cells[row, 12].Value = result.UngCuVien5;
                row++;
            }

            ws.Cells.AutoFitColumns();
        }

        private void CreateCandidateResultsSheet(ExcelWorksheet ws, List<ElectionResult> results)
        {
            int row = 1;

            // Title
            ws.Cells[row, 1].Value = "KẾT QUẢ BẦU CỬ THEO ỨNG VIÊN";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            row += 2;

            // Headers
            var headers = new[] { "Ứng cử viên", "Số phiếu bầu", "Tỉ lệ (%)", "Kết quả" };
            for (int col = 0; col < headers.Length; col++)
            {
                ws.Cells[row, col + 1].Value = headers[col];
                ws.Cells[row, col + 1].Style.Font.Bold = true;
                ws.Cells[row, col + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[row, col + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            row++;

            int totalValidVotes = results.Sum(r => r.PhieuHopLe);
            
            // Candidate results
            var candidates = new[]
            {
                new { Name = "Ứng cử viên 1", Votes = results.Sum(r => r.UngCuVien1) },
                new { Name = "Ứng cử viên 2", Votes = results.Sum(r => r.UngCuVien2) },
                new { Name = "Ứng cử viên 3", Votes = results.Sum(r => r.UngCuVien3) },
                new { Name = "Ứng cử viên 4", Votes = results.Sum(r => r.UngCuVien4) },
                new { Name = "Ứng cử viên 5", Votes = results.Sum(r => r.UngCuVien5) }
            };

            foreach (var candidate in candidates)
            {
                decimal percentage = totalValidVotes > 0 ? ((decimal)candidate.Votes / totalValidVotes) * 100 : 0;
                string status = percentage > 50 ? "Trúng cử" : 
                               percentage > 0 ? "Thất cử" : "Không có phiếu";

                ws.Cells[row, 1].Value = candidate.Name;
                ws.Cells[row, 2].Value = candidate.Votes;
                ws.Cells[row, 3].Value = Math.Round(percentage, 2) + "%";
                ws.Cells[row, 4].Value = status;
                
                // Color code the status
                if (status == "Trúng cử")
                    ws.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                else if (status == "Thất cử")
                    ws.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
                
                row++;
            }

            ws.Cells.AutoFitColumns();
        }

        private string GetColumnLetter(int col)
        {
            string result = "";
            while (col > 0)
            {
                col--;
                result = (char)('A' + col % 26) + result;
                col /= 26;
            }
            return result;
        }

        private void CreateWinnersLosersSheet(ExcelWorksheet ws, List<ElectionResult> results)
        {
            int row = 1;

            // Title
            ws.Cells[row, 1].Value = "KẾT QUẢ TRÚNG CỬ - THẤT CỰ THEO TỔ/ĐƠN VỊ";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            row += 2;

            // Calculate results for each group/unit (1-10)
            var unitResults = new Dictionary<int, dynamic>();
            int totalValidVotes = results.Sum(r => r.PhieuHopLe);

            for (int unitNum = 1; unitNum <= 10; unitNum++)
            {
                int u1Votes = results.Sum(r => r.UngCuVien1);
                int u2Votes = results.Sum(r => r.UngCuVien2);
                int u3Votes = results.Sum(r => r.UngCuVien3);
                int u4Votes = results.Sum(r => r.UngCuVien4);
                int u5Votes = results.Sum(r => r.UngCuVien5);

                var winners = new List<string>();
                var losers = new List<string>();

                if (totalValidVotes > 0)
                {
                    if (u1Votes > totalValidVotes / 2) winners.Add("Ứng cử viên 1"); else if (u1Votes > 0) losers.Add("Ứng cử viên 1");
                    if (u2Votes > totalValidVotes / 2) winners.Add("Ứng cử viên 2"); else if (u2Votes > 0) losers.Add("Ứng cử viên 2");
                    if (u3Votes > totalValidVotes / 2) winners.Add("Ứng cử viên 3"); else if (u3Votes > 0) losers.Add("Ứng cử viên 3");
                    if (u4Votes > totalValidVotes / 2) winners.Add("Ứng cử viên 4"); else if (u4Votes > 0) losers.Add("Ứng cử viên 4");
                    if (u5Votes > totalValidVotes / 2) winners.Add("Ứng cử viên 5"); else if (u5Votes > 0) losers.Add("Ứng cử viên 5");
                }

                unitResults[unitNum] = new
                {
                    UnitNum = unitNum,
                    Winners = string.Join(", ", winners.Count > 0 ? winners : new List<string> { "Không có" }),
                    Losers = string.Join(", ", losers.Count > 0 ? losers : new List<string> { "Không có" })
                };
            }

            // Headers
            var headers = new[] { "Tổ/Đơn vị", "Ứng cử viên Trúng cử", "Ứng cử viên Thất cử" };
            for (int col = 0; col < headers.Length; col++)
            {
                ws.Cells[row, col + 1].Value = headers[col];
                ws.Cells[row, col + 1].Style.Font.Bold = true;
                ws.Cells[row, col + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[row, col + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            row++;

            // Data rows
            foreach (var item in unitResults.OrderBy(x => x.Key))
            {
                ws.Cells[row, 1].Value = $"Tổ {item.Value.UnitNum}";
                
                ws.Cells[row, 2].Value = item.Value.Winners;
                ws.Cells[row, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

                ws.Cells[row, 3].Value = item.Value.Losers;
                ws.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);

                row++;
            }

            ws.Cells.AutoFitColumns();
        }

        public async Task<byte[]> ExportOfficialBallotVerificationForm(List<ElectionResult> results, string level, string area = "", List<string> footerData = null)
        {
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Biểu tổng hợp kiểm phiếu");
                ws.View.ZoomScale = 60;

                // Convert level to lowercase for comparison
                string levelLower = level?.ToLower() ?? "";
                Console.WriteLine($"[DEBUG] ExportOfficialBallotVerificationForm - Received level: '{level}'");
                Console.WriteLine($"[DEBUG] levelLower: '{levelLower}'");

                // Determine column structure based on level
                // All levels now display: UCV 1, 2, 3, 4, 5, 6, 7 (7 candidates)
                Console.WriteLine($"[DEBUG] level.ToLower(): '{level.ToLower()}'");
                
                // Calculate UCV layout based on level
                // TINH: 7 candidates (UCV 1-7), XA/QUOCHOI: 5 candidates (UCV 1-5)
                int candidateCount = levelLower.Contains("tinh") ? 7 : 5;
                int ucvStartCol_Layout = levelLower.Contains("tinh") ? 17 : 16;  // Q (17) for TINH, P (16) for XA/QUOCHOI
                int ucvCount = 7;  // All levels show UCV header 1-7 in layout
                Console.WriteLine($"[DEBUG] Calculated candidateCount: {candidateCount}, ucvStartCol_Layout: {ucvStartCol_Layout}");
                
                int uvcStartCol = 17; // Column Q (keep for compatibility)
                int totalCol = ucvStartCol_Layout + candidateCount;  // Column 24 (17+7) for TINH, Column 21 (16+5) for XA/QUOCHOI
                int maxCol = totalCol;
                
                Console.WriteLine($"[DEBUG] totalCol: {totalCol}, maxCol: {maxCol}");

                int row = 1;

                // Header section
                ws.Cells[row, 1].Value = "Xã/Phường: ...................";
                row++;
                ws.Cells[row, 1].Value = "TỔ BẦU CỬ.................";
                row += 2;

                // Title
                ws.Cells[$"A{row}:Z{row}"].Merge = true;
                ws.Cells[row, 1].Value = "BẢNG TỔNG HỢP KIỂM PHIẾU BẦU CỬ ĐẠI BIỂU HỘI ĐỒNG NHÂN DÂN TỈNH/CẤP XÃ NHIỆM KỲ 2026-2031";
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                row++;

                ws.Cells[$"A{row}:Z{row}"].Merge = true;
                ws.Cells[row, 1].Value = "TẠI TỔ BẦU CỬ SỐ 1 ĐƠN VỊ BẦU CỬ HỘI ĐỒNG NHÂN DÂN TỈNH/ CẤP XÃ SỐ: 1";
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                row += 2;

                // === LEVEL 1 HEADERS (MERGED CELLS) ===
                int level1Row = row;

                // Cols 1-3: NOT merged - single column headers
                ws.Cells[level1Row, 1].Value = "Xã/phường";
                ws.Cells[level1Row, 1].Style.Font.Bold = true;
                ws.Cells[level1Row, 1].Style.Font.Size = 10;
                ws.Cells[level1Row, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                ws.Cells[level1Row, 2].Value = "Tổ bầu cử";
                ws.Cells[level1Row, 2].Style.Font.Bold = true;
                ws.Cells[level1Row, 2].Style.Font.Size = 10;
                ws.Cells[level1Row, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                ws.Cells[level1Row, 3].Value = "Khu vực bỏ phiếu";
                ws.Cells[level1Row, 3].Style.Font.Bold = true;
                ws.Cells[level1Row, 3].Style.Font.Size = 10;
                ws.Cells[level1Row, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                // Cols 4-6: NO merge, NO text at Level 1 (empty cells for alignment)
                
                // Style cols D, E, F (merged vertically)
                ws.Cells[level1Row, 4].Value = "Tổng số cử tri trong danh sách tham gia bỏ phiếu";
                ws.Cells[level1Row, 4].Style.Font.Bold = true;
                ws.Cells[level1Row, 4].Style.Font.Size = 9;
                ws.Cells[level1Row, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                ws.Cells[level1Row, 4].Style.WrapText = true;

                ws.Cells[level1Row, 5].Value = "Số cử tri thực tế bỏ phiếu";
                ws.Cells[level1Row, 5].Style.Font.Bold = true;
                ws.Cells[level1Row, 5].Style.Font.Size = 9;
                ws.Cells[level1Row, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 5].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                ws.Cells[level1Row, 5].Style.WrapText = true;

                ws.Cells[level1Row, 6].Value = "Tỉ lệ (%)";
                ws.Cells[level1Row, 6].Style.Font.Bold = true;
                ws.Cells[level1Row, 6].Style.Font.Size = 9;
                ws.Cells[level1Row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 6].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                ws.Cells[level1Row, 6].Style.WrapText = true;

                // Cols 7-8: NOT merged - single columns
                ws.Cells[level1Row, 7].Value = "Số phiếu phát ra";
                ws.Cells[level1Row, 7].Style.Font.Bold = true;
                ws.Cells[level1Row, 7].Style.Font.Size = 10;
                ws.Cells[level1Row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 7].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                ws.Cells[level1Row, 8].Value = "Số phiếu thu vào";
                ws.Cells[level1Row, 8].Style.Font.Bold = true;
                ws.Cells[level1Row, 8].Style.Font.Size = 10;
                ws.Cells[level1Row, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 8].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 8].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                // Cols 9-10: MERGED only for XA/QUOCHOI (commune and national) levels - "Số phiếu không hợp lệ"
                if (!levelLower.Contains("tinh"))
                {
                    ws.Cells[$"I{level1Row}:J{level1Row}"].Merge = true;
                }
                ws.Cells[level1Row, 9].Value = "Số phiếu không hợp lệ";
                ws.Cells[level1Row, 9].Style.Font.Bold = true;
                ws.Cells[level1Row, 9].Style.Font.Size = 9;
                ws.Cells[level1Row, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 9].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 9].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 9].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                // Cols 11-12: MERGED - "Số phiếu hợp lệ"
                ws.Cells[$"K{level1Row}:L{level1Row}"].Merge = true;
                ws.Cells[level1Row, 11].Value = "Số phiếu hợp lệ";
                ws.Cells[level1Row, 11].Style.Font.Bold = true;
                ws.Cells[level1Row, 11].Style.Font.Size = 9;
                ws.Cells[level1Row, 11].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 11].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 11].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 11].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                // Cols 13-16: MERGED - "Phân loại phiếu"
                if (levelLower.Contains("tinh"))
                {
                    ws.Cells[$"M{level1Row}:P{level1Row}"].Merge = true;  // TINH: 4 columns (M-P)
                }
                else
                {
                    ws.Cells[$"M{level1Row}:O{level1Row}"].Merge = true;  // XA and QUOCHOI: 3 columns (M-O)
                }
                ws.Cells[level1Row, 13].Value = "Phân loại phiếu";
                ws.Cells[level1Row, 13].Style.Font.Bold = true;
                ws.Cells[level1Row, 13].Style.Font.Size = 9;
                ws.Cells[level1Row, 13].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, 13].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, 13].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, 13].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                // Cols 17-24: MERGED - "Số phiếu bầu cho mỗi người ứng cử viên"
                string uvcEndCol = GetColumnLetter(totalCol - 1);
                Console.WriteLine($"[DEBUG] UCV Headers - uvcStartCol: {uvcStartCol}, Total UCV columns: {ucvCount}, uvcEndCol: {uvcEndCol}, Merge range: Q{level1Row}:{uvcEndCol}{level1Row}");
                int headerCol = uvcStartCol; // Default to Q (17)
                if (levelLower.Contains("tinh"))
                {
                    ws.Cells[$"Q{level1Row}:{uvcEndCol}{level1Row}"].Merge = true;  // TINH: merge Q onwards
                }
                else
                {
                    ws.Cells[$"P{level1Row}:T{level1Row}"].Merge = true;  // XA and QUOCHOI: P-T
                    headerCol = 16; // P for XA/QUOCHOI level merged cell
                }
                ws.Cells[level1Row, headerCol].Value = "Số phiếu bầu cho mỗi người ứng cử viên";
                ws.Cells[level1Row, headerCol].Style.Font.Bold = true;
                ws.Cells[level1Row, headerCol].Style.Font.Size = 8;
                ws.Cells[level1Row, headerCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, headerCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, headerCol].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, headerCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(192, 192, 192));
                ws.Cells[level1Row, headerCol].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                // Total column header (W7 / Y7) - text at level 1
                Console.WriteLine($"[DEBUG] Total column - Column {totalCol}, Letter: {GetColumnLetter(totalCol)}");
                ws.Cells[level1Row, totalCol].Value = "Tổng số phiếu bầu của các ứng cử viên";
                ws.Cells[level1Row, totalCol].Style.Font.Bold = true;
                ws.Cells[level1Row, totalCol].Style.Font.Size = 9;
                ws.Cells[level1Row, totalCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[level1Row, totalCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[level1Row, totalCol].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[level1Row, totalCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                ws.Cells[level1Row, totalCol].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                ws.Cells[level1Row, totalCol].Style.WrapText = true;

                // Set row height for level 1 header
                ws.Rows[level1Row].Height = 25;

                row++;

                // === LEVEL 2 HEADERS (SUB-COLUMNS) - SEPARATE ROW (NOT MERGED WITH LEVEL 1) ===
                int level2Row = row;

                ws.Cells[level2Row, 1].Value = "Xã/phường";
                ws.Cells[level2Row, 2].Value = "Tổ bầu cử";
                ws.Cells[level2Row, 3].Value = "Khu vực bỏ phiếu";
                
                ws.Cells[level2Row, 4].Value = "Tổng số cử tri trong danh sách tham gia bỏ phiếu";
                ws.Cells[level2Row, 5].Value = "Số cử tri thực tế bỏ phiếu";
                ws.Cells[level2Row, 6].Value = "Tỉ lệ (%)";
                
                ws.Cells[level2Row, 7].Value = "Số phiếu phát ra";
                ws.Cells[level2Row, 8].Value = "Số phiếu thu vào";
                
                ws.Cells[level2Row, 9].Value = "Số phiếu";
                ws.Cells[level2Row, 10].Value = "Tỉ lệ so với số phiếu thu vào (%)";
                
                ws.Cells[level2Row, 11].Value = "Số phiếu";
                ws.Cells[level2Row, 12].Value = "Tỉ lệ so với số phiếu thu vào (%)";
                
                // Ballot classification headers (columns 13-16 for TINH, 13-15 for XA/QUOCHOI)
                if (levelLower.Contains("tinh"))
                {
                    ws.Cells[level2Row, 13].Value = "Bầu 04 đại biểu";
                    ws.Cells[level2Row, 14].Value = "Bầu 03 đại biểu";
                    ws.Cells[level2Row, 15].Value = "Bầu 02 đại biểu";
                    ws.Cells[level2Row, 16].Value = "Bầu 01 đại biểu";
                }
                else
                {
                    // XA/QUOCHOI: Bầu 03, 02, 01 at columns 13, 14, 15 (no Bầu 04)
                    ws.Cells[level2Row, 13].Value = "Bầu 03 đại biểu";
                    ws.Cells[level2Row, 14].Value = "Bầu 02 đại biểu";
                    ws.Cells[level2Row, 15].Value = "Bầu 01 đại biểu";
                }
                
                // UCV columns - columns Q (17) onwards for TINH, columns P (16) onwards for XA/QUOCHOI
                int ucvStartCol_Header = levelLower.Contains("tinh") ? 17 : 16;
                int ucvCount_Header = levelLower.Contains("tinh") ? 7 : 5;  // Only display headers for actual candidates
                for (int i = 1; i <= ucvCount_Header; i++)
                {
                    int colNum = ucvStartCol_Header + i - 1;
                    ws.Cells[level2Row, colNum].Value = $"UCV {i}";
                    Console.WriteLine($"[DEBUG] Set UCV {i} at column {colNum} ({GetColumnLetter(colNum)})");
                }

                // Total column header - no text in level2Row since it's merged with level1Row
                // ws.Cells[level2Row, totalCol].Value = "Tổng số phiếu bầu của các ứng cử viên";

                // Style level 2 headers
                for (int col = 1; col <= maxCol; col++)
                {
                    ws.Cells[level2Row, col].Style.Font.Bold = true;
                    ws.Cells[level2Row, col].Style.Font.Size = 9;
                    ws.Cells[level2Row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws.Cells[level2Row, col].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    ws.Cells[level2Row, col].Style.WrapText = true;
                    ws.Cells[level2Row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[level2Row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(200, 200, 200));
                    ws.Cells[level2Row, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }
                ws.Rows[level2Row].Height = 35;

                // === MERGE VERTICAL (LEVEL 1 + LEVEL 2) FOR COLS WITHOUT HORIZONTAL MERGE ===
                // Merge cols 1-3, 4-6, 7-8 (single headers that span 2 rows)
                ws.Cells[$"A{level1Row}:A{level2Row}"].Merge = true;
                ws.Cells[$"B{level1Row}:B{level2Row}"].Merge = true;
                ws.Cells[$"C{level1Row}:C{level2Row}"].Merge = true;
                ws.Cells[$"D{level1Row}:D{level2Row}"].Merge = true;
                ws.Cells[$"E{level1Row}:E{level2Row}"].Merge = true;
                ws.Cells[$"F{level1Row}:F{level2Row}"].Merge = true;
                ws.Cells[$"G{level1Row}:G{level2Row}"].Merge = true;
                ws.Cells[$"H{level1Row}:H{level2Row}"].Merge = true;
                
                // Merge total column (W/Y) vertically
                string totalColLetter = GetColumnLetter(totalCol);
                ws.Cells[$"{totalColLetter}{level1Row}:{totalColLetter}{level2Row}"].Merge = true;
                
                // === ADD BORDER FOR HEADERS (XA LEVEL ONLY) ===
                object u7Value = null; // Will store U7 value from X7 move (for XA/QUOCHOI levels only)
                
                try
                {
                    if (!levelLower.Contains("tinh"))
                    {
                        // Unmerge X7:X8 for XA/QUOCHOI levels
                        ws.Cells[$"{totalColLetter}{level1Row}:{totalColLetter}{level2Row}"].Merge = false;
                        Console.WriteLine("[DEBUG] Unmerged X7:X8 for XA/QUOCHOI levels");
                        
                        // Move X7 content to U7 and clear X8 for XA/QUOCHOI levels
                        var x7Cell = ws.Cells[level1Row, totalCol];
                        var u7Cell = ws.Cells[level1Row, 21];
                        var u8Cell = ws.Cells[level2Row, 21];
                        
                        // Set U7 to the header text for XA/QUOCHOI levels
                        u7Cell.Value = "Tổng số phiếu bầu của các ứng cử viên";
                        
                        // Copy X7 styling to U7
                        u7Cell.Style.Font.Bold = x7Cell.Style.Font.Bold;
                        u7Cell.Style.Font.Size = x7Cell.Style.Font.Size;
                        u7Cell.Style.HorizontalAlignment = x7Cell.Style.HorizontalAlignment;
                        u7Cell.Style.VerticalAlignment = x7Cell.Style.VerticalAlignment;
                        u7Cell.Style.Fill.PatternType = x7Cell.Style.Fill.PatternType;
                        u7Cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        u7Cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        u7Cell.Style.WrapText = x7Cell.Style.WrapText;
                        
                        // Save U7 value to restore later (to ensure it's preserved)
                        u7Value = u7Cell.Value;
                        Console.WriteLine($"[DEBUG] Saved U7 value: {u7Value}");
                        
                        // Clear X7 cell and remove borders
                        x7Cell.Value = null;
                        x7Cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.None;
                        x7Cell.Style.Border.Left.Style = ExcelBorderStyle.None;
                        x7Cell.Style.Border.Right.Style = ExcelBorderStyle.None;
                        x7Cell.Style.Border.Top.Style = ExcelBorderStyle.None;
                        x7Cell.Style.Border.Bottom.Style = ExcelBorderStyle.None;
                        Console.WriteLine("[DEBUG] Moved X7 to U7 for XA level");
                        
                        // Clear X8 cell and remove borders
                        var x8Cell = ws.Cells[level2Row, totalCol];
                        x8Cell.Value = null;
                        x8Cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.None;
                        x8Cell.Style.Border.Left.Style = ExcelBorderStyle.None;
                        x8Cell.Style.Border.Right.Style = ExcelBorderStyle.None;
                        x8Cell.Style.Border.Top.Style = ExcelBorderStyle.None;
                        x8Cell.Style.Border.Bottom.Style = ExcelBorderStyle.None;
                        Console.WriteLine("[DEBUG] Cleared X8 and removed borders for XA level");
                        
                        // Clear U8 completely before merging
                        u8Cell.Value = null;
                        u8Cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.None;
                        // Clear all borders from U8
                        u8Cell.Style.Border.Left.Style = ExcelBorderStyle.None;
                        u8Cell.Style.Border.Right.Style = ExcelBorderStyle.None;
                        u8Cell.Style.Border.Top.Style = ExcelBorderStyle.None;
                        u8Cell.Style.Border.Bottom.Style = ExcelBorderStyle.None;
                        u8Cell.Style.Border.DiagonalDown = false;
                        u8Cell.Style.Border.DiagonalUp = false;
                        Console.WriteLine("[DEBUG] Cleared U8 completely for XA level");
                        
                        // Now merge U7:U8 - should work since U8 is completely empty
                        try
                        {
                            ws.Cells[$"U{level1Row}:U{level2Row}"].Merge = true;
                            Console.WriteLine("[DEBUG] Successfully merged U7:U8 for XA/QUOCHOI levels");
                        }
                        catch (Exception mergeEx)
                        {
                            Console.WriteLine($"[DEBUG] Merge U7:U8 error: {mergeEx.Message}");
                        }
                        
                        // Add border to header range for XA/QUOCHOI levels
                        var headerRange = ws.Cells[$"A{level1Row}:T{level2Row}"];
                        headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }
                catch (Exception nonTinhEx)
                {
                    Console.WriteLine($"[ERROR] XA/QUOCHOI level processing failed: {nonTinhEx.GetBaseException().Message}");
                    throw;
                }
                
                row++;

                // === DATA ROWS - POPULATE FROM RESULTS ===
                // Populate each result into the Excel table
                Console.WriteLine($"[DEBUG] Starting data population - Total results: {results.Count}");
                
                foreach (var result in results.OrderBy(r => r.Stt).Take(4))  // Max 4 data rows (rows 9-12)
                {
                    Console.WriteLine($"[DEBUG] Processing result - Row={row}, Stt={result.Stt}");
                    
                    ws.Cells[row, 1].Value = result.Level ?? "";  // Xã/Phường
                    ws.Cells[row, 2].Value = result.To ?? "";  // Tổ (group)
                    ws.Cells[row, 3].Value = result.KhuVuc ?? "";
                    
                    ws.Cells[row, 4].Value = result.TongCuTri > 0 ? result.TongCuTri : "";
                    // Note: CuTriThucTe not available in model, using PhieuThuVe as proxy
                    ws.Cells[row, 5].Value = result.PhieuThuVe > 0 ? result.PhieuThuVe : "";
                    
                    // Calculate percentage: (PhieuThuVe / TongCuTri) * 100
                    if (result.TongCuTri > 0)
                    {
                        decimal percentageVoters = (result.PhieuThuVe / (decimal)result.TongCuTri) * 100;
                        ws.Cells[row, 6].Value = percentageVoters > 0 ? Math.Round(percentageVoters, 1) : "";
                    }
                    
                    ws.Cells[row, 7].Value = result.PhieuPhatRa > 0 ? result.PhieuPhatRa : "";
                    ws.Cells[row, 8].Value = result.PhieuThuVe > 0 ? result.PhieuThuVe : "";
                    
                    // Invalid votes
                    ws.Cells[row, 9].Value = result.PhieuKhongHopLe > 0 ? result.PhieuKhongHopLe : "";
                    
                    // Invalid percentage
                    if (result.PhieuThuVe > 0)
                    {
                        decimal invalidPercentage = (result.PhieuKhongHopLe / (decimal)result.PhieuThuVe) * 100;
                        ws.Cells[row, 10].Value = invalidPercentage > 0 ? Math.Round(invalidPercentage, 1) : "";
                    }
                    
                    // Valid votes
                    ws.Cells[row, 11].Value = result.PhieuHopLe > 0 ? result.PhieuHopLe : "";
                    
                    // Valid percentage
                    if (result.PhieuThuVe > 0)
                    {
                        decimal validPercentage = (result.PhieuHopLe / (decimal)result.PhieuThuVe) * 100;
                        ws.Cells[row, 12].Value = validPercentage > 0 ? Math.Round(validPercentage, 1) : "";
                    }
                    
                    // Ballot classification (columns 13-16 for TINH, 13-15 for XA/QUOCHOI)
                    // PhieuBau04 only for TINH level (column 13)
                    if (levelLower.Contains("tinh"))
                    {
                        ws.Cells[row, 13].Value = result.PhieuBau04 > 0 ? result.PhieuBau04 : "";
                        ws.Cells[row, 14].Value = result.PhieuBau03 > 0 ? result.PhieuBau03 : "";
                        ws.Cells[row, 15].Value = result.PhieuBau02 > 0 ? result.PhieuBau02 : "";
                        ws.Cells[row, 16].Value = result.PhieuBau01 > 0 ? result.PhieuBau01 : "";
                    }
                    else
                    {
                        // XA/QUOCHOI: Bầu 03, 02, 01 at columns 13, 14, 15 (no Bầu 04)
                        ws.Cells[row, 13].Value = result.PhieuBau03 > 0 ? result.PhieuBau03 : "";
                        ws.Cells[row, 14].Value = result.PhieuBau02 > 0 ? result.PhieuBau02 : "";
                        ws.Cells[row, 15].Value = result.PhieuBau01 > 0 ? result.PhieuBau01 : "";
                    }
                    
                    // UCV vote counts (columns Q (17) onwards for TINH, columns P (16) onwards for XA/QUOCHOI)
                    int ucvStartCol_Data = levelLower.Contains("tinh") ? 17 : 16;  // Q (17) for TINH, P (16) for XA/QUOCHOI
                    
                    Console.WriteLine($"[DEBUG] Row {row}: UCV data - Level={levelLower}, ucvStartCol_Data={ucvStartCol_Data}");
                    Console.WriteLine($"[DEBUG] UngCuVien1={result.UngCuVien1}, UngCuVien2={result.UngCuVien2}, UngCuVien3={result.UngCuVien3}, UngCuVien4={result.UngCuVien4}, UngCuVien5={result.UngCuVien5}");
                    
                    ws.Cells[row, ucvStartCol_Data].Value = result.UngCuVien1 > 0 ? result.UngCuVien1 : "";
                    ws.Cells[row, ucvStartCol_Data + 1].Value = result.UngCuVien2 > 0 ? result.UngCuVien2 : "";
                    ws.Cells[row, ucvStartCol_Data + 2].Value = result.UngCuVien3 > 0 ? result.UngCuVien3 : "";
                    ws.Cells[row, ucvStartCol_Data + 3].Value = result.UngCuVien4 > 0 ? result.UngCuVien4 : "";
                    ws.Cells[row, ucvStartCol_Data + 4].Value = result.UngCuVien5 > 0 ? result.UngCuVien5 : "";
                    
                    // For TINH level, add UCV 6 and 7
                    if (levelLower.Contains("tinh"))
                    {
                        ws.Cells[row, ucvStartCol_Data + 5].Value = result.UngCuVien6 > 0 ? result.UngCuVien6 : "";
                        ws.Cells[row, ucvStartCol_Data + 6].Value = result.UngCuVien7 > 0 ? result.UngCuVien7 : "";
                    }
                    
                    // Total column (Cộng) = direct sum of UCV votes without any transformation
                    // For XA/QUOCHOI: sum UCV 1-5 (5 candidates)
                    // For TINH: sum UCV 1-7 (7 candidates)
                    int totalVotes = result.UngCuVien1 + result.UngCuVien2 + result.UngCuVien3 + 
                                    result.UngCuVien4 + result.UngCuVien5;
                    if (levelLower.Contains("tinh"))
                    {
                        totalVotes += result.UngCuVien6 + result.UngCuVien7;
                    }
                    ws.Cells[row, totalCol].Value = totalVotes > 0 ? totalVotes : "";
                    
                    // Apply styling to data row
                    for (int col = 1; col <= maxCol; col++)
                    {
                        ws.Cells[row, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        
                        // Yellow background for UCV columns
                        if (col >= ucvStartCol_Data && col < totalCol)
                        {
                            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(254, 240, 138)); // #fef08a
                        }
                        
                        // Yellow for total column
                        if (col == totalCol)
                        {
                            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(254, 240, 138)); // #fef08a
                        }
                    }
                    
                    row++;
                    Console.WriteLine($"[DEBUG] Populated row {row - 1} with data from result STT={result.Stt}");
                }
                
                // If no results, create empty template rows
                if (results.Count == 0)
                {
                    // Create 3 empty rows for manual entry
                    for (int emptyRow = 0; emptyRow < 3; emptyRow++)
                    {
                        for (int col = 1; col <= maxCol; col++)
                        {
                            ws.Cells[row, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }
                        row++;
                    }
                }
                
                // Dots row
                ws.Cells[row, 1].Value = "...";
                ws.Cells[row, 2].Value = "...";
                for (int col = 1; col <= maxCol; col++)
                {
                    ws.Cells[row, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }
                row++;

                // Dots row
                ws.Cells[row, 1].Value = "...";
                ws.Cells[row, 2].Value = "...";
                for (int col = 1; col <= maxCol; col++)
                {
                    ws.Cells[row, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }
                row++;

                // TOTAL ROW
                ws.Cells[row, 1].Value = "Tổng";
                
                // Populate footer data from array if provided
                if (footerData != null && footerData.Count > 0)
                {
                    Console.WriteLine($"[DEBUG EXCEL] Populating row {row} (TOTAL ROW) with {footerData.Count} footer values");
                    for (int col = 1; col <= Math.Min(footerData.Count, maxCol); col++)
                    {
                        ws.Cells[row, col].Value = footerData[col - 1]; // footerData[0] -> column 1, footerData[1] -> column 2, etc.
                        Console.WriteLine($"[DEBUG EXCEL] Row {row}, Col {col}: {footerData[col - 1]}");
                    }
                    Console.WriteLine($"[DEBUG EXCEL] Completed footer data population for row {row}");
                }
                
                // Totals for UCV columns
                if (ucvCount == 5)
                {
                }
                else if (ucvCount == 7)
                {
                }

                ws.Cells[row, 1].Style.Font.Bold = true;
                for (int col = 1; col <= maxCol; col++)
                {
                    ws.Cells[row, col].Style.Font.Bold = true;
                    ws.Cells[row, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    ws.Cells[row, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }
                
                int dataEndRow = row; // Save the end row of data table
                
                // === ADD BORDER FOR DATA ROWS (XA/QUOCHOI LEVELS ONLY) ===
                if (!levelLower.Contains("tinh"))
                {
                    // Add border to entire data table range for XA/QUOCHOI levels
                    var dataTableRange = ws.Cells[$"A{level2Row + 1}:T{dataEndRow}"];
                    dataTableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    dataTableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    dataTableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    dataTableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    dataTableRange.Style.Border.DiagonalDown = false;
                    Console.WriteLine($"[DEBUG] Added borders to data table: A{level2Row + 1}:T{dataEndRow} for XA/QUOCHOI levels");
                    
                    // Add borders to U column data rows (U9, U10, U11, U12)
                    ws.Cells[9, 21].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);  // U9
                    ws.Cells[10, 21].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // U10
                    ws.Cells[11, 21].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // U11
                    ws.Cells[12, 21].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // U12
                    Console.WriteLine($"[DEBUG] Added borders to U9:U12 using BorderAround for XA/QUOCHOI levels");
                }
                
                row += 2;

                // === VERIFICATION SECTION ===
                ws.Cells[row, 1].Value = "KIỂM TRA TẠI TỔ BẦU CỬ";
                ws.Cells[row, 1].Style.Font.Bold = true;
                ws.Cells[row, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(198, 239, 206));
                row++;

                ws.Cells[row, 1].Value = "Kiểm tra lúc ..... giờ ...... ngày ...... tháng .... năm 2026 và đã nhị";
                row++;
                ws.Cells[row, 1].Value = "Biên bản kết quả kiểm phiếu của Tổ bầu cử .............................";
                row++;
                ws.Cells[row, 1].Value = "Biên bản kiểm kế việc sử dụng phiếu bầu cử .............................";
                row += 1;

                // === CALCULATION SECTION ===
                int calcRow = row;
                calcRow++;

                ws.Cells[calcRow, 8].Value = "Số phiếu bầu 04 đại biểu:";
                ws.Cells[calcRow, 10].Value = "x";
                ws.Cells[calcRow, 11].Value = 4;
                ws.Cells[calcRow, 12].Value = "=";
                calcRow++;

                ws.Cells[calcRow, 8].Value = "Số phiếu bầu 03 đại biểu:";
                ws.Cells[calcRow, 10].Value = "x";
                ws.Cells[calcRow, 11].Value = 3;
                ws.Cells[calcRow, 12].Value = "=";
                calcRow++;

                ws.Cells[calcRow, 8].Value = "Số phiếu bầu 02 đại biểu:";
                ws.Cells[calcRow, 10].Value = "x";
                ws.Cells[calcRow, 11].Value = 2;
                ws.Cells[calcRow, 12].Value = "=";
                calcRow++;

                ws.Cells[calcRow, 8].Value = "Số phiếu bầu 01 đại biểu:";
                ws.Cells[calcRow, 10].Value = "x";
                ws.Cells[calcRow, 11].Value = 1;
                ws.Cells[calcRow, 12].Value = "=";
                calcRow++;

                ws.Cells[calcRow, 11].Value = "Cộng:";
                ws.Cells[calcRow, 11].Style.Font.Bold = true;
                ws.Cells[calcRow, 13].Style.Font.Bold = true;
                calcRow += 2;

                // === SIGNATURE SECTION ===
                ws.Cells[calcRow, 2].Value = "NGƯỜI KIỂM TRA";
                ws.Cells[calcRow, 2].Style.Font.Bold = true;
                ws.Cells[calcRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[calcRow, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                ws.Cells[calcRow, 8].Value = "TỔ TRƯỞNG TỔ BẦU CỬ SỐ ......";
                ws.Cells[calcRow, 8].Style.Font.Bold = true;
                ws.Cells[calcRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[calcRow, 8].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                calcRow += 3;

                ws.Cells[calcRow, 2].Value = "(Ký, ghi họ tên)";
                ws.Cells[calcRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                ws.Cells[calcRow, 8].Value = "(Ký và ghi họ tên, đông dấu)";
                ws.Cells[calcRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // === SET COLUMN WIDTHS ===
                ws.Column(1).Width = 14;
                ws.Column(2).Width = 14;
                ws.Column(3).Width = 15;
                ws.Column(4).Width = 20;
                ws.Column(5).Width = 18;
                ws.Column(6).Width = 12;
                ws.Column(7).Width = 15;
                ws.Column(8).Width = 15;
                for (int i = 9; i <= maxCol; i++)
                {
                    ws.Column(i).Width = 13;
                }

                // Clear specific cells based on election level
                Console.WriteLine($"[DEBUG] Before clearing - Original level: '{level}'");
                Console.WriteLine($"[DEBUG] Before clearing - levelLower: '{levelLower}'");

                if (!levelLower.Contains("tinh"))
                {
                    Console.WriteLine("[DEBUG] ===== MATCHED: XÃ/QUỐC HỘI LEVEL - SHIFTING COLUMNS =====");
                    // Set level2Row headers for XÃ/QUỐC HỘI level (row 8)
                    ws.Cells[level2Row, 13].Value = "Bầu 03 đại biểu"; // Column M
                    ws.Cells[level2Row, 14].Value = "Bầu 02 đại biểu"; // Column N
                    ws.Cells[level2Row, 15].Value = "Bầu 01 đại biểu"; // Column O
                    
                    // DO NOT shift header row - UCV columns (P-T) will have "UCV 1-5" headers
                    // DO NOT shift data rows - UCV data already correctly placed in P-T columns
                    
                    // REMOVED: Code that was shifting P←Q, Q←R, etc. which overwrote UCV data
                    // REMOVED: Code that was shifting X→U→T columns
                    
                    // Do NOT unmerge or shift U7:U8 - it's already correctly set
                    // U7 contains "Tổng số phiếu bầu của các ứng cử viên" (set at line 1256)
                    
                    // Clear V8:V12 and W8:W12 for XA/QUOCHOI level
                    ws.Cells[$"V{level2Row}:W{level2Row + 4}"].Clear();
                    Console.WriteLine($"[DEBUG] Cleared cells V{level2Row}:W{level2Row + 4} for XA/QUOCHOI level");
                    
                    // Add UCV headers to P8-T8 for XA/QUOCHOI level
                    ws.Cells[level2Row, 16].Value = "UCV 1";  // P8
                    ws.Cells[level2Row, 17].Value = "UCV 2";  // Q8
                    ws.Cells[level2Row, 18].Value = "UCV 3";  // R8
                    ws.Cells[level2Row, 19].Value = "UCV 4";  // S8
                    ws.Cells[level2Row, 20].Value = "UCV 5";  // T8
                    Console.WriteLine("[DEBUG] Added UCV1-5 headers to P8-T8 for XA/QUOCHOI level");
                    
                    // Add borders to UCV cells (P8:T12) at XA/QUOCHOI level
                    var ucvRange = ws.Cells[level2Row, 16, level2Row + 4, 20]; // P8:T12
                    ucvRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ucvRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ucvRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ucvRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ucvRange.Style.Border.DiagonalDown = false;
                    Console.WriteLine("[DEBUG] Fixed UCV columns for XÃ/QUỐC HỘI level - DATA NOT SHIFTED");
                }
                else if (levelLower.Contains("tinh"))
                {
                    Console.WriteLine("[DEBUG] ===== MATCHED: TỈNH (PROVINCIAL) LEVEL - ADDING DATA =====");
                    // Add specific data for TỈNH (provincial) level
                    ws.Cells["M8"].Value = "Bầu 04 đại biểu";
                    ws.Cells["V8"].Value = "UCV 6";
                    ws.Cells["W8"].Value = "UCV 7";
                    Console.WriteLine("[DEBUG] Added: M8='Bầu 04 đại biểu', V8='UCV 6', W8='UCV 7'");
                }

                return package.GetAsByteArray();
            }
        }
    }
}

