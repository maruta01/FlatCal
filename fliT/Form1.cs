using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Management;
using Timer = System.Timers.Timer;
using FliSharp;
using Astronet_Station_V2._0.Connector;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu;
using Astro;
using AstroCalculation;
using System.Xml;
using System.Net;
using SRSLib;
using MongoDB.Driver;
using MongoDB_CCD;
using convertFitsToJPG;
using Emgu.CV.CvEnum;
using AllSky;


namespace fliT
{
    public partial class Form1 : Form
    {

        private Timer time;
        FLICCD fliCCD = new FLICCD();
        FLIFilter fliFilter = new FLIFilter();
        String fliCCDFileName = null;
        String filterFileName = null;
        bool connectStatus = false;
        bool filterConnect = false;
        long filterCount;
        bool connectPlanwave = false;
        long getFilterPosition;
        string filterSelect = "unknow";
        double aDU = 0;
        string exposureTime = "NaN";
        double lastExposureTime, continueShot;

        static MongoClient client = new MongoClient();
        static IMongoDatabase db = client.GetDatabase("CCDFlat");
        static IMongoCollection<CCD_Mongo> collection = db.GetCollection<CCD_Mongo>("data");
        CalculateExposure calculateExposure = new CalculateExposure();

        Dictionary<int, string> dictFilter = new Dictionary<int, string>();
        private bool starLoop = false;



        double LATITUDE = 18.560833;
        double LONGITUDE = 98.50566;



        ////////////////TEST///////////////
        double fristExposure = 0;
        double addExposure = 0;

        double variableAdu = 0;
        int flatCount = 0;

        //////////////////////////////////
        public Form1()
        {
            InitializeComponent();
        }
        void SetTimer()
        {
            time = new Timer(500);
            time.Elapsed += TimeElapsed;
            time.AutoReset = true;
            time.Enabled = true;
        }
        private void TimeElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (connectStatus)
            {
                FliGetData();

            }
            if (filterConnect)
            {
                GetFilter();
            }
            if (connectPlanwave)
            {
                //GetPlanwaveInfo();
                pW4Getdata();
            }
            sunPosition(out double sunAlt, out double sunAzm);

        }


        private void button1_Click(object sender, EventArgs e)
        {
            DeviceInfo[] deviceInfos = fliCCD.GetList();
            if (deviceInfos != null)
            {
                fliCCDFileName = deviceInfos[0].FileName;
                textBox1.Text = "FileName : " + deviceInfos[0].FileName + Environment.NewLine;
                textBox1.Text += "ModelName : " + deviceInfos[0].ModelName + Environment.NewLine;
                textBox1.Text += "SerialNumber : " + deviceInfos[0].SerialNumber + Environment.NewLine;
            }
            else
            {
                textBox1.Text = "No Port Connecting ... ";
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (fliCCDFileName != null)
            {
                Boolean FLICCD_Connect_status = fliCCD.Connect(fliCCDFileName);
                if (FLICCD_Connect_status)
                {
                    connectStatus = true;

                    FliGetData();
                }
                else
                {
                    textBox2.Text = "   !!! Please FLICCD Connect !!!  " + Environment.NewLine;
                }
            }
            else
            {
                textBox2.Text = "   !!! Please FLICCD Connect !!!  " + Environment.NewLine;
            }
        }



        public void FliGetData()
        {
            textBox2.Invoke((Action)(() =>
            {
                if (connectStatus)
                {
                    label6.ForeColor = System.Drawing.Color.Green;
                    label6.Text = "Connected";
                    string getBrand = fliCCD.GetBrand();
                    string getModel = fliCCD.GetModel();
                    string getSerialString = fliCCD.GetSerialString();
                    string getDeviceStatus = fliCCD.GetDeviceStatus();
                    bool shutterOpen = fliCCD.IsShutterOpen();
                    bool canSetTemp = fliCCD.CanSetTemp();
                    double getCoolerPower = fliCCD.GetCoolerPower();
                    bool getFastReadOutMode = fliCCD.GetFastReadOutMode();
                    int getReadOutMode = fliCCD.GetReadOutMode();
                    bool hasShutter = fliCCD.HasShutter();
                    double getTemperature = fliCCD.GetTemperature();
                    textBox2.Text = "Brand : " + getBrand + Environment.NewLine;
                    textBox2.Text += "GetModel : " + getModel + Environment.NewLine;
                    textBox2.Text += "GetSerialString : " + getSerialString + Environment.NewLine;
                    textBox2.Text += "GetDeviceStatus : " + getDeviceStatus + Environment.NewLine;
                    textBox2.Text += "ShutterOpen : " + shutterOpen + Environment.NewLine;
                    textBox2.Text += "CanSetTemp : " + canSetTemp + Environment.NewLine;
                    textBox2.Text += "GetCoolerPower : " + getCoolerPower + Environment.NewLine;
                    textBox2.Text += "GetFastReadOutMode : " + getFastReadOutMode + Environment.NewLine;
                    textBox2.Text += "GetReadOutMode : " + getReadOutMode + Environment.NewLine;
                    textBox2.Text += "HasShutter : " + hasShutter + Environment.NewLine;
                    textBox2.Text += "GetTemperature : " + getTemperature + Environment.NewLine;
                }
                else
                {
                    label6.Text = "   !!! Please FLICCD Connect !!!  " + Environment.NewLine;
                }
            }));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (connectStatus)
            {
                exposeCamera(Convert.ToInt32(textBox4.Text), Convert.ToInt32(textBox5.Text), Convert.ToInt32(textBox6.Text), Convert.ToDouble(textBox3.Text));
                MessageBox.Show(" Expose finish '!! Clear Last ADU !!' ");
                aDU = 0;
            }
        }


        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if (!char.IsDigit(ch) && ch != 8 && ch != '.')
            {
                e.Handled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            addFilter();
            //=============== SET UP =============//
            AstroLib.Setup(true, LATITUDE, LONGITUDE, 7, false, 2400, "Port1.PXP");
            //===================================//
            SetTimer();
        }

        //=================== Ecposure ===================//
        public void exposeCamera(int hBin, int vBin, int tdi, double exposureTime)
        {
            Image<Gray, float> image;
            Image<Rgb, byte> imageRGB;
            int ulX, ulY, lrX, lrY;
            fliCCD.GetVisibleArea(out ulX, out ulY, out lrX, out lrY);
            int width = lrX - ulX;
            int height = lrY - ulY;
            ushort[][] data = new ushort[height][];
            fliCCD.SetImageArea(ulX, ulY, lrX, lrY);
            exposureTime = exposureTime * 1000;
            fliCCD.SetExposureTime(Convert.ToInt32(exposureTime));
            fliCCD.SetHBin(hBin);
            fliCCD.SetVBin(vBin);
            fliCCD.SetTDI(tdi);
            sunPosition(out double sunAltStart, out double sunAzmStart);
            fliCCD.Expose();

            while (!fliCCD.IsDownloadReady())
                Thread.Sleep(100);
            for (int y = 0; y < height; y++)
            {
                data[y] = new ushort[width];
                fliCCD.GrabRow(data[y]);
            }
            sunPosition(out double sunAltEnd, out double sunAzmEnd);
            Matrix<float> matrix = new Matrix<float>(height, width);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    matrix.Data[y, x] = data[y][x];
                }
            }

