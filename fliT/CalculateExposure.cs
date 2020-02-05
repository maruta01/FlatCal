using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fliT
{
    class CalculateExposure
    {
        
        public void  calculateExposureFlat(double sunAlt, out double exposureTime)
        {
            exposureTime = (2.23912 * Math.Pow(sunAlt, 2))+(15.93709* sunAlt)+30.26516; //Filter R
            
        }
    }
}
