using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QazGeoDevelopment.Models
{
    public class PointJson
    {
        public DBPoint Point { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public string Key { get; set; }

        public static List<PointJson> GetPointsFrom(List<DBPoint> points)
        {
            if (points == null || points.Count == 0)
                return null;
            var result = new List<PointJson>();
            points.ForEach(p =>
            {
                result.Add(new PointJson()
                {
                    Point = p,
                    X = p.Position.X,
                    Y = p.Position.Y,
                    Z = p.Position.Z,
                });
            });
            return result;
        }
    }
}
