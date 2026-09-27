#nullable disable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        protected void RaiseCompare(int i, int j) => OnCompare?.Invoke(i, j);
        protected void RaiseSwap(int i, int j) => OnSwap?.Invoke(i, j);
        protected void RaiseIteration() { Iterations++; }

        public abstract void Sort(int[] array);

        protected bool NeedSwap(int a, int b) => Ascending ? a > b : a < b;
    }

    public class BubbleSort : SortingAlgorithm
    {
        public BubbleSort() { Name = "Пузырьковая"; }
        public override void Sort(int[] a)
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
        public override void Sort(int[] a)
        {
            for (int i = 1; i < a.Length; i++)
            {
                int key = a[i];
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
        public override void Sort(int[] a)
        {
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
                right--;
                RaiseIteration();

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
                left++;
                RaiseIteration();

                if (!swapped) break;
            }
        }
    }

    public class QuickSort : SortingAlgorithm
    {
        public QuickSort() { Name = "Быстрая"; }
        public override void Sort(int[] a) => QuickSortRec(a, 0, a.Length - 1);

        private void QuickSortRec(int[] a, int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(a, low, high);
                QuickSortRec(a, low, pi - 1);
                QuickSortRec(a, pi + 1, high);
            }
        }

        private int Partition(int[] a, int low, int high)
        {
            int pivot = a[high];
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

        public override void Sort(int[] a)
        {
            var start = DateTime.Now;
            while (!IsSorted(a))
            {
                if ((DateTime.Now - start).TotalSeconds > 10)
                    throw new TimeoutException("BOGO превысила лимит 10 секунд.");
                Shuffle(a);
                RaiseIteration();
            }
        }

        private void Shuffle(int[] a)
        {
            for (int i = a.Length - 1; i > 0; i--)
            {
                int j = _rnd.Next(i + 1);
                (a[i], a[j]) = (a[j], a[i]);
                RaiseSwap(i, j);
            }
        }

        private bool IsSorted(int[] a)
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
        public int[] Data;
        public int CompareI = -1, CompareJ = -1;
        public int SwapI = -1, SwapJ = -1;
        public long Comparisons;
        public long Swaps;
        public long Iterations;
        public double AvgMs;
        public double MinMs;
        public double MaxMs;
        public int Runs;
        public bool Finished;
        public string Error;
        public bool Visualize;

        public readonly object Sync = new object();

        public List<(int type, int i, int j)> EventLog = new();
        public readonly object LogSync = new object();

        public (int[] data, int ci, int cj, int si, int sj,
                long cmp, long swp, long iter,
                double avg, double min, double max, int runs,
                bool finished, string err) Snapshot()
        {
            lock (Sync)
            {
                var copy = new int[Data.Length];
                Array.Copy(Data, copy, Data.Length);
                return (copy, CompareI, CompareJ, SwapI, SwapJ,
                        Comparisons, Swaps, Iterations,
                        AvgMs, MinMs, MaxMs, Runs,
                        Finished, Error);
            }
        }
    }

    // ============================================================
    //  ГЛАВНАЯ ФОРМА — ЛАЗУРНАЯ ТЕМА
    // ============================================================
    public partial class Form1 : Form
    {
        // ---------- Палитра (ЛАЗУРНАЯ) ----------
        private static readonly Color BgDark      = Color.FromArgb(8, 32, 52);       // фон приложения
        private static readonly Color BgPanel     = Color.FromArgb(16, 52, 78);      // панели
        private static readonly Color BgCard      = Color.FromArgb(22, 63, 92);      // карточки
        private static readonly Color AccentRed   = Color.FromArgb(56, 189, 248);    // акцент — лазурный
        private static readonly Color AccentHover = Color.FromArgb(103, 213, 255);   // hover — светлее
        private static readonly Color AccentLight = Color.FromArgb(140, 224, 255);   // светлый лазурный
        private static readonly Color TextSoft    = Color.FromArgb(214, 238, 250);   // основной текст
        private static readonly Color TextDim     = Color.FromArgb(138, 190, 216);   // вторичный текст
        private static readonly Color CompareClr  = Color.FromArgb(250, 204, 21);    // сравнение — янтарный
        private static readonly Color SwapClr     = Color.FromArgb(255, 255, 255);   // обмен — белый
        private static readonly Color OkClr       = Color.FromArgb(110, 231, 183);   // успех — мятный

        // ---------- Константы ----------
        private const int MaxBogoElements = 10;
        private const int MaxVisualizeElements = 50;
        private const int RunsPerAlgorithm = 5;

        // ---------- Контролы ----------
        private Panel headerPanel;
        private Label titleLabel, subtitleLabel;

        private Panel leftPanel, toolbarPanel;
        private Button btnGenerate, btnExcel, btnGoogle;
        private DataGridView dataGrid;
        private Label lblCount;

        private Panel optionsPanel;
        private CheckBox cbBubble, cbInsertion, cbShaker, cbQuick, cbBogo;
        private Label lblDirection;
        private ComboBox cmbDirection;
        private Label lblDelay, lblDelayValue;
        private TrackBar tbDelay;

        private Panel workspacePanel;
        private Label workspaceHint;
        private Label lblStatus;

        private Button btnCalculate;
        private Button btnStop;

        // ---------- Состояние ----------
        private int[] _currentData = Array.Empty<int>();
        private List<AlgorithmState> _states = new();
        private System.Windows.Forms.Timer _renderTimer;
        private bool _isFullscreen = true;
        private bool _isRunning;
        private CancellationTokenSource _cts;

        // Лазурная палитра столбиков
        private readonly Dictionary<string, Color> _algoColors = new()
        {
            ["Пузырьковая"] = Color.FromArgb(56, 189, 248),   // cyan-400
            ["Вставками"]   = Color.FromArgb(45, 212, 191),   // teal-400
            ["Шейкерная"]   = Color.FromArgb(103, 232, 249),  // cyan-300
            ["Быстрая"]     = Color.FromArgb(34, 211, 238),   // cyan-500-ish
            ["BOGO"]        = Color.FromArgb(125, 211, 252)   // sky-300
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
            dataGrid.RowsRemoved     += (s, e) => SyncDataFromGrid();
            dataGrid.UserDeletingRow += (s, e) => BeginInvoke((Action)SyncDataFromGrid);

            KeyDown += Form1_KeyDown;
            Resize += (s, e) => LayoutControls();

            _renderTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _renderTimer.Tick += (s, e) =>
            {
                if (_states.Count > 0) workspacePanel.Invalidate();
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
            headerPanel = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = BgPanel };
            headerPanel.Paint += HeaderPanel_Paint;

            titleLabel = new Label
            {
                Text = "Neonix",
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = AccentLight,
                BackColor = Color.Transparent,
                AutoSize = true, Location = new Point(50, 15)
            };
            subtitleLabel = new Label
            {
                Text = "Визуализация алгоритмов сортировки",
                Font = new Font("Segoe UI", 15F, FontStyle.Italic),
                ForeColor = TextSoft,
                BackColor = Color.Transparent,
                AutoSize = true, Location = new Point(54, 68)
            };
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);

            leftPanel = new Panel { BackColor = BgCard, BorderStyle = BorderStyle.None };
            leftPanel.Paint += CardPanel_Paint;

            toolbarPanel = new Panel { Height = 44, BackColor = BgPanel, Location = new Point(10, 10) };

            btnGenerate = MakeSmallButton("Сгенерировать");
            btnExcel    = MakeSmallButton("Импорт Excel");
            btnGoogle   = MakeSmallButton("Google Sheets");

            btnGenerate.Click += BtnGenerate_Click;
            btnExcel.Click    += BtnExcel_Click;
            btnGoogle.Click   += BtnGoogle_Click;

            toolbarPanel.Controls.Add(btnGenerate);
            toolbarPanel.Controls.Add(btnExcel);
            toolbarPanel.Controls.Add(btnGoogle);

            dataGrid = new DataGridView
            {
                Location = new Point(10, 62),
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                EditMode = DataGridViewEditMode.EditOnEnter,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersWidth = 45,
                BackgroundColor = BgCard,
                GridColor = Color.FromArgb(38, 88, 122),
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
            dataGrid.DefaultCellStyle.SelectionBackColor = AccentRed;
            dataGrid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(6, 24, 38);
            dataGrid.RowHeadersDefaultCellStyle.BackColor = BgPanel;
            dataGrid.RowHeadersDefaultCellStyle.ForeColor = TextDim;
            dataGrid.RowHeadersDefaultCellStyle.SelectionBackColor = BgPanel;
            dataGrid.CellValidating += DataGrid_CellValidating;

            lblCount = new Label
            {
                Text = "Элементов: 0",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight
            };

            leftPanel.Controls.Add(toolbarPanel);
            leftPanel.Controls.Add(dataGrid);
            leftPanel.Controls.Add(lblCount);

            // ---------- Опции ----------
            optionsPanel = new Panel { BackColor = BgCard, BorderStyle = BorderStyle.None };
            optionsPanel.Paint += CardPanel_Paint;

            var lblAlgo = new Label
            {
                Text = "Алгоритмы сортировки:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AccentLight,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(15, 10)
            };
            optionsPanel.Controls.Add(lblAlgo);

            cbBubble    = MakeAlgoCheck("Пузырьковая", new Point(15, 45),  true);
            cbInsertion = MakeAlgoCheck("Вставками",   new Point(15, 75),  false);
            cbShaker    = MakeAlgoCheck("Шейкерная",   new Point(15, 105), false);
            cbQuick     = MakeAlgoCheck("Быстрая",     new Point(200, 45), true);
            cbBogo      = MakeAlgoCheck("BOGO",        new Point(200, 75), false);

            optionsPanel.Controls.Add(cbBubble);
            optionsPanel.Controls.Add(cbInsertion);
            optionsPanel.Controls.Add(cbShaker);
            optionsPanel.Controls.Add(cbQuick);
            optionsPanel.Controls.Add(cbBogo);

            lblDirection = new Label
            {
                Text = "Направление:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AccentLight,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(400, 15)
            };
            optionsPanel.Controls.Add(lblDirection);

            cmbDirection = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                BackColor = BgDark,
                ForeColor = TextSoft,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(400, 45),
                Width = 220
            };
            cmbDirection.Items.AddRange(new object[] { "По возрастанию", "По убыванию" });
            cmbDirection.SelectedIndex = 0;
            optionsPanel.Controls.Add(cmbDirection);

            lblDelay = new Label
            {
                Text = "Задержка визуализации:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AccentLight,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(650, 15)
            };
            optionsPanel.Controls.Add(lblDelay);

            tbDelay = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 15,
                TickFrequency = 10,
                Location = new Point(650, 42),
                Width = 220,
                BackColor = BgCard
            };
            optionsPanel.Controls.Add(tbDelay);

            lblDelayValue = new Label
            {
                Text = "15 мс",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = TextSoft,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(880, 45)
            };
            optionsPanel.Controls.Add(lblDelayValue);

            tbDelay.ValueChanged += (s, e) =>
            {
                lblDelayValue.Text = $"{tbDelay.Value} мс";
            };

            // ---------- Рабочая область ----------
            workspacePanel = new Panel { BackColor = BgCard, BorderStyle = BorderStyle.None };
            workspacePanel.Paint += WorkspacePanel_Paint;
            SetDoubleBuffered(workspacePanel);

            workspaceHint = new Label
            {
                Text = "Здесь будет визуализация сортировок.\n\n" +
                       "1. Введите/сгенерируйте/импортируйте данные слева.\n" +
                       "2. Отметьте алгоритмы сверху.\n" +
                       "3. Нажмите «РАССЧИТАТЬ».",
                Font = new Font("Segoe UI", 12F),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            workspacePanel.Controls.Add(workspaceHint);

            // ---------- Кнопки ----------
            btnCalculate = new Button
            {
                Text = "РАССЧИТАТЬ",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(6, 24, 38),  // тёмный текст на лазурном
                BackColor = AccentRed,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(230, 60),
                Cursor = Cursors.Hand
            };
            btnCalculate.FlatAppearance.BorderSize = 0;
            btnCalculate.FlatAppearance.MouseOverBackColor = AccentHover;
            btnCalculate.FlatAppearance.MouseDownBackColor = Color.FromArgb(14, 165, 233);
            btnCalculate.Click += BtnCalculate_Click;

            btnStop = new Button
            {
                Text = "СТОП",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(190, 24, 60),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 60),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnStop.FlatAppearance.BorderSize = 1;
            btnStop.FlatAppearance.BorderColor = Color.FromArgb(244, 63, 94);
            btnStop.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 40, 80);
            btnStop.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 15, 45);
            btnStop.Click += BtnStop_Click;

            lblStatus = new Label
            {
                Text = "Готово.",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = TextDim,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Controls.Add(optionsPanel);
            Controls.Add(workspacePanel);
            Controls.Add(leftPanel);
            Controls.Add(lblStatus);
            Controls.Add(btnStop);
            Controls.Add(btnCalculate);
            Controls.Add(headerPanel);
        }

        private CheckBox MakeAlgoCheck(string text, Point loc, bool isChecked)
        {
            return new CheckBox
            {
                Text = text,
                Location = loc,
                Size = new Size(170, 26),
                Checked = isChecked,
                Font = new Font("Segoe UI", 10F),
                ForeColor = TextSoft,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat
            };
        }

        private Button MakeSmallButton(string text)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextSoft,
                BackColor = BgDark,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 32),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = AccentRed;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(14, 116, 144);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(8, 80, 110);
            return b;
        }

        private static void SetDoubleBuffered(Control c)
        {
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(c, true, null);
        }

        /// <summary>
        /// Форматирует время в мс в человекочитаемый вид.
        /// </summary>
        private static string FormatTime(double ms)
        {
            if (ms < 0) ms = 0;

            if (ms < 1000.0)
                return $"{ms:F4} мс";

            double sec = ms / 1000.0;

            if (sec < 60.0)
                return $"{sec:F4} с";

            if (sec < 3600.0)
            {
                double min = Math.Floor(sec / 60.0);
                double remSec = sec - min * 60.0;
                return $"{min:F0} мин {remSec:F2} с";
            }

            double totalMin = Math.Floor(sec / 60.0);
            double hours = Math.Floor(totalMin / 60.0);
            double remMin = totalMin - hours * 60.0;
            double remSecFinal = sec - totalMin * 60.0;
            return $"{hours:F0} ч {remMin:F0} мин {remSecFinal:F0} с";
        }

        /// <summary>
        /// Компактный формат для отображения над панелью.
        /// </summary>
        private static string FormatTimeShort(double ms)
        {
            if (ms < 0) ms = 0;

            if (ms < 1.0)
                return $"{ms:F3} мс";

            if (ms < 1000.0)
                return $"{ms:F1} мс";

            double sec = ms / 1000.0;
            if (sec < 60.0)
                return $"{sec:F2} с";

            if (sec < 3600.0)
            {
                double min = Math.Floor(sec / 60.0);
                double remSec = sec - min * 60.0;
                return $"{min:F0}м {remSec:F0}с";
            }

            double totalMin = Math.Floor(sec / 60.0);
            double hours = Math.Floor(totalMin / 60.0);
            double remMin = totalMin - hours * 60.0;
            return $"{hours:F0}ч {remMin:F0}м";
        }

        // ============================================================
        //  РАСКЛАДКА
        // ============================================================
        private void LayoutControls()
        {
            int headerH = headerPanel.Height;
            int margin = 20;
            int gap = 20;
            int buttonH = btnCalculate.Height;
            int buttonW = btnCalculate.Width;

            int leftW = 340;
            int leftTop = headerH + margin;
            int leftH = ClientSize.Height - headerH - margin * 2;
            leftPanel.Bounds = new Rectangle(margin, leftTop, leftW, leftH);

            toolbarPanel.Bounds = new Rectangle(10, 10, leftPanel.Width - 20, 44);
            int tbW = (toolbarPanel.Width - 16) / 3;
            btnGenerate.Bounds = new Rectangle(0, 6, tbW, 32);
            btnExcel.Bounds    = new Rectangle(tbW + 8, 6, tbW, 32);
            btnGoogle.Bounds   = new Rectangle((tbW + 8) * 2, 6, tbW, 32);

            lblCount.Bounds = new Rectangle(10, leftPanel.Height - 30, leftPanel.Width - 20, 22);
            dataGrid.Bounds = new Rectangle(10, 62, leftPanel.Width - 20, leftPanel.Height - 62 - 36);

            int optLeft = leftPanel.Right + gap;
            int optTop = headerH + margin;
            int optH = 150;
            int optW = ClientSize.Width - optLeft - margin;
            optionsPanel.Bounds = new Rectangle(optLeft, optTop, optW, optH);

            int wsTop = optionsPanel.Bottom + gap;
            int wsLeft = optLeft;
            int wsRight = ClientSize.Width - margin;
            int wsBottom = ClientSize.Height - margin - buttonH - 10;
            workspacePanel.Bounds = new Rectangle(wsLeft, wsTop, wsRight - wsLeft, wsBottom - wsTop);

            lblStatus.Bounds = new Rectangle(wsLeft, wsBottom + 5, wsRight - wsLeft, 22);

            btnCalculate.Location = new Point(
                ClientSize.Width - buttonW - margin,
                ClientSize.Height - buttonH - margin);

            btnStop.Location = new Point(
                btnCalculate.Left - btnStop.Width - 10,
                ClientSize.Height - buttonH - margin);

            workspacePanel.Invalidate();
        }

        // ============================================================
        //  РИСОВАНИЕ ПАНЕЛЕЙ
        // ============================================================
        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = headerPanel.ClientRectangle;

            // Лазурный градиент
            using (var brush = new LinearGradientBrush(
                rect,
                Color.FromArgb(10, 42, 66),
                Color.FromArgb(20, 74, 108),
                LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, rect);
            }

            using var pen = new Pen(AccentRed, 3);
            g.DrawLine(pen, 0, rect.Bottom - 2, rect.Width, rect.Bottom - 2);

            using var glow = new Pen(Color.FromArgb(150, AccentLight), 1);
            g.DrawLine(glow, 0, rect.Bottom - 4, rect.Width, rect.Bottom - 4);
        }

        private void CardPanel_Paint(object sender, PaintEventArgs e)
        {
            var p = (Panel)sender;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var pen = new Pen(Color.FromArgb(160, AccentRed), 1);
            g.DrawRectangle(pen, rect);
        }

        // ============================================================
        //  ВИЗУАЛИЗАЦИЯ
        // ============================================================
        private void WorkspacePanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BgCard);

            using (var borderPen = new Pen(Color.FromArgb(160, AccentRed), 1))
                g.DrawRectangle(borderPen, 0, 0, workspacePanel.Width - 1, workspacePanel.Height - 1);

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
            var snap = st.Snapshot();
            var data = snap.data;

            using var titleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            using var infoFont = new Font("Segoe UI", 8F);
            using var valueFont = new Font("Segoe UI", 7F, FontStyle.Bold);

            string title = st.Name;
            if (snap.finished)
                title += snap.err != null ? "  ✗" : "  ✓";

            using var bgHeader = new SolidBrush(BgPanel);
            g.FillRectangle(bgHeader, area.Left, area.Top, area.Width, 22);

            using var titleBrush = new SolidBrush(st.BarColor);
            g.DrawString(title, titleFont, titleBrush, area.Left + 6, area.Top + 2);

            if (snap.finished)
            {
                string ms = snap.err != null
                    ? "ошибка"
                    : $"≈ {FormatTimeShort(snap.avg)}  (×{snap.runs})";
                var msSize = g.MeasureString(ms, infoFont);
                using var msBrush = new SolidBrush(snap.err != null ? Color.FromArgb(244, 63, 94) : OkClr);
                g.DrawString(ms, infoFont, msBrush,
                    area.Right - msSize.Width - 6, area.Top + 5);
            }

            var plot = new Rectangle(
                area.Left + 4,
                area.Top + 24,
                Math.Max(1, area.Width - 8),
                Math.Max(1, area.Height - 24 - 22));

            using (var borderPen = new Pen(Color.FromArgb(38, 88, 122), 1))
                g.DrawRectangle(borderPen, plot);

            if (data.Length == 0) return;

            int maxVal = data.Max();
            int minVal = data.Min();
            if (maxVal == minVal) maxVal = minVal + 1;

            float barW = plot.Width / (float)data.Length;

            var maxAbsStr = Math.Max(Math.Abs(maxVal), Math.Abs(minVal)).ToString();
            var maxStrSize = g.MeasureString(maxAbsStr, valueFont);
            bool showAllValues = barW >= maxStrSize.Width + 2;
            bool showHighlightedValues = barW >= 10;

            using var baseBrush = new SolidBrush(st.BarColor);
            using var cmpBrush = new SolidBrush(CompareClr);
            using var swpBrush = new SolidBrush(SwapClr);
            using var valueBrush = new SolidBrush(TextSoft);
            using var valueBrushHi = new SolidBrush(Color.FromArgb(6, 24, 38));

            for (int i = 0; i < data.Length; i++)
            {
                float norm = (data[i] - minVal + 0.5f) / (maxVal - minVal + 1f);
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

                string valueText = data[i].ToString();
                var textSize = g.MeasureString(valueText, valueFont);

                bool drawThis = showAllValues || (showHighlightedValues && (isSwap || isCmp));
                if (drawThis)
                {
                    float tx = x + (barW - textSize.Width) / 2f;
                    float ty = y - textSize.Height - 1f;
                    if (ty < plot.Top) ty = y + 1f;

                    // на светлых столбиках (обмен — белый, сравнение — янтарный) — тёмный текст
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
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Value",
                HeaderText = "Значение",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = false,
                Visible = true
            });
        }

        private void DataGrid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != 0) return;
            if (e.RowIndex < 0) return;

            var text = e.FormattedValue?.ToString();
            if (string.IsNullOrWhiteSpace(text)) return;

            if (!int.TryParse(text, out _))
            {
                MessageBox.Show(
                    $"Некорректное значение \"{text}\" в строке {e.RowIndex + 1}. Ожидается целое число.",
                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
        }

        private void SyncDataFromGrid()
        {
            try
            {
                var list = new List<int>();
                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    if (row.IsNewRow) continue;
                    var cell = row.Cells[0];
                    var val = cell.Value;
                    if (val == null || string.IsNullOrWhiteSpace(val.ToString())) continue;
                    if (!int.TryParse(val.ToString(), out int num)) continue;
                    list.Add(num);
                }
                _currentData = list.ToArray();
                lblCount.Text = $"Элементов: {_currentData.Length}";
            }
            catch { }
        }

        private void LoadDataToGrid(IEnumerable<int> data)
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
            var data = new int[dlg.Count];
            for (int i = 0; i < dlg.Count; i++)
                data[i] = rnd.Next(dlg.Min, dlg.Max + 1);
            LoadDataToGrid(data);
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

        private List<int> LoadFromExcel(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var result = new List<int>();

            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            do
            {
                while (reader.Read())
                {
                    if (reader.FieldCount == 0) continue;
                    var val = reader.GetValue(0);
                    if (val == null) continue;
                    if (int.TryParse(val.ToString(), out int num))
                        result.Add(num);
                    else
                        throw new FormatException($"Значение \"{val}\" не является целым числом.");
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

        private async Task<List<int>> LoadFromGoogleSheetsByLink(string url, bool useHtml)
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

            var result = new List<int>();

            if (useHtml)
            {
                var matches = Regex.Matches(content, @"<td[^>]*>(.*?)</td>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
                foreach (Match m in matches)
                {
                    var text = WebUtility.HtmlDecode(m.Groups[1].Value).Trim();
                    if (int.TryParse(text, out int num))
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
                    if (int.TryParse(first, out int num))
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
            tbDelay.Enabled = false;

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
                    Data = (int[])_currentData.Clone(),
                    Visualize = visualize
                };
                _states.Add(st);
            }

            workspaceHint.Visible = false;
            workspacePanel.Invalidate();

            // ============================================================
            //  ФАЗА 1: ЧИСТЫЙ ЗАМЕР
            // ============================================================
            var measureTasks = new List<Task>();

            foreach (var name in selectedNames)
            {
                var algoName = name;
                var st = _states.First(s => s.Name == algoName);
                bool isBogo = (algoName == "BOGO");
                int runs = isBogo ? 1 : RunsPerAlgorithm;

                measureTasks.Add(Task.Run(() =>
                {
                    var times = new List<double>(runs);
                    long firstCmp = 0, firstSwp = 0, firstIter = 0;
                    string error = null;

                    for (int run = 0; run < runs; run++)
                    {
                        if (token.IsCancellationRequested) break;

                        var source = (int[])_currentData.Clone();
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
                        }
                        else if (collectThis)
                        {
                            algo.OnCompare += (i, j) => System.Threading.Interlocked.Increment(ref localCmp);
                            algo.OnSwap    += (i, j) => System.Threading.Interlocked.Increment(ref localSwp);
                        }

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        try { algo.Sort(st.Data); }
                        catch (Exception ex)
                        {
                            error = ex.Message;
                            sw.Stop();
                            break;
                        }
                        sw.Stop();

                        times.Add(sw.Elapsed.TotalMilliseconds);

                        if (collectThis)
                        {
                            firstCmp = localCmp;
                            firstSwp = localSwp;
                            firstIter = algo.Iterations;
                        }

                        lock (st.Sync) st.Runs = run + 1;
                    }

                    lock (st.Sync)
                    {
                        if (times.Count > 0)
                        {
                            st.AvgMs = times.Average();
                            st.MinMs = times.Min();
                            st.MaxMs = times.Max();
                        }
                        st.Comparisons = firstCmp;
                        st.Swaps = firstSwp;
                        st.Iterations = firstIter;
                        st.Error = error;
                    }
                }, token));
            }

            try { await Task.WhenAll(measureTasks); }
            catch (OperationCanceledException) { }

            // ============================================================
            //  ФАЗА 2: ВОСПРОИЗВЕДЕНИЕ
            // ============================================================
            if (visualize && !token.IsCancellationRequested)
            {
                lblStatus.Text = "Воспроизведение визуализации...";

                var playTasks = new List<Task>();

                foreach (var name in selectedNames)
                {
                    var st = _states.First(s => s.Name == name);

                    playTasks.Add(Task.Run(() =>
                    {
                        var working = (int[])_currentData.Clone();
                        lock (st.Sync)
                        {
                            Array.Copy(working, st.Data, working.Length);
                            st.CompareI = st.CompareJ = st.SwapI = st.SwapJ = -1;
                        }

                        List<(int type, int i, int j)> log;
                        lock (st.LogSync) log = new List<(int, int, int)>(st.EventLog);

                        foreach (var ev in log)
                        {
                            if (token.IsCancellationRequested) break;

                            if (ev.type == 0)
                            {
                                lock (st.Sync)
                                {
                                    st.CompareI = ev.i;
                                    st.CompareJ = ev.j;
                                }
                            }
                            else
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

            // ============================================================
            //  ОТЧЁТ
            // ============================================================
            var ordered = _states.OrderBy(s => s.AvgMs).ToList();
            var sb = new StringBuilder();

            if (token.IsCancellationRequested)
                sb.AppendLine("=== Результаты (остановлено пользователем) ===");
            else
                sb.AppendLine("=== Результаты ===");

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
                        $"{s.Name,-16}{FormatTime(snap.avg),-20}{FormatTime(snap.min),-20}{FormatTime(snap.max),-20}" +
                        $"{snap.runs,10}{snap.iter,12}{snap.cmp,14}{snap.swp,12}");
                }
            }

            var fastest = ordered.FirstOrDefault(s => s.Error == null && s.Runs > 0);
            if (fastest != null)
                sb.AppendLine($"\nСамый быстрый (по среднему): {fastest.Name} ({FormatTime(fastest.AvgMs)})");

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
            tbDelay.Enabled = true;
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
    //  ФОРМА: ГЕНЕРАЦИЯ (лазурная)
    // ============================================================
    public class GenerateForm : Form
    {
        private NumericUpDown nudCount, nudMin, nudMax;

        public int Count => (int)nudCount.Value;
        public int Min => (int)nudMin.Value;
        public int Max => (int)nudMax.Value;

        public GenerateForm()
        {
            Text = "Генерация данных";
            ClientSize = new Size(320, 190);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.FromArgb(22, 63, 92);
            ForeColor = Color.FromArgb(214, 238, 250);

            var lblCount = new Label { Text = "Количество:", Left = 10, Top = 15, Width = 120, ForeColor = Color.FromArgb(214, 238, 250) };
            nudCount = new NumericUpDown { Left = 140, Top = 12, Width = 140, Minimum = 1, Maximum = 500, Value = 15, BackColor = Color.FromArgb(8, 32, 52), ForeColor = Color.FromArgb(214, 238, 250) };

            var lblMin = new Label { Text = "Минимум:", Left = 10, Top = 50, Width = 120, ForeColor = Color.FromArgb(214, 238, 250) };
            nudMin = new NumericUpDown { Left = 140, Top = 47, Width = 140, Minimum = -100000, Maximum = 100000, Value = 0, BackColor = Color.FromArgb(8, 32, 52), ForeColor = Color.FromArgb(214, 238, 250) };

            var lblMax = new Label { Text = "Максимум:", Left = 10, Top = 85, Width = 120, ForeColor = Color.FromArgb(214, 238, 250) };
            nudMax = new NumericUpDown { Left = 140, Top = 82, Width = 140, Minimum = -100000, Maximum = 100000, Value = 100, BackColor = Color.FromArgb(8, 32, 52), ForeColor = Color.FromArgb(214, 238, 250) };

            var btnOk = new Button
            {
                Text = "OK", Left = 110, Top = 130, Width = 80,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(56, 189, 248),
                ForeColor = Color.FromArgb(6, 24, 38),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Отмена", Left = 200, Top = 130, Width = 80,
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(8, 32, 52),
                ForeColor = Color.FromArgb(214, 238, 250),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(56, 189, 248);
            btnCancel.FlatAppearance.BorderSize = 1;

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
    //  ФОРМА: GOOGLE SHEETS (лазурная)
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
            ClientSize = new Size(560, 210);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.FromArgb(22, 63, 92);
            ForeColor = Color.FromArgb(214, 238, 250);

            var lbl = new Label
            {
                Text = "Вставьте ссылку на Google-таблицу:\n" +
                       "(таблица должна быть опубликована или доступна по ссылке)",
                Left = 15, Top = 15, Width = 520, Height = 40,
                ForeColor = Color.FromArgb(214, 238, 250),
                Font = new Font("Segoe UI", 9F)
            };

            txtUrl = new TextBox
            {
                Left = 15, Top = 60, Width = 530,
                Text = "https://docs.google.com/spreadsheets/d/",
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(8, 32, 52),
                ForeColor = Color.FromArgb(214, 238, 250)
            };

            chkHtml = new CheckBox
            {
                Text = "Использовать HTML-формат (если CSV не работает)",
                Left = 15, Top = 95, Width = 530, Height = 24,
                ForeColor = Color.FromArgb(214, 238, 250)
            };

            var lblHint = new Label
            {
                Text = "Пример: https://docs.google.com/spreadsheets/d/1AbCdEf123.../edit#gid=0",
                Left = 15, Top = 122, Width = 530, Height = 20,
                ForeColor = Color.FromArgb(138, 190, 216),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            };

            var btnOk = new Button
            {
                Text = "Загрузить", Left = 350, Top = 155, Width = 100,
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(56, 189, 248),
                ForeColor = Color.FromArgb(6, 24, 38),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Отмена", Left = 460, Top = 155, Width = 85,
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(8, 32, 52),
                ForeColor = Color.FromArgb(214, 238, 250),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(56, 189, 248);
            btnCancel.FlatAppearance.BorderSize = 1;

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