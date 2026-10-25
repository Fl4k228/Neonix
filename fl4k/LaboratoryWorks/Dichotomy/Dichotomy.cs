using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
namespace fl4k
{
    internal static class DichotomyUiRound
    {
        public static GraphicsPath Path(Rectangle rect, int radius)
        {
            int d = Math.Max(2, radius * 2);
            var r = new Rectangle(rect.X, rect.Y,
                Math.Max(1, rect.Width - 1), Math.Max(1, rect.Height - 1));
            var path = new GraphicsPath();
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
    internal sealed class DichotomyRoundedPanel : Panel
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int Radius { get; set; } = 14;
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(68, 68, 68);
        public DichotomyRoundedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }
        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            if (Width <= 1 || Height <= 1) return;
            using var path = DichotomyUiRound.Path(new Rectangle(0, 0, Width, Height), Radius);
            Region?.Dispose();
            Region = new Region(path);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Width <= 1 || Height <= 1) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = DichotomyUiRound.Path(new Rectangle(1, 1, Width - 2, Height - 2), Radius);
            using var pen = new Pen(BorderColor, 1f);
            e.Graphics.DrawPath(pen, path);
        }
    }
    internal sealed class DichotomyActionButton : Button
    {
        private bool hovered;
        private bool pressed;

        public DichotomyActionButton()
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
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

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

            using var path = DichotomyUiRound.Path(rect, 12);

            Color fill = pressed
                ? Color.FromArgb(28, 28, 28)
                : hovered
                    ? Color.FromArgb(55, 55, 55)
                    : BackColor;

            using (var brush = new SolidBrush(fill))
                e.Graphics.FillPath(brush, path);

            using (var pen = new Pen(Color.FromArgb(72, 72, 72), 1f))
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
    }

    internal sealed class DichotomyDarkMenuColorTable : ProfessionalColorTable
    {
        private readonly Color bg = Color.FromArgb(30, 30, 30);
        private readonly Color hover = Color.FromArgb(55, 55, 55);
        private readonly Color border = Color.FromArgb(72, 72, 72);
        public override Color MenuStripGradientBegin => bg;
        public override Color MenuStripGradientEnd => bg;
        public override Color ToolStripDropDownBackground => bg;
        public override Color MenuItemSelected => hover;
        public override Color MenuItemBorder => border;
        public override Color MenuItemSelectedGradientBegin => hover;
        public override Color MenuItemSelectedGradientEnd => hover;
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(40, 40, 40);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(40, 40, 40);
        public override Color ImageMarginGradientBegin => bg;
        public override Color ImageMarginGradientMiddle => bg;
        public override Color ImageMarginGradientEnd => bg;
        public override Color ToolStripBorder => border;
    }
    public partial class DichotomyForm : Form
    {
        private static readonly Color BgDark = Color.FromArgb(14, 14, 14);
        private static readonly Color BgPanel = Color.FromArgb(22, 22, 22);
        private static readonly Color BgCard = Color.FromArgb(30, 30, 30);
        private static readonly Color TextMain = Color.FromArgb(245, 245, 245);
        private static readonly Color TextDim = Color.FromArgb(155, 155, 155);
        private static readonly Color Border = Color.FromArgb(68, 68, 68);
        private static readonly Color Axis = Color.FromArgb(110, 110, 110);
        private static readonly Color Curve = Color.FromArgb(235, 235, 235);
        private readonly TextBox txtA = new();
        private readonly TextBox txtB = new();
        private readonly TextBox txtEps = new();
        private readonly TextBox txtFunction = new();
        private readonly Label lblRoot = new();
        private readonly Label lblFx = new();
        private readonly Label lblStatus = new();
        private readonly Panel graphPanel = new();
        private Func<double, double>? currentFunction;
        private double graphA;
        private double graphB;
        private double rootX;
        private double rootY;
        private bool hasResult;
        private bool findAllRoots;
        private double currentEps = 0.001;
        private int displayDecimals = 3;
        private readonly List<(double x, double y)> foundRoots = new();
        public DichotomyForm()
        {
            InitializeComponent();
            Text = "Лабораторная работа №1 — Метод дихотомии: f(x) = 0";
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
                BackColor = Color.FromArgb(18, 18, 18),
                AutoScroll = true
            };
            leftPanel.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(58, 58, 58), 1f);
                e.Graphics.DrawLine(pen, leftPanel.Width - 1, 0,
                    leftPanel.Width - 1, leftPanel.Height);
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
            leftPanel.Controls.Add(brand);
            var separator = new Panel
            {
                BackColor = Color.FromArgb(62, 62, 62),
                Location = new Point(30, 76),
                Size = new Size(230, 1)
            };
            leftPanel.Controls.Add(separator);
            var actionsTitle = new Label
            {
                Text = "ДЕЙСТВИЯ",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(205, 205, 205),
                BackColor = Color.Transparent,
                Location = new Point(30, 94),
                AutoSize = true
            };
            leftPanel.Controls.Add(actionsTitle);
            var btnCalculate = new DichotomyActionButton
            {
                Text = "Рассчитать",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(38, 38, 38),
                Location = new Point(30, 118),
                Size = new Size(230, 40)
            };

            var btnClear = new DichotomyActionButton
            {
                Text = "Очистить",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(38, 38, 38),
                Location = new Point(30, 168),
                Size = new Size(230, 40)
            };

            var btnMenu = new DichotomyActionButton
            {
                Text = "Назад",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(38, 38, 38),
                Location = new Point(30, 218),
                Size = new Size(230, 40)
            };

            btnCalculate.Click += (_, _) => CalculateRoot();
            btnClear.Click += (_, _) => ClearAll();
            btnMenu.Click += (_, _) => Close();

            leftPanel.Controls.Add(btnCalculate);
            leftPanel.Controls.Add(btnClear);
            leftPanel.Controls.Add(btnMenu);

            var paramsTitle = new Label
            {
                Text = "ПАРАМЕТРЫ",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(205, 205, 205),
                BackColor = Color.Transparent,
                Location = new Point(30, 290),
                AutoSize = true
            };
            leftPanel.Controls.Add(paramsTitle);
            var paramsCard = new DichotomyRoundedPanel
            {
                Radius = 14,
                BorderColor = Border,
                BackColor = BgCard,
                Location = new Point(30, 314),
                Size = new Size(230, 320)
            };
            leftPanel.Controls.Add(paramsCard);
            txtA.Text = "-2";
            txtB.Text = "2";
            txtEps.Text = "0,001";
            txtFunction.Text = "x^3 - x - 2";
            AddInput(paramsCard, "a", txtA, 18, 20, 194);
            AddInput(paramsCard, "b", txtB, 18, 94, 194);
            AddInput(paramsCard, "e", txtEps, 18, 168, 194);
            AddInput(paramsCard, "f(x)", txtFunction, 18, 242, 194);

            var modeButton = new DichotomyActionButton
            {
                Text = "1 корень",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(38, 38, 38),
                Location = new Point(30, 650),
                Size = new Size(230, 40)
            };

            modeButton.Click += (_, _) =>
            {
                findAllRoots = !findAllRoots;
                modeButton.Text = findAllRoots ? "Множество корней" : "1 корень";

                foundRoots.Clear();
                hasResult = false;
                lblRoot.Text = "—";
                lblFx.Text = "—";
                lblStatus.Text = findAllRoots
                    ? "Режим: поиск множества корней на [a, b]."
                    : "Режим: поиск одного корня.";
                lblStatus.ForeColor = TextDim;
                graphPanel.Invalidate();
            };

            leftPanel.Controls.Add(modeButton);

            var hintCard = new DichotomyRoundedPanel
            {
                Radius = 14,
                BorderColor = Border,
                BackColor = BgCard,
                Location = new Point(30, 712),
                Size = new Size(230, 112),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            var hint = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                Text = "Метод половинного деления\nИщем корень:  f(x) = 0\n\nEsc — выход",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            hintCard.Controls.Add(hint);
            leftPanel.Controls.Add(hintCard);
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = BgPanel
            };
            headerPanel.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(65, 65, 65), 1f);
                e.Graphics.DrawLine(pen, 0, headerPanel.Height - 1,
                    headerPanel.Width, headerPanel.Height - 1);
            };
            var pageTitle = new Label
            {
                Text = "МЕТОД ДИХОТОМИИ",
                Font = new Font("Segoe UI", 23F, FontStyle.Bold),
                ForeColor = TextMain,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(22, 10)
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
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
            var graphCard = new DichotomyRoundedPanel
            {
                Radius = 16,
                BorderColor = Border,
                BackColor = BgCard,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 14),
                Padding = new Padding(12)
            };
            var graphTitle = new Label
            {
                Text = "ГРАФИК ФУНКЦИИ",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(215, 215, 215),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };
            graphPanel.Dock = DockStyle.Fill;
            graphPanel.BackColor = Color.FromArgb(24, 24, 24);
            graphPanel.Paint += GraphPanel_Paint;
            graphPanel.Resize += (_, _) => graphPanel.Invalidate();
            graphCard.Controls.Add(graphPanel);
            graphCard.Controls.Add(graphTitle);
            var resultCard = new DichotomyRoundedPanel
            {
                Radius = 14,
                BorderColor = Border,
                BackColor = BgCard,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(18, 10, 18, 10)
            };
            var resultLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 3,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            AddResultBlock(resultLayout, "КОРЕНЬ / КОРНИ x", lblRoot, 0);
            AddResultBlock(resultLayout, "ЗНАЧЕНИЕ f(x)", lblFx, 1);
            AddResultBlock(resultLayout, "СОСТОЯНИЕ", lblStatus, 2);
            lblRoot.Text = "—";
            lblFx.Text = "—";
            lblStatus.Text = "Введите данные и выберите «Рассчитать».";
            lblStatus.ForeColor = TextDim;
            resultCard.Controls.Add(resultLayout);
            content.Controls.Add(graphCard, 0, 0);
            content.Controls.Add(resultCard, 0, 1);
            Controls.Add(content);
            Controls.Add(headerPanel);
            Controls.Add(leftPanel);
            ResumeLayout(true);
        }
        private void AddInput(Control parent, string labelText, TextBox box,
            int x, int y, int width)
        {
            var label = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(215, 215, 215),
                BackColor = Color.Transparent,
                Location = new Point(x, y),
                AutoSize = true
            };
            box.Location = new Point(x, y + 24);
            box.Size = new Size(width, 29);
            box.Font = new Font("Segoe UI", 10.5F);
            box.BackColor = Color.FromArgb(18, 18, 18);
            box.ForeColor = TextMain;
            box.BorderStyle = BorderStyle.FixedSingle;
            parent.Controls.Add(label);
            parent.Controls.Add(box);
        }
        private void AddResultBlock(TableLayoutPanel panel, string caption,
            Label valueLabel, int column)
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
        private void CalculateRoot()
        {
            hasResult = false;
            foundRoots.Clear();
            graphPanel.Invalidate();
            try
            {
                if (!TryParseNumber(txtA.Text, out double a))
                    throw new ArgumentException("Параметр a должен быть вещественным числом.");
                if (!TryParseNumber(txtB.Text, out double b))
                    throw new ArgumentException("Параметр b должен быть вещественным числом.");
                if (!TryParseNumber(txtEps.Text, out double eps))
                    throw new ArgumentException("Точность e должна быть вещественным числом.");
                if (a >= b)
                    throw new ArgumentException("Должно выполняться условие a < b.");
                if (eps <= 0)
                    throw new ArgumentException("Точность e должна быть больше нуля.");

                currentEps = eps;
                displayDecimals = GetDecimalsFromEpsilon(eps);

                if (string.IsNullOrWhiteSpace(txtFunction.Text))
                    throw new ArgumentException("Введите формулу f(x).");

                var parser = new ExpressionParser(txtFunction.Text);
                currentFunction = x => parser.Evaluate(x);
                graphA = a;
                graphB = b;

                double fa = currentFunction(a);
                double fb = currentFunction(b);
                EnsureFinite(fa, "f(a)");
                EnsureFinite(fb, "f(b)");

                if (fa * fb > 0)
                    throw new ArgumentException(
                        "На концах интервала функция имеет одинаковый знак.\n" +
                        "Для метода дихотомии нужно, чтобы f(a) и f(b) имели разные знаки.");

                if (findAllRoots)
                {
                    var roots = FindAllRoots(a, b, eps);

                    if (roots.Count == 0)
                        throw new ArgumentException("На заданном интервале корни не найдены.");

                    foundRoots.AddRange(roots);
                    rootX = foundRoots[0].x;
                    rootY = foundRoots[0].y;
                    ShowResult();
                    return;
                }

                if (Math.Abs(fa) <= eps)
                {
                    rootX = a;
                    rootY = fa;
                    foundRoots.Add((rootX, rootY));
                    ShowResult();
                    return;
                }

                if (Math.Abs(fb) <= eps)
                {
                    rootX = b;
                    rootY = fb;
                    foundRoots.Add((rootX, rootY));
                    ShowResult();
                    return;
                }

                double left = a;
                double right = b;
                double fLeft = fa;
                int iterations = 0;
                const int maxIterations = 100000;

                while (true)
                {
                    double middle = (left + right) / 2.0;
                    double fMiddle = currentFunction(middle);
                    EnsureFinite(fMiddle, "f(x)");
                    iterations++;

                    if (Math.Abs(fMiddle) <= eps || (right - left) / 2.0 <= eps)
                    {
                        rootX = middle;
                        rootY = fMiddle;
                        break;
                    }

                    if (iterations >= maxIterations)
                        throw new InvalidOperationException("Превышено допустимое число итераций.");

                    if (fLeft * fMiddle <= 0)
                    {
                        right = middle;
                    }
                    else
                    {
                        left = middle;
                        fLeft = fMiddle;
                    }
                }

                foundRoots.Add((rootX, rootY));
                ShowResult();
            }
            catch (Exception ex)
            {
                foundRoots.Clear();
                hasResult = false;
                lblRoot.Text = "—";
                lblFx.Text = "—";
                lblStatus.Text = ex.Message;
                lblStatus.ForeColor = Color.FromArgb(220, 180, 180);
                MessageBox.Show(ex.Message, "Ошибка входных данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private List<(double x, double y)> FindAllRoots(double a, double b, double eps)
        {
            var roots = new List<(double x, double y)>();

            int segments = Math.Clamp(
                (int)Math.Ceiling((b - a) / Math.Max(eps * 4.0, 0.002)),
                2000,
                20000);

            double step = (b - a) / segments;
            double nearZero = Math.Max(eps, 1e-8);
            var samples = new List<(double x, double y)>();

            for (int i = 0; i <= segments; i++)
            {
                double x = a + step * i;
                double y;

                try
                {
                    y = currentFunction!(x);
                }
                catch
                {
                    samples.Add((x, double.NaN));
                    continue;
                }

                if (double.IsNaN(y) || double.IsInfinity(y))
                {
                    samples.Add((x, double.NaN));
                    continue;
                }

                samples.Add((x, y));
            }

            for (int i = 1; i < samples.Count; i++)
            {
                var p1 = samples[i - 1];
                var p2 = samples[i];

                if (double.IsNaN(p1.y) || double.IsNaN(p2.y))
                    continue;

                if (Math.Abs(p1.y) <= nearZero)
                    AddRootUnique(roots, p1.x, p1.y, eps);

                if (Math.Abs(p2.y) <= nearZero)
                    AddRootUnique(roots, p2.x, p2.y, eps);

                if (Math.Sign(p1.y) == Math.Sign(p2.y))
                    continue;

                if (TryRefineRoot(p1.x, p2.x, p1.y, eps, out double rx, out double ry))
                    AddRootUnique(roots, rx, ry, eps);
            }

            for (int i = 1; i < samples.Count - 1; i++)
            {
                var left = samples[i - 1];
                var mid = samples[i];
                var right = samples[i + 1];

                if (double.IsNaN(left.y) || double.IsNaN(mid.y) || double.IsNaN(right.y))
                    continue;

                double absMid = Math.Abs(mid.y);

                if (absMid > Math.Max(eps * 10.0, 1e-6))
                    continue;

                if (absMid <= Math.Abs(left.y) && absMid <= Math.Abs(right.y))
                {
                    if (TryRefineMinimumAbs(left.x, right.x, eps, out double rx, out double ry))
                        AddRootUnique(roots, rx, ry, eps);
                }
            }

            roots.Sort((r1, r2) => r1.x.CompareTo(r2.x));
            return roots;
        }

        private bool TryRefineRoot(
            double left,
            double right,
            double fLeft,
            double eps,
            out double root,
            out double fRoot)
        {
            root = (left + right) / 2.0;
            fRoot = double.NaN;

            double targetWidth = Math.Max(Math.Min(eps * 0.1, 1e-8), 1e-12);

            for (int i = 0; i < 100 && right - left > targetWidth; i++)
            {
                double middle = (left + right) / 2.0;
                double fMiddle;

                try
                {
                    fMiddle = currentFunction!(middle);
                }
                catch
                {
                    return false;
                }

                if (double.IsNaN(fMiddle) || double.IsInfinity(fMiddle))
                    return false;

                if (Math.Abs(fMiddle) <= 1e-12)
                {
                    root = middle;
                    fRoot = fMiddle;
                    return true;
                }

                if (Math.Sign(fLeft) != Math.Sign(fMiddle))
                {
                    right = middle;
                }
                else
                {
                    left = middle;
                    fLeft = fMiddle;
                }
            }

            root = (left + right) / 2.0;

            try
            {
                fRoot = currentFunction!(root);
            }
            catch
            {
                return false;
            }

            if (double.IsNaN(fRoot) || double.IsInfinity(fRoot))
                return false;

            return Math.Abs(fRoot) <= Math.Max(eps * 10.0, 1e-5);
        }

        private bool TryRefineMinimumAbs(
            double left,
            double right,
            double eps,
            out double root,
            out double fRoot)
        {
            root = (left + right) / 2.0;
            fRoot = double.NaN;

            const double phi = 0.6180339887498949;

            for (int i = 0; i < 60; i++)
            {
                double x1 = right - (right - left) * phi;
                double x2 = left + (right - left) * phi;

                double y1;
                double y2;

                try
                {
                    y1 = currentFunction!(x1);
                    y2 = currentFunction!(x2);
                }
                catch
                {
                    return false;
                }

                if (double.IsNaN(y1) || double.IsInfinity(y1) ||
                    double.IsNaN(y2) || double.IsInfinity(y2))
                    return false;

                if (Math.Abs(y1) < Math.Abs(y2))
                    right = x2;
                else
                    left = x1;
            }

            root = (left + right) / 2.0;

            try
            {
                fRoot = currentFunction!(root);
            }
            catch
            {
                return false;
            }

            return !double.IsNaN(fRoot) &&
                   !double.IsInfinity(fRoot) &&
                   Math.Abs(fRoot) <= Math.Max(eps, 1e-7);
        }

        private static void AddRootUnique(
            List<(double x, double y)> roots,
            double x,
            double y,
            double eps)
        {
            double tolerance = Math.Max(eps * 4.0, 1e-6);

            foreach (var root in roots)
            {
                if (Math.Abs(root.x - x) <= tolerance)
                    return;
            }

            roots.Add((x, y));
        }

        private static int GetDecimalsFromEpsilon(double eps)
        {
            if (eps >= 1.0)
                return 0;

            int decimals = (int)Math.Ceiling(-Math.Log10(eps));
            return Math.Clamp(decimals, 0, 12);
        }

        private string FormatByEpsilon(double value)
        {
            double zeroThreshold = 0.5 * Math.Pow(10.0, -displayDecimals);

            if (Math.Abs(value) < zeroThreshold)
                value = 0.0;

            return value.ToString(
                "F" + displayDecimals,
                CultureInfo.GetCultureInfo("ru-RU"));
        }

        // f(x) выводим точнее, чем x, чтобы маленькое значение
        // не превращалось визуально в ложный ноль.
        // Функция считается от того x, который реально показан пользователю.
        private double GetDisplayedFunctionValue(double x)
        {
            if (currentFunction == null)
                return double.NaN;

            double displayedX = Math.Round(
                x,
                displayDecimals,
                MidpointRounding.AwayFromZero);

            return currentFunction(displayedX);
        }

        private string FormatFunctionValue(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return "—";

            var culture = CultureInfo.GetCultureInfo("ru-RU");
            int decimals = Math.Clamp(displayDecimals + 3, 6, 12);
            double abs = Math.Abs(value);

            if (abs < 1e-15)
                return 0.0.ToString("F" + decimals, culture);

            double scientificThreshold = Math.Pow(10.0, -decimals);
            if (abs < scientificThreshold)
                return value.ToString("0.######E+0", culture);

            return value.ToString("F" + decimals, culture);
        }

        private void ShowResult()
        {
            hasResult = true;

            if (findAllRoots)
            {
                lblRoot.Text = string.Join("; ",
                    foundRoots.Select(r => FormatByEpsilon(r.x)));

                lblFx.Text = string.Join("; ",
                    foundRoots.Select(r =>
                        FormatFunctionValue(GetDisplayedFunctionValue(r.x))));

                lblStatus.Text = $"Найдено корней: {foundRoots.Count}.";
            }
            else
            {
                lblRoot.Text = FormatByEpsilon(rootX);
                lblFx.Text = FormatFunctionValue(
                    GetDisplayedFunctionValue(rootX));
                lblStatus.Text = "Корень найден.";
            }

            lblStatus.ForeColor = Color.FromArgb(220, 220, 220);
            graphPanel.Invalidate();
        }

        private void ClearAll()
        {
            txtA.Clear();
            txtB.Clear();
            txtEps.Clear();
            txtFunction.Clear();
            lblRoot.Text = "—";
            lblFx.Text = "—";
            lblStatus.Text = "Данные очищены.";
            lblStatus.ForeColor = TextDim;
            currentFunction = null;
            foundRoots.Clear();
            hasResult = false;
            graphPanel.Invalidate();
            txtA.Focus();
        }
        private static bool TryParseNumber(string text, out double value)
        {
            text = (text ?? string.Empty).Trim().Replace(',', '.');
            return double.TryParse(text, NumberStyles.Float,
                CultureInfo.InvariantCulture, out value);
        }
        private static void EnsureFinite(double value, string name)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException($"{name} не является конечным числом.");
        }
        private void GraphPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.FromArgb(24, 24, 24));
            if (currentFunction == null || graphB <= graphA)
            {
                using var font = new Font("Segoe UI", 11F);
                TextRenderer.DrawText(g,
                    "После расчёта здесь будет построен график f(x).",
                    font, graphPanel.ClientRectangle, TextDim,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            int leftPad = 58, rightPad = 20, topPad = 20, bottomPad = 42;
            var plot = new Rectangle(
                leftPad,
                topPad,
                Math.Max(1, graphPanel.ClientSize.Width - leftPad - rightPad),
                Math.Max(1, graphPanel.ClientSize.Height - topPad - bottomPad));
            if (plot.Width < 20 || plot.Height < 20) return;
            const int samples = 1200;
            var points = new List<(double x, double y)>();
            for (int i = 0; i <= samples; i++)
            {
                double x = graphA + (graphB - graphA) * i / samples;
                double y;
                try { y = currentFunction(x); }
                catch { continue; }
                if (double.IsNaN(y) || double.IsInfinity(y)) continue;
                points.Add((x, y));
            }
            if (points.Count < 2) return;
            var sortedY = points.Select(p => p.y).OrderBy(v => v).ToArray();
            double rawMinY = sortedY[0];
            double rawMaxY = sortedY[^1];
            int lowIndex = Math.Clamp((int)(sortedY.Length * 0.02), 0, sortedY.Length - 1);
            int highIndex = Math.Clamp((int)(sortedY.Length * 0.98), 0, sortedY.Length - 1);
            double centralMinY = sortedY[lowIndex];
            double centralMaxY = sortedY[highIndex];
            double rawRange = rawMaxY - rawMinY;
            double centralRange = centralMaxY - centralMinY;
            double minY;
            double maxY;
            if (centralRange > 1e-12 && rawRange > centralRange * 4.0)
            {
                minY = centralMinY;
                maxY = centralMaxY;
            }
            else
            {
                minY = rawMinY;
                maxY = rawMaxY;
            }
            if (hasResult)
            {
                foreach (var root in foundRoots)
                {
                    minY = Math.Min(minY, root.y);
                    maxY = Math.Max(maxY, root.y);
                }
            }
            if (minY > 0) minY = 0;
            if (maxY < 0) maxY = 0;
            if (Math.Abs(maxY - minY) < 1e-12)
            {
                minY -= 1;
                maxY += 1;
            }
            else
            {
                double m = (maxY - minY) * 0.08;
                minY -= m;
                maxY += m;
            }
            float MapX(double x) => plot.Left + (float)((x - graphA) / (graphB - graphA) * plot.Width);
            float MapY(double y) => plot.Bottom - (float)((y - minY) / (maxY - minY) * plot.Height);
            using var borderPen = new Pen(Color.FromArgb(68, 68, 68));
            using var axisPen = new Pen(Axis, 1.2f);
            using var curvePen = new Pen(Curve, 2f);
            using var rootBrush = new SolidBrush(Color.White);
            using var rootOutline = new Pen(Color.FromArgb(120, 120, 120), 1f);
            using var textBrush = new SolidBrush(Color.FromArgb(205, 205, 205));
            using var smallFont = new Font("Segoe UI", 8F);
            g.DrawRectangle(borderPen, plot);
            if (graphA <= 0 && graphB >= 0)
            {
                float x0 = MapX(0);
                g.DrawLine(axisPen, x0, plot.Top, x0, plot.Bottom);
            }
            if (minY <= 0 && maxY >= 0)
            {
                float y0 = MapY(0);
                g.DrawLine(axisPen, plot.Left, y0, plot.Right, y0);
            }
            var curveState = g.Save();
            g.SetClip(plot);
            PointF? prev = null;
            double? prevY = null;
            double visibleRange = Math.Max(1e-12, maxY - minY);
            double clipMinY = minY;
            double clipMaxY = maxY;
            foreach (var p in points)
            {
                // За пределами видимого диапазона начинаем новую ветвь графика.
                // Это убирает вертикальные полосы у tg(x) возле асимптот.
                if (p.y < clipMinY || p.y > clipMaxY)
                {
                    prev = null;
                    prevY = null;
                    continue;
                }
                var cur = new PointF(MapX(p.x), MapY(p.y));
                if (prev.HasValue && prevY.HasValue)
                {
                    double valueJump = Math.Abs(p.y - prevY.Value);
                    float pixelJump = Math.Abs(cur.Y - prev.Value.Y);
                    if (valueJump <= visibleRange * 0.45 &&
                        pixelJump <= plot.Height * 0.45f)
                    {
                        g.DrawLine(curvePen, prev.Value, cur);
                    }
                }
                prev = cur;
                prevY = p.y;
            }
            g.Restore(curveState);
            if (hasResult)
            {
                foreach (var root in foundRoots)
                {
                    float rx = MapX(root.x);
                    float ry = MapY(root.y);

                    if (rx < plot.Left || rx > plot.Right ||
                        ry < plot.Top || ry > plot.Bottom)
                        continue;

                    g.FillEllipse(rootBrush, rx - 5, ry - 5, 10, 10);
                    g.DrawEllipse(rootOutline, rx - 5, ry - 5, 10, 10);

                    if (foundRoots.Count <= 12)
                        g.DrawString($"x ≈ {FormatByEpsilon(root.x)}", smallFont, rootBrush, rx + 7, ry - 20);
                }
            }
            g.DrawString(graphA.ToString("G5"), smallFont, textBrush, plot.Left - 4, plot.Bottom + 8);
            string bText = graphB.ToString("G5");
            var bSize = g.MeasureString(bText, smallFont);
            g.DrawString(bText, smallFont, textBrush, plot.Right - bSize.Width, plot.Bottom + 8);
            g.DrawString(maxY.ToString("G5"), smallFont, textBrush, 4, plot.Top - 4);
            g.DrawString(minY.ToString("G5"), smallFont, textBrush, 4, plot.Bottom - 12);
        }
    }
    internal sealed class ExpressionParser
    {
        private readonly string expression;
        public ExpressionParser(string expression)
        {
            this.expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }
        public double Evaluate(double x)
        {
            var parser = new Parser(expression, x);
            double value = parser.ParseExpression();
            parser.SkipSpaces();
            if (!parser.End)
                throw new FormatException($"Неожиданный символ в формуле: '{parser.Current}'.");
            return value;
        }
        private sealed class Parser
        {
            private readonly string text;
            private readonly double x;
            private int pos;
            public Parser(string text, double x)
            {
                this.text = text;
                this.x = x;
            }
            public bool End => pos >= text.Length;
            public char Current => End ? '\0' : text[pos];
            public void SkipSpaces()
            {
                while (!End && char.IsWhiteSpace(Current)) pos++;
            }
            public double ParseExpression()
            {
                double value = ParseTerm();
                while (true)
                {
                    SkipSpaces();
                    if (Match('+')) value += ParseTerm();
                    else if (Match('-')) value -= ParseTerm();
                    else break;
                }
                return value;
            }
            private double ParseTerm()
            {
                double value = ParsePower();
                while (true)
                {
                    SkipSpaces();
                    if (Match('*')) value *= ParsePower();
                    else if (Match('/'))
                    {
                        double d = ParsePower();
                        if (Math.Abs(d) < 1e-300)
                            throw new DivideByZeroException("Деление на ноль в формуле.");
                        value /= d;
                    }   
                    else break;
                }
                return value;
            }
            private double ParsePower()
            {
                double value = ParseUnary();
                SkipSpaces();
                if (Match('^'))
                    value = Math.Pow(value, ParsePower());
                return value;
            }
            private double ParseUnary()
            {
                SkipSpaces();
                if (Match('+')) return ParseUnary();
                if (Match('-')) return -ParseUnary();
                return ParsePrimary();
            }
            private double ParsePrimary()
            {
                SkipSpaces();
                if (Match('('))
                {
                    double value = ParseExpression();
                    SkipSpaces();
                    if (!Match(')'))
                        throw new FormatException("Не хватает закрывающей скобки ')'.");
                    return value;
                }
                if (char.IsDigit(Current) || Current == '.' || Current == ',')
                    return ParseNumber();
                if (char.IsLetter(Current))
                {
                    string name = ParseIdentifier().ToLowerInvariant();
                    if (name == "x") return x;
                    if (name == "pi") return Math.PI;
                    if (name == "e") return Math.E;
                    SkipSpaces();
                    if (!Match('('))
                        throw new FormatException($"После функции '{name}' ожидается '('.");
                    double arg = ParseExpression();
                    SkipSpaces();
                    if (!Match(')'))
                        throw new FormatException("Не хватает закрывающей скобки ')'.");
                    return name switch
                    {
                        "sin" => Math.Sin(arg),
                        "cos" => Math.Cos(arg),
                        "tan" => Math.Tan(arg),
                        "tg" => Math.Tan(arg),
                        "sqrt" => Math.Sqrt(arg),
                        "abs" => Math.Abs(arg),
                        "ln" => Math.Log(arg),
                        "log" => Math.Log10(arg),
                        "exp" => Math.Exp(arg),
                        _ => throw new FormatException($"Неизвестная функция '{name}'.")
                    };
                }
                throw new FormatException(
                    End ? "Неожиданный конец формулы."
                        : $"Неожиданный символ '{Current}' в формуле.");
            }
            private double ParseNumber()
            {
                SkipSpaces();
                int start = pos;
                bool hasExponent = false;
                while (!End)
                {
                    char c = Current;
                    if (char.IsDigit(c) || c == '.' || c == ',') pos++;
                    else if ((c == 'e' || c == 'E') && !hasExponent)
                    {
                        hasExponent = true;
                        pos++;
                        if (!End && (Current == '+' || Current == '-')) pos++;
                    }
                    else break;
                }
                string number = text[start..pos].Replace(',', '.');
                if (!double.TryParse(number, NumberStyles.Float,
                        CultureInfo.InvariantCulture, out double value))
                    throw new FormatException($"Некорректное число '{number}'.");
                return value;
            }
            private string ParseIdentifier()
            {
                SkipSpaces();
                int start = pos;
                while (!End && char.IsLetter(Current)) pos++;
                return text[start..pos];
            }
            private bool Match(char c)
            {
                SkipSpaces();
                if (!End && Current == c)
                {
                    pos++;
                    return true;
                }
                return false;
            }
        }
    }
}