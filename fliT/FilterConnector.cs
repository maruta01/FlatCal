using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FliSharp.FLI;

namespace Astronet_Station_V2._0.Connector
{
    public class FilterConnector
    {
        public string Brand = null;

        public int TDIRate = 0; //8Mhz
        public int HBin = 1;
        public int VBin = 1;

        public short XSub = 1;
        public short YSub = 1;

        public bool isCoolerOn = false;

        public virtual DeviceInfo[] GetList()
        {
            throw new NotImplementedException();
        }

        public virtual bool Connect(String name)
        {
            throw new NotImplementedException();
        }

        public virtual string GetBrand()
        {
            throw new NotImplementedException();
        }

        public virtual string GetModel()
        {
            throw new NotImplementedException();
        }

        public virtual string GetSerialString()
        {
            throw new NotImplementedException();
        }

        public virtual string GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public virtual long GetFilterPosition()
        {
            throw new NotImplementedException();
        }

        public virtual long GetFilterCount()
        {
            throw new NotImplementedException();
        }

        public virtual void SetFilterPosition(int filterPosition, int cameraPosition = -1)
        {
            throw new NotImplementedException();
        }
        
        public virtual void Close()
        {
            throw new NotImplementedException();
        }
    }
}
