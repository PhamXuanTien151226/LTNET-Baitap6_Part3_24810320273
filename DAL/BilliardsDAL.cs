using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Lab6Part3App.DAL
{
    public class BilliardsDAL
    {
        public DataTable GetBanBida()
        {
            using var conn = DatabaseHelper.GetBilliardsConnection();
            var da = new MySqlDataAdapter("SELECT MaBan, TenBan, LoaiBan, GiaTheoGio, TrangThai FROM BanBida", conn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetMenuDichVu()
        {
            using var conn = DatabaseHelper.GetBilliardsConnection();
            var da = new MySqlDataAdapter("SELECT MaDV, TenDV, DonGia, TonKho FROM MenuDichVu", conn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetChiTietDichVuBan(int maBan)
        {
            using var conn = DatabaseHelper.GetBilliardsConnection();
            string sql = @"SELECT dv.TenDV, ct.SoLuong, ct.DonGia, (ct.SoLuong * ct.DonGia) AS ThanhTien
                           FROM ChiTietHoaDon ct 
                           JOIN HoaDon hd ON ct.MaHD = hd.MaHD 
                           JOIN MenuDichVu dv ON ct.MaDV = dv.MaDV 
                           WHERE hd.MaBan = @MaB AND hd.TrangThai = 0";
            var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaB", maBan);
            var da = new MySqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public bool MoBan(int maBan)
        {
            using var conn = DatabaseHelper.GetBilliardsConnection();
            conn.Open();
            var trans = conn.BeginTransaction();
            try
            {
                var cmd1 = new MySqlCommand("UPDATE BanBida SET TrangThai = 'Đang chơi' WHERE MaBan = @MaB AND TrangThai = 'Trống'", conn, trans);
                cmd1.Parameters.AddWithValue("@MaB", maBan);
                if (cmd1.ExecuteNonQuery() == 0) { trans.Rollback(); return false; }

                var cmd2 = new MySqlCommand("INSERT INTO HoaDon (MaBan, GioBatDau, TrangThai) VALUES (@MaB, NOW(), 0)", conn, trans);
                cmd2.Parameters.AddWithValue("@MaB", maBan);
                cmd2.ExecuteNonQuery();

                trans.Commit();
                return true;
            }
            catch { trans.Rollback(); return false; }
        }

        public bool GoiThemMon(int maBan, int maDV, int soLuong)
        {
            using var conn = DatabaseHelper.GetBilliardsConnection();
            conn.Open();
            var trans = conn.BeginTransaction();
            try
            {
                var cmdHD = new MySqlCommand("SELECT MaHD FROM HoaDon WHERE MaBan = @MaB AND TrangThai = 0 LIMIT 1", conn, trans);
                cmdHD.Parameters.AddWithValue("@MaB", maBan);
                object? resHD = cmdHD.ExecuteScalar();
                if (resHD == null) { trans.Rollback(); return false; }
                int maHD = Convert.ToInt32(resHD);

                var cmdGia = new MySqlCommand("SELECT DonGia FROM MenuDichVu WHERE MaDV = @MaDV", conn, trans);
                cmdGia.Parameters.AddWithValue("@MaDV", maDV);
                decimal gia = Convert.ToDecimal(cmdGia.ExecuteScalar());

                string sqlInsert = @"INSERT INTO ChiTietHoaDon (MaHD, MaDV, SoLuong, DonGia) 
                                     VALUES (@MaHD, @MaDV, @SL, @Gia) 
                                     ON DUPLICATE KEY UPDATE SoLuong = SoLuong + @SL";
                var cmdIns = new MySqlCommand(sqlInsert, conn, trans);
                cmdIns.Parameters.AddWithValue("@MaHD", maHD);
                cmdIns.Parameters.AddWithValue("@MaDV", maDV);
                cmdIns.Parameters.AddWithValue("@SL", soLuong);
                cmdIns.Parameters.AddWithValue("@Gia", gia);
                cmdIns.ExecuteNonQuery();

                trans.Commit();
                return true;
            }
            catch { trans.Rollback(); return false; }
        }

        public (bool Success, string Message) ThanhToanBan(int maBan)
        {
            using var conn = DatabaseHelper.GetBilliardsConnection();
            conn.Open();
            var trans = conn.BeginTransaction();
            try
            {
                string sqlGet = @"SELECT hd.MaHD, hd.GioBatDau, b.GiaTheoGio 
                                  FROM HoaDon hd JOIN BanBida b ON hd.MaBan = b.MaBan 
                                  WHERE hd.MaBan = @MaB AND hd.TrangThai = 0 LIMIT 1";
                var cmdGet = new MySqlCommand(sqlGet, conn, trans);
                cmdGet.Parameters.AddWithValue("@MaB", maBan);

                int maHD = 0; DateTime gioBD = DateTime.Now; decimal giaGio = 0;
                using (var r = cmdGet.ExecuteReader())
                {
                    if (r.Read()) { maHD = r.GetInt32("MaHD"); gioBD = r.GetDateTime("GioBatDau"); giaGio = r.GetDecimal("GiaTheoGio"); }
                }

                if (maHD == 0) { trans.Rollback(); return (false, "Bàn chưa được mở!"); }

                // Tính tổng tiền dịch vụ & trừ kho (trừ thuê gậy)
                string sqlDV = @"SELECT ct.MaDV, ct.SoLuong, ct.DonGia, dv.TenDV 
                                 FROM ChiTietHoaDon ct JOIN MenuDichVu dv ON ct.MaDV = dv.MaDV WHERE ct.MaHD = @MaHD";
                var cmdDV = new MySqlCommand(sqlDV, conn, trans);
                cmdDV.Parameters.AddWithValue("@MaHD", maHD);
                var dtDV = new DataTable();
                using (var r2 = cmdDV.ExecuteReader()) { dtDV.Load(r2); }

                decimal tienDV = 0;
                foreach (DataRow row in dtDV.Rows)
                {
                    int sl = Convert.ToInt32(row["SoLuong"]);
                    decimal dg = Convert.ToDecimal(row["DonGia"]);
                    string ten = row["TenDV"].ToString()!;
                    tienDV += sl * dg;

                    if (!ten.Contains("Thuê gậy"))
                    {
                        var cmdTru = new MySqlCommand("UPDATE MenuDichVu SET TonKho = TonKho - @SL WHERE MaDV = @MaDV", conn, trans);
                        cmdTru.Parameters.AddWithValue("@SL", sl);
                        cmdTru.Parameters.AddWithValue("@MaDV", row["MaDV"]);
                        cmdTru.ExecuteNonQuery();
                    }
                }

                DateTime gioKT = DateTime.Now;
                double soGio = Math.Max(0.25, (gioKT - gioBD).TotalHours);
                decimal tienGio = (decimal)Math.Round(soGio * (double)giaGio, 0);

                var cmdUpHD = new MySqlCommand("UPDATE HoaDon SET GioKetThuc = @KT, TienGioChoi = @TG, TienDichVu = @TDV, TrangThai = 1 WHERE MaHD = @MaHD", conn, trans);
                cmdUpHD.Parameters.AddWithValue("@KT", gioKT);
                cmdUpHD.Parameters.AddWithValue("@TG", tienGio);
                cmdUpHD.Parameters.AddWithValue("@TDV", tienDV);
                cmdUpHD.Parameters.AddWithValue("@MaHD", maHD);
                cmdUpHD.ExecuteNonQuery();

                var cmdUpBan = new MySqlCommand("UPDATE BanBida SET TrangThai = 'Trống' WHERE MaBan = @MaB", conn, trans);
                cmdUpBan.Parameters.AddWithValue("@MaB", maBan);
                cmdUpBan.ExecuteNonQuery();

                trans.Commit();
                return (true, $"Thanh toán thành công!\n- Tiền giờ ({soGio:N2}h): {tienGio:N0} VNĐ\n- Tiền dịch vụ: {tienDV:N0} VNĐ\n- TỔNG CỘNG: {(tienGio + tienDV):N0} VNĐ");
            }
            catch (Exception ex) { trans.Rollback(); return (false, "Lỗi: " + ex.Message); }
        }
    }
}