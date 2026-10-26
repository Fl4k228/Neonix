using System;
using System.Collections.Generic;
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

        private readonly TextBox txtA = new();
        private readonly TextBox txtB = new();
        private readonly TextBox txtE = new();
        private readonly TextBox txtFunction = new();
        private readonly ComboBox cmbMode = new();

        private readonly Label lblX = new();
        private readonly Label lblFx = new();
        private readonly Label lblIterations = new();
        private readonly Label lblMode = new();
        private readonly Label lblStatus = new();
        private readonly Panel graphPanel = new();

        private bool hasResult;
        private bool searchMinimum = true;

        private double currentA;
        private double currentB;
        private double currentEps;
        private double bestX;
        private double bestY;
        private int iterations;
        private string currentFunctionText = "x^2";
        private Func<double, double>? functionDelegate;

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

            var leftPanel = new Panel()
            {
                Dock = DockStyle.Left,
                Width = 290,
                BackColor = Color.FromArgb(18, 18, 18),
                AutoScroll = true
            };

            leftPanel.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(58, 58, 58));
                e.Graphics.DrawLine(pen, leftPanel.Width - 1, 0,
                    leftPanel.Width - 1, leftPanel.Height);
            };

            var brand = new Label()
            {
                Text = "NEONIX",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 18),
                Size = new Size(290, 42)
            };

            var separator = new Panel()
            {
                BackColor = Color.FromArgb(62, 62, 62),
                Location = new Point(30, 76),
                Size = new Size(230, 1)
            };

            var actionsTitle = new Label()
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
            var btnBack = CreateButton("Назад", 30, 218, 230, 40);

            btnCalculate.Click += (_, _) => CalculateGoldenSection();
            btnClear.Click += (_, _) => ClearAll();
            btnBack.Click += (_, _) => Close();

            var paramsTitle = new Label()
            {
                Text = "ПАРАМЕТРЫ",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(205, 205, 205),
                BackColor = Color.Transparent,
                Location = new Point(30, 290),
                AutoSize = true
            };

            var paramsCard = new GoldenSectionRoundedPanel()
            {
                BackColor = BgCard,
                Location = new Point(30, 314),
                Size = new Size(230, 330)
            };

            var lblA = CreateParamLabel("a", 18, 18);
            txtA.Text = "-5";
            SetupInput(txtA, 18, 42, 194);

            var lblB = CreateParamLabel("b", 18, 78);
            txtB.Text = "5";
            SetupInput(txtB, 18, 102, 194);

            var lblE = CreateParamLabel("e", 18, 138);
            txtE.Text = "0,001";
            SetupInput(txtE, 18, 162, 194);

            var lblF = CreateParamLabel("f(x)", 18, 198);
            txtFunction.Text = "x^2";
            SetupInput(txtFunction, 18, 222, 194);

            var lblSearchMode = CreateParamLabel("Режим", 18, 258);

            cmbMode.Location = new Point(18, 282);
            cmbMode.Size = new Size(194, 29);
            cmbMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMode.Font = new Font("Segoe UI", 10F);
            cmbMode.BackColor = Color.FromArgb(18, 18, 18);
            cmbMode.ForeColor = TextMain;
            cmbMode.FlatStyle = FlatStyle.Flat;
            cmbMode.Items.Add("Минимум");
            cmbMode.Items.Add("Максимум");
            cmbMode.SelectedIndex = 0;

            paramsCard.Controls.Add(lblA);
            paramsCard.Controls.Add(txtA);
            paramsCard.Controls.Add(lblB);
            paramsCard.Controls.Add(txtB);
            paramsCard.Controls.Add(lblE);
            paramsCard.Controls.Add(txtE);
            paramsCard.Controls.Add(lblF);
            paramsCard.Controls.Add(txtFunction);
            paramsCard.Controls.Add(lblSearchMode);
            paramsCard.Controls.Add(cmbMode);

            leftPanel.Controls.Add(brand);
            leftPanel.Controls.Add(separator);
            leftPanel.Controls.Add(actionsTitle);
            leftPanel.Controls.Add(btnCalculate);
            leftPanel.Controls.Add(btnClear);
            leftPanel.Controls.Add(btnBack);
            leftPanel.Controls.Add(paramsTitle);
            leftPanel.Controls.Add(paramsCard);

            var headerPanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = BgPanel
            };

            headerPanel.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(65, 65, 65));
                e.Graphics.DrawLine(pen, 0, headerPanel.Height - 1,
                    headerPanel.Width, headerPanel.Height - 1);
            };

            var pageTitle = new Label()
            {
                Text = "ЗОЛОТОЕ СЕЧЕНИЕ",
                Font = new Font("Segoe UI", 23F, FontStyle.Bold),
                ForeColor = TextMain,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(22, 8)
            };

            var pageSubtitle = new Label()
            {
            };

            headerPanel.Controls.Add(pageTitle);
            headerPanel.Controls.Add(pageSubtitle);

            var content = new TableLayoutPanel()
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

            var graphCard = new GoldenSectionRoundedPanel()
            {
                BackColor = BgCard,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 14),
                Padding = new Padding(12)
            };

            var graphTitle = new Label()
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

            var resultCard = new GoldenSectionRoundedPanel()
            {
                BackColor = BgCard,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(18, 10, 18, 10)
            };

            var resultLayout = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 5,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            AddResultBlock(resultLayout, "РЕЖИМ", lblMode, 0);
            AddResultBlock(resultLayout, "x*", lblX, 1);
            AddResultBlock(resultLayout, "f(x*)", lblFx, 2);
            AddResultBlock(resultLayout, "ИТЕРАЦИИ", lblIterations, 3);
            AddResultBlock(resultLayout, "СОСТОЯНИЕ", lblStatus, 4);

            ResetResultLabels();

            resultCard.Controls.Add(resultLayout);

            content.Controls.Add(graphCard, 0, 0);
            content.Controls.Add(resultCard, 0, 1);

            Controls.Add(content);
            Controls.Add(headerPanel);
            Controls.Add(leftPanel);

            ResumeLayout(true);
        }

        private Label CreateParamLabel(string text, int x, int y)
        {
            return new Label()
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(215, 215, 215),
                BackColor = Color.Transparent,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private void SetupInput(TextBox textBox, int x, int y, int width)
        {
            textBox.Location = new Point(x, y);
            textBox.Size = new Size(width, 29);
            textBox.Font = new Font("Segoe UI", 10.5F);
            textBox.BackColor = Color.FromArgb(18, 18, 18);
            textBox.ForeColor = TextMain;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private Button CreateButton(string text, int x, int y, int width, int height)
        {
            return new GoldenSectionButton()
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
            var captionLabel = new Label()
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

        private void ResetResultLabels()
        {
            lblMode.Text = "—";
            lblX.Text = "—";
            lblFx.Text = "—";
            lblIterations.Text = "—";
            lblStatus.Text = "Введите данные и нажмите «Рассчитать».";
            lblStatus.ForeColor = TextDim;
        }

        private void CalculateGoldenSection()
        {
            try
            {
                if (!TryParseNumber(txtA.Text, out double a))
                    throw new ArgumentException("Параметр a должен быть числом.");

                if (!TryParseNumber(txtB.Text, out double b))
                    throw new ArgumentException("Параметр b должен быть числом.");

                if (!TryParseNumber(txtE.Text, out double eps))
                    throw new ArgumentException("Точность e должна быть числом.");

                if (a >= b)
                    throw new ArgumentException("Должно выполняться условие a < b.");

                if (eps <= 0)
                    throw new ArgumentException("Точность e должна быть больше нуля.");

                string expression = (txtFunction.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(expression))
                    throw new ArgumentException("Введите функцию f(x).");

                // ВАЖНО: отдельное имя парсера, чтобы не конфликтовать
                // с ExpressionParser из Dichotomy.cs.
                var parser = new GoldenSectionExpressionParser(expression);

                Func<double, double> func = x =>
                {
                    double value = parser.Evaluate(x);

                    if (double.IsNaN(value) || double.IsInfinity(value))
                        throw new ArithmeticException(
                            $"f(x) не является конечным числом при x = {x:G8}.");

                    return value;
                };

                bool modeMin = cmbMode.SelectedIndex != 1;

                // Проверяем несколько точек, а не только концы.
                // Это позволяет нормально работать, например, с функциями,
                // у которых область определения ограничена.
                for (int i = 0; i <= 20; i++)
                {
                    double x = a + (b - a) * i / 20.0;
                    _ = func(x);
                }

                SearchResult result = GoldenSectionSearch(
                    func, a, b, eps, modeMin);

                currentA = a;
                currentB = b;
                currentEps = eps;
                currentFunctionText = expression;
                functionDelegate = func;
                searchMinimum = modeMin;

                bestX = result.X;
                bestY = result.Fx;
                iterations = result.Iterations;
                hasResult = true;

                lblMode.Text = modeMin ? "Минимум" : "Максимум";
                lblX.Text = FormatNumber(bestX, eps);
                lblFx.Text = bestY.ToString("G12",
                    CultureInfo.GetCultureInfo("ru-RU"));
                lblIterations.Text = iterations.ToString();
                lblStatus.Text = "Экстремум найден.";
                lblStatus.ForeColor = Color.FromArgb(220, 220, 220);

                graphPanel.Invalidate();
            }
            catch (Exception ex)
            {
                hasResult = false;
                functionDelegate = null;

                ResetResultLabels();
                lblStatus.Text = ex.Message;
                lblStatus.ForeColor = Color.FromArgb(220, 180, 180);

                graphPanel.Invalidate();

                MessageBox.Show(
                    ex.Message,
                    "Ошибка входных данных",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ClearAll()
        {
            txtA.Text = "-5";
            txtB.Text = "5";
            txtE.Text = "0,001";
            txtFunction.Text = "x^2";
            cmbMode.SelectedIndex = 0;

            currentA = 0;
            currentB = 0;
            currentEps = 0;
            bestX = 0;
            bestY = 0;
            iterations = 0;
            currentFunctionText = "x^2";
            functionDelegate = null;
            hasResult = false;

            ResetResultLabels();
            lblStatus.Text = "Данные очищены.";

            txtA.Focus();
            graphPanel.Invalidate();
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

        private static string FormatNumber(double value, double eps)
        {
            int decimals = eps >= 1
                ? 0
                : Math.Clamp((int)Math.Ceiling(-Math.Log10(eps)), 0, 12);

            if (Math.Abs(value) < 0.5 * Math.Pow(10, -decimals))
                value = 0;

            return value.ToString(
                "F" + decimals,
                CultureInfo.GetCultureInfo("ru-RU"));
        }

        private static SearchResult GoldenSectionSearch(
            Func<double, double> f,
            double a,
            double b,
            double eps,
            bool findMinimum)
        {
            double phi = (1.0 + Math.Sqrt(5.0)) / 2.0;

            double x1 = b - (b - a) / phi;
            double x2 = a + (b - a) / phi;

            double f1 = f(x1);
            double f2 = f(x2);

            int iterationCount = 0;
            const int maxIterations = 10000;

            while (Math.Abs(b - a) > eps)
            {
                iterationCount++;

                if (iterationCount > maxIterations)
                    throw new InvalidOperationException(
                        "Превышено допустимое число итераций.");

                bool leftIsBetter =
                    findMinimum ? f1 <= f2 : f1 >= f2;

                if (leftIsBetter)
                {
                    b = x2;
                    x2 = x1;
                    f2 = f1;

                    x1 = b - (b - a) / phi;
                    f1 = f(x1);
                }
                else
                {
                    a = x1;
                    x1 = x2;
                    f1 = f2;

                    x2 = a + (b - a) / phi;
                    f2 = f(x2);
                }
            }

            double resultX = (a + b) / 2.0;
            double resultFx = f(resultX);

            return new SearchResult(
                resultX,
                resultFx,
                iterationCount);
        }

        private void GraphPanel_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.Clear(Color.FromArgb(24, 24, 24));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (functionDelegate == null)
            {
                using var emptyFont = new Font("Segoe UI", 11F);

                TextRenderer.DrawText(
                    g,
                    "После расчёта здесь будет показан график функции и найденный экстремум.",
                    emptyFont,
                    graphPanel.ClientRectangle,
                    TextDim,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.WordBreak);

                return;
            }

            int left = 58;
            int top = 22;
            int right = Math.Max(left + 100,
                graphPanel.ClientSize.Width - 26);
            int bottom = Math.Max(top + 100,
                graphPanel.ClientSize.Height - 40);

            var plotRect = new Rectangle(
                left,
                top,
                right - left,
                bottom - top);

            const int samples = 1000;
            var points =
                new List<(double x, double y, bool valid)>(
                    samples + 1);

            double yMin = double.PositiveInfinity;
            double yMax = double.NegativeInfinity;

            for (int i = 0; i <= samples; i++)
            {
                double x =
                    currentA +
                    (currentB - currentA) * i / samples;

                try
                {
                    double y = functionDelegate(x);

                    if (double.IsNaN(y) ||
                        double.IsInfinity(y))
                    {
                        points.Add((x, 0, false));
                        continue;
                    }

                    points.Add((x, y, true));

                    yMin = Math.Min(yMin, y);
                    yMax = Math.Max(yMax, y);
                }
                catch
                {
                    points.Add((x, 0, false));
                }
            }

            if (double.IsInfinity(yMin) ||
                double.IsInfinity(yMax))
            {
                using var errorFont =
                    new Font("Segoe UI", 10F);

                TextRenderer.DrawText(
                    g,
                    "На выбранном интервале график не удалось построить.",
                    errorFont,
                    plotRect,
                    TextDim,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter);

                return;
            }

            if (hasResult)
            {
                yMin = Math.Min(yMin, bestY);
                yMax = Math.Max(yMax, bestY);
            }

            if (Math.Abs(yMax - yMin) < 1e-12)
            {
                yMin -= 1;
                yMax += 1;
            }
            else
            {
                double padding =
                    (yMax - yMin) * 0.1;

                yMin -= padding;
                yMax += padding;
            }

            float MapX(double x) =>
                plotRect.Left +
                (float)((x - currentA) /
                (currentB - currentA) *
                plotRect.Width);

            float MapY(double y) =>
                plotRect.Bottom -
                (float)((y - yMin) /
                (yMax - yMin) *
                plotRect.Height);

            using var borderPen =
                new Pen(Color.FromArgb(62, 62, 62));
            using var axisPen =
                new Pen(Color.FromArgb(85, 85, 85), 1.2F);
            using var graphPen =
                new Pen(Color.FromArgb(235, 235, 235), 2F);
            using var pointBrush =
                new SolidBrush(Color.White);
            using var textBrush =
                new SolidBrush(Color.FromArgb(230, 230, 230));
            using var dimBrush =
                new SolidBrush(TextDim);

            // Имена отличаются от emptyFont выше — конфликта CS0136 нет.
            using var axisFont =
                new Font("Segoe UI", 8.5F);
            using var resultFont =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            g.DrawRectangle(borderPen, plotRect);

            if (currentA <= 0 && currentB >= 0)
            {
                float x0 = MapX(0);
                g.DrawLine(
                    axisPen,
                    x0,
                    plotRect.Top,
                    x0,
                    plotRect.Bottom);
            }

            if (yMin <= 0 && yMax >= 0)
            {
                float y0 = MapY(0);
                g.DrawLine(
                    axisPen,
                    plotRect.Left,
                    y0,
                    plotRect.Right,
                    y0);
            }

            var segment = new List<PointF>();

            foreach (var point in points)
            {
                if (!point.valid)
                {
                    DrawSegmentIfNeeded(
                        g, graphPen, segment);
                    segment.Clear();
                    continue;
                }

                float px = MapX(point.x);
                float py = MapY(point.y);

                if (py < plotRect.Top - 1000 ||
                    py > plotRect.Bottom + 1000)
                {
                    DrawSegmentIfNeeded(
                        g, graphPen, segment);
                    segment.Clear();
                    continue;
                }

                segment.Add(new PointF(px, py));
            }

            DrawSegmentIfNeeded(
                g, graphPen, segment);

            if (hasResult &&
                !double.IsNaN(bestY) &&
                !double.IsInfinity(bestY))
            {
                float ex = MapX(bestX);
                float ey = MapY(bestY);

                g.FillEllipse(
                    pointBrush,
                    ex - 5,
                    ey - 5,
                    10,
                    10);

                string pointText =
                    $"x* ≈ {FormatNumber(bestX, currentEps)}; " +
                    $"f(x*) ≈ {bestY:G6}";

                SizeF pointTextSize =
                    g.MeasureString(
                        pointText,
                        resultFont);

                float textX = ex + 10;
                float textY = ey - 26;

                if (textX + pointTextSize.Width >
                    plotRect.Right)
                {
                    textX =
                        ex -
                        pointTextSize.Width -
                        10;
                }

                if (textY < plotRect.Top)
                    textY = ey + 8;

                g.DrawString(
                    pointText,
                    resultFont,
                    textBrush,
                    textX,
                    textY);
            }

            string aText =
                currentA.ToString(
                    "G6",
                    CultureInfo.InvariantCulture);

            string bText =
                currentB.ToString(
                    "G6",
                    CultureInfo.InvariantCulture);

            SizeF bTextSize =
                g.MeasureString(
                    bText,
                    axisFont);

            g.DrawString(
                aText,
                axisFont,
                dimBrush,
                plotRect.Left - 2,
                plotRect.Bottom + 6);

            g.DrawString(
                bText,
                axisFont,
                dimBrush,
                plotRect.Right - bTextSize.Width,
                plotRect.Bottom + 6);

            g.DrawString(
                yMax.ToString(
                    "G6",
                    CultureInfo.InvariantCulture),
                axisFont,
                dimBrush,
                6,
                plotRect.Top - 2);

            g.DrawString(
                yMin.ToString(
                    "G6",
                    CultureInfo.InvariantCulture),
                axisFont,
                dimBrush,
                6,
                plotRect.Bottom - 14);

            string graphInfo =
                $"f(x) = {currentFunctionText}   |   " +
                $"[{currentA:G6}; {currentB:G6}]   |   " +
                $"e = {currentEps:G6}   |   " +
                (searchMinimum
                    ? "Минимум"
                    : "Максимум");

            g.DrawString(
                graphInfo,
                axisFont,
                dimBrush,
                plotRect.Left + 6,
                plotRect.Top + 6);
        }

        private static void DrawSegmentIfNeeded(
            Graphics g,
            Pen pen,
            List<PointF> segment)
        {
            if (segment.Count >= 2)
                g.DrawLines(
                    pen,
                    segment.ToArray());
        }

        private readonly struct SearchResult
        {
            public double X { get; }
            public double Fx { get; }
            public int Iterations { get; }

            public SearchResult(
                double x,
                double fx,
                int iterations)
            {
                X = x;
                Fx = fx;
                Iterations = iterations;
            }
        }
    }

    // Отдельное имя класса — не конфликтует с ExpressionParser из Dichotomy.cs.
    internal sealed class GoldenSectionExpressionParser
    {
        private readonly string expression;

        public GoldenSectionExpressionParser(string expression)
        {
            this.expression =
                expression ??
                throw new ArgumentNullException(
                    nameof(expression));
        }

        public double Evaluate(double x)
        {
            var parser =
                new Parser(expression, x);

            double value =
                parser.ParseExpression();

            parser.SkipSpaces();

            if (!parser.End)
            {
                throw new FormatException(
                    $"Неожиданный символ в формуле: '{parser.Current}'.");
            }

            return value;
        }

        private sealed class Parser
        {
            private readonly string text;
            private readonly double x;
            private int pos;

            public Parser(
                string text,
                double x)
            {
                this.text = text;
                this.x = x;
            }

            public bool End =>
                pos >= text.Length;

            public char Current =>
                End ? '\0' : text[pos];

            public void SkipSpaces()
            {
                while (!End &&
                       char.IsWhiteSpace(Current))
                {
                    pos++;
                }
            }

            public double ParseExpression()
            {
                double value = ParseTerm();

                while (true)
                {
                    SkipSpaces();

                    if (Match('+'))
                        value += ParseTerm();
                    else if (Match('-'))
                        value -= ParseTerm();
                    else
                        break;
                }

                return value;
            }

            private double ParseTerm()
            {
                double value = ParsePower();

                while (true)
                {
                    SkipSpaces();

                    if (Match('*'))
                    {
                        value *= ParsePower();
                    }
                    else if (Match('/'))
                    {
                        double divisor =
                            ParsePower();

                        if (Math.Abs(divisor) <
                            1e-300)
                        {
                            throw new DivideByZeroException(
                                "Деление на ноль в формуле.");
                        }

                        value /= divisor;
                    }
                    else
                    {
                        break;
                    }
                }

                return value;
            }

            private double ParsePower()
            {
                double value = ParseUnary();
                SkipSpaces();

                if (Match('^'))
                {
                    value =
                        Math.Pow(
                            value,
                            ParsePower());
                }

                return value;
            }

            private double ParseUnary()
            {
                SkipSpaces();

                if (Match('+'))
                    return ParseUnary();

                if (Match('-'))
                    return -ParseUnary();

                return ParsePrimary();
            }

            private double ParsePrimary()
            {
                SkipSpaces();

                if (Match('('))
                {
                    double value =
                        ParseExpression();

                    SkipSpaces();

                    if (!Match(')'))
                    {
                        throw new FormatException(
                            "Не хватает закрывающей скобки ')'.");
                    }

                    return value;
                }

                if (char.IsDigit(Current) ||
                    Current == '.' ||
                    Current == ',')
                {
                    return ParseNumber();
                }

                if (char.IsLetter(Current))
                {
                    string name =
                        ParseIdentifier()
                            .ToLowerInvariant();

                    if (name == "x")
                        return x;

                    if (name == "pi")
                        return Math.PI;

                    if (name == "e")
                        return Math.E;

                    SkipSpaces();

                    if (!Match('('))
                    {
                        throw new FormatException(
                            $"После функции '{name}' ожидается '('.");
                    }

                    double arg =
                        ParseExpression();

                    SkipSpaces();

                    if (!Match(')'))
                    {
                        throw new FormatException(
                            "Не хватает закрывающей скобки ')'.");
                    }

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
                        _ => throw new FormatException(
                            $"Неизвестная функция '{name}'.")
                    };
                }

                throw new FormatException(
                    End
                        ? "Неожиданный конец формулы."
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

                    if (char.IsDigit(c) ||
                        c == '.' ||
                        c == ',')
                    {
                        pos++;
                    }
                    else if ((c == 'e' ||
                              c == 'E') &&
                             !hasExponent)
                    {
                        hasExponent = true;
                        pos++;

                        if (!End &&
                            (Current == '+' ||
                             Current == '-'))
                        {
                            pos++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                string number =
                    text[start..pos]
                        .Replace(',', '.');

                if (!double.TryParse(
                        number,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double value))
                {
                    throw new FormatException(
                        $"Некорректное число '{number}'.");
                }

                return value;
            }

            private string ParseIdentifier()
            {
                SkipSpaces();

                int start = pos;

                while (!End &&
                       char.IsLetter(Current))
                {
                    pos++;
                }

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

        protected override void OnResize(
            EventArgs eventargs)
        {
            base.OnResize(eventargs);

            if (Width < 3 || Height < 3)
                return;

            using var path =
                CreateRoundedPath(
                    new Rectangle(
                        0,
                        0,
                        Width,
                        Height),
                    14);

            Region?.Dispose();
            Region = new Region(path);
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Width < 3 || Height < 3)
                return;

            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            using var path =
                CreateRoundedPath(
                    new Rectangle(
                        1,
                        1,
                        Width - 3,
                        Height - 3),
                    14);

            using var pen =
                new Pen(
                    Color.FromArgb(
                        68,
                        68,
                        68));

            e.Graphics.DrawPath(
                pen,
                path);
        }

        private static GraphicsPath CreateRoundedPath(
            Rectangle rect,
            int radius)
        {
            int d =
                Math.Max(
                    2,
                    radius * 2);

            var path =
                new GraphicsPath();

            path.AddArc(
                rect.Left,
                rect.Top,
                d,
                d,
                180,
                90);

            path.AddArc(
                rect.Right - d,
                rect.Top,
                d,
                d,
                270,
                90);

            path.AddArc(
                rect.Right - d,
                rect.Bottom - d,
                d,
                d,
                0,
                90);

            path.AddArc(
                rect.Left,
                rect.Bottom - d,
                d,
                d,
                90,
                90);

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

            FlatStyle =
                FlatStyle.Flat;

            FlatAppearance.BorderSize =
                0;

            Cursor =
                Cursors.Hand;
        }

        protected override void OnMouseEnter(
            EventArgs e)
        {
            base.OnMouseEnter(e);
            hovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(
            EventArgs e)
        {
            base.OnMouseLeave(e);
            hovered = false;
            pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(
            MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(
            MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            pressed = false;
            Invalidate();
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            Color parentColor =
                Parent?.BackColor ==
                Color.Transparent
                    ? Color.FromArgb(
                        18,
                        18,
                        18)
                    : Parent?.BackColor ??
                      Color.FromArgb(
                          18,
                          18,
                          18);

            e.Graphics.Clear(
                parentColor);

            var rect =
                new Rectangle(
                    1,
                    1,
                    Math.Max(
                        1,
                        Width - 3),
                    Math.Max(
                        1,
                        Height - 3));

            using var path =
                CreateRoundedPath(
                    rect,
                    12);

            Color fill =
                pressed
                    ? Color.FromArgb(
                        28,
                        28,
                        28)
                    : hovered
                        ? Color.FromArgb(
                            55,
                            55,
                            55)
                        : BackColor;

            using (var brush =
                   new SolidBrush(fill))
            {
                e.Graphics.FillPath(
                    brush,
                    path);
            }

            using (var pen =
                   new Pen(
                       Color.FromArgb(
                           72,
                           72,
                           72)))
            {
                e.Graphics.DrawPath(
                    pen,
                    path);
            }

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

        private static GraphicsPath CreateRoundedPath(
            Rectangle rect,
            int radius)
        {
            int d =
                Math.Max(
                    2,
                    radius * 2);

            var path =
                new GraphicsPath();

            path.AddArc(
                rect.Left,
                rect.Top,
                d,
                d,
                180,
                90);

            path.AddArc(
                rect.Right - d,
                rect.Top,
                d,
                d,
                270,
                90);

            path.AddArc(
                rect.Right - d,
                rect.Bottom - d,
                d,
                d,
                0,
                90);

            path.AddArc(
                rect.Left,
                rect.Bottom - d,
                d,
                d,
                90,
                90);

            path.CloseFigure();

            return path;
        }
    }
}
