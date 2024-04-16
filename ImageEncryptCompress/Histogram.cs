
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

///Algorithms Project
///Intelligent Scissors
///
namespace ImageEncryptCompress
{
    public class Histogram
    {
        int[] redHistogram=new int[256];
        int[] greenHistogram=new int[256];
        int[] blueHistogram=new int[256];
        public Histogram(RGBPixel[,] img)
        {
            int Height =ImageOperations.GetHeight(img);
            int Width = ImageOperations.GetWidth(img);
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    redHistogram[(int)img[i,j].red] ++;
                    blueHistogram[(int)img[i,j].blue] ++;
                    greenHistogram[(int)img[i,j].green] ++;
                }
            }
            PlotHistogram(redHistogram, greenHistogram, blueHistogram);
        }

        private void PlotHistogram(int[] rHistogram, int[] gHistogram, int[] bHistogram)
        {
            // Create a new form to display the histograms
            Form histogramForm = new Form();
            histogramForm.Text = "RGB Histogram";
            histogramForm.Size = new Size(800, 400);

            // Create a panel to hold the histograms
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
    
            // Create a bitmap to draw the histograms
            Bitmap bitmap = new Bitmap(panel.Width, panel.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                // Draw the histograms
                DrawHistogram(graphics, rHistogram, Color.Red,1);
                DrawHistogram(graphics, gHistogram, Color.Green,200);
                DrawHistogram(graphics, bHistogram, Color.Blue,400);

            }

            // Display the bitmap in a picture box
            PictureBox pictureBox = new PictureBox();
            pictureBox.Width = 800;
            pictureBox.Height = 400;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Image = bitmap;

            // Add the picture box to the panel
            panel.Controls.Add(pictureBox);

            // Add the panel to the form
            histogramForm.Controls.Add(panel);

            // Show the form
            histogramForm.ShowDialog();
        }

        private void DrawHistogram(Graphics graphics, int[] histogram, Color color, float factor)
        {
            
            // Find the maximum value in the histogram
            int maxValue = histogram.Max();

            // Calculate the height of each bar
            float scaleFactor = (float)graphics.VisibleClipBounds.Height / maxValue;

            // Calculate the width of each bar
            float barWidth = graphics.VisibleClipBounds.Width / histogram.Length;

            // Draw the bars
            for (int i = 0; i < histogram.Length; i++)
            {
                float barHeight = histogram[i] * scaleFactor;
                RectangleF barRect = new RectangleF((i * barWidth), graphics.VisibleClipBounds.Height - barHeight, barWidth, barHeight);
                graphics.FillRectangle(new SolidBrush(color), barRect);
            }
        }
    }
}
