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

namespace SmartStartDeliveryForm
{
    public partial class Console : Form
    {
        public Console()
        {
            InitializeComponent();
        }

        private void Console_Load(object sender, EventArgs e)
        {

        }

        public void log(string text)
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

        public void clearConsole()
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
