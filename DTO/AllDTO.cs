using System;
using System.Collections.Generic;

namespace Lab6Part3App.DTO
{
    // DTO Bài 6.7: Kê đơn thuốc
    public class ChiTietDonThuocDTO
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string LieuDung { get; set; } = string.Empty;
        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class DonThuocDTO
    {
        public int MaHoSo { get; set; }
        public decimal TienKham { get; set; } = 150000;
        public decimal TongTien { get; set; }
        public List<ChiTietDonThuocDTO> ChiTiet { get; set; } = new List<ChiTietDonThuocDTO>();
    }
}