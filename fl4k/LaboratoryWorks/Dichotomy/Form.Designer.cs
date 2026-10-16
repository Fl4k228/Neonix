using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace fl4k
{
    partial class DichotomyForm
    {
        private IContainer components;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 760);
            Name = "DichotomyForm";
            Text = "Метод дихотомии";
        }
    }
}
