using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Lab6Part3App.BUS;
using Lab6Part3App.DTO;

namespace Lab6Part3App.GUI
{
    public class frmClinicMain : Form
    {
        private ClinicBUS bus = new ClinicBUS();
        private List<ChiTietDonThuocDTO> donList = new List<ChiTietDonThuocDTO>();

        private ComboBox cboHoSo = null!, cboThuoc = null!;
        private NumericUpDown numSL = null!;
        private TextBox txtLieuDung = null!;
        private DataGridView dgvDon = null!;
        private Label lblTongTien = null!;

        public frmClinicMain()
        {
            this.Text = "Bài 6.7: Quản lý Phòng khám & Kê đơn thuốc (Transaction ACID)";
            this.Size = new Size(1100, 620);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel pnlLeft = new Panel { Dock = DockStyle.Left, Width = 380, Padding = new Padding(15) };
            Label l1 = new Label { Text = "Hồ sơ bệnh nhân khám:", Location = new Point(15, 15), AutoSize = true };
            cboHoSo = new ComboBox { Location = new Point(15, 38), Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };

            Label l2 = new Label { Text = "Chọn thuốc kê đơn:", Location = new Point(15, 80), AutoSize = true };
            cboThuoc = new ComboBox { Location = new Point(15, 103), Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };

            Label l3 = new Label { Text = "Số lượng xuất:", Location = new Point(15, 145), AutoSize = true };
            numSL = new NumericUpDown { Location = new Point(15, 168), Width = 100, Minimum = 1, Maximum = 1000, Value = 1 };

            Label l4 = new Label { Text = "Liều dùng hướng dẫn:", Location = new Point(15, 210), AutoSize = true };
            txtLieuDung = new TextBox { Location = new Point(15, 233), Width = 340, Text = "Sáng 1 viên, Chiều 1 viên sau ăn" };

            Button btnAdd = new Button { Text = "Thêm Thuốc vào Đơn", Location = new Point(15, 280), Width = 340, Height = 38, BackColor = Color.LightSkyBlue, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;

            pnlLeft.Controls.AddRange(new Control[] { l1, cboHoSo, l2, cboThuoc, l3, numSL, l4, txtLieuDung, btnAdd });

            Panel pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            dgvDon = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White };

            Panel pnlBot = new Panel { Dock = DockStyle.Bottom, Height = 70 };
            lblTongTien = new Label { Text = "Tổng viện phí (Gồm 150k phí khám): 150,000 VNĐ", Location = new Point(15, 20), AutoSize = true, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.DarkRed };
            Button btnSave = new Button { Text = "LƯU & XUẤT ĐƠN (ACID)", Location = new Point(420, 12), Size = new Size(220, 45), BackColor = Color.ForestGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;

            pnlBot.Controls.AddRange(new Control[] { lblTongTien, btnSave });
            pnlRight.Controls.Add(dgvDon);
            pnlRight.Controls.Add(pnlBot);

            this.Controls.Add(pnlRight);
            this.Controls.Add(pnlLeft);

            LoadData();
        }

        private void LoadData()
        {
            cboHoSo.DataSource = bus.LayHoSo();
            cboHoSo.DisplayMember = "ThongTinKham";
            cboHoSo.ValueMember = "MaHoSo";

            cboThuoc.DataSource = bus.LayThuoc();
            cboThuoc.DisplayMember = "TenThuoc";
            cboThuoc.ValueMember = "MaThuoc";
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (cboThuoc.SelectedItem is DataRowView row)
            {
                donList.Add(new ChiTietDonThuocDTO
                {
                    MaThuoc = Convert.ToInt32(row["MaThuoc"]),
                    TenThuoc = row["TenThuoc"].ToString()!,
                    SoLuong = (int)numSL.Value,
                    DonGia = Convert.ToDecimal(row["DonGia"]),
                    LieuDung = txtLieuDung.Text
                });
                CapNhatDon();
            }
        }

        private void CapNhatDon()
        {
            dgvDon.DataSource = null;
            dgvDon.DataSource = donList;
            decimal tong = 150000;
            foreach (var item in donList) tong += item.ThanhTien;
            lblTongTien.Text = $"Tổng viện phí (Gồm 150k phí khám): {tong:N0} VNĐ";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            decimal tong = 150000;
            foreach (var item in donList) tong += item.ThanhTien;

            var don = new DonThuocDTO
            {
                MaHoSo = Convert.ToInt32(cboHoSo.SelectedValue),
                TienKham = 150000,
                TongTien = tong,
                ChiTiet = new List<ChiTietDonThuocDTO>(donList)
            };

            var res = bus.KeDonThuoc(don);
            if (res.Success)
            {
                MessageBox.Show(res.Message, "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                donList.Clear();
                CapNhatDon();
                LoadData();
            }
            else
            {
                MessageBox.Show(res.Message, "Lỗi Tồn Kho (Đã Rollback)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}