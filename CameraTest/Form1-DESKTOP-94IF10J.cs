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

        public Form1()
        {
            InitializeComponent();
        }

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        Bitmap bitmap;
        bool cameraExists = false;


        private void Form1_Load(object sender, EventArgs e)
        {
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
            try
            {
                bitmap.Save(@"Images\pic(" + i + ").png", ImageFormat.Png);
                i++;
                label3.Text = "Created image";
            }     
            catch 
            { 
                MessageBox.Show("Cannot take picture as camera does not exist.\n Please connect a camera to take a picture", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                label3.Text = "Camera stopped";
                videoCaptureDevice.Stop();
                pic.Image = null;
            }
            catch
            {
                MessageBox.Show("Cannot stop camera as Camera does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}