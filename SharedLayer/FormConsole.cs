using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;

namespace StartSmartDeliveryForm.SharedLayer
{
    public partial class FormConsole : Form
    {
        private static FormConsole? s_instance;
        private static readonly object s_lock = new();

        public FormConsole()
        {
            InitializeComponent();
        }

        private void FormConsole_Load(object sender, System.EventArgs e)
        {

        }

        //Singleton Pattern (Global instance)
        public static FormConsole Instance
        {
            get
            {
                lock (s_lock)
                {
                    if (s_instance == null || s_instance.IsDisposed)
                    {
                        s_instance = new FormConsole();
                    }
                    return s_instance;
                }
            }
        }

        public void Log(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() =>
                {
                    richTextBox1.AppendText($"{text}{Environment.NewLine}");
                }));
            }
            else
            {
                richTextBox1.AppendText($"{text}{Environment.NewLine}");
            }
        }

        public void ClearConsole()
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() =>
                {
                    richTextBox1.Clear();
                }));
            }
            else
            {
                richTextBox1.Clear();
            }
        }
    }
}
