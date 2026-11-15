using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Lab6Part3App.BUS;

namespace Lab6Part3App.GUI
{
    public class frmLibraryMain : Form
    {
        private LibraryBUS bus = new LibraryBUS();
        private ComboBox cboDocGia = null!, cboTrangThai = null!;
        private DataGridView dgvPhieu = null!;

        public frmLibraryMain()
        {
            this.Text = "Bài 6.6: Hệ thống Quản lý Mượn Trả Sách Thư viện";
            this.Size = new Size(1050, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };
            Label l1 = new Label { Text = "Độc giả:", Location = new Point(15, 18), AutoSize = true };
            cboDocGia = new ComboBox { Location = new Point(75, 15), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            Label l2 = new Label { Text = "Trạng thái:", Location = new Point(290, 18), AutoSize = true };
            cboTrangThai = new ComboBox { Location = new Point(365, 15), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Đang mượn", "Đã trả", "Quá hạn" });
            cboTrangThai.SelectedIndex = 0;

            Button btnLoc = new Button { Text = "Lọc Phiếu", Location = new Point(510, 13), Width = 95, Height = 30 };
            Button btnTra = new Button { Text = "Xác nhận Trả Sách", Location = new Point(615, 13), Width = 160, Height = 30, BackColor = Color.LightGreen, FlatStyle = FlatStyle.Flat };
            Button btnQuaHan = new Button { Text = "Cảnh báo Quá hạn", Location = new Point(785, 13), Width = 150, Height = 30, BackColor = Color.LightCoral, FlatStyle = FlatStyle.Flat };

            btnLoc.Click += (s, e) => LoadPhieu();
            btnTra.Click += BtnTra_Click;
            btnQuaHan.Click += BtnQuaHan_Click;

            pnlTop.Controls.AddRange(new Control[] { l1, cboDocGia, l2, cboTrangThai, btnLoc, btnTra, btnQuaHan });

            dgvPhieu = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White };
            this.Controls.Add(dgvPhieu);
            this.Controls.Add(pnlTop);

            LoadDocGia();
        }

        private void LoadDocGia()
        {
            DataTable dt = bus.LayDocGia();
            DataRow allRow = dt.NewRow();
            allRow["MaDocGia"] = 0;
            allRow["HoTen"] = "-- Tất cả Độc giả --";
            dt.Rows.InsertAt(allRow, 0);

            cboDocGia.DataSource = dt;
            cboDocGia.DisplayMember = "HoTen";
            cboDocGia.ValueMember = "MaDocGia";
            LoadPhieu();
        }

        private void LoadPhieu()
        {
            int maDG = cboDocGia.SelectedValue != null ? Convert.ToInt32(cboDocGia.SelectedValue) : 0;
            string tt = cboTrangThai.SelectedItem?.ToString() ?? "Tất cả";
            dgvPhieu.DataSource = bus.LocPhieuMuon(maDG, tt);
        }

        private void BtnTra_Click(object? sender, EventArgs e)
        {
            if (dgvPhieu.CurrentRow?.DataBoundItem is DataRowView row)
            {
                int maP = Convert.ToInt32(row["MaPhieu"]);
                if (bus.XuLyTraSach(maP))
                {
                    MessageBox.Show($"Đã trả sách cho phiếu #{maP} và cộng lại tồn kho!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPhieu();
                }
                else
                {
                    MessageBox.Show("Phiếu này đã được trả trước đó!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void BtnQuaHan_Click(object? sender, EventArgs e)
        {
            DataTable dt = bus.LayQuaHan();
            string msg = "=== DANH SÁCH PHIẾU MƯỢN SÁCH QUÁ HẠN ===\n\n";
            foreach (DataRow r in dt.Rows)
            {
                msg += $"- Phiếu #{r["MaPhieu"]} | Độc giả: {r["HoTen"]} | Hạn trả: {Convert.ToDateTime(r["HanTra"]):dd/MM/yyyy}\n";
            }
            if (dt.Rows.Count == 0) msg += "Không có độc giả nào trễ hạn trả sách.";
            MessageBox.Show(msg, "Cảnh báo Thư viện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}