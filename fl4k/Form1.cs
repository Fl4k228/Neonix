#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDataReader;

namespace fl4k
{
    // ============================================================
    //  АЛГОРИТМЫ СОРТИРОВКИ
    // ============================================================
    public abstract class SortingAlgorithm
    {
        public string Name { get; protected set; }
        public bool Ascending { get; set; } = true;
        public long Iterations { get; protected set; }

        public event Action<int, int> OnCompare;
        public event Action<int, int> OnSwap;
        public event Action OnIteration;

        protected void RaiseCompare(int i, int j) => OnCompare?.Invoke(i, j);
        protected void RaiseSwap(int i, int j) => OnSwap?.Invoke(i, j);
        protected void RaiseIteration()
        {
            Iterations++;
            OnIteration?.Invoke();
        }

        public abstract void Sort(double[] array);

        protected bool NeedSwap(double a, double b) => Ascending ? a > b : a < b;
    }

    public class BubbleSort : SortingAlgorithm
    {
        public BubbleSort() { Name = "Пузырьковая"; }
        public override void Sort(double[] a)
        {
            int n = a.Length;
            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;
                for (int j = 0; j < n - 1 - i; j++)
                {
                    RaiseCompare(j, j + 1);
                    if (NeedSwap(a[j], a[j + 1]))
                    {
                        (a[j], a[j + 1]) = (a[j + 1], a[j]);
                        RaiseSwap(j, j + 1);
                        swapped = true;
                    }
                }
                RaiseIteration();
                if (!swapped) break;
            }
        }
    }

    public class InsertionSort : SortingAlgorithm
    {
        public InsertionSort() { Name = "Вставками"; }
        public override void Sort(double[] a)
        {
            for (int i = 1; i < a.Length; i++)
            {
                double key = a[i];
                int j = i - 1;
                while (j >= 0)
                {
                    RaiseCompare(j, j + 1);
                    if (!NeedSwap(a[j], key)) break;
                    a[j + 1] = a[j];
                    RaiseSwap(j, j + 1);
                    j--;
                }
                a[j + 1] = key;
                RaiseIteration();
            }
        }
    }

    public class ShakerSort : SortingAlgorithm
    {
        public ShakerSort() { Name = "Шейкерная"; }
        public override void Sort(double[] a)
        {
            Iterations = 0;
            int left = 0, right = a.Length - 1;
            while (left < right)
            {
                bool swapped = false;
                for (int i = left; i < right; i++)
                {
                    RaiseCompare(i, i + 1);
                    if (NeedSwap(a[i], a[i + 1]))
                    {
                        (a[i], a[i + 1]) = (a[i + 1], a[i]);
                        RaiseSwap(i, i + 1);
                        swapped = true;
                    }
                }
                // Одна итерация — один непустой проход в одном направлении.
                RaiseIteration();
                if (!swapped) break;
                right--;
                if (left >= right) break;

                swapped = false;
                for (int i = right; i > left; i--)
                {
                    RaiseCompare(i - 1, i);
                    if (NeedSwap(a[i - 1], a[i]))
                    {
                        (a[i - 1], a[i]) = (a[i], a[i - 1]);
                        RaiseSwap(i - 1, i);
                        swapped = true;
                    }
                }
                RaiseIteration();
                if (!swapped) break;
                left++;
            }
        }
    }

    public class QuickSort : SortingAlgorithm
    {
        public QuickSort() { Name = "Быстрая"; }
        public override void Sort(double[] a) => QuickSortRec(a, 0, a.Length - 1);

        private void QuickSortRec(double[] a, int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(a, low, high);
                QuickSortRec(a, low, pi - 1);
                QuickSortRec(a, pi + 1, high);
            }
        }

        private int Partition(double[] a, int low, int high)
        {
            double pivot = a[high];
            int i = low - 1;
            for (int j = low; j < high; j++)
            {
                RaiseCompare(j, high);
                bool move = Ascending ? a[j] <= pivot : a[j] >= pivot;
                if (move)
                {
                    i++;
                    (a[i], a[j]) = (a[j], a[i]);
                    RaiseSwap(i, j);
                }
            }
            (a[i + 1], a[high]) = (a[high], a[i + 1]);
            RaiseSwap(i + 1, high);
            RaiseIteration();
            return i + 1;
        }
    }

    public class BogoSort : SortingAlgorithm
    {
        private readonly Random _rnd = new Random();
        public BogoSort() { Name = "BOGO"; }

        public override void Sort(double[] a)
        {
            var start = DateTime.Now;
            while (!IsSorted(a))
            {
                if ((DateTime.Now - start).TotalSeconds > 100)
                    throw new TimeoutException("BOGO превысила лимит 100 секунд.");
                Shuffle(a);
                RaiseIteration();
            }
        }

        private void Shuffle(double[] a)
        {
            for (int i = a.Length - 1; i > 0; i--)
            {
                int j = _rnd.Next(i + 1);
                (a[i], a[j]) = (a[j], a[i]);
                RaiseSwap(i, j);
            }
        }

        private bool IsSorted(double[] a)
        {
            for (int i = 0; i < a.Length - 1; i++)
            {
                RaiseCompare(i, i + 1);
                if (Ascending && a[i] > a[i + 1]) return false;
                if (!Ascending && a[i] < a[i + 1]) return false;
            }
            return true;
        }
    }

    // ============================================================
    //  СОСТОЯНИЕ АЛГОРИТМА
    // ============================================================
    public class AlgorithmState
    {
        public string Name;
        public Color BarColor;
        public double[] Data;
        public int CompareI = -1, CompareJ = -1;
        public int SwapI = -1, SwapJ = -1;
        public long Comparisons;
        public long Swaps;
        public long Iterations;

        public long AvgTicks;
        public long MinTicks;
        public long MaxTicks;

        public int Runs;
        public bool Finished;
        public string Error;
        public bool Visualize;

        public readonly object Sync = new object();

        public List<(int type, int i, int j)> EventLog = new();
        public readonly object LogSync = new object();

        public (double[] data, int ci, int cj, int si, int sj,
                long cmp, long swp, long iter,
                long avgTicks, long minTicks, long maxTicks, int runs,
                bool finished, string err) Snapshot()
        {
            lock (Sync)
            {
                var copy = new double[Data.Length];
                Array.Copy(Data, copy, Data.Length);
                return (copy, CompareI, CompareJ, SwapI, SwapJ,
                        Comparisons, Swaps, Iterations,
                        AvgTicks, MinTicks, MaxTicks, Runs,
                        Finished, Error);
            }
        }
    }

    public static class UiRound
    {
        public static GraphicsPath Path(Rectangle rect, int radius)
        {
            int d = Math.Max(2, radius * 2);
            var r = new Rectangle(rect.X, rect.Y, Math.Max(1, rect.Width - 1), Math.Max(1, rect.Height - 1));
            var path = new GraphicsPath();
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

    }

    public class SmoothButton : Button
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int CornerRadius { get; set; } = 12;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color HoverBackColor { get; set; } = Color.FromArgb(58, 58, 58);

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color PressedBackColor { get; set; } = Color.FromArgb(28, 28, 28);

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(72, 72, 72);

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int BorderThickness { get; set; } = 1;

        private bool _hovered;
        private bool _pressed;

        public SmoothButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovered = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _pressed = false;
            Invalidate();
        }

        private Color GetSurfaceColor()
        {
            Control p = Parent;
            while (p != null)
            {
                if (p.BackColor != Color.Transparent)
                    return p.BackColor;
                p = p.Parent;
            }
            return Color.FromArgb(18, 18, 18);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Не перерисовываем родителя через InvokePaint: это и создавало
            // белые/серые хвосты под кнопками при наведении.
            pevent.Graphics.Clear(GetSurfaceColor());
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.Clear(GetSurfaceColor());
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
            using var path = UiRound.Path(rect, CornerRadius);

            Color fillColor = Enabled
                ? (_pressed ? PressedBackColor : (_hovered ? HoverBackColor : BackColor))
                : Color.FromArgb(32, 32, 32);

            using (var fill = new SolidBrush(fillColor))
                g.FillPath(fill, path);

            if (BorderThickness > 0)
            {
                using var pen = new Pen(BorderColor, BorderThickness);
                pen.Alignment = PenAlignment.Inset;
                g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                rect,
                Enabled ? ForeColor : Color.FromArgb(105, 105, 105),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }
    }

    // ============================================================
    //  КАСТОМНЫЙ ЧЕКБОКС
    // ============================================================
    public class NeonCheckBox : CheckBox
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color CheckColor { get; set; } = Color.White;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color BoxBackColor { get; set; } = Color.FromArgb(34, 34, 34);

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color BoxBorderClr { get; set; } = Color.FromArgb(105, 105, 105);

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color BoxTextColor { get; set; } = Color.White;

        public NeonCheckBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            ForeColor = BoxTextColor;
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 10F);
            Height = 26;
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            var bg = Parent != null ? Parent.BackColor : Color.Transparent;
            if (bg != Color.Transparent)
            {
                using (var bgBrush = new SolidBrush(bg))
                    g.FillRectangle(bgBrush, ClientRectangle);
            }

            int boxSize = 18;
            int boxX = 2;
            int boxY = (Height - boxSize) / 2;
            var boxRect = new Rectangle(boxX, boxY, boxSize, boxSize);

            using (var boxPath = UiRound.Path(boxRect, 5))
            {
                using (var backBrush = new SolidBrush(BoxBackColor))
                    g.FillPath(backBrush, boxPath);
                using (var borderPen = new Pen(Checked ? CheckColor : BoxBorderClr, 1.6f))
                    g.DrawPath(borderPen, boxPath);
            }

            if (Checked)
            {
                using (var checkPen = new Pen(CheckColor, 2.6f))
                {
                    checkPen.StartCap = LineCap.Round;
                    checkPen.EndCap = LineCap.Round;

                    g.DrawLine(checkPen, boxX + 4, boxY + 9, boxX + 7, boxY + 13);
                    g.DrawLine(checkPen, boxX + 7, boxY + 13, boxX + 14, boxY + 5);
                }
            }

            var textRect = new Rectangle(boxX + boxSize + 8, 0,
                Math.Max(1, Width - boxSize - 12), Height);

            using (var textBrush = new SolidBrush(BoxTextColor))
            using (var sf = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Near
            })
            {
                g.DrawString(Text, Font, textBrush, textRect, sf);
            }
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            BoxBackColor = Color.FromArgb(52, 52, 52);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            BoxBackColor = Color.FromArgb(34, 34, 34);
            Invalidate();
        }
    }

    // ============================================================
    //  ГЛАВНАЯ ФОРМА
    // ============================================================
    public partial class Form1 : Form
    {
        // ---------- Палитра: графит / чёрный / белый ----------
        private static readonly Color BgDark      = Color.FromArgb(14, 14, 14);
        private static readonly Color BgPanel     = Color.FromArgb(22, 22, 22);
        private static readonly Color BgCard      = Color.FromArgb(30, 30, 30);
        private static readonly Color AccentRed   = Color.FromArgb(238, 238, 238);
        private static readonly Color AccentLight = Color.FromArgb(225, 225, 225);
        private static readonly Color TextSoft    = Color.FromArgb(245, 245, 245);
        private static readonly Color TextDim     = Color.FromArgb(155, 155, 155);
        private static readonly Color CompareClr  = Color.FromArgb(145, 145, 145);
        private static readonly Color SwapClr     = Color.White;
        private static readonly Color OkClr       = Color.FromArgb(220, 220, 220);


        // ---------- Константы ----------
        private const int MaxBogoElements = 50;
        private const int MaxVisualizeElements = 50;
        private const int RunsPerAlgorithm = 5;
        private const int SlowAlgorithmWarnThreshold = 5000;

        private int _decimalPlaces = 3;

        // ---------- Контролы ----------
        private Panel headerPanel;
        private Label titleLabel;

        private Panel leftPanel;
        private Button btnGenerate, btnExcel, btnGoogle, btnClear;
        private DataGridView dataGrid;
        private Label lblCount;

        private Panel optionsPanel;
        private NeonCheckBox cbBubble, cbInsertion, cbShaker, cbQuick, cbBogo;
        private Label lblDirection;
        private ComboBox cmbDirection;
        private Label lblDecimalPlaces;
        private ComboBox cmbDecimalPlaces;
        private Label lblDelay, lblDelayValue;
        private TrackBar tbDelay;

        private Panel workspacePanel;
        private Label workspaceHint;
        private Label lblStatus;

        private Button btnCalculate;
        private Button btnStop;

        // ---------- Состояние ----------
        private double[] _currentData = Array.Empty<double>();
        private List<AlgorithmState> _states = new();
        private System.Windows.Forms.Timer _renderTimer;
        private bool _isFullscreen = true;
        private bool _isRunning;
        private CancellationTokenSource _cts;
        private int _hoveredRowHeaderIndex = -1;

        private readonly Dictionary<string, Color> _algoColors = new()
        {
            ["Пузырьковая"] = Color.FromArgb(235, 235, 235),
            ["Вставками"]   = Color.FromArgb(195, 195, 195),
            ["Шейкерная"]   = Color.FromArgb(160, 160, 160),
            ["Быстрая"]     = Color.FromArgb(220, 220, 220),
            ["BOGO"]        = Color.FromArgb(125, 125, 125)
        };

        public Form1()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            BackColor = BgDark;
            KeyPreview = true;

            BuildUi();
            SetupGrid();

            dataGrid.CellValueChanged += (s, e) => SyncDataFromGrid();
            dataGrid.CellEndEdit += (s, e) =>
            {
                BeginInvoke((Action)(() =>
                {
                    RemoveEmptyDataRows();
                    SyncDataFromGrid();
                }));
            };
            dataGrid.RowsRemoved     += (s, e) => SyncDataFromGrid();
            dataGrid.UserDeletingRow += (s, e) => BeginInvoke((Action)SyncDataFromGrid);

            KeyDown += Form1_KeyDown;
            Resize += (s, e) => LayoutControls();

            _renderTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _renderTimer.Tick += (s, e) =>
            {
                if (_states.Count > 0 && workspacePanel.Width > 0 && workspacePanel.Height > 0)
                    workspacePanel.Invalidate();
            };
            _renderTimer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            EnterFullscreen();
            LayoutControls();
        }

        // ============================================================
        //  ХЕЛПЕРЫ
        // ============================================================
        private static bool TryParseDouble(string s, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Trim().Replace(',', '.');
            return double.TryParse(s, NumberStyles.Float,
                CultureInfo.InvariantCulture, out value);
        }

        private string FormatValue(double v)
        {
            if (_decimalPlaces <= 0)
            {
                if (Math.Abs(v - Math.Round(v)) < 1e-9 && Math.Abs(v) < 1e15)
                    return ((long)Math.Round(v)).ToString(CultureInfo.InvariantCulture);
                return Math.Round(v).ToString("0", CultureInfo.InvariantCulture);
            }

            string fmt = "0." + new string('0', _decimalPlaces);
            return v.ToString(fmt, CultureInfo.InvariantCulture);
        }

        private string GetGridFormat()
        {
            if (_decimalPlaces <= 0) return "0";
            return "0." + new string('0', _decimalPlaces);
        }

        private void RefreshGridFormat()
        {
            if (dataGrid.Columns["Value"] == null) return;
            dataGrid.Columns["Value"].DefaultCellStyle.Format = GetGridFormat();
            dataGrid.Refresh();
        }

        // ============================================================
        //  ФОРМАТ ВРЕМЕНИ — МС С 4 ЗНАКАМИ
        // ============================================================
        private static string FormatTimeTicks(long ticks)
        {
            if (ticks < 0) ticks = 0;
            double freq = Stopwatch.Frequency;
            double ms = ticks * 1e3 / freq;
            return $"{ms:F4} мс";
        }


        // Для интерфейса "итерация" означает именно повтор полного логического прохода.
        // Первый проход существует в алгоритме, но считается проверочным и в счётчик не входит.
        // BOGO оставляем без изменений.
        private static long GetDisplayedIterations(string algorithmName, long rawIterations)
        {
            if (algorithmName == "BOGO")
                return rawIterations;

            return Math.Max(0L, rawIterations - 1L);
        }

        // ============================================================
        //  FULLSCREEN
        // ============================================================
        private void EnterFullscreen()
        {
            var screen = Screen.FromControl(this);
            FormBorderStyle = FormBorderStyle.None;
            Bounds = screen.Bounds;
            WindowState = FormWindowState.Normal;
            _isFullscreen = true;
        }

        private void ExitFullscreen()
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Normal;

            var screen = Screen.FromControl(this);
            int w = Math.Min(1200, screen.WorkingArea.Width - 100);
            int h = Math.Min(700, screen.WorkingArea.Height - 100);
            Size = new Size(w, h);
            Location = new Point(
                screen.WorkingArea.Left + (screen.WorkingArea.Width - w) / 2,
                screen.WorkingArea.Top + (screen.WorkingArea.Height - h) / 2);

            _isFullscreen = false;
            LayoutControls();
        }

        // ============================================================
        //  UI
        // ============================================================
        private void BuildUi()
        {
            // ============================================================
            //  ЛЕВЫЙ САЙДБАР
            // ============================================================
            leftPanel = new Panel { BackColor = Color.FromArgb(18, 18, 18), AutoScroll = true };
            leftPanel.Paint += SidebarPanel_Paint;

            titleLabel = new Label
            {
                Text = "NEONIX",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(titleLabel);

            btnGenerate = MakeSmallButton("Сгенерировать");
            btnExcel    = MakeSmallButton("Импорт Excel");
            btnGoogle   = MakeSmallButton("Google Sheets");
            btnClear    = MakeSmallButton("Очистить");
            btnGenerate.Click += BtnGenerate_Click;
            btnExcel.Click    += BtnExcel_Click;
            btnGoogle.Click   += BtnGoogle_Click;
            btnClear.Click    += BtnClear_Click;
            leftPanel.Controls.Add(btnGenerate);
            leftPanel.Controls.Add(btnExcel);
            leftPanel.Controls.Add(btnGoogle);
            leftPanel.Controls.Add(btnClear);

            cbBubble    = MakeAlgoCheck("Пузырьковая", new Point(0, 0), true);
            cbInsertion = MakeAlgoCheck("Вставками",   new Point(0, 0), false);
            cbShaker    = MakeAlgoCheck("Шейкерная",   new Point(0, 0), false);
            cbQuick     = MakeAlgoCheck("Быстрая",     new Point(0, 0), true);
            cbBogo      = MakeAlgoCheck("BOGO",        new Point(0, 0), false);
            leftPanel.Controls.Add(cbBubble);
            leftPanel.Controls.Add(cbInsertion);
            leftPanel.Controls.Add(cbShaker);
            leftPanel.Controls.Add(cbQuick);
            leftPanel.Controls.Add(cbBogo);

            lblCount = new Label
            {
                Text = "Элементов: 0",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblCount);

            // ============================================================
            //  HEADER
            // ============================================================
            headerPanel = new Panel { BackColor = BgPanel };
            headerPanel.Paint += HeaderPanel_Paint;

            var pageTitle = new Label
            {
                Name = "pageTitle",
                Text = "ВИЗУАЛИЗАЦИЯ СОРТИРОВОК",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = TextSoft,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            headerPanel.Controls.Add(pageTitle);

            // ============================================================
            //  ОПЦИИ
            // ============================================================
            optionsPanel = new Panel { BackColor = Color.FromArgb(18, 18, 18) };
            optionsPanel.Paint += CardPanel_Paint;

            lblDirection = new Label
            {
                Text = "Направление:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AccentLight,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            cmbDirection = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                BackColor = BgPanel,
                ForeColor = TextSoft,
                FlatStyle = FlatStyle.Flat
            };
            cmbDirection.Items.AddRange(new object[] { "По возрастанию", "По убыванию" });
            cmbDirection.SelectedIndex = 0;

            lblDecimalPlaces = new Label
            {
                Text = "Знаков после запятой:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AccentLight,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            cmbDecimalPlaces = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                BackColor = BgPanel,
                ForeColor = TextSoft,
                FlatStyle = FlatStyle.Flat
            };
            for (int i = 0; i <= 8; i++)
                cmbDecimalPlaces.Items.Add(i.ToString());
            cmbDecimalPlaces.SelectedIndex = 3;
            cmbDecimalPlaces.SelectedIndexChanged += (s, e) =>
            {
                _decimalPlaces = cmbDecimalPlaces.SelectedIndex;
                RefreshGridFormat();
                if (workspacePanel.Width > 0 && workspacePanel.Height > 0)
                    workspacePanel.Invalidate();
            };

            lblDelay = new Label
            {
                Text = "Задержка визуализации:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AccentLight,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            tbDelay = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 15,
                TickFrequency = 10,
                BackColor = BgCard
            };
            lblDelayValue = new Label
            {
                Text = "15 мс",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = TextSoft,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            tbDelay.ValueChanged += (s, e) =>
            {
                lblDelayValue.Text = $"{tbDelay.Value} мс";
            };

            optionsPanel.Controls.Add(lblDirection);
            optionsPanel.Controls.Add(cmbDirection);
            optionsPanel.Controls.Add(lblDecimalPlaces);
            optionsPanel.Controls.Add(cmbDecimalPlaces);
            optionsPanel.Controls.Add(lblDelay);
            optionsPanel.Controls.Add(tbDelay);
            optionsPanel.Controls.Add(lblDelayValue);

            // ============================================================
            //  DATA GRID
            // ============================================================
            dataGrid = new DataGridView
            {
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                EditMode = DataGridViewEditMode.EditOnEnter,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersWidth = 40,
                BackgroundColor = BgCard,
                GridColor = Color.FromArgb(55, 55, 55),
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Vertical
            };
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = BgPanel;
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = AccentLight;
            dataGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = BgPanel;
            dataGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGrid.DefaultCellStyle.BackColor = BgDark;
            dataGrid.DefaultCellStyle.ForeColor = TextSoft;
            dataGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(72, 72, 72);
            dataGrid.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGrid.RowHeadersDefaultCellStyle.BackColor = BgPanel;
            dataGrid.RowHeadersDefaultCellStyle.ForeColor = TextDim;
            dataGrid.RowHeadersDefaultCellStyle.SelectionBackColor = BgPanel;
            dataGrid.CellValidating += DataGrid_CellValidating;
            dataGrid.CellMouseEnter += DataGrid_CellMouseEnter;
            dataGrid.CellMouseLeave += DataGrid_CellMouseLeave;
            dataGrid.CellMouseClick += DataGrid_CellMouseClick;
            dataGrid.CellPainting += DataGrid_CellPainting;
            dataGrid.MouseLeave += (s, e) =>
            {
                if (_hoveredRowHeaderIndex >= 0)
                {
                    int oldIndex = _hoveredRowHeaderIndex;
                    _hoveredRowHeaderIndex = -1;
                    dataGrid.Cursor = Cursors.Default;
                    if (oldIndex < dataGrid.Rows.Count)
                        dataGrid.InvalidateCell(-1, oldIndex);
                }
            };

            // ============================================================
            //  WORKSPACE
            // ============================================================
            workspacePanel = new Panel { BackColor = BgCard };
            workspacePanel.Paint += WorkspacePanel_Paint;
            SetDoubleBuffered(workspacePanel);

            workspaceHint = new Label
            {
                Text = "Здесь будет визуализация сортировок.\n\n" +
                       "1. Введите/сгенерируйте/импортируйте данные.\n" +
                       "2. Отметьте алгоритмы слева.\n" +
                       "3. Нажмите «РАССЧИТАТЬ».",
                Font = new Font("Segoe UI", 11F),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            workspacePanel.Controls.Add(workspaceHint);

            // ============================================================
            //  КНОПКИ
            // ============================================================
            btnCalculate = new SmoothButton
            {
                Text = "РАССЧИТАТЬ",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 48),
                Cursor = Cursors.Hand
            };
            if (btnCalculate is SmoothButton calcBtn)
            {
                calcBtn.CornerRadius = 14;
                calcBtn.HoverBackColor = Color.FromArgb(220, 220, 220);
                calcBtn.PressedBackColor = Color.FromArgb(185, 185, 185);
                calcBtn.BorderColor = Color.FromArgb(235, 235, 235);
            }
            btnCalculate.Click += BtnCalculate_Click;

            btnStop = new SmoothButton
            {
                Text = "СТОП",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(48, 48, 48),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 48),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            if (btnStop is SmoothButton stopBtn)
            {
                stopBtn.CornerRadius = 14;
                stopBtn.HoverBackColor = Color.FromArgb(64, 64, 64);
                stopBtn.PressedBackColor = Color.FromArgb(36, 36, 36);
                stopBtn.BorderColor = Color.FromArgb(82, 82, 82);
            }
            btnStop.Click += BtnStop_Click;

            lblStatus = new Label
            {
                Text = "Готово.",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Controls.Add(leftPanel);
            Controls.Add(headerPanel);
            leftPanel.Controls.Add(optionsPanel);
            Controls.Add(dataGrid);
            Controls.Add(workspacePanel);
            Controls.Add(btnCalculate);
            Controls.Add(btnStop);
            Controls.Add(lblStatus);
        }

        private NeonCheckBox MakeAlgoCheck(string text, Point loc, bool isChecked)
        {
            return new NeonCheckBox
            {
                Text = text,
                Location = loc,
                Size = new Size(200, 26),
                Checked = isChecked,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.Transparent,
                CheckColor = Color.White,
                BoxBackColor = Color.FromArgb(34, 34, 34),
                BoxBorderClr = Color.FromArgb(105, 105, 105),
                BoxTextColor = Color.White
            };
        }

        private Button MakeSmallButton(string text)
        {
            var b = new SmoothButton
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(38, 38, 38),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 36),
                Cursor = Cursors.Hand
            };
            if (b is SmoothButton smooth)
            {
                smooth.CornerRadius = 11;
                smooth.BorderColor = Color.FromArgb(72, 72, 72);
                smooth.BorderThickness = 1;
                smooth.HoverBackColor = Color.FromArgb(55, 55, 55);
                smooth.PressedBackColor = Color.FromArgb(28, 28, 28);
            }
            return b;
        }

        private static void SetDoubleBuffered(Control c)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(c, true, null);
        }

        // ============================================================
        //  РАСКЛАДКА
        // ============================================================
        private void LayoutControls()
        {
            // Защита от вызова при свёрнутом окне
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            int sideW = 260;
            int headerH = 60;
            int margin = 20;
            int gap = 15;

            leftPanel.SuspendLayout();
            leftPanel.AutoScrollPosition = Point.Empty;
            leftPanel.Bounds = new Rectangle(0, 0, sideW, ClientSize.Height);
            // Все основные элементы центрируем относительно ширины сайдбара.
            int controlW = 200;
            int controlX = (sideW - controlW) / 2;
            int settingsW = 228;
            int settingsX = (sideW - settingsW) / 2;

            titleLabel.Bounds = new Rectangle(0, 20, sideW, 40);

            // Сначала действия с данными, затем выбор алгоритмов.
            btnGenerate.Bounds = new Rectangle(controlX, 92, controlW, 36);
            btnExcel.Bounds = new Rectangle(controlX, 138, controlW, 36);
            btnGoogle.Bounds = new Rectangle(controlX, 184, controlW, 36);
            btnClear.Bounds = new Rectangle(controlX, 230, controlW, 36);

            int cbTop = 316;
            int cbStep = 28;
            cbBubble.Bounds    = new Rectangle(controlX, cbTop, controlW, 26);
            cbInsertion.Bounds = new Rectangle(controlX, cbTop + cbStep, controlW, 26);
            cbShaker.Bounds    = new Rectangle(controlX, cbTop + cbStep * 2, controlW, 26);
            cbQuick.Bounds     = new Rectangle(controlX, cbTop + cbStep * 3, controlW, 26);
            cbBogo.Bounds      = new Rectangle(controlX, cbTop + cbStep * 4, controlW, 26);

            // Панель настроек шире остальных элементов, но тоже строго по центру.
            optionsPanel.Bounds = new Rectangle(settingsX, 484, settingsW, 242);
            int settingsPad = 12;
            int settingsInnerW = settingsW - settingsPad * 2;
            lblDirection.Location = new Point(settingsPad, 10);
            cmbDirection.SetBounds(settingsPad, 32, settingsInnerW, 26);
            lblDecimalPlaces.Location = new Point(settingsPad, 76);
            cmbDecimalPlaces.SetBounds(settingsPad, 98, settingsInnerW, 26);
            lblDelay.Location = new Point(settingsPad, 142);
            // Размер и положение оставлены такими же, как в 111.
            int delayValueW = 44;
            int delayGap = 2;
            int sliderW = Math.Max(80, settingsInnerW - delayValueW - delayGap);
            tbDelay.SetBounds(settingsPad, 162, sliderW, 46);
            lblDelayValue.SetBounds(settingsPad + sliderW + delayGap, 168, delayValueW, 22);
            lblDelayValue.TextAlign = ContentAlignment.MiddleLeft;

            int countY = Math.Max(744, leftPanel.ClientSize.Height - 34);
            lblCount.Bounds = new Rectangle(controlX, countY, controlW, 24);
            leftPanel.AutoScrollMinSize = new Size(0, Math.Max(780, countY + 34));
            leftPanel.ResumeLayout();

            headerPanel.Bounds = new Rectangle(sideW, 0,
                Math.Max(1, ClientSize.Width - sideW), headerH);
            var pageTitle = headerPanel.Controls["pageTitle"];
            if (pageTitle != null) pageTitle.Location = new Point(margin, 12);

            // Таблица и визуализация начинаются сразу под заголовком.
            int mainLeft = sideW + margin;
            int contentTop = headerH + margin;
            int mainWidth = Math.Max(1, ClientSize.Width - mainLeft - margin);
            int contentHeight = Math.Max(1, ClientSize.Height - contentTop - 90);
            int gridW = Math.Min(260, Math.Max(120, mainWidth / 4));
            dataGrid.Bounds = new Rectangle(mainLeft, contentTop, gridW, contentHeight);
            int wsLeft = mainLeft + gridW + gap;
            workspacePanel.Bounds = new Rectangle(wsLeft, contentTop,
                Math.Max(1, mainWidth - gridW - gap), contentHeight);
            int buttonTop = contentTop + contentHeight + 12;
            btnCalculate.Bounds = new Rectangle(mainLeft + mainWidth - 200, buttonTop, 200, 48);
            btnStop.Bounds = new Rectangle(btnCalculate.Left - 130, buttonTop, 120, 48);
            lblStatus.Bounds = new Rectangle(mainLeft, contentTop + contentHeight + 22,
                Math.Max(1, btnStop.Left - mainLeft - 10), 24);


            if (workspacePanel.Width > 0 && workspacePanel.Height > 0)
                workspacePanel.Invalidate();
            if (leftPanel.Width > 0 && leftPanel.Height > 0)
                leftPanel.Invalidate();
            if (headerPanel.Width > 0 && headerPanel.Height > 0)
                headerPanel.Invalidate();
        }

        // ============================================================
        //  РИСОВАНИЕ
        // ============================================================
        private void SidebarPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            var rect = leftPanel.ClientRectangle;

            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (var brush = new SolidBrush(Color.FromArgb(18, 18, 18)))
            {
                g.FillRectangle(brush, rect);
            }

            var saved = g.Save();
            g.TranslateTransform(leftPanel.AutoScrollPosition.X, leftPanel.AutoScrollPosition.Y);
            using (var sepPen = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
                g.DrawLine(sepPen, 30, 78, rect.Width - 30, 78);
            using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.FromArgb(205, 205, 205)))
                g.DrawString("АЛГОРИТМЫ СОРТИРОВКИ", font, textBrush, 30, 294);
            g.Restore(saved);
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = headerPanel.ClientRectangle;

            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (var brush = new SolidBrush(BgPanel))
                g.FillRectangle(brush, rect);

            using (var pen = new Pen(Color.FromArgb(65, 65, 65), 1))
                g.DrawLine(pen, 0, rect.Bottom - 1, rect.Width, rect.Bottom - 1);
        }

        private void CardPanel_Paint(object sender, PaintEventArgs e)
        {
            var p = (Panel)sender;
            if (p.Width <= 0 || p.Height <= 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            var rect = new Rectangle(1, 1, Math.Max(1, p.Width - 3), Math.Max(1, p.Height - 3));
            using var path = UiRound.Path(rect, 14);
            using var fill = new SolidBrush(BgCard);
            using var pen = new Pen(Color.FromArgb(68, 68, 68), 1);
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }

        // ============================================================
        //  ВИЗУАЛИЗАЦИЯ
        // ============================================================
        private void WorkspacePanel_Paint(object sender, PaintEventArgs e)
        {
            if (workspacePanel.Width <= 0 || workspacePanel.Height <= 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.Clear(BgDark);
            using (var outerPath = UiRound.Path(new Rectangle(1, 1, Math.Max(1, workspacePanel.Width - 3), Math.Max(1, workspacePanel.Height - 3)), 16))
            using (var fill = new SolidBrush(BgCard))
            using (var borderPen = new Pen(Color.FromArgb(68, 68, 68), 1))
            {
                g.FillPath(fill, outerPath);
                g.DrawPath(borderPen, outerPath);
            }

            if (_states.Count == 0) return;

            bool anyVisual = _states.Any(s => s.Visualize);
            if (!anyVisual)
            {
                workspaceHint.Visible = true;
                return;
            }

            workspaceHint.Visible = false;

            var visualStates = _states.Where(s => s.Visualize).ToList();
            int n = visualStates.Count;
            int cols = n <= 2 ? n : (n <= 4 ? 2 : 3);
            int rows = (int)Math.Ceiling(n / (double)cols);

            int pad = 12;
            int cellW = (workspacePanel.Width - pad * (cols + 1)) / cols;
            int cellH = (workspacePanel.Height - pad * (rows + 1)) / rows;

            if (cellW <= 0 || cellH <= 0) return;

            for (int idx = 0; idx < n; idx++)
            {
                int r = idx / cols;
                int c = idx % cols;
                var area = new Rectangle(
                    pad + c * (cellW + pad),
                    pad + r * (cellH + pad),
                    cellW,
                    cellH);

                DrawAlgorithm(g, visualStates[idx], area);
            }
        }

        private void DrawAlgorithm(Graphics g, AlgorithmState st, Rectangle area)
        {
            if (area.Width <= 0 || area.Height <= 0) return;

            var snap = st.Snapshot();
            var data = snap.data;

            using var titleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            using var infoFont = new Font("Segoe UI", 8F);
            using var valueFont = new Font("Segoe UI", 7F, FontStyle.Bold);

            string title = st.Name;
            if (snap.finished)
                title += snap.err != null ? "  ✗" : "  ✓";

            using (var cardPath = UiRound.Path(new Rectangle(area.Left + 1, area.Top + 1, Math.Max(1, area.Width - 2), Math.Max(1, area.Height - 2)), 10))
            using (var cardFill = new SolidBrush(Color.FromArgb(24, 24, 24)))
            using (var cardBorder = new Pen(Color.FromArgb(62, 62, 62), 1))
            {
                g.FillPath(cardFill, cardPath);
                g.DrawPath(cardBorder, cardPath);
            }

            using var bgHeader = new SolidBrush(Color.FromArgb(34, 34, 34));
            var headerRect = new Rectangle(area.Left + 1, area.Top + 1, Math.Max(1, area.Width - 2), 24);
            using (var headerPath = UiRound.Path(headerRect, 10))
                g.FillPath(bgHeader, headerPath);

            using var titleBrush = new SolidBrush(st.BarColor);
            g.DrawString(title, titleFont, titleBrush, area.Left + 6, area.Top + 2);

            if (snap.finished)
            {
                string ms = snap.err != null
                    ? "ошибка"
                    : $"≈ {FormatTimeTicks(snap.avgTicks)}  (×{snap.runs})";
                var msSize = g.MeasureString(ms, infoFont);
                using var msBrush = new SolidBrush(snap.err != null ? Color.FromArgb(175, 175, 175) : OkClr);
                g.DrawString(ms, infoFont, msBrush,
                    area.Right - msSize.Width - 6, area.Top + 5);
            }

            var plot = new Rectangle(
                area.Left + 4,
                area.Top + 24,
                Math.Max(1, area.Width - 8),
                Math.Max(1, area.Height - 24 - 22));

            using (var borderPen = new Pen(Color.FromArgb(70, 70, 70), 1))
                g.DrawRectangle(borderPen, plot);

            if (data.Length == 0) return;

            double maxVal = data.Max();
            double minVal = data.Min();
            if (Math.Abs(maxVal - minVal) < 1e-12) maxVal = minVal + 1;

            float barW = plot.Width / (float)data.Length;

            double maxAbsVal = Math.Max(Math.Abs(maxVal), Math.Abs(minVal));
            var maxAbsStr = FormatValue(maxAbsVal);
            var maxStrSize = g.MeasureString(maxAbsStr, valueFont);
            bool showAllValues = barW >= maxStrSize.Width + 2;
            bool showHighlightedValues = barW >= 10;

            using var baseBrush = new SolidBrush(st.BarColor);
            using var cmpBrush = new SolidBrush(CompareClr);
            using var swpBrush = new SolidBrush(SwapClr);
            using var valueBrush = new SolidBrush(TextSoft);
            using var valueBrushHi = new SolidBrush(Color.FromArgb(18, 18, 18));

            for (int i = 0; i < data.Length; i++)
            {
                float norm = (float)((data[i] - minVal + 0.5) / (maxVal - minVal + 1.0));
                float h = Math.Max(2f, norm * (plot.Height - 4 - 14));
                float x = plot.Left + i * barW;
                float y = plot.Bottom - h;
                var rect = new RectangleF(x, y, Math.Max(1f, barW - 0.8f), h);

                bool isSwap = (i == snap.si || i == snap.sj);
                bool isCmp = (i == snap.ci || i == snap.cj);

                Brush brush = baseBrush;
                if (isSwap) brush = swpBrush;
                else if (isCmp) brush = cmpBrush;

                g.FillRectangle(brush, rect);

                string valueText = FormatValue(data[i]);
                var textSize = g.MeasureString(valueText, valueFont);

                bool drawThis = showAllValues || (showHighlightedValues && (isSwap || isCmp));
                if (drawThis)
                {
                    float tx = x + (barW - textSize.Width) / 2f;
                    float ty = y - textSize.Height - 1f;
                    if (ty < plot.Top) ty = y + 1f;

                    var brushText = (isSwap || isCmp) ? valueBrushHi : valueBrush;
                    g.DrawString(valueText, valueFont, brushText, tx, ty);
                }
            }

            string info = $"сравн.: {snap.cmp}   обменов: {snap.swp}   итераций: {snap.iter}";
            using var infoBrush = new SolidBrush(TextDim);
            g.DrawString(info, infoFont, infoBrush, plot.Left + 2, plot.Bottom + 2);
        }

        // ============================================================
        //  ДАННЫЕ
        // ============================================================
        private void SetupGrid()
        {
            dataGrid.Columns.Clear();
            var col = new DataGridViewTextBoxColumn
            {
                Name = "Value",
                HeaderText = "Значение",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = false,
                Visible = true
            };
            col.DefaultCellStyle.Format = GetGridFormat();
            dataGrid.Columns.Add(col);
        }

        private void DataGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != -1 || e.RowIndex < 0 || e.RowIndex >= dataGrid.Rows.Count)
                return;

            if (dataGrid.Rows[e.RowIndex].IsNewRow)
                return;

            if (_hoveredRowHeaderIndex != e.RowIndex)
            {
                int oldIndex = _hoveredRowHeaderIndex;
                _hoveredRowHeaderIndex = e.RowIndex;
                dataGrid.Cursor = Cursors.Hand;

                if (oldIndex >= 0 && oldIndex < dataGrid.Rows.Count)
                    dataGrid.InvalidateCell(-1, oldIndex);

                dataGrid.InvalidateCell(-1, e.RowIndex);
            }
        }

        private void DataGrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != -1 || e.RowIndex != _hoveredRowHeaderIndex)
                return;

            int oldIndex = _hoveredRowHeaderIndex;
            _hoveredRowHeaderIndex = -1;
            dataGrid.Cursor = Cursors.Default;

            if (oldIndex >= 0 && oldIndex < dataGrid.Rows.Count)
                dataGrid.InvalidateCell(-1, oldIndex);
        }

        private void DataGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left ||
                e.ColumnIndex != -1 ||
                e.RowIndex < 0 ||
                e.RowIndex >= dataGrid.Rows.Count ||
                dataGrid.Rows[e.RowIndex].IsNewRow)
                return;

            // Завершаем редактирование, если оно было активно.
            if (dataGrid.IsCurrentCellInEditMode)
                dataGrid.EndEdit();

            _hoveredRowHeaderIndex = -1;
            dataGrid.Cursor = Cursors.Default;
            dataGrid.Rows.RemoveAt(e.RowIndex);
            SyncDataFromGrid();
        }

        private void DataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex != -1 ||
                e.RowIndex < 0 ||
                e.RowIndex >= dataGrid.Rows.Count ||
                dataGrid.Rows[e.RowIndex].IsNewRow ||
                e.RowIndex != _hoveredRowHeaderIndex)
                return;

            // На наведённом заголовке строки вместо стандартной стрелки рисуем кнопку удаления.
            e.PaintBackground(e.ClipBounds, true);

            var rect = e.CellBounds;
            using (var hoverBrush = new SolidBrush(Color.FromArgb(46, 46, 46)))
                e.Graphics.FillRectangle(hoverBrush, rect);

            using (var pen = new Pen(Color.FromArgb(235, 235, 235), 2f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                int cx = rect.Left + rect.Width / 2;
                int cy = rect.Top + rect.Height / 2;
                int r = 5;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawLine(pen, cx - r, cy - r, cx + r, cy + r);
                e.Graphics.DrawLine(pen, cx + r, cy - r, cx - r, cy + r);
            }

            e.Handled = true;
        }

        private void DataGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != 0) return;
            if (e.RowIndex < 0) return;

            var text = e.FormattedValue?.ToString();
            if (string.IsNullOrWhiteSpace(text)) return;

            if (!TryParseDouble(text, out _))
            {
                MessageBox.Show(
                    $"Некорректное значение \"{text}\" в строке {e.RowIndex + 1}. Ожидается число.",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        }

        private void RemoveEmptyDataRows()
        {
            // Удаляем только обычные пустые строки.
            // Нижнюю служебную строку DataGridView для добавления нового значения не трогаем.
            for (int i = dataGrid.Rows.Count - 1; i >= 0; i--)
            {
                var row = dataGrid.Rows[i];
                if (row.IsNewRow) continue;

                object value = row.Cells[0].Value;
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    dataGrid.Rows.RemoveAt(i);
            }
        }

        private void SyncDataFromGrid()
        {
            try
            {
                var list = new List<double>();
                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    if (row.IsNewRow) continue;
                    var cell = row.Cells[0];
                    var val = cell.Value;
                    if (val == null) continue;

                    double d;
                    if (val is double dv) d = dv;
                    else if (!TryParseDouble(val.ToString(), out d)) continue;

                    list.Add(d);
                }
                _currentData = list.ToArray();
                lblCount.Text = $"Элементов: {_currentData.Length}";
            }
            catch { }
        }

        private void LoadDataToGrid(IEnumerable<double> data)
        {
            dataGrid.SuspendLayout();
            bool prevAllow = dataGrid.AllowUserToAddRows;
            dataGrid.AllowUserToAddRows = false;
            dataGrid.Rows.Clear();

            var list = data.ToList();
            for (int i = 0; i < list.Count; i++)
                dataGrid.Rows.Add();
            for (int i = 0; i < list.Count; i++)
                dataGrid.Rows[i].Cells[0].Value = list[i];

            dataGrid.AllowUserToAddRows = prevAllow;
            dataGrid.ResumeLayout();
            dataGrid.Refresh();
            dataGrid.Invalidate();

            SyncDataFromGrid();
        }

        // ============================================================
        //  ФАБРИКА
        // ============================================================
        private static SortingAlgorithm CreateAlgoByName(string name)
        {
            switch (name)
            {
                case "Пузырьковая": return new BubbleSort();
                case "Вставками":   return new InsertionSort();
                case "Шейкерная":   return new ShakerSort();
                case "Быстрая":     return new QuickSort();
                case "BOGO":        return new BogoSort();
                default: throw new ArgumentException("Неизвестный алгоритм: " + name);
            }
        }

        // ============================================================
        //  ГЕНЕРАЦИЯ
        // ============================================================
        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            using var dlg = new GenerateForm();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var rnd = new Random();
            var data = new double[dlg.Count];
            double min = (double)dlg.Min;
            double max = (double)dlg.Max;

            for (int i = 0; i < dlg.Count; i++)
            {
                double v = min + rnd.NextDouble() * (max - min);
                data[i] = Math.Round(v, _decimalPlaces);
            }
            LoadDataToGrid(data);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (_isRunning) return;

            dataGrid.CancelEdit();
            dataGrid.Rows.Clear();
            _currentData = Array.Empty<double>();
            _states.Clear();

            lblCount.Text = "Элементов: 0";
            lblStatus.Text = "Данные очищены.";

            workspaceHint.Text = "Здесь будет визуализация сортировок.\n\n" +
                                 "1. Введите/сгенерируйте/импортируйте данные.\n" +
                                 "2. Отметьте алгоритмы слева.\n" +
                                 "3. Нажмите «РАССЧИТАТЬ».";
            workspaceHint.Font = new Font("Segoe UI", 11F);
            workspaceHint.ForeColor = TextDim;
            workspaceHint.Visible = true;

            dataGrid.Refresh();
            workspacePanel.Invalidate();
        }

        // ============================================================
        //  EXCEL
        // ============================================================
        private void BtnExcel_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Excel файлы (*.xlsx;*.xls)|*.xlsx;*.xls|Все файлы (*.*)|*.*",
                Title = "Выберите Excel-файл"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var data = LoadFromExcel(dlg.FileName);
                if (data.Count == 0)
                {
                    MessageBox.Show("Файл не содержит числовых данных.",
                        "Пустой файл", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                LoadDataToGrid(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки Excel: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<double> LoadFromExcel(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var result = new List<double>();

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            do
            {
                while (reader.Read())
                {
                    if (reader.FieldCount == 0) continue;
                    var val = reader.GetValue(0);
                    if (val == null) continue;

                    if (val is double d) { result.Add(d); continue; }
                    if (val is int i2) { result.Add(i2); continue; }
                    if (val is decimal m) { result.Add((double)m); continue; }
                    if (val is long l) { result.Add(l); continue; }

                    if (TryParseDouble(val.ToString(), out double num))
                        result.Add(num);
                    else
                        throw new FormatException($"Значение \"{val}\" не является числом.");
                }
            } while (reader.NextResult());

            return result;
        }

        // ============================================================
        //  GOOGLE SHEETS
        // ============================================================
        private async void BtnGoogle_Click(object sender, EventArgs e)
        {
            using var dlg = new GoogleLinkForm();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var data = await LoadFromGoogleSheetsByLink(dlg.SheetUrl, dlg.UseHtml);
                if (data.Count == 0)
                {
                    MessageBox.Show("Таблица не содержит числовых данных.",
                        "Пустая таблица", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                LoadDataToGrid(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки Google Sheets: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<double>> LoadFromGoogleSheetsByLink(string url, bool useHtml)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Пустая ссылка.");

            string spreadsheetId = ExtractSpreadsheetId(url);
            string gid = ExtractGid(url) ?? "0";

            string csvUrl = useHtml
                ? $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/gviz/tq?tqx=out:html&gid={gid}"
                : $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv&gid={gid}";

            string content;
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                content = await client.GetStringAsync(csvUrl);
            }

            var result = new List<double>();

            if (useHtml)
            {
                var matches = Regex.Matches(content, @"<td[^>]*>(.*?)</td>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
                foreach (Match m in matches)
                {
                    var text = WebUtility.HtmlDecode(m.Groups[1].Value).Trim();
                    if (TryParseDouble(text, out double num))
                        result.Add(num);
                }
            }
            else
            {
                var lines = content.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var first = line.Split(',')[0].Trim().Trim('"');
                    if (TryParseDouble(first, out double num))
                        result.Add(num);
                }
            }

            return result;
        }

        private static string ExtractSpreadsheetId(string url)
        {
            var m = Regex.Match(url, @"/spreadsheets/d/([a-zA-Z0-9-_]+)");
            if (m.Success) return m.Groups[1].Value;
            throw new FormatException(
                "Не удалось извлечь ID таблицы из ссылки. Проверьте, что ссылка вида " +
                "https://docs.google.com/spreadsheets/d/XXXXXXXXX/edit");
        }

        private static string ExtractGid(string url)
        {
            var m = Regex.Match(url, @"[#&]gid=(\d+)");
            return m.Success ? m.Groups[1].Value : null;
        }

        // ============================================================
        //  СТОП
        // ============================================================
        private void BtnStop_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            lblStatus.Text = "Остановка...";
        }

        // ============================================================
        //  РАССЧИТАТЬ
        // ============================================================
        private async void BtnCalculate_Click(object sender, EventArgs e)
        {
            if (_isRunning) return;

            SyncDataFromGrid();
            if (_currentData.Length == 0)
            {
                MessageBox.Show("Нет данных для сортировки.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbBogo.Checked && _currentData.Length > MaxBogoElements)
            {
                MessageBox.Show(
                    $"BOGO-сортировку нельзя запускать, если элементов больше {MaxBogoElements}.\n" +
                    $"Сейчас элементов: {_currentData.Length}.",
                    "Ограничение BOGO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool hasSlowAlgorithm = cbBubble.Checked || cbInsertion.Checked || cbShaker.Checked;
            if (hasSlowAlgorithm && _currentData.Length > SlowAlgorithmWarnThreshold)
            {
                var result = MessageBox.Show(
                    $"Выбраны медленные алгоритмы (O(n²)), а элементов — {_currentData.Length}.\n\n" +
                    "Это может занять очень много времени (десятки секунд или минут).\n\n" +
                    "Продолжить?",
                    "Большой массив",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes) return;
            }

            var selectedNames = new List<string>();
            if (cbBubble.Checked)    selectedNames.Add("Пузырьковая");
            if (cbInsertion.Checked) selectedNames.Add("Вставками");
            if (cbShaker.Checked)    selectedNames.Add("Шейкерная");
            if (cbQuick.Checked)     selectedNames.Add("Быстрая");
            if (cbBogo.Checked)      selectedNames.Add("BOGO");

            if (selectedNames.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один алгоритм сортировки.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isRunning = true;
            btnCalculate.Enabled = false;
            btnStop.Enabled = true;
            btnGenerate.Enabled = false;
            btnExcel.Enabled = false;
            btnGoogle.Enabled = false;
            btnClear.Enabled = false;
            tbDelay.Enabled = false;
            cmbDecimalPlaces.Enabled = false;

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            bool asc = cmbDirection.SelectedIndex == 0;
            bool visualize = _currentData.Length <= MaxVisualizeElements;
            int delayMs = tbDelay.Value;

            lblStatus.Text = visualize
                ? $"Замер ({RunsPerAlgorithm} прогонов, кроме BOGO)..."
                : $"Сортировка выполняется (визуализация отключена, элементов > {MaxVisualizeElements}).";

            _states = new List<AlgorithmState>();
            foreach (var name in selectedNames)
            {
                var st = new AlgorithmState
                {
                    Name = name,
                    BarColor = _algoColors.TryGetValue(name, out var c) ? c : AccentRed,
                    Data = (double[])_currentData.Clone(),
                    Visualize = visualize
                };
                _states.Add(st);
            }

            workspaceHint.Visible = false;
            if (workspacePanel.Width > 0 && workspacePanel.Height > 0)
                workspacePanel.Invalidate();

            // ФАЗА 1
            var measureTasks = new List<Task>();

            foreach (var name in selectedNames)
            {
                var algoName = name;
                var st = _states.First(s => s.Name == algoName);
                bool isBogo = (algoName == "BOGO");
                int runs = isBogo ? 1 : RunsPerAlgorithm;

                measureTasks.Add(Task.Run(() =>
                {
                    var times = new List<long>(runs);
                    long firstCmp = 0, firstSwp = 0, firstIter = 0;
                    string error = null;

                    for (int run = 0; run < runs; run++)
                    {
                        if (token.IsCancellationRequested) break;

                        var source = (double[])_currentData.Clone();
                        Array.Copy(source, st.Data, source.Length);

                        var algo = CreateAlgoByName(algoName);
                        algo.Ascending = asc;

                        bool collectThis = (run == 0);
                        long localCmp = 0, localSwp = 0;

                        if (collectThis && visualize)
                        {
                            algo.OnCompare += (i, j) =>
                            {
                                System.Threading.Interlocked.Increment(ref localCmp);
                                lock (st.LogSync) st.EventLog.Add((0, i, j));
                            };
                            algo.OnSwap += (i, j) =>
                            {
                                System.Threading.Interlocked.Increment(ref localSwp);
                                lock (st.LogSync) st.EventLog.Add((1, i, j));
                            };
                            algo.OnIteration += () =>
                            {
                                lock (st.LogSync) st.EventLog.Add((2, -1, -1));
                            };
                        }
                        else if (collectThis)
                        {
                            algo.OnCompare += (i, j) => System.Threading.Interlocked.Increment(ref localCmp);
                            algo.OnSwap    += (i, j) => System.Threading.Interlocked.Increment(ref localSwp);
                        }

                        long startTicks = Stopwatch.GetTimestamp();
                        try { algo.Sort(st.Data); }
                        catch (Exception ex)
                        {
                            error = ex.Message;
                            break;
                        }
                        long endTicks = Stopwatch.GetTimestamp();
                        long elapsedTicks = endTicks - startTicks;

                        times.Add(elapsedTicks);

                        if (collectThis)
                        {
                            firstCmp = localCmp;
                            firstSwp = localSwp;
                            firstIter = GetDisplayedIterations(algoName, algo.Iterations);
                        }

                        lock (st.Sync) st.Runs = run + 1;
                    }

                    lock (st.Sync)
                    {
                        if (times.Count > 0)
                        {
                            long sum = 0;
                            foreach (var t in times) sum += t;

                            st.AvgTicks = sum / times.Count;
                            st.MinTicks = times.Min();
                            st.MaxTicks = times.Max();
                        }
                        // При визуализации счётчики стартуют с нуля и растут
                        // непосредственно во время воспроизведения событий.
                        if (visualize)
                        {
                            st.Comparisons = 0;
                            st.Swaps = 0;
                            st.Iterations = 0;
                        }
                        else
                        {
                            st.Comparisons = firstCmp;
                            st.Swaps = firstSwp;
                            st.Iterations = firstIter;
                        }
                        st.Error = error;
                    }
                }, token));
            }

            try { await Task.WhenAll(measureTasks); }
            catch (OperationCanceledException) { }

            // ФАЗА 2
            if (visualize && !token.IsCancellationRequested)
            {
                lblStatus.Text = "Визуализация... счётчики обновляются в реальном времени";

                var playTasks = new List<Task>();

                foreach (var name in selectedNames)
                {
                    var st = _states.First(s => s.Name == name);

                    playTasks.Add(Task.Run(() =>
                    {
                        var working = (double[])_currentData.Clone();
                        lock (st.Sync)
                        {
                            Array.Copy(working, st.Data, working.Length);
                            st.CompareI = st.CompareJ = st.SwapI = st.SwapJ = -1;
                            st.Comparisons = 0;
                            st.Swaps = 0;
                            st.Iterations = 0;
                        }

                        List<(int type, int i, int j)> log;
                        lock (st.LogSync) log = new List<(int, int, int)>(st.EventLog);

                        // Реализацию алгоритмов не меняем: события OnIteration остаются как есть.
                        // Для всех сортировок, кроме BOGO, первый полный проход — проверочный.
                        // Поэтому отображаем количество ПОВТОРОВ: все проходы, кроме первого.
                        int remainingIterationEvents = log.Count(e => e.type == 2);

                        foreach (var ev in log)
                        {
                            if (token.IsCancellationRequested) break;

                            if (ev.type == 0)
                            {
                                lock (st.Sync)
                                {
                                    st.CompareI = ev.i;
                                    st.CompareJ = ev.j;
                                    st.Comparisons++;
                                }
                            }
                            else if (ev.type == 1)
                            {
                                lock (st.Sync)
                                {
                                    if (ev.i >= 0 && ev.j >= 0 &&
                                        ev.i < st.Data.Length && ev.j < st.Data.Length)
                                    {
                                        (st.Data[ev.i], st.Data[ev.j]) = (st.Data[ev.j], st.Data[ev.i]);
                                    }
                                    st.SwapI = ev.i;
                                    st.SwapJ = ev.j;
                                    st.Swaps++;
                                }
                            }
                            else if (ev.type == 2)
                            {
                                if (st.Name == "BOGO")
                                {
                                    // BOGO не трогаем — считаем его итерации как раньше.
                                    lock (st.Sync)
                                    {
                                        st.Iterations++;
                                    }
                                }
                                else
                                {
                                    // Событие приходит в конце полного логического прохода.
                                    // Если после него остаётся ещё один проход, значит алгоритм
                                    // начинает повтор — именно его и считаем как +1 итерацию.
                                    remainingIterationEvents--;

                                    if (remainingIterationEvents > 0)
                                    {
                                        lock (st.Sync)
                                        {
                                            st.Iterations++;
                                        }
                                    }
                                }
                            }

                            if (delayMs > 0)
                            {
                                int slept = 0;
                                while (slept < delayMs && !token.IsCancellationRequested)
                                {
                                    int chunk = Math.Min(10, delayMs - slept);
                                    Thread.Sleep(chunk);
                                    slept += chunk;
                                }
                            }
                        }

                        lock (st.Sync)
                        {
                            st.CompareI = st.CompareJ = st.SwapI = st.SwapJ = -1;
                            st.Finished = true;
                        }
                    }, token));
                }

                try { await Task.WhenAll(playTasks); }
                catch (OperationCanceledException) { }
            }
            else
            {
                foreach (var st in _states)
                {
                    lock (st.Sync) st.Finished = true;
                }
            }

            // ОТЧЁТ
            var ordered = _states.OrderBy(s => s.AvgTicks).ToList();
            var sb = new StringBuilder();

            double freq = Stopwatch.Frequency;
            string resolution = freq >= 1_000_000_000
                ? $"{1e12 / freq:F0} пс"
                : freq >= 1_000_000
                    ? $"{1e9 / freq:F1} нс"
                    : freq >= 1_000
                        ? $"{1e6 / freq:F2} мкс"
                        : $"{1e3 / freq:F4} мс";

            if (token.IsCancellationRequested)
                sb.AppendLine("=== Результаты (остановлено пользователем) ===");
            else
                sb.AppendLine($"=== Результаты (разрешение таймера: {resolution}, {freq:F0} Гц) ===");

            sb.AppendLine(
                $"{"Алгоритм",-16}{"Среднее",-20}{"Мин.",-20}{"Макс.",-20}" +
                $"{"Прогонов",10}{"Итерации",12}{"Сравн.",14}{"Обмены",12}");
            sb.AppendLine(new string('-', 124));

            foreach (var s in ordered)
            {
                var snap = s.Snapshot();
                if (snap.err != null)
                {
                    sb.AppendLine(
                        $"{s.Name,-16}{"ошибка",-20}{"",-20}{"",-20}" +
                        $"{snap.runs,10}{snap.iter,12}{snap.cmp,14}{snap.swp,12}  {snap.err}");
                }
                else
                {
                    sb.AppendLine(
                        $"{s.Name,-16}{FormatTimeTicks(snap.avgTicks),-20}{FormatTimeTicks(snap.minTicks),-20}{FormatTimeTicks(snap.maxTicks),-20}" +
                        $"{snap.runs,10}{snap.iter,12}{snap.cmp,14}{snap.swp,12}");
                }
            }

            var fastest = ordered.FirstOrDefault(s => s.Error == null && s.Runs > 0);
            if (fastest != null)
                sb.AppendLine($"\nСамый быстрый (по среднему): {fastest.Name} ({FormatTimeTicks(fastest.AvgTicks)})");

            if (!visualize)
                sb.AppendLine($"\nВизуализация отключена: элементов больше {MaxVisualizeElements}.");

            if (token.IsCancellationRequested)
                sb.AppendLine("\n*** Остановлено пользователем ***");

            workspaceHint.Text = sb.ToString();
            workspaceHint.Font = new Font("Consolas", 10F);
            workspaceHint.ForeColor = TextSoft;
            workspaceHint.Visible = true;

            lblStatus.Text = token.IsCancellationRequested
                ? "Остановлено пользователем."
                : $"Готово. {_states.Count} алгоритм(ов). По {RunsPerAlgorithm} прогонов (BOGO — 1).";

            _isRunning = false;
            btnCalculate.Enabled = true;
            btnStop.Enabled = false;
            btnGenerate.Enabled = true;
            btnExcel.Enabled = true;
            btnGoogle.Enabled = true;
            btnClear.Enabled = true;
            tbDelay.Enabled = true;
            cmbDecimalPlaces.Enabled = true;
            if (workspacePanel.Width > 0 && workspacePanel.Height > 0)
                workspacePanel.Invalidate();
        }

        // ============================================================
        //  КЛАВИШИ
        // ============================================================
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_isRunning)
                {
                    _cts?.Cancel();
                    lblStatus.Text = "Остановка...";
                }
                else if (_isFullscreen) ExitFullscreen();
                else Close();
            }
            else if (e.KeyCode == Keys.F11)
            {
                if (_isFullscreen) ExitFullscreen();
                else EnterFullscreen();
            }
        }
    }

    // ============================================================
    //  ФОРМА: ГЕНЕРАЦИЯ
    // ============================================================
    public class GenerateForm : Form
    {
        private NumericUpDown nudCount, nudMin, nudMax;

        public int Count => (int)nudCount.Value;
        public decimal Min => nudMin.Value;
        public decimal Max => nudMax.Value;

        public GenerateForm()
        {
            Text = "Генерация данных";
            ClientSize = new Size(360, 220);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.FromArgb(28, 28, 28);
            ForeColor = Color.FromArgb(242, 242, 242);

            var lblCount = new Label { Text = "Количество:", Left = 10, Top = 15, Width = 120, ForeColor = Color.FromArgb(242, 242, 242) };
            nudCount = new NumericUpDown
            {
                Left = 140, Top = 12, Width = 160,
                Minimum = 1,
                Maximum = 1_000_000,
                Value = 15,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.FromArgb(242, 242, 242)
            };

            var lblMin = new Label { Text = "Минимум:", Left = 10, Top = 50, Width = 120, ForeColor = Color.FromArgb(242, 242, 242) };
            nudMin = new NumericUpDown
            {
                Left = 140, Top = 47, Width = 160,
                Minimum = -1_000_000, Maximum = 1_000_000, Value = 0,
                DecimalPlaces = 2, Increment = 0.5m,
                BackColor = Color.FromArgb(18, 18, 18), ForeColor = Color.FromArgb(242, 242, 242)
            };

            var lblMax = new Label { Text = "Максимум:", Left = 10, Top = 85, Width = 120, ForeColor = Color.FromArgb(242, 242, 242) };
            nudMax = new NumericUpDown
            {
                Left = 140, Top = 82, Width = 160,
                Minimum = -1_000_000, Maximum = 1_000_000, Value = 100,
                DecimalPlaces = 2, Increment = 0.5m,
                BackColor = Color.FromArgb(18, 18, 18), ForeColor = Color.FromArgb(242, 242, 242)
            };

            var btnOk = new SmoothButton
            {
                Text = "Сгенерировать", Left = 120, Top = 158, Width = 120, Height = 36,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(238, 238, 238),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.CornerRadius = 8;
            btnOk.BorderColor = Color.FromArgb(238, 238, 238);
            btnOk.BorderThickness = 1;
            btnOk.HoverBackColor = Color.FromArgb(218, 218, 218);
            btnOk.PressedBackColor = Color.FromArgb(185, 185, 185);

            var btnCancel = new SmoothButton
            {
                Text = "Отмена", Left = 250, Top = 158, Width = 90, Height = 36,
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.FromArgb(242, 242, 242),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.CornerRadius = 8;
            btnCancel.BorderColor = Color.FromArgb(78, 78, 78);
            btnCancel.BorderThickness = 1;
            btnCancel.HoverBackColor = Color.FromArgb(46, 46, 46);
            btnCancel.PressedBackColor = Color.FromArgb(24, 24, 24);

            btnOk.Click += (s, e) =>
            {
                if (nudMin.Value >= nudMax.Value)
                {
                    MessageBox.Show("Минимум должен быть меньше максимума.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };

            Controls.AddRange(new Control[] { lblCount, nudCount, lblMin, nudMin, lblMax, nudMax, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }

    // ============================================================
    //  ФОРМА: GOOGLE SHEETS
    // ============================================================
    public class GoogleLinkForm : Form
    {
        private TextBox txtUrl;
        private CheckBox chkHtml;

        public string SheetUrl => txtUrl.Text.Trim();
        public bool UseHtml => chkHtml.Checked;

        public GoogleLinkForm()
        {
            Text = "Импорт из Google Sheets";
            ClientSize = new Size(580, 250);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.FromArgb(28, 28, 28);
            ForeColor = Color.FromArgb(242, 242, 242);

            var lbl = new Label
            {
                Text = "Вставьте ссылку на Google-таблицу:\n" +
                       "(таблица должна быть опубликована или доступна по ссылке)",
                Left = 15, Top = 15, Width = 520, Height = 40,
                ForeColor = Color.FromArgb(242, 242, 242),
                Font = new Font("Segoe UI", 9F)
            };

            txtUrl = new TextBox
            {
                Left = 15, Top = 60, Width = 530,
                Text = "https://docs.google.com/spreadsheets/d/",
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.FromArgb(242, 242, 242)
            };

            chkHtml = new CheckBox
            {
                Text = "Использовать HTML-формат (если CSV не работает)",
                Left = 15, Top = 95, Width = 530, Height = 24,
                ForeColor = Color.FromArgb(242, 242, 242)
            };

            var lblHint = new Label
            {
                Text = "Пример: https://docs.google.com/spreadsheets/d/1AbCdEf123.../edit#gid=0",
                Left = 15, Top = 122, Width = 530, Height = 20,
                ForeColor = Color.FromArgb(150, 150, 150),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            };

            var btnOk = new SmoothButton
            {
                Text = "Загрузить", Left = 350, Top = 192, Width = 110, Height = 36,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(238, 238, 238),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.CornerRadius = 8;
            btnOk.BorderColor = Color.FromArgb(238, 238, 238);
            btnOk.BorderThickness = 1;
            btnOk.HoverBackColor = Color.FromArgb(218, 218, 218);
            btnOk.PressedBackColor = Color.FromArgb(185, 185, 185);

            var btnCancel = new SmoothButton
            {
                Text = "Отмена", Left = 470, Top = 192, Width = 95, Height = 36,
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.FromArgb(242, 242, 242),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.CornerRadius = 8;
            btnCancel.BorderColor = Color.FromArgb(78, 78, 78);
            btnCancel.BorderThickness = 1;
            btnCancel.HoverBackColor = Color.FromArgb(46, 46, 46);
            btnCancel.PressedBackColor = Color.FromArgb(24, 24, 24);

            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(SheetUrl))
                {
                    MessageBox.Show("Вставьте ссылку на таблицу.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }

                var m = Regex.Match(SheetUrl, @"/spreadsheets/d/([a-zA-Z0-9-_]+)");
                if (!m.Success)
                {
                    MessageBox.Show(
                        "Не удалось найти ID таблицы в ссылке.\n\n" +
                        "Убедитесь, что ссылка вида:\n" +
                        "https://docs.google.com/spreadsheets/d/XXXXXXXXX/edit",
                        "Неверная ссылка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };

            Controls.AddRange(new Control[] { lbl, txtUrl, chkHtml, lblHint, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}