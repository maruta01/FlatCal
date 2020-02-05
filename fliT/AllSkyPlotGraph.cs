using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace fliT
{
    class AllSkyPlotGraph
    {
        public double telescopeRadius { get; set; }
        public int imageboxHeight { get; set; }
        

        public Point CalculatePoint(Double Az, Double Alt)
        {
            int X, Y;
            int A = (int)(this.imageboxHeight / 2.0), B = (int)(this.imageboxHeight / 2.0);
            Az = Az * Math.PI / 180.0;
            Alt = Alt * Math.PI / 180.0;
            Az = Az - (Math.PI / 2.0);
            Double R = CalculateRadius(Alt);
            //R = R * (-1);
            X = Convert.ToInt32(A + (R * -1) * Math.Cos(Az));
            Y = Convert.ToInt32(B + R * Math.Sin(Az));

            Point point = new Point(X - Convert.ToInt32(this.telescopeRadius), Y - Convert.ToInt32(this.telescopeRadius));
            return (point);

        }
        private Double CalculateRadius(Double El)
        {
            Double Radius = 0;
            if (El > 0)
                try
                {
                    Radius = El * (this.imageboxHeight / 4.0) / (Double)(Math.PI / 4.0);
                    Radius = this.imageboxHeight / 2.0 - Radius;
                }
                catch { }
            else
                Radius = this.imageboxHeight / 2.0;
            return Radius;
        }


        public void addBackground(Image<Bgr, byte> allSkyImage, out Image<Bgr, byte> addImgBg)
        {

            Image<Bgr, byte> imgBg = new Image<Bgr, byte>("allskybg.png"); // Background size 640x640 px
            addImgBg = imgBg;
            int X1 = (imgBg.Height - allSkyImage.Height) / 2; // px bg0 -> img
            int X2 = X1 + allSkyImage.Height; // px img -> bg640

            for (int i = 0; i < imgBg.Height; i++)
            {
                for (int j = 0; j < imgBg.Width; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (i <= X1 || i >= X2)
                        {
                            addImgBg.Data[i, j, k] = imgBg.Data[i, j, k];
                        }
                        else
                        {
                            addImgBg.Data[i, j, k] = allSkyImage.Data[i - X1, j, k];
                        }
                    }
                }
            }
        }




    }
}
