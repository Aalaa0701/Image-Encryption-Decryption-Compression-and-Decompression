using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Priority_Queue;
using System.Linq;
using System.ComponentModel;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageEncryptCompress
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct DecompressedImage
    {
        public string redRep, greenRep, blueRep;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    public class Node
    {
        public int colorval;
        public int freq;
        public Node left;
        public Node right;
        //int val;
        public Node(int colorval,int freq,Node left ,Node right)
        {
            this.freq = freq;
            this.left = left;
            this.right = right;
            this.colorval = colorval;
            //this.val = val;
        }
    }
  
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }
        public static int GetHeightCompressed(DecompressedImage[,] compressedImage)
        {
            return compressedImage.GetLength(0);

        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }
        public static int GetWidthCompressed(DecompressedImage[,] compressedImage)
        {
            return compressedImage.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


       /// <summary>
       /// Apply Gaussian smoothing filter to enhance the edge detection 
       /// </summary>
       /// <param name="ImageMatrix">Colored image matrix</param>
       /// <param name="filterSize">Gaussian mask size</param>
       /// <param name="sigma">Gaussian sigma</param>
       /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

           
            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }
       //static HashSet<int> redPasswords= new HashSet<int>();
       //static HashSet<int> greenPasswords = new HashSet<int>();
       //static HashSet<int> bluePasswords = new HashSet<int>();
        public static RGBPixel[,] ImageEncryption(RGBPixel[,] ImageMatrix,String initKey, int tapPosition)
        {
            //redPasswords.Clear();
            //greenPasswords.Clear();
            //bluePasswords.Clear();
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);
            string key = initKey;
            RGBPixel[,] resultImageMatrix=new RGBPixel[Height,Width];
            Array.Copy(ImageMatrix, 0, resultImageMatrix, 0, ImageMatrix.Length);
            for (int i = 0; i < Height; i++)
            {
                for(int j = 0; j < Width; j++)
                {
                    int redPassword = BitwiseOperations.GeneratePassword(ref key, tapPosition);
                    int greenPassword = BitwiseOperations.GeneratePassword(ref key, tapPosition);
                    int bluePassword = BitwiseOperations.GeneratePassword(ref key, tapPosition);
                    //redPasswords.Add(redPassword);
                    //greenPasswords.Add(greenPassword);
                    //bluePasswords.Add(bluePassword);
                    resultImageMatrix[i, j].red ^= (byte)redPassword;
                    resultImageMatrix[i, j].green ^= (byte)greenPassword;
                    resultImageMatrix[i, j].blue ^= (byte)bluePassword;
                }

            }
            //MessageBox.Show($"red:{redPasswords.Count}\n" +
            //    $"blue:{bluePasswords.Count}\n" +
            //    $"green:{greenPasswords.Count}\n");
            return resultImageMatrix;
        }
        public static RGBPixel[,] ImageEncryptionAlphanumeric(RGBPixel[,] ImageMatrix, String initKey, int tapPosition)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);
            StringBuilder initialSeed = new StringBuilder(initKey);
            int seedLength = initialSeed.Length;
            RGBPixel[,] resultImageMatrix = new RGBPixel[Height, Width];
            Array.Copy(ImageMatrix, 0, resultImageMatrix, 0, ImageMatrix.Length);
            for(int i = 0; i < Height; i++)
            {
                for(int j = 0; j < Width; j++)
                {
                    byte redChar = BitwiseOperations.GenerateAlphanumericPassword(ref initialSeed, tapPosition, seedLength);
                    byte greenChar = BitwiseOperations.GenerateAlphanumericPassword(ref initialSeed, tapPosition, seedLength);
                    byte blueChar = BitwiseOperations.GenerateAlphanumericPassword(ref initialSeed, tapPosition, seedLength);
                    resultImageMatrix[i, j].red ^= redChar;
                    resultImageMatrix[i, j].green ^= greenChar;
                    resultImageMatrix[i, j].blue ^= blueChar;
                }
            }

            return resultImageMatrix;

        }
        static SimplePriorityQueue<Node> priorityQueueRed = new SimplePriorityQueue<Node>();
        static SimplePriorityQueue<Node> priorityQueueBlue = new SimplePriorityQueue<Node>();
        static SimplePriorityQueue<Node> priorityQueueGreen = new SimplePriorityQueue<Node>();
        public static Dictionary<int, int> encodeint = new Dictionary<int, int>();
        //traverse tree 
        public static Dictionary<int, string> Traverse(Node root, string bit, Dictionary<int, string> encode)
        {
            //encode.Clear();
            if (root == null)
            {
                return encode;
            }
            if (root.left == null && root.right == null)
            {
                    encode[root.colorval] = bit;
            }
            Traverse(root.left, bit + "0",encode);
            Traverse(root.right, bit + "1", encode);
            return encode;

        }
        public static RGBPixel[,] Compression(ref int numOfBytesRed, ref int numOfBytesGreen, ref int numOfBytesBlue, ref Dictionary<int, string> redDict, ref Dictionary<int, string> greenDict, ref Dictionary<int, string> blueDict, RGBPixel[,] ImageMatrix)
        {   
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);
            Dictionary<int , int> freqRed = new Dictionary<int , int>();
            Dictionary<int , int> freqBlue = new Dictionary<int , int>();
            Dictionary<int , int> freqGreen = new Dictionary<int , int>();
            Node left, right;
            int newFreq;
            int originalSizer=0;
            int originalSizeb=0;
            int originalSizeg=0;
            int compressedSizer=0;
            int compressedSizeb=0;
            int compressedSizeg=0;
            //frequency of red
            Histogram histogram = new Histogram(ImageMatrix);
            for(int i = 0; i < histogram.redHistogram.Count();i++)
            {
                if (histogram.redHistogram[i]!=0)
                    freqRed[i] = histogram.redHistogram[i];
                
            }
            //frequency of blue 
            for (int i = 0; i < histogram.blueHistogram.Count(); i++)
            {
                if (histogram.blueHistogram[i] != 0)
                    freqBlue[i] = histogram.blueHistogram[i];

            }
            //frequency of green
            for (int i = 0; i < histogram.greenHistogram.Count(); i++)
            {
                if (histogram.greenHistogram[i] != 0)
                    freqGreen[i] = histogram.greenHistogram[i];

            }
            //calc original size
            foreach(var i in freqRed)
            {
               
                originalSizer += (freqRed[i.Key]*8);
            }

            foreach (var i in freqBlue)
            {
                originalSizeb += (freqBlue[i.Key]*8);
            }
            foreach (var i in freqGreen)
            {
                originalSizeg += (freqGreen[i.Key] * 8);
            }

            //huffman tree for red values
            foreach (var i in freqRed.Keys)
            {
                Node newNode = new Node(i,freqRed[i],null,null);
                priorityQueueRed.Enqueue(newNode, freqRed[i]);
            }
            while (priorityQueueRed.Count != 1)
            {
                left = priorityQueueRed.Dequeue();
                right = priorityQueueRed.Dequeue();
                newFreq = left.freq + right.freq;
                Node newNode = new Node(260, newFreq, left, right);
                priorityQueueRed.Enqueue(newNode, newFreq);
            }
            //huffman tree for blue values
            foreach (var i in freqBlue.Keys)
            {
                Node newNode = new Node(i, freqBlue[i], null, null);
                priorityQueueBlue.Enqueue(newNode, freqBlue[i]);
            }
            while (priorityQueueBlue.Count != 1)
            {
                left = priorityQueueBlue.Dequeue();
                right = priorityQueueBlue.Dequeue();
                newFreq = left.freq + right.freq;
                Node newNode = new Node(260, newFreq, left, right);
                priorityQueueBlue.Enqueue(newNode, newFreq);
            }
            //huffman tree for green values
            foreach (var i in freqGreen.Keys)
            {
                Node newNode = new Node(i, freqGreen[i], null, null);
                priorityQueueGreen.Enqueue(newNode, freqGreen[i]);
            }
            while (priorityQueueGreen.Count != 1)
            {
                left = priorityQueueGreen.Dequeue();
                right = priorityQueueGreen.Dequeue();
                newFreq = left.freq + right.freq;
                Node newNode = new Node(260, newFreq, left, right);
                priorityQueueGreen.Enqueue(newNode, newFreq);
            }


            //Dictionary<int, string> redencoding = new Dictionary<int, string>();
            Traverse(priorityQueueRed.Dequeue(), "",redDict);
            //Dictionary<int, string> blueencoding = new Dictionary<int, string>();
            Traverse(priorityQueueBlue.Dequeue(), "",blueDict);
            //Dictionary<int, string> greenencoding = new Dictionary<int, string>();
            Traverse(priorityQueueGreen.Dequeue(), "",greenDict);
          
            foreach (var i in redDict)
            {
                compressedSizer += freqRed[i.Key] * i.Value.Length;
            }
            foreach (var i in blueDict)
            {
                compressedSizeb += freqBlue[i.Key] * i.Value.Length;
            }
            foreach (var i in greenDict)
            {
                compressedSizeg += freqGreen[i.Key] * i.Value.Length;
            }
            //new variables 
            numOfBytesRed = compressedSizer;
            numOfBytesGreen = compressedSizeg;
            numOfBytesBlue = compressedSizeb;

            double origSize = Height * Width * 24;
            double compressedSize = compressedSizer + compressedSizeg + compressedSizeb;
            double compRatio = (compressedSize / origSize) * 100;
            MessageBox.Show((compressedSizer + compressedSizeg + compressedSizeb).ToString());
            MessageBox.Show(origSize.ToString());
            MessageBox.Show($"Compression ratio: {compRatio}");
            double redRatio = ((double)compressedSizer / (double)originalSizer) * 100;
            double blueRatio = ((double)compressedSizeb / (double)originalSizeb) * 100;
            double greenRatio = ((double)compressedSizeg / (double)originalSizeg) * 100;
            MessageBox.Show($"Red ratio :{redRatio}\n" +
            $"Blue ratio:{blueRatio}\n" +
            $"Green ratio:{greenRatio}\n");
            return ImageMatrix;
        }
       

        public static RGBPixel[,] Decompression(DecompressedImage[,] compressedImage, Dictionary<string, int> redDictionary, Dictionary<string, int> blueDictionary, Dictionary<string, int> greenDictionary)
        {
            int Height = GetHeightCompressed(compressedImage);
            int Width = GetWidthCompressed(compressedImage);

            RGBPixel[,] decompressedImage = new RGBPixel[Height, Width];
            int redIndex = 0, blueIndex = 0, greenIndex = 0;

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    
                    
                    byte redVal = (byte)redDictionary[compressedImage[i, j].redRep];
                    byte blueVal = (byte)blueDictionary[compressedImage[i, j].blueRep];
                    byte greenVal = (byte)greenDictionary[compressedImage[i, j].greenRep];

                    decompressedImage[i, j] = new RGBPixel { red = redVal, blue = blueVal, green = greenVal };
                    
                }
            }

            return decompressedImage;
        }

        public static int DecodeValue(Dictionary<int, string> dictionary, int encodedValue, ref int bitIndex)
        {
            string code = "";
            int value = 0;

            while (!dictionary.ContainsValue(code))
            {
                int bit = GetBit(encodedValue, bitIndex);
                code += bit.ToString();
                bitIndex++;
            }

            foreach (var pair in dictionary)
            {
                if (pair.Value == code)
                {
                    value = pair.Key;
                    break;
                }
            }

            return value;
        }

        public static int GetBit(int value, int bitIndex)
        {
            return (value >> bitIndex) & 1;
        }
    }
}
