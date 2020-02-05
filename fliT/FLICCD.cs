using FliSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FliSharp.FLI;

namespace Astronet_Station_V2._0.Connector
{
    public class FLICCD : CCDConnector
    {
        private FLI fli = null;

        public FLICCD()
        {
            TDIRate = 0; //8Mhz
            HBin = 1;
            VBin = 1;

            XSub = 1;
            YSub = 1;

            isCoolerOn = false;
            Brand = "FLI";
        }

        public override DeviceInfo[] GetList()
        {
            DeviceName[] names;
            
            names = FLI.List(FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB);

            if (names.Count() > 0)
            {
                DeviceInfo[] info = new DeviceInfo[names.Count()];

                int i = 0;

                foreach (DeviceName name in names)
                {
                    info[i] = new DeviceInfo();
                    info[i].FileName = name.FileName;
                    info[i].ModelName = name.ModelName;

                    FLI fli = new FLI(name.FileName, FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB);
                    name.SerialNumber = fli.GetSerialString();

                    info[i].SerialNumber = name.SerialNumber;

                    ++i;
                }

                return info;
            }
            else
            {
                return null;
            }
        }

        public override bool Connect(String name)
        {
            fli = new FLI(name, FLI.DOMAIN.CAMERA | FLI.DOMAIN.USB);
            return true;
        }

        public override string GetBrand()
        {
            if (fli == null) return null;
            return Brand;
        }

        public override string GetModel()
        {
            if (fli == null) return null;
            return fli.GetModel();
        }

        public override string GetSerialString()
        {
            if (fli == null) return null;

            return fli.GetSerialString();
        }

        public override string GetDeviceStatus()
        {
            if (fli == null) return null;

            try
            {
                STATUS status = fli.GetDeviceStatus();

                //Console.WriteLine((STATUS)status.ToString());
                if (status.HasFlag(STATUS.CAMERA_STATUS_UNKNOWN))
                {
                    return "Unknow";
                }
                else if (status.HasFlag(STATUS.CAMERA_STATUS_EXPOSING))
                {
                    return "Exposing";
                }
                else if (status.HasFlag(STATUS.CAMERA_STATUS_READING_CCD))
                {
                    return "Reading";
                }
                else if (status.HasFlag(STATUS.CAMERA_STATUS_MASK))
                {
                    return "Mask";
                }
                else if (status.HasFlag(STATUS.CAMERA_STATUS_WAITING_FOR_TRIGGER))
                {
                    return "Wait Trigger";
                }                
                else if (status.HasFlag(STATUS.CAMERA_STATUS_IDLE))
                {
                    return "Idle";
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public override bool IsShutterOpen()
        {
            return (this.GetDeviceStatus() == "Exposing" ? true : false);
        }

        public override bool CanSetTemp()
        {
            return true;
        }

        public override double GetCoolerPower()
        {
            return fli.GetCoolerPower();
        }

        public override bool GetFastReadOutMode()
        {
            if(TDIRate == 8000000)
            {
                return false;
            }

            return false;
        }

        public override int GetReadOutMode()
        {
            return TDIRate;
        }

        public override bool HasShutter()
        {
            return true;
        }

        public override double GetTemperature()
        {
            return fli.GetTemperature();
        }

        public override void FLIGetArrayArea(out int ul_x, out int ul_y, out int lr_x, out int lr_y)
        {
            fli.GetArrayArea(out ul_x, out ul_y, out lr_x, out lr_y);
        }

        public override void GetVisibleArea(out int ul_x, out int ul_y, out int lr_x, out int lr_y)
        {
            fli.GetVisibleArea(out ul_x, out ul_y, out lr_x, out lr_y);
        }                

        public override void SetImageArea(int ul_x, int ul_y, int lr_x, int lr_y)
        {
            fli.SetImageArea(ul_x, ul_y, lr_x, lr_y);
        }

        public override void SetFrameType(Helper.FRAME_TYPE frameType)
        {
            FRAME_TYPE fliFrameType = FRAME_TYPE.NORMAL;

            if (frameType == Helper.FRAME_TYPE.NORMAL) fliFrameType = FRAME_TYPE.NORMAL;
            if (frameType == Helper.FRAME_TYPE.DARK) fliFrameType = FRAME_TYPE.DARK;
            if (frameType == Helper.FRAME_TYPE.FLOOD) fliFrameType = FRAME_TYPE.FLOOD;
            if (frameType == Helper.FRAME_TYPE.RBI_FLUSH) fliFrameType = FRAME_TYPE.RBI_FLUSH;

            fli.SetFrameType(fliFrameType);
        }

        public override void SetHBin(int HBin)
        {
            fli.SetHBin(HBin);
            this.HBin = HBin;
        }

        public override void SetVBin(int VBin)
        {
            fli.SetVBin(VBin);
            this.VBin = VBin;
        }

        public override void SetTDI(int TDIRate)
        {
            fli.SetTDI(TDIRate);
            this.TDIRate = TDIRate;
        }

        public override void SetExposureTime(int expTime)
        {
            fli.SetExposureTime(expTime);
        }

        public override void Expose()
        {
            fli.ExposeFrame();
        }

        public override bool IsDownloadReady()
        {
            return fli.IsDownloadReady();
        }

        public override void GrabRow(ushort[] buff)
        {
            fli.GrabRow(buff);
        }

        public override void SetTemperature(double temp)
        {
            fli.SetTemperature(temp);
        }

        public override void SetCoolerOn(bool isCoolerOn)
        {
            if (isCoolerOn)
            {             
                if(this.GetSerialString() == "PL0093812")
                {
                    this.SetTemperature(-10);
                }
                else
                {
                    this.SetTemperature(-20);
                }
                
                this.isCoolerOn = true;
            }
            else
            {
                this.SetTemperature(20);
                this.isCoolerOn = false;
            }
        }

        public override void CancelExposure()
        {
            fli.CancelExposure();
        }

        public override void Close()
        {
            if (fli != null)
            {
                fli.Close();
            }
        }

      
    }
}
