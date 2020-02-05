using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB_CCD
{
    class CCD_Mongo
    {

        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("SunAltStart")]
        public double SunAltStart { get; set; }

        [BsonElement("SunAltEnd")]
        public double SunAltEnd { get; set; }

        [BsonElement("SunAzmStart")]
        public double SunAzmStart { get; set; }
        [BsonElement("SunAzmEnd")]
        public double SunAzmEnd { get; set; }

        [BsonElement("Filter")]
        public string Filter { get; set; }

        [BsonElement("ExposureTime")]
        public double ExposureTime { get; set; }

        [BsonElement("Adu")]
        public double Adu { get; set; }

        [BsonElement("FitsImage")]
        public string FitsImage { get; set; }

        [BsonElement("JpgImage")]
        public string JpgImage { get; set; }

        [BsonElement("AllskyImage")]
        public string AllskyImage { get; set; }

        [BsonElement("DateTime")]
        public DateTime DateTime { get; set; }

        public CCD_Mongo(double sunAltStart, double sunAltEnd,double sunAzmStart , double sunAzmEnd,string filter,double exposureTime, double adu ,string fitsImage, string jpgImage,string allskyImage, DateTime dateTime)
        {
            SunAltStart = sunAltStart;
            SunAltEnd = sunAltEnd;
            SunAzmStart = sunAzmStart;
            SunAzmEnd = sunAzmEnd;
            Filter = filter;
            Adu = adu;
            FitsImage=fitsImage;
            JpgImage = jpgImage;
            AllskyImage = allskyImage;
            DateTime = dateTime;
            ExposureTime = exposureTime;

        }

 



    }
}
