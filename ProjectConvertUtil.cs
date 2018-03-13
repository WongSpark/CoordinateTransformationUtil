using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileManager.Test
{
    class ProjectionConvertUtil
    {
        /*
        * BD-09：百度坐标系(百度地图)
        * GCJ-02：火星坐标系（谷歌中国地图、高德地图）
        * WGS84：地球坐标系（国际通用坐标系，谷歌地图）
        */

        public double x_PI = 3.14159265358979324 * 3000.0 / 180.0;
        public double PI = 3.1415926535897932384626;
        public double a = 6378245.0;
        public double ee = 0.00669342162296594323;

        //百度坐标系转火星坐标系
        public double[] bd09togcj02(double bd_lon, double bd_lat)
        {
            double x = bd_lon - 0.0065;
            double y = bd_lat - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_PI);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_PI);

            double gcj_lon = z * Math.Cos(theta);
            double gcj_lat = z * Math.Sin(theta);
            double[] gcj = { gcj_lon, gcj_lat };//火星坐标系值

            //火星坐标系转wgs84
            double[] wgs = gcj02towgs84(gcj[0], gcj[1]);
            return wgs;
        }

        //火星坐标系转wgs84
        public double[] gcj02towgs84(double gcj_lon, double gcj_lat)
        {
            if (out_of_china(gcj_lon, gcj_lat))
            {
                //不在国内，不进行纠偏
                double[] back = { gcj_lon, gcj_lat };
                return back;
            }
            else
            {
                var dlon = transformlon(gcj_lon - 105.0, gcj_lat - 35.0);
                var dlat = transformlat(gcj_lon - 105.0, gcj_lat - 35.0);
                var radlat = gcj_lat / 180.0 * PI;
                var magic = Math.Sin(radlat);
                magic = 1 - ee * magic * magic;
                var sqrtmagic = Math.Sqrt(magic);
                dlon = (dlon * 180.0) / (a / sqrtmagic * Math.Cos(radlat) * PI);
                dlat = (dlat * 180.0) / ((a * (1 - ee)) / (magic * sqrtmagic) * PI);
                double mglon = gcj_lon + dlon;
                double mglat = gcj_lat + dlat;
                double wgs_lon = gcj_lon * 2 - mglon;
                double wgs_lat = gcj_lat * 2 - mglat;
                double[] wgs = { wgs_lon, wgs_lat };//wgs84坐标系值
                return wgs;
            }
        }

        //火星坐标系转百度坐标系
        public double[] gcj02tobd09(double gcj_lon, double gcj_lat)
        {
            double z = Math.Sqrt(gcj_lon * gcj_lon + gcj_lat * gcj_lat) + 0.00002 * Math.Sin(gcj_lat * x_PI);
            double theta = Math.Atan2(gcj_lat, gcj_lon) + 0.000003 * Math.Cos(gcj_lon * x_PI);
            double bd_lon = z * Math.Cos(theta) + 0.0065;
            double bd_lat = z * Math.Sin(theta) + 0.006;
            double[] bd = { bd_lon, bd_lat };
            return bd;
        }

        //wgs84转火星坐标系
        public double[] wgs84togcj02(double wgs_lon, double wgs_lat)
        {
            if (out_of_china(wgs_lon, wgs_lat))
            {
                //不在国内
                double[] back = { wgs_lon, wgs_lat };
                return back;
            }
            else
            {
                double dwgs_lon = transformlon(wgs_lon - 105.0, wgs_lat - 35.0);
                double dwgs_lat = transformlat(wgs_lon - 105.0, wgs_lat - 35.0);
                double radwgs_lat = wgs_lat / 180.0 * PI;
                double magic = Math.Sin(radwgs_lat);
                magic = 1 - ee * magic * magic;
                double sqrtmagic = Math.Sqrt(magic);
                dwgs_lon = (dwgs_lon * 180.0) / (a / sqrtmagic * Math.Cos(radwgs_lat) * PI);
                dwgs_lat = (dwgs_lat * 180.0) / ((a * (1 - ee)) / (magic * sqrtmagic) * PI);
                double gcj_lon = wgs_lon + dwgs_lon;
                double gcj_lat = wgs_lat + dwgs_lat;
                double[] gcj = { gcj_lon, gcj_lat };
                return gcj;
            }
        }

        private double transformlon(double lon, double lat)
        {
            var ret = 300.0 + lon + 2.0 * lat + 0.1 * lon * lon + 0.1 * lon * lat + 0.1 * Math.Sqrt(Math.Abs(lon));
            ret += (20.0 * Math.Sin(6.0 * lon * PI) + 20.0 * Math.Sin(2.0 * lon * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lon * PI) + 40.0 * Math.Sin(lon / 3.0 * PI)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(lon / 12.0 * PI) + 300.0 * Math.Sin(lon / 30.0 * PI)) * 2.0 / 3.0;
            return ret;
        }

        private double transformlat(double lon, double lat)
        {
            var ret = -100.0 + 2.0 * lon + 3.0 * lat + 0.2 * lat * lat + 0.1 * lon * lat + 0.2 * Math.Sqrt(Math.Abs(lon));
            ret += (20.0 * Math.Sin(6.0 * lon * PI) + 20.0 * Math.Sin(2.0 * lon * PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lat * PI) + 40.0 * Math.Sin(lat / 3.0 * PI)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(lat / 12.0 * PI) + 320 * Math.Sin(lat * PI / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        //判断是否在国内，不在国内则不做偏移
        private Boolean out_of_china(double lon, double lat)
        {
            return (lon < 72.004 || lon > 137.8347) || ((lat < 0.8293 || lat > 55.8271) || false);
        }
    }
}
