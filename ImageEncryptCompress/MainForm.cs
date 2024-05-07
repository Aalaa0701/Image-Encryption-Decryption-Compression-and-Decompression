using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
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
        RGBPixel[,] OperatedImageMatrix;
        DecompressedImage[,] decompressedImage;
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
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDirectory, "binaryfile.bin");
            //variables for reading
            bool isEncryptedRead = false;
            string seedRead = string.Empty;
            int tapPosRead = 0;
            Dictionary<string, int> redDictRead = new Dictionary<string, int>();
            Dictionary<string, int> greenDictRead = new Dictionary<string, int>();
            Dictionary<string, int> blueDictRead = new Dictionary<string, int>();
            int heightRead = 0;
            int widthRead = 0;
            int redDictReadCount = 0;
            int greenDictReadCount = 0;
            int blueDictReadCount = 0;
            int numOfBytesRed = 0;
            int numOfBytesGreen = 0;
            int numOfBytesBlue = 0;

            //variables for writing
            bool isEncrypted = false;
            Dictionary<int, string> redDict = new Dictionary<int, string>();
            Dictionary<int, string> greenDict = new Dictionary<int, string>();
            Dictionary<int, string> blueDict = new Dictionary<int, string>();
            
            RaiseError();
            string key = string.Empty;
            int tapPos = 0;
            Histogram histoCrypted;
            Histogram histo;
            switch (ModeSelect.Text)
            {
                case "Encrypt":
                    isEncrypted = true;
                    key = KeyTextBox.Text;
                    tapPos = Convert.ToInt32(TapPosTextBox.Text);
                    OperatedImageMatrix = ImageOperations.ImageEncryption(ImageMatrix, key, tapPos);
                    histoCrypted = new Histogram(OperatedImageMatrix);
                    histo = new Histogram(ImageMatrix);

                    if (histo.Derivation() < histoCrypted.Derivation())
                    {
                        MessageBox.Show("image already encrypted");
                        return;
                    }

                    ImageOperations.Compression(ref numOfBytesRed, ref numOfBytesGreen, ref numOfBytesBlue, ref redDict, ref greenDict, ref blueDict, OperatedImageMatrix);
                    WriteInaFile(filePath, redDict, greenDict, blueDict, isEncrypted, key, 
                        tapPos, numOfBytesRed, numOfBytesGreen, numOfBytesBlue);

                    break;
                case "Decrypt":
                    //if (histo.Derivation() > histoCrypted.Derivation())
                    //{
                    //    MessageBox.Show("image already decrypted");
                    //    return;
                    //}
                    break;
                case "Compress":
                    ImageOperations.Compression(ref numOfBytesRed, ref numOfBytesGreen, ref numOfBytesBlue, ref redDict, ref greenDict, ref blueDict, ImageMatrix);
                    WriteInaFile(filePath, redDict, greenDict, blueDict, isEncrypted, key, tapPos, 
                        numOfBytesRed, numOfBytesGreen, numOfBytesBlue);
                    break;
                case "Decompress":
                   ReadFromFile(ref isEncryptedRead, filePath, ref redDictRead, ref greenDictRead, ref blueDictRead, 
                       ref heightRead, ref widthRead, ref seedRead, ref tapPosRead, 
                       ref redDictReadCount, ref greenDictReadCount, ref blueDictReadCount);
                    RGBPixel[,] ImageAfterDecompression = ImageOperations.ImageDecompression(decompressedImage, redDictRead, greenDictRead, blueDictRead);
                    if (isEncryptedRead)
                    {
                        OperatedImageMatrix = ImageOperations.ImageEncryption(ImageAfterDecompression, seedRead, tapPosRead);
                    }
                    else
                    {
                        OperatedImageMatrix = ImageAfterDecompression;
                    }
                    break;
                case "Crack":
                    OperatedImageMatrix = ImageMatrix;
                    int n = Convert.ToInt32(KeyTextBox.Text);
                    CrackImage(Convert.ToInt32(n));
                    break;
            }
            ImageOperations.DisplayImage(OperatedImageMatrix, pictureBox2);
        }
        private void WriteInaFile(string filePath, Dictionary<int, string> redDict, Dictionary<int, string> greenDict, Dictionary<int, string> blueDict, 
             bool isEncrypted, string seed, int tapPos, int numOfBitsRed, int numOfBitsGreen, int numOfBitsBlue)
        {
            StringBuilder redBits = new StringBuilder("");
            StringBuilder greenBits = new StringBuilder("");
            StringBuilder blueBits = new StringBuilder("");
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                writer.Write(isEncrypted);
                if (isEncrypted)
                {
                    writer.Write(seed);
                    writer.Write(tapPos);
                }
                writer.Write(redDict.Count);
                foreach (int keyVal in redDict.Keys)
                {
                    writer.Write(redDict[keyVal]);
                    writer.Write(keyVal);
                }
                writer.Write(greenDict.Count);
                foreach (int keyVal in greenDict.Keys)
                {
                    writer.Write(greenDict[keyVal]);
                    writer.Write(keyVal);

                }
                writer.Write(blueDict.Count);
                foreach (int keyVal in blueDict.Keys)
                {
                    writer.Write(blueDict[keyVal]);
                    writer.Write(keyVal);

                }
                int height = ImageOperations.GetHeight(ImageMatrix);
                int width = ImageOperations.GetWidth(ImageMatrix);
                writer.Write(height);
                writer.Write(width);
                int remainingBits = 0;
                //write red bits first
                int numOfBytesRed = 0;
                if (numOfBitsRed % 8 == 0)
                    numOfBytesRed = numOfBitsRed / 8;
                else
                    numOfBytesRed = (numOfBitsRed / 8) + 1;
                int writtenRed = 0;
                writer.Write(numOfBytesRed);
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        //RED
                        string redVal = redDict[(int)OperatedImageMatrix[i, j].red];
                        int redBitsLength = redBits.Length;
                        int redLength = redVal.Length;
                        if (redLength <= 8 - redBitsLength)
                            redBits.Append(redVal);
                        else
                        {
                            remainingBits = 8 - redBitsLength;
                            redBits.Append(redVal.Substring(0,remainingBits));
                            writer.Write(Convert.ToByte(redBits.ToString(), 2));
                            writtenRed++;
                            redBits.Clear();
                            redBits.Append(redVal.Substring(remainingBits));

                        }

                        if(redBits.Length == 8 || (i == height - 1 && j == width - 1))
                        {
                            //convert to byte
                            byte redByte = Convert.ToByte(redBits.ToString(), 2);
                            //write
                            writer.Write(redByte);
                            writtenRed++;
                            //clear
                            redBits.Clear();
                        }

                    }

                }

                //write green bits
                int writtenGreen = 0;
                int numOfBytesGreen = 0;
                if (numOfBitsGreen % 8 == 0)
                    numOfBytesGreen = numOfBitsGreen / 8;
                else
                    numOfBytesGreen = (numOfBitsGreen / 8) + 1;
                writer.Write(numOfBytesGreen);
                for (int i = 0; i < height; i++)
                {
                    for(int j = 0; j < width; j++)
                    {

                        //GREEN
                        string greenVal = greenDict[(int)OperatedImageMatrix[i, j].green];
                        int greenBitsLength = greenBits.Length;
                        int greenLength = greenVal.Length;
                        if (greenLength <= 8 - greenBitsLength)
                            greenBits.Append(greenVal);
                        else
                        {
                            remainingBits = 8 - greenBitsLength;
                            greenBits.Append(greenVal.Substring(0, remainingBits));
                            writer.Write(Convert.ToByte(greenBits.ToString(), 2));
                            writtenGreen++;
                            greenBits.Clear();
                            greenBits.Append(greenVal.Substring(remainingBits));

                        }



                        if (greenBits.Length == 8 || (i == height - 1 && j == width - 1))
                        {
                            //convert to byte
                            byte greenByte = Convert.ToByte(greenBits.ToString(), 2);
                            //write
                            writer.Write(greenByte);
                            writtenGreen++;
                            //clear
                            greenBits.Clear();
                        }

                    }
                }

                //writ blue bits
                int writtenBlue = 0;
                int numOfBytesBlue = 0;
                if (numOfBitsBlue % 8 == 0)
                    numOfBytesBlue = numOfBitsBlue / 8;
                else
                    numOfBytesBlue = (numOfBitsBlue / 8) + 1;
                writer.Write(numOfBytesBlue);
                for (int i = 0;i < height; i++)
                {
                    for(int j = 0;j < width;j++)
                    {

                        //BLUE
                        string blueVal = blueDict[(int)OperatedImageMatrix[i, j].blue];
                        int blueBitsLength = blueBits.Length;
                        int blueLength = blueVal.Length;
                        if (blueLength <= 8 - blueBitsLength)
                            blueBits.Append(blueVal);
                        else
                        {
                            remainingBits = 8 - blueBitsLength;
                            blueBits.Append(blueVal.Substring(0, remainingBits));
                            writer.Write(Convert.ToByte(blueBits.ToString(), 2));
                            writtenBlue++;
                            blueBits.Clear();
                            blueBits.Append(blueVal.Substring(remainingBits));

                        }



                        if (blueBits.Length == 8 || (i == height - 1 && j == width - 1))
                        {
                            //convert to byte
                            byte blueByte = Convert.ToByte(blueBits.ToString(), 2);
                            //write
                            writer.Write(blueByte);
                            writtenBlue++;
                            //clear
                            blueBits.Clear();
                        }
                    }
                }
            }
        }

        private void ReadFromFile(ref bool isEncryptedRead, string filePath, ref Dictionary<string, int> redDictRead,
            ref Dictionary<string, int> greenDictRead, ref Dictionary<string, int> blueDictRead, ref int heightRead,
            ref int widthRead, ref string seedRead, ref int tapPosRead, ref int redDictCount, ref int greenDictCount, ref int blueDictCount)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                isEncryptedRead = reader.ReadBoolean();
                if (isEncryptedRead)
                {
                    seedRead = reader.ReadString();
                    tapPosRead = reader.ReadInt32();
                }
                redDictCount = reader.ReadInt32();
                for(int i = 0; i < redDictCount; i++)
                {
                    redDictRead[reader.ReadString()] = reader.ReadInt32();
                }
                greenDictCount = reader.ReadInt32();
                for(int i = 0;i < greenDictCount; i++)
                {
                    greenDictRead[reader.ReadString()] = reader.ReadInt32();
                }
                blueDictCount = reader.ReadInt32();
                for(int i = 0; i < blueDictCount; i++)
                {
                    blueDictRead[reader.ReadString()] = reader.ReadInt32();
                }
                heightRead = reader.ReadInt32();
                widthRead = reader.ReadInt32();
                try
                {
                    decompressedImage = new DecompressedImage[heightRead, widthRead];
                    //read red bytes
                    int numOfBytesRed = reader.ReadInt32();
                    byte[] redBytes = new byte[numOfBytesRed];
                    List<string> redList = new List<string>();
                    for(int i = 0; i < numOfBytesRed; i++) {
                        redBytes[i] = reader.ReadByte();
                    }
                    ReadDecompressedImage(redBytes, ref redList, redDictRead);
                    
                    //read green bytes
                    int numOfBytesGreen = reader.ReadInt32();
                    byte[] greenBytes = new byte[numOfBytesGreen];
                    List<string> greenList = new List<string>();
                    for (int i = 0; i < numOfBytesGreen; i++)
                    {
                        greenBytes[i] = reader.ReadByte();
                    }
                    ReadDecompressedImage(greenBytes, ref greenList, greenDictRead);
                    //read blue bytes
                    int numOfBytesBlue = reader.ReadInt32();
                    byte[] blueBytes = new byte[numOfBytesBlue];
                    List<string> blueList = new List<string>();
                    for (int i = 0; i < numOfBytesBlue; i++) {
                        blueBytes[i] = reader.ReadByte();
                    }
                    ReadDecompressedImage(blueBytes, ref blueList, blueDictRead);

                    //put values
                    PutDecompressedValues(redList, greenList, blueList);
                }
                catch(OutOfMemoryException e) 
                {
                    Console.WriteLine($"EXCEPTION: {e.Message}");
                }
            }
        }

        private void ReadDecompressedImage(byte[] arr, ref List<string> list, Dictionary<string, int> dict)
        {
            StringBuilder sb = new StringBuilder();
            int startIndex = 0;
            int movingIndex = 0;
            bool continueToNext = false;
            for(int i = 0; i < arr.Length; i++)
            {
                continueToNext = false;
                string thisByte = Convert.ToString(arr[i], 2).PadLeft(8, '0');
                startIndex = 0;
                while( startIndex < 8 )
                {
                    movingIndex = startIndex;
                    sb.Append(thisByte[startIndex]);
                    if (dict.ContainsKey(sb.ToString()))
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                        startIndex++;
                    }
                    else {
                        do
                        {
                            movingIndex++;
                            if(movingIndex == 8)
                            {
                                continueToNext = true;
                                break;
                            }
                            sb.Append(thisByte[movingIndex]);

                        } while (!dict.ContainsKey(sb.ToString()));
                        if (continueToNext)
                            break;
                        startIndex = movingIndex + 1;
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                }
            }
        }

        private void PutDecompressedValues(List<string> redList, List<string> greenList, List<string> blueList)
        {
            int height = decompressedImage.GetLength(0);
            int width = decompressedImage.GetLength(1);

            int indexInList = 0;
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    decompressedImage[i , j].redRep = redList[indexInList];
                    decompressedImage[i , j].greenRep = greenList[indexInList];
                    decompressedImage[i , j].blueRep = blueList[indexInList];
                    indexInList++;
                }
            }
        }
        private void CrackImage(int n)
        {

            string key = string.Empty;
            int keyNum = (int)Math.Pow(2, n);
            RGBPixel[,] resultImageMatrix = new RGBPixel[Width, Height];
            Histogram histo = new Histogram(ImageMatrix);
            double maxOrMin = histo.Derivation();
            for (int i = 0; i < keyNum; i++)
            {
                int tap = Convert.ToInt32(TapPosTextBox.Text);
                key = Convert.ToString(i, 2).PadLeft(n, '0');
                OperatedImageMatrix = ImageOperations.ImageEncryption(ImageMatrix, key, tap);
                Histogram histoCrypted = new Histogram(OperatedImageMatrix);
                //MessageBox.Show($"maxMIN: {maxOrMin}, curr {histoCrypted.Derivation()}");
                if (histoCrypted.Derivation() >= maxOrMin)
                {
                    resultImageMatrix = OperatedImageMatrix;
                    maxOrMin = histoCrypted.Derivation();
                }
                ImageOperations.DisplayImage(OperatedImageMatrix, pictureBox2);

            }
            ImageOperations.DisplayImage(resultImageMatrix, pictureBox2);
            OperatedImageMatrix = resultImageMatrix;
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

        private void switchBTN_Click(object sender, EventArgs e)
        {
            System.Drawing.Image img= pictureBox1.Image;
            pictureBox1.Image = pictureBox2.Image;
            ImageMatrix= OperatedImageMatrix;
            pictureBox2.Image= img;
        }
    }
}