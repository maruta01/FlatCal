using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fliT
{
    public class FlatCalculator
    {
        private string filterName; // Filter Name
        private double lastAdu; // Lasted Adu 
        private double lastExposureTime; // Lasted Exposure Time
        private double targetAdu; // Adu Target 
        private int  grabTime; // grab at time  ** start at 1 **
        private double exposureTimeCalculate; // calculated exposure Time  for grab 
        private double variableAdu; // use for calculate next grab


        public double flatExposureTime(string filterName, double lastAdu, double lastExposureTime, double targetAdu, int grabTime)
        {
            this.filterName = filterName;
            this.lastAdu = lastAdu;
            this.lastExposureTime = lastExposureTime;
            this.targetAdu = targetAdu;
            this.grabTime = grabTime;
           
            if (this.grabTime == 1) // frist grab for use frist Adu referent  .....
            {
              this.variableAdu = this.lastAdu; 
            }
            double aduCostError = this.lastAdu - this.targetAdu; // find difference between lastAdu and adu Target ..

            if (aduCostError < 0) // LastADU less than TargetADU..
            {
                this.variableAdu = this.variableAdu + Math.Abs(aduCostError);
            }
            else if (aduCostError > 0) // LastADU more than TargetADU..
            {
                this.variableAdu = variableAdu - Math.Abs(aduCostError);
            }
            exposureTimeCalculate = exposureCal(this.lastExposureTime, this.lastAdu, variableAdu); //Calculate the next exposure Time ..

            return (this.exposureTimeCalculate);
        }

        private double exposureCal(double oldExposure, double adu, double variableAdu)
        {
            double simulete = adu / oldExposure;
            double exposureCal = variableAdu / simulete;
            return (exposureCal);
        }
      

    }
}
