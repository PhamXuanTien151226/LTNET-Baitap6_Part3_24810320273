using System;
using System.Data;
using MySql.Data.MySqlClient;
using Lab6Part3App.DTO;

namespace Lab6Part3App.DAL
{
    public class ClinicDAL
    {
        public DataTable GetHoSoKham()
        {
            using var conn = DatabaseHelper.GetClinicConnection();
            string sql = @"SELECT hs.MaHoSo, CONCAT(bn.HoTen, ' - ', hs.ChanDoan) AS ThongTinKham 
                           FROM HoSoKham hs JOIN BenhNhan bn ON hs.MaBN = bn.MaBN";
            var da = new MySqlDataAdapter(sql, conn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public DataTable GetDanhMucThuoc()
        {
            using var conn = DatabaseHelper.GetClinicConnection();
            var da = new MySqlDataAdapter("SELECT MaThuoc, TenThuoc, DonViTinh, DonGia, SoLuongTon FROM Thuoc", conn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public (bool Success, string Message) LuuVaXuatDonThuoc(DonThuocDTO don)
        {
            using var conn = DatabaseHelper.GetClinicConnection();
            conn.Open();
            var trans = conn.BeginTransaction();
            try
            {
                string sqlDon = "INSERT INTO DonThuoc (MaHoSo, TienKham, TongTien, TrangThai) VALUES (@MaHS, @TK, @Tong, 'Đã thanh toán'); SELECT LAST_INSERT_ID();";
                var cmdD = new MySqlCommand(sqlDon, conn, trans);
                cmdD.Parameters.AddWithValue("@MaHS", don.MaHoSo);
                cmdD.Parameters.AddWithValue("@TK", don.TienKham);
                cmdD.Parameters.AddWithValue("@Tong", don.TongTien);
                int maDon = Convert.ToInt32(cmdD.ExecuteScalar());

                foreach (var item in don.ChiTiet)
                {
                    var cmdCheck = new MySqlCommand("SELECT SoLuongTon FROM Thuoc WHERE MaThuoc = @MaT FOR UPDATE", conn, trans);
                    cmdCheck.Parameters.AddWithValue("@MaT", item.MaThuoc);
                    int ton = Convert.ToInt32(cmdCheck.ExecuteScalar());

                    if (ton < item.SoLuong)
                    {
                        trans.Rollback();
                        return (false, $"Thuốc [{item.TenThuoc}] chỉ còn tồn {ton}, không đủ xuất {item.SoLuong}!");
                    }

                    var cmdTru = new MySqlCommand("UPDATE Thuoc SET SoLuongTon = SoLuongTon - @SL WHERE MaThuoc = @MaT", conn, trans);
                    cmdTru.Parameters.AddWithValue("@SL", item.SoLuong);
                    cmdTru.Parameters.AddWithValue("@MaT", item.MaThuoc);
                    cmdTru.ExecuteNonQuery();

                    var cmdCT = new MySqlCommand("INSERT INTO ChiTietDonThuoc (MaDon, MaThuoc, SoLuong, LieuDung) VALUES (@MaD, @MaT, @SL, @LD)", conn, trans);
                    cmdCT.Parameters.AddWithValue("@MaD", maDon);
                    cmdCT.Parameters.AddWithValue("@MaT", item.MaThuoc);
                    cmdCT.Parameters.AddWithValue("@SL", item.SoLuong);
                    cmdCT.Parameters.AddWithValue("@LD", item.LieuDung);
                    cmdCT.ExecuteNonQuery();
                }

                trans.Commit();
                return (true, $"Xuất đơn thuốc thành công! Mã đơn: DT-{maDon:D4}\nTổng viện phí: {don.TongTien:N0} VNĐ");
            }
            catch (Exception ex)
            {
                try { trans.Rollback(); } catch { }
                return (false, "Lỗi Transaction: " + ex.Message);
            }
        }
    }
}