            image = matrix.Mat.ToImage<Gray, float>();
            //imageBox1.Image = image2;

            string ADU = Convert.ToString(image.GetAverage());
            string[] words = ADU.Split('[', ']');
            aDU = Convert.ToDouble(words[1]);
            string localDate = DateTime.Now.ToString("dd/MM/yyyy_THH':'mm':'ss");
            string fileFitsName = "ImageFits/Sun_alt_ " + sunAltStart + "_ADU_" + aDU + ".fits";
            addRecord(localDate, Convert.ToString(sunAltStart), Convert.ToString(sunAltEnd), Convert.ToString(sunAzmStart), Convert.ToString(sunAzmEnd), Convert.ToString((exposureTime / 1000)), filterSelect, Convert.ToString(ADU), "dataRecord/info_expose.txt");

            //========== write .fits =========//
            SRSLib.ImageLib.ImageType imageFit = new SRSLib.ImageLib.ImageType();
            imageFit.N1 = height;
            imageFit.N2 = width;
            imageFit.N3 = 1;
            imageFit.Simple = true;
            imageFit.HaveHistogram = true;
            imageFit.HaveDateTime = true;
            imageFit.HaveLevels = true;
            imageFit.XBinning = hBin;
            imageFit.YBinning = vBin;
            imageFit.NAxis = 2;
            imageFit.BScale = 1;
            imageFit.DateObsStr = Convert.ToString(DateTime.UtcNow);
            imageFit.HaveDateTime = true;
            String[] header_fit = new String[50];
            imageFit.Header = header_fit;
            imageFit.BScale = 1;
            imageFit.Filter = filterSelect;
            imageFit.DateObsStr = localDate;
            imageFit.Exposure = (exposureTime / 1000);
            imageFit.JD = AstroCalculation.AstroLib.GetLocalJD();
            RaDec raDec = Sun.GetRaDec(AstroTime.JulianDayUTC(DateTime.Now));
            imageFit.DecRad = Convert.ToDouble(raDec.Dec.Rads);
            imageFit.RARad = Convert.ToDouble(raDec.Ra.Rads);
            imageFit.ImageType = "Flat Field";
            imageFit.Data = matrix.Data;
            imageFit.Temperature = fliCCD.GetTemperature();

            SRSLib.ImageLib.WriteFITS(ref imageFit, fileFitsName, false);

            //===================================================//

            //====================== Convert .Fits To .JPG ===========================//

