using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Lab6Part3App.BUS;

namespace Lab6Part3App.GUI
{
    public class frmBilliardsMain : Form
    {
        private BilliardsBUS bus = new BilliardsBUS();
        private FlowLayoutPanel pnlTables = null!;
        private DataGridView dgvOrder = null!;
        private ComboBox cboMenu = null!;
        private NumericUpDown numSL = null!;
        private Label lblBanChon = null!;
        private int selectedMaBan = 0;

        public frmBilliardsMain()
        {
            this.Text = "Bài 6.8: Hệ thống Quản lý Bàn Bida & Dịch vụ Giải trí";
            this.Size = new Size(1100, 620);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Sơ đồ bàn bên trái
            GroupBox grpLeft = new GroupBox { Text = "Sơ đồ Bàn Bida trực quan", Dock = DockStyle.Left, Width = 520, Padding = new Padding(10) };
            pnlTables = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            grpLeft.Controls.Add(pnlTables);

            // Gọi món & Thanh toán bên phải
            GroupBox grpRight = new GroupBox { Text = "Quản lý Giờ chơi & Gọi đồ (Disconnected Model)", Dock = DockStyle.Fill, Padding = new Padding(10) };
            Panel pnlActions = new Panel { Dock = DockStyle.Top, Height = 135 };

            lblBanChon = new Label { Text = "Đang chọn: Chưa chọn bàn", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.DarkBlue };
            Button btnMoBan = new Button { Text = "MỞ BÀN CHƠI", Location = new Point(15, 45), Size = new Size(150, 35), BackColor = Color.LightSkyBlue, FlatStyle = FlatStyle.Flat };
            Button btnThanhToan = new Button { Text = "THANH TOÁN & TÍNH GIỜ", Location = new Point(175, 45), Size = new Size(210, 35), BackColor = Color.LightCoral, Font = new Font("Segoe UI", 9F, FontStyle.Bold), FlatStyle = FlatStyle.Flat };

            cboMenu = new ComboBox { Location = new Point(15, 92), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            numSL = new NumericUpDown { Location = new Point(245, 93), Width = 60, Minimum = 1, Maximum = 50, Value = 1 };
            Button btnGoiMon = new Button { Text = "Gọi Đồ", Location = new Point(315, 90), Size = new Size(90, 30), BackColor = Color.LightYellow };

            btnMoBan.Click += (s, e) => { if (selectedMaBan > 0 && bus.MoBanChoi(selectedMaBan)) { RenderTables(); LoadOrders(); } };
            btnGoiMon.Click += (s, e) => { if (selectedMaBan > 0 && cboMenu.SelectedValue != null && bus.ThemDichVu(selectedMaBan, Convert.ToInt32(cboMenu.SelectedValue), (int)numSL.Value)) LoadOrders(); };
            btnThanhToan.Click += (s, e) =>
            {
                if (selectedMaBan == 0) return;
                var res = bus.ThanhToan(selectedMaBan);
                MessageBox.Show(res.Message, res.Success ? "Hóa đơn Bida" : "Cảnh báo");
                if (res.Success) { RenderTables(); dgvOrder.DataSource = null; }
            };

            pnlActions.Controls.AddRange(new Control[] { lblBanChon, btnMoBan, btnThanhToan, cboMenu, numSL, btnGoiMon });

            dgvOrder = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White };

            grpRight.Controls.Add(dgvOrder);
            grpRight.Controls.Add(pnlActions);

            this.Controls.Add(grpRight);
            this.Controls.Add(grpLeft);

            LoadMenu();
            RenderTables();
        }

        private void LoadMenu()
        {
            cboMenu.DataSource = bus.LayMenu();
            cboMenu.DisplayMember = "TenDV";
            cboMenu.ValueMember = "MaDV";
        }

        private void RenderTables()
        {
            pnlTables.Controls.Clear();
            DataTable dt = bus.LayBan();
            foreach (DataRow r in dt.Rows)
            {
                int maB = Convert.ToInt32(r["MaBan"]);
                string tenB = r["TenBan"].ToString()!;
                string tt = r["TrangThai"].ToString()!;
                decimal gia = Convert.ToDecimal(r["GiaTheoGio"]);

                Button btn = new Button
                {
                    Text = $"{tenB}\n[{tt}]\n{gia:N0} đ/h",
                    Size = new Size(145, 105),
                    Margin = new Padding(10),
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    BackColor = tt == "Trống" ? Color.LightGreen : Color.Orange,
                    FlatStyle = FlatStyle.Flat
                };

                btn.Click += (s, e) =>
                {
                    selectedMaBan = maB;
                    lblBanChon.Text = $"Đang chọn: {tenB} ({tt})";
                    LoadOrders();
                };
                pnlTables.Controls.Add(btn);
            }
        }

        private void LoadOrders()
        {
            if (selectedMaBan > 0) dgvOrder.DataSource = bus.LayDichVuBan(selectedMaBan);
        }
    }
}