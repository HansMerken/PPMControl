using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControlGUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main()
        {
            Form2 form2 = new Form2();
            form2.Show();

            Form1 programForm = new Form1();
            programForm.Show();
            System.Diagnostics.Debug.WriteLine("Hellow WOrld");
            Application.EnableVisualStyles();
           Application.SetCompatibleTextRenderingDefault(false);
           Application.Run(programForm);
        }
    }
}
