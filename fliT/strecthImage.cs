using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace convertFitsToJPG
{
    class strecthImage
    {
        public static void GetStrecthType(String StretchProfile, out Double lowerPercen, out Double upperPercen)
        {
            switch (StretchProfile)
            {
                case "low": upperPercen = 99.71; lowerPercen = 5.6; break;
                case "medium": upperPercen = 85; lowerPercen = 80; break;
                case "hight": upperPercen = 99.25; lowerPercen = 50.00; break;
                case "moon": upperPercen = 99.87; lowerPercen = 95.04; break;
                case "planet": upperPercen = 99.92; lowerPercen = 99.16; break;
                case "Max Value": upperPercen = 100.00; lowerPercen = 0.0; break;
                default: upperPercen = 100.00; lowerPercen = 0.0; break;
            }
        }


        public static Matrix<UInt16> ConvertStretchImageU16BitToJPG(Matrix<UInt16> CVMat, Double LowerPercen, Double UpperPercen)
        {
            int DataLength = CVMat.Rows * CVMat.Cols;
            UInt16[] Data = new UInt16[DataLength];
            CVMat.Mat.CopyTo(Data);

            int LowerPosition = Convert.ToInt32(DataLength * LowerPercen / 100);
            int UpperPosition = Convert.ToInt32(DataLength * UpperPercen / 100);

            List<UInt16> DataList = Data.ToList();
            DataList.Sort();

            UInt16 LowerValue = DataList[LowerPosition == DataLength ? LowerPosition - 1 : LowerPosition];
            UInt16 UpperValue = DataList[UpperPosition == DataLength ? UpperPosition - 1 : UpperPosition];

            Double A = (Double)ushort.MinValue;
            Double B = (Double)ushort.MaxValue;
            Double C = (Double)LowerValue;
            Double D = (Double)UpperValue;
           
            Matrix <UInt16> NewImg = (CVMat - C) * ((B - A) / (D - C)) + A;
            NewImg = NewImg / 257;
            return NewImg;
        }
    
    }
}
