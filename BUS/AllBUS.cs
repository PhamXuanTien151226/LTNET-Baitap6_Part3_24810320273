using System.Data;
using Lab6Part3App.DAL;
using Lab6Part3App.DTO;

namespace Lab6Part3App.BUS
{
    public class LibraryBUS
    {
        private LibraryDAL dal = new LibraryDAL();
        public DataTable LayDocGia() => dal.GetDocGia();
        public DataTable LocPhieuMuon(int maDG, string status) => dal.GetPhieuMuon(maDG, status);
        public bool XuLyTraSach(int maPhieu) => dal.TraSach(maPhieu);
        public DataTable LayQuaHan() => dal.GetPhieuQuaHan();
    }

    public class ClinicBUS
    {
        private ClinicDAL dal = new ClinicDAL();
        public DataTable LayHoSo() => dal.GetHoSoKham();
        public DataTable LayThuoc() => dal.GetDanhMucThuoc();
        public (bool Success, string Message) KeDonThuoc(DonThuocDTO don)
        {
            if (don.ChiTiet.Count == 0) return (false, "Vui lòng chọn ít nhất 1 loại thuốc vào đơn!");
            return dal.LuuVaXuatDonThuoc(don);
        }
    }

    public class BilliardsBUS
    {
        private BilliardsDAL dal = new BilliardsDAL();
        public DataTable LayBan() => dal.GetBanBida();
        public DataTable LayMenu() => dal.GetMenuDichVu();
        public DataTable LayDichVuBan(int maBan) => dal.GetChiTietDichVuBan(maBan);
        public bool MoBanChoi(int maBan) => dal.MoBan(maBan);
        public bool ThemDichVu(int maBan, int maDV, int sl) => dal.GoiThemMon(maBan, maDV, sl);
        public (bool Success, string Message) ThanhToan(int maBan) => dal.ThanhToanBan(maBan);
    }
}