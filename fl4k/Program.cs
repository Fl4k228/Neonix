using System;
using System.Windows.Forms;

namespace fl4k
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LaboratoryApplicationContext());
        }
    }

    internal sealed class LaboratoryApplicationContext : ApplicationContext
    {
        private Form? currentForm;
        private bool exiting;

        public LaboratoryApplicationContext()
        {
            ShowMainForm();
        }

        private void ShowMainForm()
        {
            if (exiting)
                return;

            var form = new MainForm();

            form.DichotomyRequested += () =>
                OpenLaboratory(new DichotomyForm());

            form.NeonixRequested += () =>
                OpenLaboratory(new NeonixForm());

            form.ExitRequested += ExitApplication;
            form.FormClosed += MainForm_FormClosed;

            SwitchForm(form);
        }

        private void OpenLaboratory(Form laboratory)
        {
            laboratory.FormClosed += Laboratory_FormClosed;
            SwitchForm(laboratory);
        }

        private void SwitchForm(Form next)
        {
            Form? previous = currentForm;
            currentForm = next;

            next.Show();

            if (previous != null && !previous.IsDisposed)
            {
                previous.Close();
                previous.Dispose();
            }
        }

        private void Laboratory_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (exiting || !ReferenceEquals(sender, currentForm))
                return;

            currentForm = null;
            ShowMainForm();
        }

        private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (exiting || !ReferenceEquals(sender, currentForm))
                return;

            ExitApplication();
        }

        private void ExitApplication()
        {
            if (exiting)
                return;

            exiting = true;

            Form? current = currentForm;
            currentForm = null;

            if (current != null && !current.IsDisposed)
                current.Close();

            ExitThread();
        }
    }
}
