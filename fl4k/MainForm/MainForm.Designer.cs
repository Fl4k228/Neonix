using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace fl4k
{
    partial class MainForm
    {
        private IContainer components;
        private Panel leftPanel = null!;
        private Panel headerPanel = null!;
        private Panel contentPanel = null!;
        private MainRoundedPanel choiceCard = null!;
        private MainMenuButton btnDichotomy = null!;
        private MainMenuButton btnGoldenSection = null!;
        private MainMenuButton btnNeonix = null!;
        private MainMenuButton btnExit = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();

            leftPanel = new Panel();
            headerPanel = new Panel();
            contentPanel = new Panel();
            choiceCard = new MainRoundedPanel();

            btnDichotomy = new MainMenuButton();
            btnGoldenSection = new MainMenuButton();
            btnNeonix = new MainMenuButton();
            btnExit = new MainMenuButton();

            var brand = new Label();
            var separator = new Panel();
            var menuCaption = new Label();
            var headerTitle = new Label();
            var headerSubtitle = new Label();
            var cardTitle = new Label();
            var cardSubtitle = new Label();

            SuspendLayout();

            BackColor = Color.FromArgb(14, 14, 14);
            ForeColor = Color.FromArgb(245, 245, 245);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ClientSize = new Size(1280, 760);
            Name = "MainForm";
            Text = "NEONIX — Лабораторные работы";

            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = 260;
            leftPanel.BackColor = Color.FromArgb(18, 18, 18);

            brand.Text = "NEONIX";
            brand.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            brand.ForeColor = Color.White;
            brand.BackColor = Color.Transparent;
            brand.TextAlign = ContentAlignment.MiddleCenter;
            brand.SetBounds(0, 20, 260, 40);

            separator.BackColor = Color.FromArgb(62, 62, 62);
            separator.SetBounds(30, 78, 200, 1);

            menuCaption.Text = "ГЛАВНОЕ МЕНЮ";
            menuCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            menuCaption.ForeColor = Color.FromArgb(205, 205, 205);
            menuCaption.BackColor = Color.Transparent;
            menuCaption.AutoSize = true;
            menuCaption.Location = new Point(30, 98);

            leftPanel.Controls.Add(brand);
            leftPanel.Controls.Add(separator);
            leftPanel.Controls.Add(menuCaption);

            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 78;
            headerPanel.BackColor = Color.FromArgb(22, 22, 22);

            headerTitle.Text = "ЛАБОРАТОРНЫЕ РАБОТЫ";
            headerTitle.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
            headerTitle.ForeColor = Color.FromArgb(245, 245, 245);
            headerTitle.BackColor = Color.Transparent;
            headerTitle.AutoSize = true;
            headerTitle.Location = new Point(22, 8);

            headerSubtitle.Text = "Выберите работу для запуска";
            headerSubtitle.Font = new Font("Segoe UI", 9F);
            headerSubtitle.ForeColor = Color.FromArgb(155, 155, 155);
            headerSubtitle.BackColor = Color.Transparent;
            headerSubtitle.AutoSize = true;
            headerSubtitle.Location = new Point(25, 50);

            headerPanel.Controls.Add(headerTitle);
            headerPanel.Controls.Add(headerSubtitle);

            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(14, 14, 14);

            choiceCard.Size = new Size(660, 430);
            choiceCard.BackColor = Color.FromArgb(30, 30, 30);

            cardTitle.Text = "ВЫБЕРИТЕ ЛАБОРАТОРНУЮ РАБОТУ";
            cardTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            cardTitle.ForeColor = Color.White;
            cardTitle.BackColor = Color.Transparent;
            cardTitle.TextAlign = ContentAlignment.MiddleCenter;
            cardTitle.SetBounds(30, 28, 600, 38);

            cardSubtitle.Text = "После выхода из лабораторной работы вы вернётесь в это меню.";
            cardSubtitle.Font = new Font("Segoe UI", 9F);
            cardSubtitle.ForeColor = Color.FromArgb(155, 155, 155);
            cardSubtitle.BackColor = Color.Transparent;
            cardSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            cardSubtitle.SetBounds(30, 66, 600, 28);

            btnDichotomy.Text = "ДИХОТОМИЯ";
            btnDichotomy.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnDichotomy.ForeColor = Color.White;
            btnDichotomy.BackColor = Color.FromArgb(38, 38, 38);
            btnDichotomy.SetBounds(80, 192, 500, 56);
            btnDichotomy.Click += btnDichotomy_Click;

            btnGoldenSection.Text = "ЗОЛОТОЕ СЕЧЕНИЕ";
            btnGoldenSection.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnGoldenSection.ForeColor = Color.White;
            btnGoldenSection.BackColor = Color.FromArgb(38, 38, 38);
            btnGoldenSection.SetBounds(80, 262, 500, 56);
            btnGoldenSection.Click += btnGoldenSection_Click;

            btnNeonix.Text = "ВИЗУАЛИЗАЦИЯ СОРТИРОВКИ";
            btnNeonix.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnNeonix.ForeColor = Color.White;
            btnNeonix.BackColor = Color.FromArgb(38, 38, 38);
            btnNeonix.SetBounds(80, 122, 500, 56);
            btnNeonix.Click += btnNeonix_Click;

            btnExit.Text = "ВЫХОД";
            btnExit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExit.ForeColor = Color.White;
            btnExit.BackColor = Color.FromArgb(34, 34, 34);
            btnExit.SetBounds(80, 344, 500, 48);
            btnExit.Click += btnExit_Click;

            choiceCard.Controls.Add(cardTitle);
            choiceCard.Controls.Add(cardSubtitle);
            choiceCard.Controls.Add(btnDichotomy);
            choiceCard.Controls.Add(btnGoldenSection);
            choiceCard.Controls.Add(btnNeonix);
            choiceCard.Controls.Add(btnExit);

            contentPanel.Controls.Add(choiceCard);

            Controls.Add(contentPanel);
            Controls.Add(headerPanel);
            Controls.Add(leftPanel);

            ResumeLayout(false);
        }
    }
}
