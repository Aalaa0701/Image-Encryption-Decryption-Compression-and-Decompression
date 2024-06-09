using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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


        //global variables
        bool compressed100 = false;
        

        RGBPixel[,] ImageMatrix;
        RGBPixel[,] OperatedImageMatrix;
        DecompressedImage[,] decompressedImage;
        string fileName;

        bool isEncrypted = false;
        Dictionary<int, string> redDict = new Dictionary<int, string>();
        Dictionary<int, string> greenDict = new Dictionary<int, string>();
        Dictionary<int, string> blueDict = new Dictionary<int, string>();
        int numOfBytesRed = 0;
        int numOfBytesGreen = 0;
        int numOfBytesBlue = 0;
        string key = string.Empty;
        int tapPos = 0;


        //variables for reading
        bool isEncryptedRead = false;
        string seedRead = string.Empty;
        int tapPosRead = 0;
        Dictionary<int, int> redDictRead = new Dictionary<int, int>();
        Dictionary<int, int> greenDictRead = new Dictionary<int, int>();
        Dictionary<int, int> blueDictRead = new Dictionary<int, int>();
        int heightRead = 0;
        int widthRead = 0;
        string fileReadPath;



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
            
            //Histogram histoCrypted;
            //Histogram histo;
            switch (ModeSelect.Text)
            {
                case "Encrypt":
                   Stopwatch sw= new Stopwatch();
                    
                    isEncrypted = true;
                    key = KeyTextBox.Text;
                    tapPos = Convert.ToInt32(TapPosTextBox.Text);
                    sw.Reset();
                    sw.Start();
                    OperatedImageMatrix = ImageOperations.ImageEncryptionAlphanumeric(ImageMatrix, key, tapPos);
                    //histoCrypted = new Histogram(OperatedImageMatrix);
                    //histo = new Histogram(ImageMatrix);

                    //if (histo.Derivation() < histoCrypted.Derivation())
                    //{
                    //    MessageBox.Show("image already encrypted");
                    //    return;
                    //}

                    ImageOperations.Compression(ref compressed100, ref numOfBytesRed, ref numOfBytesGreen, ref numOfBytesBlue, ref redDict, ref greenDict, ref blueDict, OperatedImageMatrix);
                    sw.Stop();
                    MessageBox.Show(sw.Elapsed.ToString());
                    break;
                case "Decrypt":
                    //isEncrypted = false;
                    key = KeyTextBox.Text;
                    tapPos = Convert.ToInt32(TapPosTextBox.Text);
                    OperatedImageMatrix = ImageOperations.ImageEncryption(ImageMatrix, key, tapPos);
                    //if (histo.Derivation() > histoCrypted.Derivation())
                    //{
                    //    MessageBox.Show("image already decrypted");
                    //    return;
                    //}
                    break;
                case "Compress":
                    ImageOperations.Compression(ref compressed100, ref numOfBytesRed, ref numOfBytesGreen, ref numOfBytesBlue, ref redDict, ref greenDict, ref blueDict, ImageMatrix);
                    OperatedImageMatrix = ImageMatrix;
                    break;
                case "Decompress":
                    sw = new Stopwatch();
                    int numOfBitsRed = 0;
                    int numOfBitsGreen = 0;
                    int numOfBitsBlue = 0;
                    byte[] redBytes = null;
                    byte[] greenBytes = null;
                    byte[] blueBytes = null;
                    ReadFromFile(ref numOfBitsRed, ref numOfBitsGreen, ref numOfBitsBlue, ref redBytes, ref greenBytes, ref blueBytes);
                    sw.Reset();
                    sw.Start();
                    ReadEveryPixel(numOfBitsRed, numOfBitsGreen, numOfBitsBlue, redBytes, greenBytes, blueBytes);
                    ImageMatrix = ImageOperations.Decompression(decompressedImage, redDictRead, blueDictRead, greenDictRead);
                    if (isEncryptedRead)
                        OperatedImageMatrix = ImageOperations.ImageEncryption(ImageMatrix, seedRead, tapPosRead);
                    else
                        OperatedImageMatrix = ImageMatrix;
                    sw.Stop();
                    MessageBox.Show(sw.Elapsed.ToString()); 
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
       string seed, int tapPos, int numOfBitsRed, int numOfBitsGreen, int numOfBitsBlue)
        {
            StringBuilder redBits = new StringBuilder("");
            StringBuilder greenBits = new StringBuilder("");
            StringBuilder blueBits = new StringBuilder("");
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                Stopwatch sw = new Stopwatch();
                sw.Reset();
                sw.Start();

                writer.Write(isEncrypted);
                if (isEncrypted)
                {
                    writer.Write(seed);
                    writer.Write((byte)tapPos);
                }
                writer.Write(compressed100);
                if(!compressed100)
                {
                    short val = 0;
                    byte valLength = 0;
                    writer.Write(redDict.Count);
                    foreach (int keyVal in redDict.Keys)
                    {
                        val = Convert.ToInt16(redDict[keyVal], 2);
                        valLength = (byte)redDict[keyVal].Length;
                        writer.Write((short) val);
                        writer.Write((byte)valLength);
                        writer.Write((byte)keyVal);
                    }
                    writer.Write(greenDict.Count);
                    foreach (int keyVal in greenDict.Keys)
                    {
                        val = Convert.ToInt16(greenDict[keyVal], 2);
                        valLength = (byte)greenDict[keyVal].Length;
                        writer.Write((short)val);
                        writer.Write((byte)valLength);
                        //writer.Write(greenDict[keyVal]);
                        writer.Write((byte)keyVal);

                    }
                    writer.Write(blueDict.Count);
                    foreach (int keyVal in blueDict.Keys)
                    {
                        val = Convert.ToInt16(blueDict[keyVal], 2);
                        valLength = (byte)blueDict[keyVal].Length;
                        writer.Write((short)val);
                        writer.Write((byte)valLength);
                        //writer.Write(blueDict[keyVal]);
                        writer.Write((byte)keyVal);

                    }
                }
                int height = ImageOperations.GetHeight(ImageMatrix);
                int width = ImageOperations.GetWidth(ImageMatrix);
                writer.Write(height);
                writer.Write(width);
                int remainingBits = 0;
                //write red bits first

                writer.Write(numOfBitsRed);
                if (!compressed100)
                {

                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            //RED
                            
                            string redVal = redDict[(int)OperatedImageMatrix[i, j].red];
                            int redBitsLength = redBits.Length;
                            int redLength = redVal.Length;
                            remainingBits = 8 - redBitsLength;
                            if (redLength <= remainingBits)
                            {
                                redBits.Append(redVal);

                            }
                            else
                            {
                                int startIndex = 0;
                                int takenBits = 0;
                                do
                                {
                                    remainingBits = 8 - redBits.Length;
                                    takenBits += remainingBits;
                                    redBits.Append(redVal.Substring(startIndex, remainingBits));
                                    writer.Write(Convert.ToByte(redBits.ToString(), 2));
                                    redBits.Clear();
                                    startIndex = takenBits;

                                } while (redLength - takenBits > 8);
                                if (redLength - takenBits > 0)
                                {
                                    redBits.Append(redVal.Substring(startIndex));
                                }
                            }
                            if (redBits.Length == 8 || (i == height - 1 && j == width - 1))
                            {
                                if (redBits.Length > 0)
                                {
                                    byte redByte = Convert.ToByte(redBits.ToString(), 2);
                                    writer.Write(redByte);
                                    redBits.Clear();

                                }
                            }

                        }

                    }

                    writer.Write(numOfBitsGreen);
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            //GREEN

                            string greenVal = greenDict[(int)OperatedImageMatrix[i, j].green];
                            int greenBitsLength = greenBits.Length;
                            int greenLength = greenVal.Length;
                            remainingBits = 8 - greenBitsLength;
                            if (greenLength <= remainingBits)
                            {
                                greenBits.Append(greenVal);

                            }
                            else
                            {
                                int startIndex = 0;
                                int takenBits = 0;
                                do
                                {
                                    remainingBits = 8 - greenBits.Length;
                                    takenBits += remainingBits;
                                    greenBits.Append(greenVal.Substring(startIndex, remainingBits));
                                    writer.Write(Convert.ToByte(greenBits.ToString(), 2));
                                    greenBits.Clear();
                                    startIndex = takenBits;

                                } while (greenLength - takenBits > 8);
                                if (greenLength - takenBits > 0)
                                {
                                    greenBits.Append(greenVal.Substring(startIndex));
                                }
                            }
                            if (greenBits.Length == 8 || (i == height - 1 && j == width - 1))
                            {
                                if (greenBits.Length > 0)
                                {
                                    byte greenByte = Convert.ToByte(greenBits.ToString(), 2);
                                    writer.Write(greenByte);
                                    greenBits.Clear();

                                }
                            }

                        }

                    }
                    writer.Write(numOfBitsBlue);
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            //BLUE

                            string blueVal = blueDict[(int)OperatedImageMatrix[i, j].blue];
                            int blueBitsLength = blueBits.Length;
                            int blueLength = blueVal.Length;
                            remainingBits = 8 - blueBitsLength;
                            if (blueLength <= remainingBits)
                            {
                                blueBits.Append(blueVal);

                            }
                            else
                            {
                                int startIndex = 0;
                                int takenBits = 0;
                                do
                                {
                                    remainingBits = 8 - blueBits.Length;
                                    takenBits += remainingBits;
                                    blueBits.Append(blueVal.Substring(startIndex, remainingBits));
                                    writer.Write(Convert.ToByte(blueBits.ToString(), 2));
                                    blueBits.Clear();
                                    startIndex = takenBits;

                                } while (blueLength - takenBits > 8);
                                if (blueLength - takenBits > 0)
                                {
                                    blueBits.Append(blueVal.Substring(startIndex));
                                }
                            }
                            if (blueBits.Length == 8 || (i == height - 1 && j == width - 1))
                            {
                                if (blueBits.Length > 0)
                                {
                                    byte blueByte = Convert.ToByte(blueBits.ToString(), 2);
                                    writer.Write(blueByte);
                                    blueBits.Clear();

                                }
                            }

                        }

                    }
                }
                else
                {
                    for(int i = 0; i < height; i++)
                    {
                        for(int j = 0; j < width; j++)
                        {
                            writer.Write((byte)OperatedImageMatrix[i, j].red);
                        }
                    }
                    writer.Write(numOfBitsGreen);
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            writer.Write((byte)OperatedImageMatrix[i, j].green);
                        }
                    }
                    writer.Write(numOfBitsBlue);
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            writer.Write((byte)OperatedImageMatrix[i, j].blue);
                        }
                    }
                }
                sw.Stop();
                MessageBox.Show(sw.Elapsed.ToString());

            }
        }

        private void ReadFromFile(ref int numOfBitsRed, ref int numOfBitsGreen, ref int numOfBitsBlue, ref byte[] redArr, ref byte[] greenArr,
            ref byte[] blueArr)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(fileReadPath, FileMode.Open)))
            {
                isEncryptedRead = reader.ReadBoolean();
                if (isEncryptedRead)
                {
                    seedRead = reader.ReadString();
                    tapPosRead = reader.ReadByte();
                }
                compressed100 = reader.ReadBoolean();
                void readDicts(ref Dictionary<int, int> dict)
                {
                    string valRep;
                    short val;
                    byte valLength;
                    int count= reader.ReadInt32();
                    for(int i=0; i < count; i++)
                    {

                        val = reader.ReadInt16(); 
                        valLength = reader.ReadByte();
                        valRep = Convert.ToString(val, 2).PadLeft(valLength, '0');

                        dict[valRep.GetHashCode()] = (int)reader.ReadByte(); 
                    }
                }
                
                if(!compressed100)
                {
                    readDicts(ref redDictRead);
                    readDicts(ref greenDictRead);
                    readDicts(ref blueDictRead);
                }
                heightRead = reader.ReadInt32();
                widthRead = reader.ReadInt32();
                try
                {
                    decompressedImage = new DecompressedImage[heightRead, widthRead];

                    void readColors(ref int bitsCount,ref byte[] colorBytes)
                    {
                        bitsCount= reader.ReadInt32();
                        int byteCount = bitsCount % 8 == 0 ? bitsCount / 8 : bitsCount / 8 + 1;
                        colorBytes=new byte[byteCount];
                        for (int i = 0; i < byteCount; i++)
                        {
                            colorBytes[i] = reader.ReadByte();
                        }
                    }

                    //read red bytes into red list
                    readColors(ref numOfBitsRed, ref redArr);
                    readColors(ref numOfBitsGreen, ref greenArr);
                    readColors(ref numOfBitsBlue, ref blueArr);
                   
                }
                catch (OutOfMemoryException e)
                {
                    Console.WriteLine($"EXCEPTION: {e.Message}");
                }
            }
        }

        private void ReadEveryPixel(int numOfBitsRed, int numOfBitsGreen, int numOfBitsBlue, byte[] redArr, byte[] greenArr,
            byte[] blueArr)
        {
            List<string> redList = new List<string>();
            List<string> greenList = new List<string>();
            List<string> blueList = new List<string>();

            if (compressed100)
            {
                //read red bytes into green list
                readUncompressedImage(redArr, ref redList, ref redDictRead);
                //read green bytes into green list
                readUncompressedImage(greenArr, ref greenList, ref greenDictRead);
                //read blue bytes into blue list
                readUncompressedImage(blueArr, ref blueList, ref blueDictRead);
            }
            else
            {
                //Task handlingValues1 = Task.Run(() => ReadCompressedImage(redArr, ref redList, redDictRead, numOfBitsRed));
                //Task handlingValues2 = Task.Run(() => ReadCompressedImage(blueArr, ref blueList, blueDictRead, numOfBitsBlue));
                //Task handlingValues3 = Task.Run(() => ReadCompressedImage(greenArr, ref greenList, greenDictRead, numOfBitsGreen));

                //Task.WaitAll(handlingValues1, handlingValues2, handlingValues3);
                //Stopwatch readEvery = new Stopwatch();
                //readEvery.Reset();
                //readEvery.Start();
                //read red bytes into green list
                ReadCompressedImage(redArr, ref redList, redDictRead, numOfBitsRed);
                //read green bytes into green list
                ReadCompressedImage(greenArr, ref greenList, greenDictRead, numOfBitsGreen);
                //read blue bytes into blue list
                ReadCompressedImage(blueArr, ref blueList, blueDictRead, numOfBitsBlue);
                //readEvery.Stop();
                //MessageBox.Show(readEvery.Elapsed.ToString());
            }
            //put values
            PutDecompressedValues(redList, greenList, blueList);
        }

        private void readUncompressedImage(byte[] arr, ref List<string> list, ref Dictionary<int, int> dict)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < arr.Length; i++)
            {
                string thisByte = Convert.ToString(arr[i], 2).PadLeft(8, '0');
                list.Add(thisByte);
                if (!dict.ContainsKey(thisByte.GetHashCode()))
                {
                    dict[thisByte.GetHashCode()] = (int)arr[i];
                }
            }

        }

        private void ReadCompressedImage(byte[] arr, ref List<string> list, Dictionary<int, int> dict, int numberOfBits)
        {
            bool divisibleBy8 = false;
            if(numberOfBits % 8 == 0)
                divisibleBy8 = true;
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < arr.Length - 1; i++)
            {
                string thisByte = Convert.ToString(arr[i], 2).PadLeft(8, '0');
                for(int j = 0; j < thisByte.Length; j++)
                {
                    sb.Append(thisByte[j]);
                    if (dict.ContainsKey(sb.ToString().GetHashCode()))
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                

            }
            string lastByte;
            if(!divisibleBy8) { 
                lastByte = Convert.ToString(arr[arr.Length - 1], 2).PadRight(8, '0');
            }
            else
            {
                lastByte = Convert.ToString(arr[arr.Length - 1], 2).PadLeft(8, '0');
            }
            for (int j = 0; j < lastByte.Length; j++)
            {
                sb.Append(lastByte[j]);
                if (dict.ContainsKey(sb.ToString().GetHashCode()))
                {
                    list.Add(sb.ToString());
                    sb.Clear();
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
                //int tap = Convert.ToInt32(TapPosTextBox.Text);
                key = Convert.ToString(i, 2).PadLeft(n, '0');
                for(int j = 0; j < key.Length; j++)
                {
                    OperatedImageMatrix = ImageOperations.ImageEncryption(ImageMatrix, key, j);
                    Histogram histoCrypted = new Histogram(OperatedImageMatrix);
                    //MessageBox.Show($"maxMIN: {maxOrMin}, curr {histoCrypted.Derivation()}");
                    if (histoCrypted.Derivation() >= maxOrMin)
                    {
                        resultImageMatrix = OperatedImageMatrix;
                        maxOrMin = histoCrypted.Derivation();
                    }
                    //ImageOperations.DisplayImage(OperatedImageMatrix, pictureBox2);
                }


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
            //string[] names= fileName.Split('\\');
            SaveFileDialog fileSave = new SaveFileDialog();
            fileSave.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            fileSave.Title = "save encrepted image"; 
            fileSave.Filter= "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*";
            //fileSave.FileName = $"ecrypted {names[names.Length-1]}";
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

        private void SaveFileBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            saveFileDialog.Title = "save Compressed File";
            saveFileDialog.DefaultExt = "bin";
            saveFileDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";

            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;

                WriteInaFile(fileName, redDict, greenDict, blueDict, key, tapPos, numOfBytesRed, numOfBytesGreen, numOfBytesBlue);

                MessageBox.Show("File saved successfully.");
            }
            else
            {
                MessageBox.Show("File save operation cancelled by the user.");
            }
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                fileReadPath = OpenedFilePath;
            }
        }
    }
}