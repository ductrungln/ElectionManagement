import sqlite3
from datetime import datetime

try:
    conn = sqlite3.connect('ElectionManagement.db')
    c = conn.cursor()
    
    now = datetime.now().isoformat()
    
    # Insert test data - 22 values for 22 columns
    # Note: "To" is quoted as [To] because it's a reserved keyword
    sql = 'INSERT INTO ElectionResults (Stt, Level, KhuVuc, TongCuTri, "To", PhieuPhatRa, PhieuThuVe, PhieuHopLe, PhieuKhongHopLe, PhieuBau04, PhieuBau03, PhieuBau02, PhieuBau01, UngCuVien1, UngCuVien2, UngCuVien3, UngCuVien4, UngCuVien5, UngCuVien6, UngCuVien7, UngCuVien8, CreatedAt) VALUES (1, "xa", "Khu vuc 1", 500, "1", 500, 450, 445, 5, 50, 60, 70, 80, 80, 75, 70, 65, 60, 55, 50, 0, ?)'
    c.execute(sql, (now,))
    
    conn.commit()
    print('Test data inserted successfully')
    conn.close()
except Exception as e:
    print(f'Error: {str(e)}')
