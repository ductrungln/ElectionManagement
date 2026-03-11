using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
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
    }
}
