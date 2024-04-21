using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace CameraTest
{
    internal class ManageSettings
    {
        ComboBox comboBoxConfigFiles;
        Panel parentPanel;
        public ManageSettings()
        {

        }

        public void AddSetting(ref Panel parentPanel)
        {
            this.parentPanel = parentPanel;
            Panel localParentPanel = parentPanel;

            Button btnApply = new Button();
            btnApply.Size = new Size(50, 25); // Set the size of the button
            btnApply.Anchor = AnchorStyles.Top | AnchorStyles.Right; // Anchor the button to the bottom right corner
            btnApply.Location = new Point(parentPanel.Width - btnApply.Width - 10, 10);
            btnApply.Text = "&Apply"; // Set the text of the button
            btnApply.Click += btnApply_Click; // Attach event handler


            TrackBar[] trackBars = new TrackBar[5];
            TextBox[] textBoxes = new TextBox[5];
            string[] titles = new string[] { "Hidden layer count, input manually:", "Learning Rate", "Kernel Size", "Kernel Step", "Pool Size"};
            int[] maximum = new int[] {50, 20, 5, 5, 5};

            comboBoxConfigFiles = new ComboBox();

            string[] configFiles = Directory.GetFiles("Data\\Configs");
            foreach (string file in configFiles)
            {
                // Add only file names (without full path) to the dropdown
                comboBoxConfigFiles.Items.Add(Path.GetFileName(file));
            }
            comboBoxConfigFiles.Location = new Point(parentPanel.Width - comboBoxConfigFiles.Width - 20, comboBoxConfigFiles.Height + btnApply.Height);
            comboBoxConfigFiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBoxConfigFiles.Size = new Size(120, 20);
            comboBoxConfigFiles.SelectedIndex = 0;
            comboBoxConfigFiles.SelectedIndexChanged += comboBoxConfigFiles_SelectedIndexChanged;

            parentPanel.Controls.Add(comboBoxConfigFiles);
            // TrackBar creation and configuration loop
            for (int i = 0; i < 5; i++)
            {
                int currentIndex = i; // Capture the current value of i

                trackBars[i] = new TrackBar();
                trackBars[i].Minimum = 0;
                trackBars[i].Maximum = maximum[i];
                trackBars[i].SmallChange = 1; // Assuming appropriate small change
                trackBars[i].TickFrequency = 10; // Assuming appropriate tick frequency
                UpdateTrackBarSize(localParentPanel, trackBars[currentIndex]);
                trackBars[i].ValueChanged += (sender, e) =>
                {
                    if (currentIndex == 1)
                    {
                        double ActVal = trackBars[currentIndex].Value / 100.0;
                        textBoxes[currentIndex].Text = ActVal.ToString();
                    }
                    else
                    {
                        textBoxes[currentIndex].Text = trackBars[currentIndex].Value.ToString();
                    }
                }; // Update corresponding textbox value

                localParentPanel.SizeChanged += (sender, e) => UpdateTrackBarSize(localParentPanel, trackBars[currentIndex]);

                Label headingLabel = new Label();
                headingLabel.Text = titles[i]; // Heading text
                headingLabel.AutoSize = true;


                textBoxes[i] = new TextBox();
                textBoxes[i].Name = titles[i];
                textBoxes[i].Text = trackBars[i].Value.ToString();

                textBoxes[i].ReadOnly = true;
                textBoxes[i].TextAlign = HorizontalAlignment.Center;


                // Set location for controls
                int yOffset = (trackBars[i].Height + headingLabel.Height + textBoxes[i].Height + 20)  * i + trackBars[i].Height; // Adjust 10 for spacing
                headingLabel.Location = new Point(10, yOffset);
                trackBars[i].Location = new Point(10, headingLabel.Location.Y + headingLabel.Height);
                textBoxes[i].Location = new Point(10, trackBars[i].Location.Y + trackBars[i].Height);

                // Add controls to parent panel
                parentPanel.Controls.Add(trackBars[i]);
                parentPanel.Controls.Add(headingLabel);
                parentPanel.Controls.Add(textBoxes[i]);


            }

            Label label = new Label();
            label.Text = "CNN Algorithms \nPlease input manually and use correct spelling:";
            label.AutoSize = true;
            label.Location = new Point(textBoxes[4].Location.X, textBoxes[4].Location.Y + label.Height + 50);

            TextBox CNNAlgo = new TextBox();
            CNNAlgo.Size = new Size(400, 50);
            CNNAlgo.Location = new Point(label.Location.X, label.Location.Y + CNNAlgo.Height + 10);
            CNNAlgo.Name = "CNNAlgo";
            CNNAlgo.Text = string.Join(",", TOMLHandle.GetCNNStruct());

            trackBars[0].Visible = false;
            textBoxes[0].Location = new Point(10, 70);

            textBoxes[0].Text = string.Join(" ", TOMLHandle.GetHiddenLayerCount());
            textBoxes[1].Text = TOMLHandle.GetLearningRate().ToString();
            textBoxes[2].Text = TOMLHandle.GetKernelSize().ToString();
            textBoxes[3].Text = TOMLHandle.GetKernelStep().ToString();
            textBoxes[4].Text = TOMLHandle.GetPoolSize().ToString();

            trackBars[1].Value = (int)(TOMLHandle.GetLearningRate() * 100);
            trackBars[2].Value = (int)TOMLHandle.GetKernelSize();
            trackBars[3].Value = (int)TOMLHandle.GetKernelStep();
            trackBars[4].Value = (int)TOMLHandle.GetPoolSize();

            textBoxes[0].ReadOnly = false;

            int[,] Resolutions = new int[,] { {28, 28 }, {100,100} ,{200,200} ,{640, 360 } ,{640 , 480} ,{1280, 720} };

            ComboBox TargRes = new ComboBox();
            TargRes.Name = "TargRes";
            TargRes.Location = new Point(parentPanel.Width - 400 , 25);
            TargRes.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TargRes.Size = new Size(200, 10);

            for(int i = 0; i < Resolutions.GetLength(0); i++)
            {
                TargRes.Items.Add(Resolutions[i,0].ToString() + "x" + Resolutions[i, 1].ToString());
            }

            TargRes.SelectedItem = string.Join("x",TOMLHandle.GetTargetResolution());

            TextBox TargResTitle = new TextBox();
            TargResTitle.Text = "Target Resolution";
            TargResTitle.ReadOnly = true;
            TargResTitle.Size = TargRes.Size;
            TargResTitle.Location = new Point(TargRes.Location.X, TargRes.Location.Y - TargResTitle.Height);
            TargResTitle.Name = "TargetResolution";
            TargResTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            parentPanel.Controls.Add(TargResTitle);
            parentPanel.Controls.Add(TargRes);
            parentPanel.Controls.Add(CNNAlgo);
            parentPanel.Controls.Add(btnApply);
            parentPanel.Controls.Add(label);

            Label Testbox = new Label();
            Testbox.Location = new Point(comboBoxConfigFiles.Location.X - 10, comboBoxConfigFiles.Location.Y + 40);
            Testbox.Text = "Number of images used in test";
            Testbox.AutoSize = true;
            Testbox.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            TextBox TestNum = new TextBox();
            TestNum.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            TestNum.Text = TOMLHandle.GetTestCount().ToString();

            TrackBar TestCount = new TrackBar();
            TestCount.Maximum = 2000;
            TestCount.Location = new Point(comboBoxConfigFiles.Location.X - 10, comboBoxConfigFiles.Location.Y + 60);
            TestCount.ValueChanged += (sender, e) =>
            {
                TestNum.Text = TestCount.Value.ToString();
            };
            TestCount.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            TestCount.Value = (int)TOMLHandle.GetTestCount();


            TestNum.Location = new Point(TestCount.Location.X, TestCount.Location.Y + 30);
            TestNum.Name = "TestCount";


            parentPanel.Controls.Add(Testbox);
            parentPanel.Controls.Add(TestNum);
            parentPanel.Controls.Add(TestCount);

        }
        private void UpdateTrackBarSize(Panel parentPanel, TrackBar trackBar)
        {
            // Adjust the size of the trackbar to match the width of the parent panel and set a fixed height
            trackBar.Size = new Size(parentPanel.Width / 2, 30);
        }
        private void comboBoxConfigFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filename = comboBoxConfigFiles.SelectedItem.ToString();
        }

        public void btnApply_Click(object sender, EventArgs e)
        {
            Panel settingsPanel = parentPanel; // Assuming pnlSettings is accessible within your current method

            // Access the TextBox within the Panel
            foreach (Control control in settingsPanel.Controls)
            {
                if (control is TextBox && control.Name != "TargetResolution")
                {
                    try
                    {
                        double[] DoubleArray = new double[0];
                        if (control.Name != "CNNAlgo")
                        {
                             DoubleArray = control.Text.Split(' ').Select(double.Parse).ToArray();
                        }
                        // Depending on the name of the textbox, write the values to the appropriate property of the struct
                        switch (control.Name)
                        {
                            case "Hidden layer count, input manually:":
                                TOMLWrite.dataStruct.HiddenLayerCount = DoubleArray;
                                break;
                            case "Learning Rate":
                                TOMLWrite.dataStruct.LearningRate = DoubleArray[0]; // Assuming LearningRate is a single value
                                break;
                            case "Kernel Size":
                                TOMLWrite.dataStruct.KernelSize = DoubleArray[0]; // Assuming KernelSize is a single value
                                break;
                            case "Kernel Step":
                                TOMLWrite.dataStruct.KernelStep = DoubleArray[0]; // Assuming KernelStep is a single value
                                break;
                            case "Pool Size":
                                TOMLWrite.dataStruct.PoolSize = DoubleArray[0]; // Assuming PoolSize is a single value
                                break;
                            case "TargetResolution":
                                TOMLWrite.dataStruct.TestCount = DoubleArray[0];
                                break;
                            case "TestCount":
                                TOMLWrite.dataStruct.TestCount = DoubleArray[0];
                                break;
                            case "CNNAlgo":
                                TOMLWrite.dataStruct.CNNAlgo = control.Text.Split(',');
                                break;
                            case null:
                                break;
                                // Add cases for other textboxes here if needed
                        }
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show($"Invalid input in {control.Name}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if(control is ComboBox && control.Name == "TargRes")
                {
                    try
                    {
                        TOMLWrite.dataStruct.TargetRes = control.Text.Split('x').Select(int.Parse).ToArray();
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show($"Invalid input in {control.Name}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }

            TOMLWrite.WriteAllData(comboBoxConfigFiles.SelectedItem.ToString());
        }
    }
}
