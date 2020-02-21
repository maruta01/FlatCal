using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.CvEnum;

namespace AllSky
{
    class Allskyplotgraph
    {
        private Image<Bgr, byte> allSkyImage;
        private double azm;
        private double alt;
        private int telescopeRadius = 5;

        public Image<Bgr, byte> plotTelescope(Image<Bgr, byte> allSkyImage, Point point, int telescopeRadius = 5)
        {
            this.allSkyImage = allSkyImage;
         
            this.telescopeRadius = telescopeRadius;

            Image<Bgr, byte> allskyplot = addGraph(allSkyImage);

            
            Point pointTextAzm = point;
            pointTextAzm.X = pointTextAzm.X + 20;
            Point pointTextAlt = pointTextAzm;
            pointTextAlt.Y = pointTextAlt.Y + 25;

            CvInvoke.Circle(allskyplot, point, telescopeRadius, new MCvScalar(0, 0, 255), 2);
            CvInvoke.PutText(allskyplot, "Azm: " + azm, pointTextAzm, FontFace.HersheySimplex, 0.65, new Bgr(Color.Red).MCvScalar);
            CvInvoke.PutText(allskyplot, "Alt: " + alt, pointTextAlt, FontFace.HersheySimplex, 0.65, new Bgr(Color.Red).MCvScalar);

            return (allskyplot);
        }
        public Point calculatePoint(Double azm, Double alt)
        {
            this.alt = alt;
            this.azm = azm;
            int X, Y;
            int A = (int)(500 / 2.0), B = (int)(500 / 2.0);
            azm = azm * Math.PI / 180.0;
            alt = alt * Math.PI / 180.0;
            azm = azm - (Math.PI / 2.0);
            Double R = calculateRadius(alt);
            //R = R * (-1);
            X = Convert.ToInt32(A + (R * -1) * Math.Cos(azm));
            Y = Convert.ToInt32(B + R * Math.Sin(azm));
            Point point = new Point(X - Convert.ToInt32(telescopeRadius), Y - Convert.ToInt32(telescopeRadius));
            return (point);
        }


        private Double calculateRadius(Double El)
        {
            Double Radius = 0;
            if (El > 0)
                try
                {
                    Radius = El * (500 / 4.0) / (Double)(Math.PI / 4.0);
                    Radius = 500 / 2.0 - Radius;
                }
                catch { }
            else
                Radius = 500 / 2.0;
            return Radius;
        }


        private Image<Bgr, byte> addGraph(Image<Bgr, byte> allSkyImage)
        {
            Image<Bgr, byte> imgBg = new Image<Bgr, byte>(640, 640);
            Image<Bgr, byte> addImgBg = imgBg;
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
                            addImgBg.Data[i, j, k] = 255;
                        }
                        else
                        {
                            addImgBg.Data[i, j, k] = this.allSkyImage.Data[i - X1, j, k];
                        }
                    }
                }
            }
            CvInvoke.Circle(addImgBg, new Point((addImgBg.Height / 2), (addImgBg.Width / 2)), 320, new MCvScalar(0, 0, 0), 2);
            return (addImgBg);
        }



    }
}
