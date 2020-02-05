using FliSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FliSharp.FLI;

namespace Astronet_Station_V2._0.Connector
{
    public class FLIFilter : FilterConnector
    {
        private FLI fli = null;

        public FLIFilter()
        {
            Brand = "FLI";
        }

        public override DeviceInfo[] GetList()
        {
            DeviceName[] names;

            names = FLI.List(FLI.DOMAIN.FILTERWHEEL | FLI.DOMAIN.USB);

            if (names.Count() > 0)
            {
                DeviceInfo[] info = new DeviceInfo[names.Count()];

                int i = 0;

                foreach (DeviceName name in names)
                {
                    info[i] = new DeviceInfo();
                    info[i].FileName = name.FileName;

                    info[i].ModelName = name.ModelName;

                    FLI fli = new FLI(name.FileName, FLI.DOMAIN.FILTERWHEEL | FLI.DOMAIN.USB);
                    name.SerialNumber = fli.GetSerialString();

                    if (name.SerialNumber == "")
                        info[i].SerialNumber = name.ModelName;
                    else
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

        public override bool Connect(string name)
        {
            fli = new FLI(name, FLI.DOMAIN.FILTERWHEEL | FLI.DOMAIN.USB);
            fli.SetFilterPos(0);
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

            if (fli.GetSerialString() == "") return fli.GetModel();

            return fli.GetSerialString();
        }

        public override string GetDeviceStatus()
        {
            if (fli == null) return null;

            try
            {

                STATUS status = fli.GetDeviceStatus();

                if (status.HasFlag(STATUS.FILTER_POSITION_UNKNOWN))
                {
                    return "Unknow";
                }
                else if (status.HasFlag(STATUS.FILTER_STATUS_HOME))
                {
                    return "Home";
                }
                else if (status.HasFlag(STATUS.FILTER_STATUS_HOME_SUCCEEDED))
                {
                    return "Succeeded";
                }
                else if (status.HasFlag(STATUS.FILTER_STATUS_HOMING))
                {
                    return "Homing";
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

        public override long GetFilterPosition()
        {
            return fli.GetFilterPos();
        }

        public override long GetFilterCount()
        {
            return fli.GetFilterCount();
        }

        public override void SetFilterPosition(int filterPosition, int cameraPosition = -1)
        {
            fli.SetFilterPos(filterPosition);
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
