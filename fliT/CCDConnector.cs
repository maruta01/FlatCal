using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FliSharp.FLI;

namespace Astronet_Station_V2._0.Connector
{

    public class DeviceInfo
    {
        public string FileName ;
        public string ModelName;
        public string SerialNumber;
    }


    public class CCDConnector
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

        public virtual bool IsShutterOpen()
        {
            throw new NotImplementedException();
        }

        public virtual bool CanSetTemp()
        {
            throw new NotImplementedException();
        }

        public virtual double GetCoolerPower()
        {
            throw new NotImplementedException();
        }

        public virtual bool GetFastReadOutMode()
        {
            throw new NotImplementedException();
        }

        public virtual int GetReadOutMode()
        {
            return TDIRate;
        }

        public virtual bool HasShutter()
        {
            return true;
        }

        public virtual double GetTemperature()
        {
            throw new NotImplementedException();
        }

        public virtual void FLIGetArrayArea(out int ul_x, out int ul_y, out int lr_x, out int lr_y)
        {
            throw new NotImplementedException();
        }

        public virtual void GetVisibleArea(out int ul_x, out int ul_y, out int lr_x, out int lr_y)
        {
            throw new NotImplementedException();
        }

        public virtual void SetImageArea(int ul_x, int ul_y, int lr_x, int lr_y)
        {
            throw new NotImplementedException();
        }

        public virtual void SetFrameType(Helper.FRAME_TYPE frameType)
        {
            throw new NotImplementedException();
        }

        public virtual void SetHBin(int HBin)
        {
            throw new NotImplementedException();
        }

        public virtual void SetVBin(int VBin)
        {
            throw new NotImplementedException();
        }

        public virtual void SetTDI(int TDIRate)
        {
            throw new NotImplementedException();
        }

        public virtual void SetExposureTime(int expTime)
        {
            throw new NotImplementedException();
        }

        public virtual void Expose()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsDownloadReady()
        {
            throw new NotImplementedException();
        }

        public virtual ushort[] GetImageData()
        {
            throw new NotImplementedException();
        }

        public virtual int GetImageDataSize()
        {
            throw new NotImplementedException();
        }

        public virtual void GrabRow(ushort[] buff)
        {
            throw new NotImplementedException();
        }

        public virtual void SetTemperature(double temp)
        {
            throw new NotImplementedException();
        }

        public virtual void SetCoolerOn(bool isCoolerOn)
        {
            throw new NotImplementedException();
        }

        public virtual void CancelExposure()
        {
            throw new NotImplementedException();
        }

        public virtual void Close()
        {
            throw new NotImplementedException();
        }

    }
}
