using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraTest
{
    internal static class TrainingMenu
    {
        static Panel TrainPanel;
        static TextBox Cost;
        public static TextBox status;
        private static Form1 mainForm;

        static TextBox predict = new TextBox();


        public static void SetPanel(Panel panel)
        {
            TrainPanel = panel;
        }

        public static void form1_init(Form form1)
        {
            mainForm = form1 as Form1;
        }

        public static async void Trainmenu()
        {

            status = new TextBox();
            status.Size = new Size(400, 300);
            status.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            status.Location = new Point(10, 10);
            status.ReadOnly = true;
            TrainPanel.Controls.Add(status);

            Button TrainStart = new Button();
            TrainStart.Size = new Size(200, 100);
            TrainStart.Location = new Point(status.Location.X, status.Location.Y + 30);
            TrainStart.Text = "Train";

            TrainStart.Click += async (sender, e) =>
            {
                status.Text = "Processing...";
                await Task.Run(() =>
                {
                    CNNEntryPoint.Entry();
                });
                status.Text = "Done!";

            };
            TrainPanel.Controls.Add(TrainStart);

            Cost = new TextBox();
            Cost.Size = new Size(40, 40);
            Cost.Location = new Point(status.Width + 20 , 10);

            Button CloseMenu = new Button();
            CloseMenu.Size = new Size(100, 50);
            CloseMenu.Text = "Main";
            CloseMenu.Location = new Point(TrainPanel.Width - CloseMenu.Width - 10 , 10);
            CloseMenu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CloseMenu.Click += CloseMenu_Click;

            TrainPanel.Controls.Add(CloseMenu);
            TrainPanel.Controls.Add(Cost);

            TrainPanel.Visible = true;

            Button Test = new Button();
            Test.Size = new Size(30, 30);
            Test.Location = new Point(status.Width + 20 , Test.Height);
            Test.Click += Test_Click;

            predict.Size = new Size(200, 200);
            predict.Location = new Point(status.Width + Test.Width + 30 , predict.Height);

            TrainPanel.Controls.Add(Test);
            TrainPanel.Controls.Add(predict);
        }

        public static void Test_Click(object sender, EventArgs e)
        {
            PredictInput predictInput = new PredictInput();
            predict.Text = predictInput.FindNumInPicture(1, 200);
        }

        public static void CloseMenu_Click(object sender, EventArgs e)
        {
            foreach (Control control in TrainPanel.Controls)
            {
                control.Visible = false;
            }

            // Show controls in the main form
            foreach (Control control in mainForm.Controls)
            {
                if(control != Form1.pnlSettings && control != mainForm.btnMain)
                {
                    control.Visible = true;
                }
            }
        }
       
        public static void SetCost(double cost)
        {
            // Check if invoking is required
            if (TrainingMenu.status.InvokeRequired)
            {
                // If it is, invoke the method on the UI thread
                TrainingMenu.status.Invoke((MethodInvoker)delegate {
                    TrainingMenu.Cost.Text = cost.ToString();
                });
            }
            else
            {
                // If not, directly update the textbox
                TrainingMenu.Cost.Text = cost.ToString();
            }
        }
        

    }
}
