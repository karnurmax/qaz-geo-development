using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using QazGeoDevelopment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QazGeoDevelopment.Core
{
    public class Finisher
    {
        double ToleranceValue = 0.000001;
        //private BlockTable bt;
        //private BlockTableRecord btr;

        Document Doc { get; set; }
        Editor Editor { get; set; }
        Autodesk.AutoCAD.ApplicationServices.TransactionManager TransactionManager { get; set; }
        Database Database { get; set; }
        //Transaction Transaction { get; set; }
        private List<PointJson> PointJsonList { get; set; }
        public void Go()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable bt;
                bt = tr.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord btr;
                btr = tr.GetObject(bt[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                RXClass rxClassOfPoint = new DBPoint().GetRXClass();
                List<DBPoint> points = new List<DBPoint>();
                foreach (var obj in btr)
                {
                    var item = tr.GetObject(obj, OpenMode.ForWrite);
                    if (item.GetRXClass() == rxClassOfPoint)
                    {
                        points.Add(item as DBPoint);
                    }
                }
                acDoc.Editor.SetImpliedSelection(points.Select(p => p.Id).ToArray());

                List<PointJson> list = this.SortByCoordinates(PointJson.GetPointsFrom(points));

                List<PointJson> resolved = new List<PointJson>();
                while (list.Count > 0)
                {
                    using (Polyline acPoly = FindRectanglePoints(list, resolved))
                    {
                        btr.AppendEntity(acPoly);
                        tr.AddNewlyCreatedDBObject(acPoly, true);
                    }
                }


                tr.Commit();
                return;
            }

        }

        private Polyline FindRectanglePoints(List<PointJson> list, List<PointJson> resolved)
        {
            Polyline polyline = new Polyline();
            PointJson first = null, second = null, third = null;
            first = list[0];
            bool notFound = true;
            while (notFound)
            {
                for (int i = 1; i < list.Count; i++)
                {
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        double x = 0, y = 0;
                        second = list[i]; third = list[j];
                        var a = Math.Sqrt(Math.Pow(second.X - first.X, 2) + Math.Pow(second.Y - first.Y, 2));
                        var b = Math.Sqrt(Math.Pow(third.X - second.X, 2) + Math.Pow(third.Y - second.Y, 2));
                        var c = Math.Sqrt(Math.Pow(first.X - third.X, 2) + Math.Pow(first.Y - third.Y, 2));

                        var ab = Math.Abs((second.X - first.X) * (third.X - first.X) + (second.Y - first.Y) * (third.Y - first.Y));
                        var bc = Math.Abs((first.X - second.X) * (third.X - second.X) + (first.Y - second.Y) * (third.Y - second.Y));
                        var ca = Math.Abs((first.X - third.X) * (second.X - third.X) + (first.Y - third.Y) * (second.Y - third.Y));

                        if (ab < ToleranceValue)
                        {
                            x = third.X + second.X - first.X;
                            y = third.Y + second.Y - first.Y;

                            polyline.AddVertexAt(0, new Point2d(first.X, first.Y), 0, 0, 0);
                            polyline.AddVertexAt(1, new Point2d(second.X, second.Y), 0, 0, 0);
                            polyline.AddVertexAt(2, new Point2d(x, y), 0, 0, 0);
                            polyline.AddVertexAt(3, new Point2d(third.X, third.Y), 0, 0, 0);
                            polyline.AddVertexAt(4, new Point2d(first.X, first.Y), 0, 0, 0);

                            polyline.Color = Color.FromRgb(199, 212, 61);//yellow
                        }
                        else if (bc < ToleranceValue)
                        {
                            x = third.X + first.X - second.X;
                            y = third.Y + first.Y - second.Y;

                            polyline.AddVertexAt(0, new Point2d(first.X, first.Y), 0, 0, 0);
                            polyline.AddVertexAt(1, new Point2d(x, y), 0, 0, 0);
                            polyline.AddVertexAt(2, new Point2d(third.X, third.Y), 0, 0, 0);
                            polyline.AddVertexAt(3, new Point2d(second.X, second.Y), 0, 0, 0);
                            polyline.AddVertexAt(4, new Point2d(first.X, first.Y), 0, 0, 0);

                            polyline.Color = Color.FromRgb(50, 168, 82);//green
                        }
                        else if (ca < ToleranceValue)
                        {
                            x = second.X + first.X - third.X;
                            y = second.Y + first.Y - third.Y;

                            polyline.AddVertexAt(0, new Point2d(first.X, first.Y), 0, 0, 0);
                            polyline.AddVertexAt(1, new Point2d(x, y), 0, 0, 0);
                            polyline.AddVertexAt(2, new Point2d(second.X, second.Y), 0, 0, 0);
                            polyline.AddVertexAt(3, new Point2d(third.X, third.Y), 0, 0, 0);
                            polyline.AddVertexAt(4, new Point2d(first.X, first.Y), 0, 0, 0);

                            polyline.Color = Color.FromRgb(55, 175, 219);//blue
                        }
                        else
                            continue;



                        first.Point.Color = Color.FromRgb(50, 168, 82);//green
                        second.Point.Color = Color.FromRgb(199, 212, 61);//yellow
                        third.Point.Color = Color.FromRgb(43, 130, 171);//blue


                        resolved.Add(first);
                        resolved.Add(second);
                        resolved.Add(third);

                        list.Remove(first);
                        list.Remove(second);
                        list.Remove(third);
                        return polyline;

                    }
                }
            }
            throw new System.Exception("rectangle not found");
        }

        private void Constructor()
        {
            this.Doc = Application.DocumentManager.MdiActiveDocument;
            this.Database = this.Doc.Database;
            this.Editor = this.Doc.Editor;
            this.TransactionManager = Doc.TransactionManager;
        }

        private void Exit()
        {
            //this.Transaction.Commit();
        }

        private void Prepare(Transaction tr)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            RXClass rxClassOfPoint = new DBPoint().GetRXClass();
            //List<DBPoint> points = this.ObjectsOfType1(acCurDb, rxClassOfPoint, tr).Cast<DBPoint>().ToList();
            //acDoc.Editor.SetImpliedSelection(points.Select(p => p.Id).ToArray());
            //this.PointJsonList = PointJson.GetPointsFrom(points);
        }

        private void Draw(Transaction tr)
        {
            var list = this.PointJsonList;
            List<List<PointJson>> resolvedGroups = new List<List<PointJson>>();
            var sorted = SortByCoordinates(list);

            while (sorted.Count > 0)
            {
                var first = sorted.FirstOrDefault();
                first.Point.Color = Color.FromRgb(7, 7, 7);
                var second = GetNearestPoint(sorted, first);
                var third = GetNearestPoint(sorted, first, new List<PointJson>() { first, second });
                this.Editor.SetImpliedSelection(new ObjectId[] { first.Point.Id, second.Point.Id, third.Point.Id });
                return;
            }
        }

        private PointJson GetNearestPoint(List<PointJson> list, PointJson first, List<PointJson> exclude)
        {
            PointJson result = null;
            foreach (PointJson p in list)
            {
                if (exclude.Contains(p))
                    continue;
                if (result == null)
                {
                    result = p;
                    continue;
                }
                double distanceToResult = CalcDistance(first, result);
                double distanceToCurrentPoint = CalcDistance(first, p);
                if (distanceToCurrentPoint < distanceToResult)
                {
                    result = p;
                }
            }
            return result;
        }

        private PointJson GetNearestPoint(List<PointJson> list, PointJson first)
        {
            PointJson result = null;
            foreach (PointJson p in list)
            {
                if (p == first)
                    continue;
                if (result == null)
                {
                    result = p;
                    continue;
                }
                double distanceToResult = CalcDistance(first, result);
                double distanceToCurrentPoint = CalcDistance(first, p);
                if (distanceToCurrentPoint < distanceToResult)
                {
                    result = p;
                }
            }
            return result;
        }

        private List<PointJson> SortByCoordinates(List<PointJson> list)
        {
            var result = new List<PointJson>();
            result.AddRange(list);
            result.Sort((a, b) =>
            {
                var xDiff = b.X - a.X;
                if (xDiff < 0)
                    return 1;
                else if (xDiff > 0)
                    return -1;
                var yDiff = b.Y - a.Y;
                if (yDiff > 0)
                    return 1;
                else if (yDiff < 0)
                    return -1;
                return 0;
            });
            return result;

        }

        //public IEnumerable<object> ObjectsOfType1(Database db, RXClass rXClass, Transaction tr)
        //{
        //    foreach (var obj in btr)
        //    {
        //        var item = tr.GetObject(obj, OpenMode.ForWrite);
        //        if (item.GetRXClass() == rXClass)
        //        {
        //            yield return item;
        //        }
        //    }
        //}

        public static double CalcDistance(PointJson a, PointJson b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }
    }
}
