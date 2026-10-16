using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace fl4k
{
    public partial class MainForm : Form
    {
        public event Action? DichotomyRequested;
        public event Action? NeonixRequested;
        public event Action? ExitRequested;

        public MainForm()
        {
            InitializeComponent();

            KeyPreview = true;
            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    ExitRequested?.Invoke();
            };

            Resize += (_, _) => CenterChoiceCard();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var screen = Screen.FromControl(this);
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;
            Bounds = screen.Bounds;

            CenterChoiceCard();
        }

        private void CenterChoiceCard()
        {
            if (choiceCard == null || contentPanel == null)
                return;

            int x = Math.Max(24, (contentPanel.ClientSize.Width - choiceCard.Width) / 2);
            int y = Math.Max(24, (contentPanel.ClientSize.Height - choiceCard.Height) / 2);
            choiceCard.Location = new Point(x, y);
        }

        private void btnDichotomy_Click(object? sender, EventArgs e)
            => DichotomyRequested?.Invoke();

        private void btnNeonix_Click(object? sender, EventArgs e)
            => NeonixRequested?.Invoke();

        private void btnExit_Click(object? sender, EventArgs e)
            => ExitRequested?.Invoke();
    }

    internal sealed class MainMenuButton : Button
    {
        private bool hovered;
        private bool pressed;

        public MainMenuButton()
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
                    ? Color.FromArgb(30, 30, 30)
                    : Parent?.BackColor ?? Color.FromArgb(30, 30, 30);

            e.Graphics.Clear(parentColor);

            var rect = new Rectangle(
                1,
                1,
                Math.Max(1, Width - 3),
                Math.Max(1, Height - 3));

            using var path = CreateRoundPath(rect, 14);

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

        private static GraphicsPath CreateRoundPath(Rectangle rect, int radius)
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

    internal sealed class MainRoundedPanel : Panel
    {
        public MainRoundedPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Width < 3 || Height < 3)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using var path = CreateRoundPath(rect, 18);
            using var border = new Pen(Color.FromArgb(68, 68, 68), 1f);

            e.Graphics.DrawPath(border, path);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);

            if (Width < 3 || Height < 3)
                return;

            using var path = CreateRoundPath(new Rectangle(0, 0, Width, Height), 18);
            Region?.Dispose();
            Region = new Region(path);
        }

        private static GraphicsPath CreateRoundPath(Rectangle rect, int radius)
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
