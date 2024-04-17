using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ImageEncryptCompress
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        string fileName;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                fileName=OpenedFilePath;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        //private void btnGaussSmooth_Click(object sender, EventArgs e)
        //{
        //    double sigma = double.Parse(txtGaussSigma.Text);
        //    int maskSize = (int)nudMaskSize.Value ;
        //    ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
        //    ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        //}

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = ModeSelect.SelectedItem.ToString();
            btnApplyOperation.Text= text+"\nImage";
            label2.Text= text + "ed Image";
        }

        private void btnApplyOperation_Click(object sender, EventArgs e)
        {
            RaiseError();
            string key=KeyTextBox.Text;
            int tapPos = Convert.ToInt32(TapPosTextBox.Text);
            RGBPixel[,] OperatedImageMatrix=ImageMatrix;
            switch (ModeSelect.Text)
            {
                case "Encrypt":

                    int Height = ImageMatrix.GetLength(0);
                    int Width = ImageMatrix.GetLength(1);
                    int redFreq = 0;
                    int blueFreq = 0;
                    int greenFreq = 0;
                    Histogram histo = new Histogram(ImageMatrix);
                    bool hasZeroInRed = false;
                    bool hasZeroInGreen = false;
                    bool hasZeroInBlue = false;
                    for (int i = 0; i < 256; i++)
                    {
                        //condition if frequency of certain color R G B is 0 break and can be encrypted
                        if (histo.redHistogram[i] == 0)
                            hasZeroInRed = true;
                        if (histo.blueHistogram[i] == 0)
                            hasZeroInBlue = true;
                        if (histo.greenHistogram[i] == 0)
                            hasZeroInGreen = true;
                        redFreq += histo.redHistogram[i];
                        blueFreq += histo.blueHistogram[i];
                        greenFreq += histo.greenHistogram[i];
                    }
                    if (redFreq == greenFreq && redFreq == blueFreq && redFreq == Width * Height)
                    {
                        if (hasZeroInRed || hasZeroInGreen || hasZeroInBlue)
                            OperatedImageMatrix = ImageOperations.ImageEncryption(ImageMatrix, key, tapPos);
                        

                    }
                    //Histogram histoCrypted = new Histogram(OperatedImageMatrix);
                    //Histogram histo = new Histogram(ImageMatrix);
                    break;
                case "Decrypt":
                    OperatedImageMatrix = ImageOperations.ImageEncryption(ImageMatrix, key, tapPos);
                    break;
                case "Compress":
                    break;
                case "Decompress":
                    break;
            }
            ImageOperations.DisplayImage(OperatedImageMatrix, pictureBox2);
        }
        private void RaiseError()
        {
            if (KeyTextBox.Text==null||TapPosTextBox.Text==null)
            {
                KeyTextBox.Text = "11111111";
                TapPosTextBox.Text = "1";
                MessageBox.Show("key or tap position info are missing!!");
            }
        }

        private void SaveBTN_Click(object sender, EventArgs e)
        {
            string[] names= fileName.Split('\\');
            SaveFileDialog fileSave = new SaveFileDialog();
            fileSave.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            fileSave.Title = "save encrepted image"; 
            fileSave.Filter= "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*";
            fileSave.FileName = $"ecrypted {names[names.Length-1]}";
            DialogResult result = fileSave.ShowDialog();
            if(result == DialogResult.OK&&pictureBox2.Image!=null)
            {
                pictureBox2.Image.Save(fileSave.FileName);
            }
        }
    }
}