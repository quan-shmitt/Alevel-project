using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging;
using System.Threading;

namespace CameraTest
{
    public partial class Form1 : Form
    {
        public static Panel pnlSettings;
        private static Panel TrainPanel;
        public static TextBox status;

        public Form1()
        {
            InitializeComponent();

            ButtonLock();
            PicLock();

            this.MinimumSize = new Size(700, 200);
        }

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        Bitmap bitmap;
        bool cameraExists = false;
        ManageSettings manageSettings = new ManageSettings();



        private void ButtonLock()
        {
            btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right; // Anchor to top-right corner
            btnSettings.Location = new Point(this.ClientSize.Width - btnSettings.Width - 10, 10); // Adjust position

            // Add the button to the form
            this.Controls.Add(btnSettings);

            btnMain.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMain.Location = new Point(this.ClientSize.Width - btnMain.Width - btnSettings.Width - 10, 10);
        }

        private void PicLock()
        {
            this.Resize += Form1_Resize;
            pic.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void GenSettings()
        {

            pnlSettings = new Panel();
            pnlSettings.BorderStyle = BorderStyle.Fixed3D;
            pnlSettings.Size = ClientSize;
            pnlSettings.Visible = false;
            Controls.Add(pnlSettings);

            manageSettings.AddSetting(ref pnlSettings);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pic.Size = new Size(this.ClientRectangle.Width - 100, this.ClientRectangle.Height - 100);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form1_Resize(sender, e);
            GetConfigs();


            btnMain.Visible = false;
            // Call GenSettings to create and configure the panels
            GenSettings();


            // Set the initial visibility of panels
            pnlSettings.Visible = false;

            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in filterInfoCollection)
            {
                cboCamera.Items.Add(filterInfo.Name);
            }

            try
            {
                cboCamera.SelectedIndex = 0;
                videoCaptureDevice = new VideoCaptureDevice();
                cameraExists = true;
            }
            catch
            {
            }

        }

        public void GetConfigs()
        {
            string[] ConfigList = Directory.GetFiles("Data\\Configs");

            TOMLHandle.GetToml(ConfigList[0]);
            TOMLWrite.GetToml(ConfigList[0]);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pic.Image = (Bitmap)eventArgs.Frame.Clone();
            bitmap = (Bitmap)eventArgs.Frame.Clone();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cameraExists)
            {
                if (videoCaptureDevice.IsRunning == true)
                {
                    videoCaptureDevice.Stop();
                }
            }
            for(int j = 1; j < i; j++)
            {
                File.Delete(@"Images/pic(" + j + ").png");
            }
        }

        private void btnStart_Click_1(object sender, EventArgs e)
        {
            try
            {
                label2.Text = "button Clicked";
                videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[cboCamera.SelectedIndex].MonikerString);
                videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
                videoCaptureDevice.Start();
            }
            catch
            {
                MessageBox.Show("Camera device not detected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pic_Click(object sender, EventArgs e)
        {

        }
        
        int i = 1;
        private void button1_Click(object sender, EventArgs e)
        {
            if (!cameraExists)
            {
                MessageBox.Show("Cannot take picture as camera does not exist.\n Please connect a camera to take a picture", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            else
            {
                bitmap.Save(@"Images\pic(" + i + ").png", ImageFormat.Png);
                i++;
                label3.Text = "Created image";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!cameraExists)
            {
                MessageBox.Show("Cannot stop camera as Camera does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            else
            {
                label3.Text = "Camera stopped";
                videoCaptureDevice.Stop();
                pic.Image = null;
            }            
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            // Hide all other controls
            foreach (Control control in Controls)
            {
                if (control != pnlSettings)
                {
                    control.Visible = false;
                }
            }
            btnMain.Visible = true;

            // Make the settings panel fill the entire form
            pnlSettings.Dock = DockStyle.Fill;
            pnlSettings.Visible = true;

        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            // Hide the settings panel and show the main panel

            foreach(Control control in Controls)
            {
                control.Visible = true;
            }
            btnMain.Visible = false;

            pnlSettings.Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            init_TrainingMenu();
        }

        private void init_TrainingMenu()
        {
            TrainPanel = new Panel();
            TrainPanel.Dock = DockStyle.Fill;
            TrainPanel.BorderStyle = BorderStyle.Fixed3D;
            Controls.Add(TrainPanel);
            TrainPanel.Visible = true;

            foreach(Control control in Controls)
            {
                if (control != TrainPanel)
                {
                    control.Visible = false;
                }
            }
            TrainingMenu.SetPanel(TrainPanel);
            TrainingMenu.form1_init(this);
            TrainingMenu.Trainmenu();
        }
    }
}