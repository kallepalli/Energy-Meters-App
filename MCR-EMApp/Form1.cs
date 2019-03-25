using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCR_EMApp
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();

        }

        private void btnFetchData_Click(object sender, EventArgs e)
        {
            try
            {

                FetchData fd = new FetchData(".//Settings.json", serialPort1);
                fd.GetDataFromMeters(this);
                lblMeterNo.Text = "Data collection completed.";
                lblTag.Text = "";
                lblValue.Text = "";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }
    }
}
