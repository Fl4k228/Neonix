using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace fl4k
{
    public partial class GoldenSectionForm : Form
    {
        private static readonly Color BgDark = Color.FromArgb(14, 14, 14);
        private static readonly Color BgPanel = Color.FromArgb(22, 22, 22);
        private static readonly Color BgCard = Color.FromArgb(30, 30, 30);
        private static readonly Color TextMain = Color.FromArgb(245, 245, 245);
        private static readonly Color TextDim = Color.FromArgb(155, 155, 155);

        private readonly TextBox txtLength = new();
        private readonly Label lblPhi = new();
        private readonly Label lblLarge = new();
        private readonly Label lblSmall = new();
        private readonly Label lblRatio = new();
        private readonly Label lblStatus = new();
        private readonly Panel diagramPanel = new();

        private double totalLength;
        private double largePart;
        private double smallPart;
        private bool hasResult;

        public GoldenSectionForm()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            WindowState = FormWindowState.Maximized;
            Bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            BackColor = BgDark;
            ForeColor = TextMain;
            Font = new Font("Segoe UI", 10F);
            KeyPreview = true;

            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    Close();
            };

            BuildUi();
        }

        private void BuildUi()
        {
            SuspendLayout();

            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 290,
                BackColor = Color.FromArgb(18, 18, 18)
            };

            leftPanel.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(58, 58, 58));
                e.Graphics.DrawLine(
                    pen,
                    leftPanel.Width - 1,
                    0,
                    leftPanel.Width - 1,
                    leftPanel.Height);
            };

            var brand = new Label
            {
                Text = "NEONIX",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 18),
                Size = new Size(290, 42)
            };

            var separator = new Panel
            {
                BackColor = Color.FromArgb(62, 62, 62),
                Location = new Point(30, 76),
                Size = new Size(230, 1)
            };

            var actionsTitle = new Label
            {
                Text = "ДЕЙСТВИЯ",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(205, 205, 205),
                BackColor = Color.Transparent,
                Location = new Point(30, 94),
                AutoSize = true
            };

            var btnCalculate = CreateButton("Рассчитать", 30, 118, 230, 40);
            var btnClear = CreateButton("Очистить", 30, 168, 230, 40);
            var btnMenu = CreateButton("Назад", 30, 218, 230, 40);

            btnCalculate.Click += (_, _) => CalculateGoldenSection();
            btnClear.Click += (_, _) => ClearAll();
            btnMenu.Click += (_, _) => Close();

            var paramsTitle = new Label
            {
                Text = "ПАРАМЕТРЫ",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(205, 205, 205),
                BackColor = Color.Transparent,
                Location = new Point(30, 290),
                AutoSize = true
            };

            var paramsCard = new GoldenSectionRoundedPanel
            {
                BackColor = BgCard,
                Location = new Point(30, 314),
                Size = new Size(230, 126)
            };

            var lengthLabel = new Label
            {
                Text = "Длина отрезка L",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(215, 215, 215),
                BackColor = Color.Transparent,
                Location = new Point(18, 18),
                AutoSize = true
            };

            txtLength.Text = "100";
            txtLength.Location = new Point(18, 50);
            txtLength.Size = new Size(194, 29);
            txtLength.Font = new Font("Segoe UI", 10.5F);
            txtLength.BackColor = Color.FromArgb(18, 18, 18);
            txtLength.ForeColor = TextMain;
            txtLength.BorderStyle = BorderStyle.FixedSingle;

            var inputHint = new Label
            {
                Text = "L > 0",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                Location = new Point(18, 86),
                AutoSize = true
            };

            paramsCard.Controls.Add(lengthLabel);
            paramsCard.Controls.Add(txtLength);
            paramsCard.Controls.Add(inputHint);

            var theoryCard = new GoldenSectionRoundedPanel
            {
                BackColor = BgCard,
                Location = new Point(30, 466),
                Size = new Size(230, 180),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };

            var theory = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                Text =
                    "ЗОЛОТОЕ СЕЧЕНИЕ\n\n" +
                    "a / b = (a + b) / a = φ\n\n" +
                    "φ = (1 + √5) / 2\n" +
                    "φ ≈ 1,618\n\n" +
                    "Esc — выход",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            theoryCard.Controls.Add(theory);

            leftPanel.Controls.Add(brand);
            leftPanel.Controls.Add(separator);
            leftPanel.Controls.Add(actionsTitle);
            leftPanel.Controls.Add(btnCalculate);
            leftPanel.Controls.Add(btnClear);
            leftPanel.Controls.Add(btnMenu);
            leftPanel.Controls.Add(paramsTitle);
            leftPanel.Controls.Add(paramsCard);
            leftPanel.Controls.Add(theoryCard);

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = BgPanel
            };

            headerPanel.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(65, 65, 65));
                e.Graphics.DrawLine(
                    pen,
                    0,
                    headerPanel.Height - 1,
                    headerPanel.Width,
                    headerPanel.Height - 1);
            };

            var pageTitle = new Label
            {
                Text = "ЗОЛОТОЕ СЕЧЕНИЕ",
                Font = new Font("Segoe UI", 23F, FontStyle.Bold),
                ForeColor = TextMain,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(22, 8)
            };

            headerPanel.Controls.Add(pageTitle);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BgDark,
                Padding = new Padding(20, 18, 20, 18),
                ColumnCount = 1,
                RowCount = 2
            };

            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));

            var diagramCard = new GoldenSectionRoundedPanel
            {
                BackColor = BgCard,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 14),
                Padding = new Padding(12)
            };

            var diagramTitle = new Label
            {
                Text = "СХЕМА ЗОЛОТОГО СЕЧЕНИЯ",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(215, 215, 215),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            diagramPanel.Dock = DockStyle.Fill;
            diagramPanel.BackColor = Color.FromArgb(24, 24, 24);
            diagramPanel.Paint += DiagramPanel_Paint;
            diagramPanel.Resize += (_, _) => diagramPanel.Invalidate();

            diagramCard.Controls.Add(diagramPanel);
            diagramCard.Controls.Add(diagramTitle);

            var resultCard = new GoldenSectionRoundedPanel
            {
                BackColor = BgCard,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(18, 10, 18, 10)
            };

            var resultLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 5,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            AddResultBlock(resultLayout, "φ", lblPhi, 0);
            AddResultBlock(resultLayout, "БОЛЬШАЯ ЧАСТЬ a", lblLarge, 1);
            AddResultBlock(resultLayout, "МЕНЬШАЯ ЧАСТЬ b", lblSmall, 2);
            AddResultBlock(resultLayout, "ПРОВЕРКА ОТНОШЕНИЯ", lblRatio, 3);
            AddResultBlock(resultLayout, "СОСТОЯНИЕ", lblStatus, 4);

            lblPhi.Text = "—";
            lblLarge.Text = "—";
            lblSmall.Text = "—";
            lblRatio.Text = "—";
            lblStatus.Text = "Введите длину отрезка и нажмите «Рассчитать».";
            lblStatus.ForeColor = TextDim;

            resultCard.Controls.Add(resultLayout);

            content.Controls.Add(diagramCard, 0, 0);
            content.Controls.Add(resultCard, 0, 1);

            Controls.Add(content);
            Controls.Add(headerPanel);
            Controls.Add(leftPanel);

            ResumeLayout(true);
        }

        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            return new GoldenSectionButton
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(38, 38, 38),
                Location = new Point(x, y),
                Size = new Size(width, height)
            };
        }

        private void AddResultBlock(
            TableLayoutPanel panel,
            string caption,
            Label valueLabel,
            int column)
        {
            var captionLabel = new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.BottomLeft
            };

            valueLabel.Dock = DockStyle.Fill;
            valueLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            valueLabel.ForeColor = TextMain;
            valueLabel.BackColor = Color.Transparent;
            valueLabel.TextAlign = ContentAlignment.TopLeft;
            valueLabel.AutoEllipsis = true;

            panel.Controls.Add(captionLabel, column, 0);
            panel.Controls.Add(valueLabel, column, 1);
        }

        private void CalculateGoldenSection()
        {
            try
            {
                if (!TryParseNumber(txtLength.Text, out double length))
                    throw new ArgumentException("Длина отрезка должна быть вещественным числом.");

                if (length <= 0)
                    throw new ArgumentException("Длина отрезка должна быть больше нуля.");

                double phi = (1.0 + Math.Sqrt(5.0)) / 2.0;

                totalLength = length;
                largePart = totalLength / phi;
                smallPart = totalLength - largePart;
                hasResult = true;

                double ratio1 = largePart / smallPart;
                double ratio2 = totalLength / largePart;

                lblPhi.Text = phi.ToString("G12", CultureInfo.InvariantCulture);
                lblLarge.Text = largePart.ToString("G12", CultureInfo.InvariantCulture);
                lblSmall.Text = smallPart.ToString("G12", CultureInfo.InvariantCulture);
                lblRatio.Text = $"a/b = {ratio1:G8}; L/a = {ratio2:G8}";
                lblStatus.Text = "Золотое сечение рассчитано.";
                lblStatus.ForeColor = Color.FromArgb(220, 220, 220);

                diagramPanel.Invalidate();
            }
            catch (Exception ex)
            {
                hasResult = false;
                lblPhi.Text = "—";
                lblLarge.Text = "—";
                lblSmall.Text = "—";
                lblRatio.Text = "—";
                lblStatus.Text = ex.Message;
                lblStatus.ForeColor = Color.FromArgb(220, 180, 180);
                diagramPanel.Invalidate();

                MessageBox.Show(
                    ex.Message,
                    "Ошибка входных данных",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ClearAll()
        {
            txtLength.Clear();

            hasResult = false;
            totalLength = 0;
            largePart = 0;
            smallPart = 0;

            lblPhi.Text = "—";
            lblLarge.Text = "—";
            lblSmall.Text = "—";
            lblRatio.Text = "—";
            lblStatus.Text = "Данные очищены.";
            lblStatus.ForeColor = TextDim;

            diagramPanel.Invalidate();
            txtLength.Focus();
        }

        private static bool TryParseNumber(string text, out double value)
        {
            text = (text ?? string.Empty).Trim().Replace(',', '.');

            return double.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }

        private void DiagramPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.FromArgb(24, 24, 24));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (!hasResult)
            {
                using var font = new Font("Segoe UI", 11F);

                TextRenderer.DrawText(
                    g,
                    "После расчёта здесь будет показано деление отрезка по золотому сечению.",
                    font,
                    diagramPanel.ClientRectangle,
                    TextDim,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter);

                return;
            }

            int left = 70;
            int right = Math.Max(left + 100, diagramPanel.ClientSize.Width - 70);
            int centerY = diagramPanel.ClientSize.Height / 2;

            int totalWidth = Math.Max(1, right - left);
            int largeWidth = (int)Math.Round(totalWidth * (largePart / totalLength));
            int splitX = left + largeWidth;

            using var basePen = new Pen(Color.FromArgb(235, 235, 235), 4F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            using var splitPen = new Pen(Color.FromArgb(115, 115, 115), 2F);
            using var textBrush = new SolidBrush(Color.FromArgb(235, 235, 235));
            using var dimBrush = new SolidBrush(TextDim);
            using var bigFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var labelFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            using var smallFont = new Font("Segoe UI", 9F);

            g.DrawLine(basePen, left, centerY, right, centerY);
            g.DrawLine(splitPen, splitX, centerY - 28, splitX, centerY + 28);

            using var pointBrush = new SolidBrush(Color.White);
            g.FillEllipse(pointBrush, left - 5, centerY - 5, 10, 10);
            g.FillEllipse(pointBrush, splitX - 5, centerY - 5, 10, 10);
            g.FillEllipse(pointBrush, right - 5, centerY - 5, 10, 10);

            string phiText = "φ ≈ 1,618";
            SizeF phiSize = g.MeasureString(phiText, bigFont);

            g.DrawString(
                phiText,
                bigFont,
                textBrush,
                diagramPanel.ClientSize.Width / 2f - phiSize.Width / 2f,
                Math.Max(20, centerY - 120));

            string largeText = $"a = {largePart:G8}";
            string smallText = $"b = {smallPart:G8}";

            SizeF largeSize = g.MeasureString(largeText, labelFont);
            SizeF smallSize = g.MeasureString(smallText, labelFont);

            g.DrawString(
                largeText,
                labelFont,
                textBrush,
                left + largeWidth / 2f - largeSize.Width / 2f,
                centerY + 32);

            g.DrawString(
                smallText,
                labelFont,
                textBrush,
                splitX + (right - splitX) / 2f - smallSize.Width / 2f,
                centerY + 32);

            string formula = "a / b = (a + b) / a = φ";
            SizeF formulaSize = g.MeasureString(formula, smallFont);

            g.DrawString(
                formula,
                smallFont,
                dimBrush,
                diagramPanel.ClientSize.Width / 2f - formulaSize.Width / 2f,
                centerY + 82);

            g.DrawString("0", smallFont, dimBrush, left - 4, centerY - 48);
            g.DrawString("a", smallFont, dimBrush, splitX - 4, centerY - 48);

            string totalText = $"L = {totalLength:G8}";
            SizeF totalSize = g.MeasureString(totalText, smallFont);

            g.DrawString(
                totalText,
                smallFont,
                dimBrush,
                right - totalSize.Width,
                centerY - 48);
        }
    }

    internal sealed class GoldenSectionRoundedPanel : Panel
    {
        public GoldenSectionRoundedPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);

            if (Width < 3 || Height < 3)
                return;

            using var path = CreateRoundedPath(
                new Rectangle(0, 0, Width, Height),
                14);

            Region?.Dispose();
            Region = new Region(path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Width < 3 || Height < 3)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = CreateRoundedPath(
                new Rectangle(1, 1, Width - 3, Height - 3),
                14);

            using var pen = new Pen(Color.FromArgb(68, 68, 68));

            e.Graphics.DrawPath(pen, path);
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            int d = Math.Max(2, radius * 2);
            var path = new GraphicsPath();

            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    internal sealed class GoldenSectionButton : Button
    {
        private bool hovered;
        private bool pressed;

        public GoldenSectionButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            hovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hovered = false;
            pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            pressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color parentColor =
                Parent?.BackColor == Color.Transparent
                    ? Color.FromArgb(18, 18, 18)
                    : Parent?.BackColor ?? Color.FromArgb(18, 18, 18);

            e.Graphics.Clear(parentColor);

            var rect = new Rectangle(
                1,
                1,
                Math.Max(1, Width - 3),
                Math.Max(1, Height - 3));

            using var path = CreateRoundedPath(rect, 12);

            Color fill = pressed
                ? Color.FromArgb(28, 28, 28)
                : hovered
                    ? Color.FromArgb(55, 55, 55)
                    : BackColor;

            using (var brush = new SolidBrush(fill))
                e.Graphics.FillPath(brush, path);

            using (var pen = new Pen(Color.FromArgb(72, 72, 72)))
                e.Graphics.DrawPath(pen, path);

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                rect,
                ForeColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            int d = Math.Max(2, radius * 2);
            var path = new GraphicsPath();

            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
