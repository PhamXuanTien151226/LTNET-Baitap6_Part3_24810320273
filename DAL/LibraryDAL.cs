using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Lab6Part3App.DAL
{
    public class LibraryDAL
    {
        public DataTable GetDocGia()
        {
            using var conn = DatabaseHelper.GetLibConnection();
            var da = new MySqlDataAdapter("SELECT MaDocGia, HoTen, Email, NgayHetHanThe FROM DocGia", conn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetPhieuMuon(int maDocGia, string trangThai)
        {
            using var conn = DatabaseHelper.GetLibConnection();
            string sql = @"SELECT pm.MaPhieu, dg.HoTen, ds.TenSach, ct.SoLuong, pm.NgayMuon, pm.HanTra, pm.TrangThai 
                           FROM PhieuMuon pm 
                           JOIN DocGia dg ON pm.MaDocGia = dg.MaDocGia 
                           JOIN ChiTietMuon ct ON pm.MaPhieu = ct.MaPhieu 
                           JOIN DauSach ds ON ct.MaSach = ds.MaSach WHERE 1=1";
            if (maDocGia > 0) sql += " AND pm.MaDocGia = @MaDG";
            if (trangThai != "Tất cả") sql += " AND pm.TrangThai = @Status";

            var cmd = new MySqlCommand(sql, conn);
            if (maDocGia > 0) cmd.Parameters.AddWithValue("@MaDG", maDocGia);
            if (trangThai != "Tất cả") cmd.Parameters.AddWithValue("@Status", trangThai);

            var da = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        // Trả sách & cộng lại tồn kho (Transaction)
        public bool TraSach(int maPhieu)
        {
            using var conn = DatabaseHelper.GetLibConnection();
            conn.Open();
            var trans = conn.BeginTransaction();
            try
            {
                var cmdUpPM = new MySqlCommand("UPDATE PhieuMuon SET TrangThai = 'Đã trả' WHERE MaPhieu = @MaP AND TrangThai != 'Đã trả'", conn, trans);
                cmdUpPM.Parameters.AddWithValue("@MaP", maPhieu);
                if (cmdUpPM.ExecuteNonQuery() == 0) { trans.Rollback(); return false; }

                // Cộng lại số lượng sách còn
                string sqlCong = @"UPDATE DauSach ds 
                                   JOIN ChiTietMuon ct ON ds.MaSach = ct.MaSach 
                                   SET ds.SoLuongCon = ds.SoLuongCon + ct.SoLuong 
                                   WHERE ct.MaPhieu = @MaP";
                var cmdCong = new MySqlCommand(sqlCong, conn, trans);
                cmdCong.Parameters.AddWithValue("@MaP", maPhieu);
                cmdCong.ExecuteNonQuery();

                trans.Commit();
                return true;
            }
            catch { trans.Rollback(); return false; }
        }

        // SqlDataReader cảnh báo quá hạn
        public DataTable GetPhieuQuaHan()
        {
            using var conn = DatabaseHelper.GetLibConnection();
            conn.Open();
            string sql = @"SELECT pm.MaPhieu, dg.HoTen, pm.HanTra, pm.TrangThai 
                           FROM PhieuMuon pm JOIN DocGia dg ON pm.MaDocGia = dg.MaDocGia 
                           WHERE pm.HanTra < NOW() AND pm.TrangThai != 'Đã trả'";
            var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }
    }
}