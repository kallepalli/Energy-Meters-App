namespace MCR_EMApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.lblMeterNo = new System.Windows.Forms.Label();
            this.lblTag = new System.Windows.Forms.Label();
            this.lblValue = new System.Windows.Forms.Label();
            this.btnFetchData = new System.Windows.Forms.Button();
            this.rbEnergy = new System.Windows.Forms.RadioButton();
            this.rbData = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // serialPort1
            // 
            this.serialPort1.PortName = "COM2";
            // 
            // lblMeterNo
            // 
            this.lblMeterNo.AutoSize = true;
            this.lblMeterNo.Location = new System.Drawing.Point(43, 62);
            this.lblMeterNo.Name = "lblMeterNo";
            this.lblMeterNo.Size = new System.Drawing.Size(24, 13);
            this.lblMeterNo.TabIndex = 1;
            this.lblMeterNo.Text = "text";
            // 
            // lblTag
            // 
            this.lblTag.AutoSize = true;
            this.lblTag.Location = new System.Drawing.Point(43, 96);
            this.lblTag.Name = "lblTag";
            this.lblTag.Size = new System.Drawing.Size(24, 13);
            this.lblTag.TabIndex = 2;
            this.lblTag.Text = "text";
            // 
            // lblValue
            // 
            this.lblValue.AutoSize = true;
            this.lblValue.Location = new System.Drawing.Point(43, 116);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(24, 13);
            this.lblValue.TabIndex = 3;
            this.lblValue.Text = "text";
            // 
            // btnFetchData
            // 
            this.btnFetchData.Location = new System.Drawing.Point(243, 18);
            this.btnFetchData.Name = "btnFetchData";
            this.btnFetchData.Size = new System.Drawing.Size(75, 23);
            this.btnFetchData.TabIndex = 4;
            this.btnFetchData.Text = "Get Data";
            this.btnFetchData.UseVisualStyleBackColor = true;
            this.btnFetchData.Click += new System.EventHandler(this.btnFetchData_Click);
            // 
            // rbEnergy
            // 
            this.rbEnergy.AutoSize = true;
            this.rbEnergy.Checked = true;
            this.rbEnergy.Location = new System.Drawing.Point(34, 19);
            this.rbEnergy.Name = "rbEnergy";
            this.rbEnergy.Size = new System.Drawing.Size(58, 17);
            this.rbEnergy.TabIndex = 6;
            this.rbEnergy.TabStop = true;
            this.rbEnergy.Text = "Energy";
            this.rbEnergy.UseVisualStyleBackColor = true;
            // 
            // rbData
            // 
            this.rbData.AutoSize = true;
            this.rbData.Location = new System.Drawing.Point(125, 19);
            this.rbData.Name = "rbData";
            this.rbData.Size = new System.Drawing.Size(62, 17);
            this.rbData.TabIndex = 7;
            this.rbData.TabStop = true;
            this.rbData.Text = "All Data";
            this.rbData.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbEnergy);
            this.groupBox1.Controls.Add(this.rbData);
            this.groupBox1.Location = new System.Drawing.Point(12, -1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(225, 49);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select Option";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 172);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnFetchData);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.lblTag);
            this.Controls.Add(this.lblMeterNo);
            this.Name = "Form1";
            this.Text = "EM Reading App";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort serialPort1;
        public System.Windows.Forms.Label lblMeterNo;
        public System.Windows.Forms.Label lblTag;
        public System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.Button btnFetchData;
        private System.Windows.Forms.RadioButton rbData;
        private System.Windows.Forms.RadioButton rbEnergy;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

