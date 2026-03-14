using System;

namespace ElectionManagement.Models
{
    public class ElectionResult
    {
        public int Id { get; set; }
        public int Stt { get; set; }
        public string Level { get; set; } // which level this record belongs to
        public string KhuVuc { get; set; }
        public int TongCuTri { get; set; }
        public string To { get; set; } = ""; // Tổ (group) - 1 to 10

        public int PhieuPhatRa { get; set; }
        public int PhieuThuVe { get; set; }
        public int PhieuHopLe { get; set; }
        public int PhieuKhongHopLe { get; set; }

        // Ballot classification fields
        public int PhieuBau04 { get; set; }
        public int PhieuBau03 { get; set; }
        public int PhieuBau02 { get; set; }
        public int PhieuBau01 { get; set; }

        public int UngCuVien1 { get; set; }
        public int UngCuVien2 { get; set; }
        public int UngCuVien3 { get; set; }
        public int UngCuVien4 { get; set; }
        public int UngCuVien5 { get; set; }
        public int UngCuVien6 { get; set; }
        public int UngCuVien7 { get; set; }
        public int UngCuVien8 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class ElectionProgress
    {
        public int Id { get; set; }
        public int Stt { get; set; }
        public string Level { get; set; } // new: xa, tinh, quochoi
        public string TenKhuVuc { get; set; }
        public string DonVi { get; set; }
        public int TongCuTri { get; set; }

        public int Gio8 { get; set; }    // 8:00
        public int Gio10 { get; set; }   // 10:00
        public int Gio12 { get; set; }   // 12:00
        public int Gio14 { get; set; }   // 14:00
        public int Gio16 { get; set; }   // 16:00
        public int Gio19 { get; set; }   // 19:00

        // Thêm 2 cột mới cho tiến độ bầu cử
        public int TongCuTriDiBau { get; set; } = 0; // Tổng số cử tri đi bầu
        public decimal TiLe { get; set; } = 0; // Tỉ lệ (%) = TongCuTriDiBau / TongCuTri * 100

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class ImportLog
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ImportType { get; set; }
        public string Level { get; set; }
        public int TotalRows { get; set; }
        public int SuccessRows { get; set; }
        public int ErrorRows { get; set; }
        public string Errors { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