            String stretchType = comboBox2.Text;
            strecthImage.GetStrecthType(stretchType, out double lowerPercen, out double upperPercen);
            Matrix<UInt16> NewImg = strecthImage.ConvertStretchImageU16BitToJPG(matrix.Convert<UInt16>(), lowerPercen, upperPercen);
            string imageJpg = "imageJPG/Sun_alt_ " + sunAltStart + "_ADU_" + aDU + " .jpg";

            NewImg.Save(imageJpg);

            pictureBox1.Image = Image.FromFile(imageJpg);
            //=======================================================================//
            //=========================Plot Graph AllSky=========================//
            AllskyPlotGraph(out string fileNameAllsky);

            //===================================================================//

            //======================Insert Mongo DB ====================//
            CCD_Mongo cCDMongo = new CCD_Mongo(sunAltStart, sunAltEnd, sunAzmStart, sunAzmEnd, filterSelect, (exposureTime / 1000), aDU, fileFitsName, imageJpg, fileNameAllsky, DateTime.Now);
            collection.InsertOne(cCDMongo);
            //==========================================================//

            NewImg = null;
            data = null;
            matrix = null;
            image = null;

            textBox14.Text = Convert.ToString(Math.Round(Convert.ToDecimal(aDU), 2));
            this.exposureTime = Convert.ToString((exposureTime / 1000));
            textBox13.Text = this.exposureTime;
        }
        //=============== addRecord flie ====================//
        public void addRecord(string dateTime, string sunAltStart, string sunAltEnd, string sunAzmStart, string sunAzmEnd, string exposeTime, string filterPos, string ADU, string filePath)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
                {
                    file.WriteLine(dateTime + "," + sunAltStart + "," + sunAltEnd + "," + sunAzmStart + "," + sunAzmEnd + "," + exposeTime + "," + filterPos + "," + ADU);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("this program did an copsie :", ex);
            }

        }
        //======================================================//


        private void imageBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if (!char.IsDigit(ch) && ch != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if (!char.IsDigit(ch) && ch != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if (!char.IsDigit(ch) && ch != 8)
            {
                e.Handled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeviceInfo[] deviceInfo_filter = fliFilter.GetList();

            if (deviceInfo_filter != null)
            {
                filterFileName = deviceInfo_filter[0].FileName;
                textBox7.Text = deviceInfo_filter[0].FileName;
            }
            else
            {
                textBox7.Text = "No port connect";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (filterFileName != null)
            {
                fliFilter.Connect(filterFileName);
                label15.ForeColor = System.Drawing.Color.Green;
                label15.Text = "Connected";
                GetFilter();
                filterConnect = true;
            }
            else
            {
                filterConnect = false;
            }
        }
        public void GetFilter()
        {
            if (filterConnect)
            {
                textBox8.Invoke((Action)(() =>
                {
                    string GetBrand = fliFilter.GetBrand();
                    string GetModel = fliFilter.GetModel();
                    string GetSerialString = fliFilter.GetSerialString();
                    string GetDeviceStatus = fliFilter.GetDeviceStatus();
                    getFilterPosition = fliFilter.GetFilterPosition();
                    filterCount = fliFilter.GetFilterCount();

                    textBox8.Text = "GetBrand :" + GetBrand + Environment.NewLine;
                    textBox8.Text += "GetModel :" + GetModel + Environment.NewLine;
                    textBox8.Text += "GetSerialString :" + GetSerialString + Environment.NewLine;
                    textBox8.Text += "GetDeviceStatus :" + GetDeviceStatus + Environment.NewLine;
                    textBox8.Text += "GetFilterPosition :" + getFilterPosition + Environment.NewLine;
                    textBox8.Text += "GetFilterCount :" + filterCount + Environment.NewLine;
                }));
            }
            else
            {
                textBox8.Text = "Disconnect" + Environment.NewLine;
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (filterCount > 1)
            {
                fliFilter.SetFilterPosition(0, -1);
                filterSelect = "No Filter";
            }

        }

        public void ShowImgNewForm()
        {
            Form form = new Form();
            form.Width = 600;
            form.Height = 600;
            PictureBox pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Image = Image.FromFile("ss.jpg");
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            form.Controls.Add(pictureBox);
            //imageBox1.Image = img;
            form.ShowDialog();
        }


        //=================== GET Info PW3 =========================//
        //private void GetPlanwaveInfo()
        //{
        //    double lst, altRadians, azRadians;
        //    String uRLString = "http://localhost:8220/status";
        //    XmlTextReader reader = new XmlTextReader(uRLString);
        //    textBox9.BeginInvoke((Action)(() =>
        //    {
        //        label20.ForeColor = System.Drawing.Color.Green;
        //        label20.Text = "Connected";
        //        String elementName = "";
        //        textBox9.Text = "";
        //        while (reader.Read())
        //        {
        //            switch (reader.NodeType)
        //            {
        //                case XmlNodeType.Element:
        //                    elementName = reader.Name;

        //                    if (elementName == "utc" ||
        //                        elementName == "jd" ||
        //                        elementName == "lst" ||
        //                        elementName == "azm_radian" ||
        //                        elementName == "alt_radian")
        //                    {
        //                        textBox9.Text += " " + reader.Name + " : ";
        //                    }
        //                    break;
        //                case XmlNodeType.Text:

        //                    if (elementName == "utc" ||
        //                        elementName == "jd" ||
        //                        elementName == "lst" ||
        //                        elementName == "azm_radian" ||
        //                        elementName == "alt_radian")
        //                    {
        //                        switch (elementName)
        //                        {
        //                            case "lst": string[] lst_split = reader.Value.Split(' '); lst = Convert.ToDouble(lst_split[0]); textBox9.Text += reader.Value; break;
        //                            case "azm_radian": textBox9.Text += Convert.ToString((Convert.ToDouble(reader.Value) * 57.2957795131)); string[] altSplit = reader.Value.Split(' '); altRadians = Convert.ToDouble(altSplit[0]) * 57.2957795131; break;
        //                            case "alt_radian": textBox9.Text += Convert.ToString((Convert.ToDouble(reader.Value) * 57.2957795131)); string[] azmSplit = reader.Value.Split(' '); azRadians = Convert.ToDouble(azmSplit[0]) * 57.2957795131; break;
        //                            default: textBox9.Text += reader.Value; break;
        //                        }
        //                        textBox9.Text += System.Environment.NewLine;
        //                    }
        //                    elementName = "";
        //                    break;
        //            }
        //        }
        //    }));
        //}

        //=============================================//

        //=============  Calculate Sun Position ==============//mmmmmm
        public void sunPosition(out double sunAlt, out double sunAzm)
        {
            try
            {
                RaDec raDec = Sun.GetRaDec(AstroTime.JulianDayUTC(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day));
                AltAz sunAltAzm = AstroLib.RADecToAltAz(DateTime.Now, raDec);
                String altSunBeforeSprit = Convert.ToString(sunAltAzm.Alt);
                String[] sunAltSprit = altSunBeforeSprit.Split(' ');
                Double sunAltSprited = Convert.ToDouble(sunAltSprit[0]);
                String azmSunBeforeSprit = Convert.ToString(sunAltAzm.Az);
                String[] sunAzmSprit = azmSunBeforeSprit.Split(' ');
                Double sunAzmSprited = Convert.ToDouble(sunAzmSprit[0]);
                sunAlt = sunAltSprited;
                sunAzm = sunAzmSprited;
                double revertSunAzm = sunAzm - 180;
                if (revertSunAzm < 0) { revertSunAzm = revertSunAzm + 360; }
                textBox10.Invoke((Action)(() =>
                {
                    textBox10.Text = "Local Date Time: " + DateTime.Now + Environment.NewLine +
                                     "Latitude Ref : " + LATITUDE + Environment.NewLine +
                                     "Longitude Ref : " + LONGITUDE + Environment.NewLine +
                                     "Sun Alt : " + Math.Round(Convert.ToDecimal(sunAltSprited), 2) + Environment.NewLine +
                                     "Sun Azm : " + Math.Round(Convert.ToDecimal(sunAzmSprited), 2) + Environment.NewLine +
                                     "Revert Azm : " + Math.Round(Convert.ToDecimal(revertSunAzm), 5);
                    textBox14.Text = Convert.ToString(Math.Round(Convert.ToDecimal(aDU), 2));
                    textBox13.Text = exposureTime;

                    textBox20.Text = Convert.ToString(Convert.ToDecimal(variableAdu)); //TEST
                }));
            }
            catch { sunAlt = 0; sunAzm = 0; }
        }
        //==========================================================//

        //================= AZM ART Camera ================//
        private void rotatingCameraUrl(String alt, String azm)
        {
            Console.WriteLine("ALT:" + alt + "AZ: " + azm);
            String url = "http://localhost:8220/?device=mount&cmd=move&alt=" + alt + "&azm=" + azm;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string content = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }
        //===============================================//


        private void ConnectPWI_Click(object sender, EventArgs e)
        {
            connectPlanwave = true;
        }




        public void addFilter()
        {
            //==================== ADD Dict Filter =============//

            dictFilter.Add(0, "No filter");
            dictFilter.Add(1, "H_alpha");
            dictFilter.Add(2, "R");
            dictFilter.Add(3, "V");
            dictFilter.Add(4, "B");
            dictFilter.Add(5, "Luminance");
            dictFilter.Add(10, "Red");
            dictFilter.Add(15, "Green");
            dictFilter.Add(20, "Blue");

        }

        public void slectFilter()
        {
            foreach (KeyValuePair<int, string> item in dictFilter)
            {

                if (item.Value == comboBox1.SelectedItem)
                {
                    fliFilter.SetFilterPosition(item.Key, -1);
                    filterSelect = item.Value;
                }
            }
        }




        private void button18_Click(object sender, EventArgs e)
        {
            slectFilter();
        }

        private void button8_Click(object sender, EventArgs e)
        {

            sunPosition(out double sunAlt, out double sunAzm);
            calculateExposure.calculateExposureFlat(sunAlt, out double exposureTime);
            exposureTime = Convert.ToDouble(Decimal.Round(Convert.ToDecimal(exposureTime), 2));
            textBox11.Text = Convert.ToString(exposureTime);
            lastExposureTime = Convert.ToDouble(textBox11.Text);
            continueShot = Convert.ToDouble(textBox12.Text);
            textBox11.Enabled = false;
            textBox12.Enabled = false;
            Console.WriteLine("sunAlt");

            if (starLoop)
            {
                starLoop = false;
                button8.Text = "Start Expose";
                button8.BackColor = System.Drawing.Color.Lime;
                timer1.Stop();
            }
            else
            {
                starLoop = true;
                button8.Text = "Stop Expose";
                button8.BackColor = System.Drawing.Color.Red;
                timer1.Start();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            button8.Invoke((Action)(() =>
            {
                if (starLoop)
                {
                    if (aDU < 40000)
                    {
                        exposeCamera(Convert.ToInt32(textBox4.Text), Convert.ToInt32(textBox5.Text), Convert.ToInt32(textBox6.Text), lastExposureTime);
                        Console.WriteLine(lastExposureTime);
                        lastExposureTime = lastExposureTime + continueShot;
                    }
                    if (aDU >= 40000)
                    {
                        starLoop = false;
                        button8.Text = "Start Expose";
                        button8.BackColor = System.Drawing.Color.Lime;
                        timer1.Stop();
                    }
                }
            }));
        }

        public void AllskyPlotGraph(out string fileNameAllsky)
        {

            var response = new WebClient().DownloadString("http://localhost:8220/status");
            Dictionary<string, string> dataPw4 = loadIni(response);

            double Azm = Convert.ToDouble(dataPw4["mount.azimuth_degs"]);
            double Alt = Convert.ToDouble(dataPw4["mount.altitude_degs"]);


            //=========== ACCESS Shared folder ==============//
            try
            {
                int nm = ALLSKY_SHAREFILE.ImpersonateUser.LOGON32_LOGON_NEW_CREDENTIALS;
                ALLSKY_SHAREFILE.ImpersonateUser impersonateUser = new ALLSKY_SHAREFILE.ImpersonateUser("ALLSKY", "192.168.20.201", "ALLSKY1234", nm);
                string allskyPath = @"\\192.168.20.201\allsky_temp\LASTEST.JPG";
                pictureBox2.Image = new Bitmap(allskyPath);
                pictureBox2.Image.Save(@"LASTALLSKY.JPG", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch
            { }
            Image<Bgr, byte> img = new Image<Bgr, byte>("LASTALLSKY.JPG");

            Allskyplotgraph allskyplotgraph = new Allskyplotgraph();

            Point point = allskyplotgraph.calculatePoint(Azm, Alt);

            Image<Bgr, byte> imgAddGraph = allskyplotgraph.plotTelescope(img, point, 5);
            imageBox1.Image = imgAddGraph;
            fileNameAllsky = "AllskyImage/Alt" + Alt + "AZ" + Azm + ".jpg";
            imgAddGraph.Save(fileNameAllsky);

        }

        public Dictionary<string, string> loadIni(string ini)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();

            foreach (var line in ini.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                .Select(t => t.Trim()))
            {
                var idx = line.IndexOf("=");
                if (idx != -1)
                    d[line.Substring(0, idx)] = line.Substring(idx + 1);
            }
            return d;
        }

        public void pW4Getdata()
        {

            textBox9.Invoke((Action)(() =>
            {
                try
                {
                    var response = new WebClient().DownloadString("http://localhost:8220/status");
                    Dictionary<string, string> dataPw4 = loadIni(response);
                    Console.WriteLine(dataPw4["mount.azimuth_degs"]);
                    textBox9.Text = "latitude_degs" + dataPw4["site.latitude_degs"] + Environment.NewLine;
                    textBox9.Text += "longitude_degs" + dataPw4["site.longitude_degs"] + Environment.NewLine;
                    textBox9.Text += "height_meters" + dataPw4["site.height_meters"] + Environment.NewLine;
                    textBox9.Text += "azimuth_degs" + dataPw4["mount.azimuth_degs"] + Environment.NewLine;
                    textBox9.Text += "altitude_degs" + dataPw4["mount.altitude_degs"] + Environment.NewLine;
                    textBox9.Text += "ra_apparent_hours" + dataPw4["mount.ra_apparent_hours"] + Environment.NewLine;
                    textBox9.Text += "dec_apparent_degs" + dataPw4["mount.dec_apparent_degs"] + Environment.NewLine;

                    label20.ForeColor = System.Drawing.Color.Green;
                    label20.Text = "Connected";
                }
                catch
                {
                    textBox9.Text = "Unable to Connect";
                    connectPlanwave = false;
                }
            }));
        }

        //============================ TEST Exposecal ===============================//

        private void button7_Click(object sender, EventArgs e)
        {
            if (connectStatus)
            {
                double exposureTime = 0;
                if (checkBox1.Checked == true)
                {
                    sunPosition(out double sunAlt, out double sunAzm);
                    //exposureTime = exposureformDB(37500, filterSelect);
                    exposureTime = flatCalculator.exposureTimeFristGrab(sunAlt, filterSelect, Convert.ToDouble(textBox19.Text));
                    exposureTime = Convert.ToDouble(Math.Round(Convert.ToDecimal(exposureTime), 2));
                }
                else if (checkBox1.Checked == false)
                {
                    exposureTime = 1;
                }
                textBox11.Text = Convert.ToString(exposureTime);
                lastExposureTime = exposureTime;
                fristExposure = lastExposureTime;
                textBox11.Enabled = false;
                textBox12.Enabled = false;
                fristconut = 0;
                flatCount = 1;
                aDU = 0;
                if (starLoop)
                {
                    starLoop = false;
                    button7.Text = "Start Expose";
                    button7.BackColor = System.Drawing.Color.Lime;
                    timer2.Stop();
                }
                else
                {
                    starLoop = true;
                    button7.Text = "Stop Expose";
                    button7.BackColor = System.Drawing.Color.Red;
                    timer2.Start();
                }
            }
            else
            {
                MessageBox.Show(" !! FLICCD Disconnect !!");
            }
        }
        int fristconut = 0;
        private void timer2_Tick(object sender, EventArgs e)
        {
            button7.Invoke((Action)(() =>
            {
                if (starLoop)
                {
                    double targetADU = Convert.ToDouble(textBox19.Text);

                    if (fristconut < 1)
                    {
                        exposeCamera(Convert.ToInt32(textBox4.Text), Convert.ToInt32(textBox5.Text), Convert.ToInt32(textBox6.Text), lastExposureTime);
                        variableAdu = aDU;
                    }

                    double CostError = aDU - targetADU;
                    if (fristconut >= 1)
                    {
                        if (CostError < 0)
                        {
                            variableAdu = variableAdu + Math.Abs(CostError);
                        }
                        else if (CostError > 0)
                        {
                            variableAdu = variableAdu - Math.Abs(CostError);
                        }
                        double exposeCal = exposureCal(lastExposureTime, aDU, variableAdu);
                        lastExposureTime = exposeCal;
                        exposeCamera(Convert.ToInt32(textBox4.Text), Convert.ToInt32(textBox5.Text), Convert.ToInt32(textBox6.Text), lastExposureTime);
                    }
                    fristconut++;
                    if (Math.Abs(CostError) <= 2500)
                    {
                        flatCount++;
                    }
                    if (flatCount > 5)
                    {
                        Console.WriteLine("End");
                        starLoop = false;
                        button7.Text = "Start Expose Flat";
                        button7.BackColor = System.Drawing.Color.Lime;
                        flatCount = 0;
                        MessageBox.Show(" Flat continue End  'Clear  Last ADU' ");
                        aDU = 0;
                        variableAdu = 0;
                        timer2.Stop();
                    }
                }
            }));
        }

        public double exposureCal(double oldExposure, double adu, double variableAdu)
        {
            double simulete = adu / oldExposure;
            double exposureCal = variableAdu / simulete;
            return (exposureCal);
        }

        //============================================================================//
        public double findFristAduVariable(double exposure, double adu)
        {
            double simulete = adu / exposure;
            double aduVariableFrist = exposure * simulete;
            return (aduVariableFrist);
        }

        public double variableFindAdu(string filter)
        {
            double variableFindAdu = 0;

            switch (filter)
            {
                case "H_alpha": variableFindAdu = 0; break;
                case "R": variableFindAdu = 41000; break;
                case "V": variableFindAdu = 0; break;
                case "B": variableFindAdu = 0; break;
                case "Luminance": variableFindAdu = 0; break;
                case "Red": variableFindAdu = 0; break;
                case "Green": variableFindAdu = 0; break;
                case "Blue": variableFindAdu = 0; break;
                default: variableFindAdu = 0; break;
            }
            return (variableFindAdu);
        }

        //***************************************************************//
        //================Calculate Flat Optimizer TEST============//


        public void flatOptimizer()
        {
            double aduTarget = 37500;
            double alpha = 2000;
            var altSunDay1 = new List<string>();
            var exposureTimeDay1 = new List<string>();
            var AduReal = new List<string>();
            using (var rd = new StreamReader("dataFlat.csv"))
            {
                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(',');
                    altSunDay1.Add(splits[1]);
                    exposureTimeDay1.Add(splits[5]);
                    AduReal.Add(splits[7]);
                }
            }
            double variableDef = 2;
            double exp_time_hat = 0;
            double[] expTime = new double[altSunDay1.Count];
            double[] aduApp = new double[altSunDay1.Count];

            expTime[0] = Convert.ToDouble(exposureTimeDay1[0]);
            //expTime[0] = 2;
            for (int i = 1; i < altSunDay1.Count; i++)
            {
                aduApp[i] = (-1.5 * Math.Pow(10, 4)) * Convert.ToDouble(altSunDay1[i - 1]) + (-2 * Math.Pow(10, 3)) * Convert.ToDouble(expTime[i - 1]);
                double optimate = aduApp[i] + (1 / i) * (Convert.ToDouble(AduReal[i]) - aduApp[i]);
                double Error = aduTarget - optimate;

                if (Error < 0)
                {
                    exp_time_hat = Convert.ToDouble(expTime[i - 1]) + (Math.Abs(Convert.ToDouble(expTime[i - 1]) - Convert.ToDouble(expTime[i]))) / variableDef;
                    Console.WriteLine("Positive direction");
                }
                else if (Error > 0)
                {
                    exp_time_hat = Convert.ToDouble(expTime[i - 1]) - (Math.Abs(Convert.ToDouble(expTime[i - 1]) - Convert.ToDouble(expTime[i]))) / variableDef;
                    Console.WriteLine("Negative direction");
                }
                expTime[i] = exp_time_hat;
                Console.WriteLine("Flat frame optimate :ADU: " + optimate + " expTime: " + expTime[i] + " at Loop: " + i);

                if (Math.Abs(Error) <= alpha)
                {
                    goto L1;
                }
            }
        L1: Console.WriteLine("End LOOP");
        }

        //================== Optimizer TEST ==================//


        //===================================================//

        //===================================================//
        //**************************************************************//

        //=========== Continue Flat ============//
        int amostFlat = 0;
        private void button11_Click(object sender, EventArgs e)
        {
            amostFlat = Convert.ToInt32(comboBox11.Text);
            enableAmostFlat(amostFlat);
        }
        private void enableAmostFlat(int amostFlat)
        {
            switch (amostFlat)
            {
                case 1:
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    comboBox6.Enabled = false;
                    comboBox7.Enabled = false;
                    comboBox8.Enabled = false;
                    comboBox9.Enabled = false;
                    break;
                case 2:
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = false;
                    comboBox6.Enabled = false;
                    comboBox7.Enabled = false;
                    comboBox8.Enabled = false;
                    comboBox9.Enabled = false;
                    break;
                case 3:
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    comboBox6.Enabled = false;
                    comboBox7.Enabled = false;
                    comboBox8.Enabled = false;
                    comboBox9.Enabled = false;
                    break;
                case 4:
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    comboBox6.Enabled = true;
                    comboBox7.Enabled = false;
                    comboBox8.Enabled = false;
                    comboBox9.Enabled = false;
                    break;
                case 5:
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    comboBox6.Enabled = true;
                    comboBox7.Enabled = true;
                    comboBox8.Enabled = false;
                    comboBox9.Enabled = false;
                    break;
                case 6:
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    comboBox6.Enabled = true;
                    comboBox7.Enabled = true;
                    comboBox8.Enabled = true;
                    comboBox9.Enabled = false;
                    break;
                case 7:
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    comboBox6.Enabled = true;
                    comboBox7.Enabled = true;
                    comboBox8.Enabled = true;
                    comboBox9.Enabled = true;
                    break;
                default:
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    comboBox6.Enabled = false;
                    comboBox7.Enabled = false;
                    comboBox8.Enabled = false;
                    comboBox9.Enabled = false;
                    break;
            }
        }

        string[] nameFlat = { };

        int loop = 1;
        int filtercount = 0;
        double lastexp = 1;
        double targetAdu = 0;
        int queuefilter = 0;
        string[] filterArray = null;
        bool onChangeFilter = true;
        FlatCalculator flatCalculator = new FlatCalculator();
        private void button12_Click(object sender, EventArgs e)
        {
            sunPosition(out double sunAlt, out double sunAzm);
            targetAdu = Convert.ToInt32(textBox19.Text);
            filtercount = Convert.ToInt32(comboBox11.Text);

            filterArray = filterQueue(filtercount);

            lastexp = flatCalculator.exposureTimeFristGrab(sunAlt, filterSelect, targetAdu);
            loop = 1;
            onChangeFilter = true;

            flatCount = 0;

            if (starLoop)
            {
                starLoop = false;
                button12.Text = "Start Expose";
                button12.BackColor = System.Drawing.Color.Lime;
                timer4.Stop();
            }
            else
            {
                starLoop = true;
                button12.Text = "Stop Expose";
                button12.BackColor = System.Drawing.Color.Red;
                timer4.Start();
            }
        }


        private void timer4_Tick(object sender, EventArgs e)
        {
            button12.Invoke((Action)(() =>
            {
                if (starLoop)
                {

                    if (queuefilter < filterArray.Length && onChangeFilter == true)
                    {

                        comboBox1.Text = filterArray[queuefilter];
                        Console.WriteLine("change Filter: " + comboBox1.Text);
                        slectFilter();
                        onChangeFilter = false;
                        queuefilter++;
                    }
                    else if (queuefilter <= filterArray.Length && onChangeFilter == false)
                    {

                        exposeCamera(Convert.ToInt32(textBox4.Text), Convert.ToInt32(textBox5.Text), Convert.ToInt32(textBox6.Text), lastexp);
                        lastexp = flatCalculator.flatExposureTime(comboBox1.Text, aDU, lastexp, targetAdu, loop);
                        loop++;
                    }
                    if (Math.Abs(aDU - targetAdu) <= 2500)
                    {
                        Console.WriteLine("ADU PASS");
                        flatCount++;
                        if (flatCount >= 5)
                        {
                            onChangeFilter = true;
                            flatCount = 0;
                            loop = 0;
                            aDU = 0;
                            sunPosition(out double sunAlt, out double sunAzm);
                            lastexp = flatCalculator.exposureTimeFristGrab(sunAlt, filterSelect, targetAdu);

                            if (queuefilter >= filterArray.Length)
                            {
                                starLoop = false;
                                button12.Text = "Start Expose";
                                button12.BackColor = System.Drawing.Color.Lime;
                                timer4.Stop();
                            }
                        }
                    }
                }


            }));
        }
        public String[] readFlatContinuous(int amostFlat)
        {
            string[] nameFlat = new string[amostFlat];
            for (int i = 0; i < amostFlat; i++)
            {
                switch (i)
                {
                    case 0: nameFlat[i] = comboBox3.Text; break;
                    case 1: nameFlat[i] = comboBox4.Text; break;
                    case 2: nameFlat[i] = comboBox5.Text; break;
                    case 3: nameFlat[i] = comboBox6.Text; break;
                    case 4: nameFlat[i] = comboBox7.Text; break;
                    case 5: nameFlat[i] = comboBox8.Text; break;
                    case 6: nameFlat[i] = comboBox9.Text; break;
                }
            }
            return (nameFlat);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            AllskyPlotGraph(out string fileNameAllsky);
        }

        //=========== Continue Flat ============//

        //==============DB Expost=============//
        public double exposureformDB(double aduTarget, string filter)
        {
            double exposure = 0;
            sunPosition(out double sunAlt, out double sunAzm);

            try
            {
                var things = db.GetCollection<CCD_Mongo>("data");
                var sunaltDB = things.AsQueryable().Where(x => x.SunAltStart >= (sunAlt - 0.2) & x.SunAltStart < (sunAlt + 0.2) & x.Filter == filter & x.Adu >= (aduTarget - 1000) & x.Adu <= (aduTarget + 1000)).Min(x => x.SunAltStart);
                var command = things.AsQueryable().Where(y => y.SunAltStart == sunaltDB & y.Filter == filter).FirstOrDefault();
                exposure = command.ExposureTime;
            }
            catch
            {
                exposure = 1.00;
            }
            return (exposure);
        }
        //===================================//

        public string[] filterQueue(int queue)
        {
            string[] filterArray = new string[queue];
            switch (queue)
            {
                case 1:
                    filterArray[0] = comboBox3.Text;
                    break;
                case 2:
                    filterArray[0] = comboBox3.Text;
                    filterArray[1] = comboBox4.Text;
                    break;
                case 3:
                    filterArray[0] = comboBox3.Text;
                    filterArray[1] = comboBox4.Text;
                    filterArray[2] = comboBox5.Text;
                    break;
                case 4:
                    filterArray[0] = comboBox3.Text;
                    filterArray[1] = comboBox4.Text;
                    filterArray[2] = comboBox5.Text;
                    filterArray[3] = comboBox6.Text;
                    break;
                case 5:
                    filterArray[0] = comboBox3.Text;
                    filterArray[1] = comboBox4.Text;
                    filterArray[2] = comboBox5.Text;
                    filterArray[3] = comboBox6.Text;
                    filterArray[4] = comboBox7.Text;
                    break;
                case 6:
                    filterArray[0] = comboBox3.Text;
                    filterArray[1] = comboBox4.Text;
                    filterArray[2] = comboBox5.Text;
                    filterArray[3] = comboBox6.Text;
                    filterArray[4] = comboBox7.Text;
                    filterArray[5] = comboBox8.Text;
                    break;

                case 7:
                    filterArray[0] = comboBox3.Text;
                    filterArray[1] = comboBox4.Text;
                    filterArray[2] = comboBox5.Text;
                    filterArray[3] = comboBox6.Text;
                    filterArray[4] = comboBox7.Text;
                    filterArray[5] = comboBox8.Text;
                    filterArray[6] = comboBox9.Text;
                    break;
                default: break;
            }
            return (filterArray);
        }
    }



}



