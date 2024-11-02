using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace StartSmartDeliveryForm
{
    public partial class FormConsole : Form
    {
        private static FormConsole _instance;
        private static readonly object _lock = new object();

        public FormConsole()
        {
            InitializeComponent();
        }

        private void Console_Load(object sender, EventArgs e)
        {

        }

        //Singleton Pattern (Global instance)
        public static FormConsole Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null || _instance.IsDisposed)
                    {
                        _instance = new FormConsole();
                    }
                    return _instance;
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
