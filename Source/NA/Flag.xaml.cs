using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Media.Imaging;

namespace NA
{
    public partial class Flag : UserControl
    {
        private Map _PMap = null;
        private int count = 0;
        private ESRI.ArcGIS.Client.Geometry.PointCollection PCol = null;
        private Storyboard sb = new Storyboard();
        DoubleAnimation dba;
        DoubleAnimation dba1;
        /// <summary>
        /// 执行结果路线 
        /// </summary>
        private Graphic _Route = null;
        public Graphic Route
        {
            get { return _Route; }
            set { _Route = value; }
        }
        public Flag()
        {
            InitializeComponent();
        }


      
      
        public string ImageSource
        {
            set
            {
                Image1.Source = new BitmapImage(new Uri(value, UriKind.Relative));
            }
        }
        public Map BindMap
        {
            set { _PMap = value; }
            get { return _PMap; }
        }
        public static readonly DependencyProperty Xproperty = DependencyProperty.Register("X", typeof(double), typeof(Flag), new PropertyMetadata(OnXChanged));
        public double X
        {
            get { return (double)base.GetValue(Xproperty); }
            set { base.SetValue(Xproperty, value); ResetEnvelop(); }
        }
        private static void OnXChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as Flag).X = (double)e.NewValue;
        }
        public static readonly DependencyProperty Yproperty = DependencyProperty.Register("Y", typeof(double), typeof(Flag), new PropertyMetadata(OnYChanged));
        public double Y
        {
            get { return (double)base.GetValue(Yproperty); }
            set { base.SetValue(Yproperty, value); ResetEnvelop(); }
        }
        private static void OnYChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as Flag).Y = (double)e.NewValue;
        }
        private ElementLayer _bindLayer = null;
        public ElementLayer bindLayer
        {
            get { return _bindLayer; }
            set { _bindLayer = value; }
        }
        private void ResetEnvelop()
        {
            Flag ele = (Flag)_bindLayer.Children[0];
            ElementLayer.SetEnvelope(ele, new ESRI.ArcGIS.Client.Geometry.Envelope(X, Y, X, Y));
            if (_PMap != null)
            {
                ESRI.ArcGIS.Client.Geometry.Polygon gon = new ESRI.ArcGIS.Client.Geometry.Polygon();
                ESRI.ArcGIS.Client.Geometry.PointCollection con = new ESRI.ArcGIS.Client.Geometry.PointCollection();
            }
        }
        /// <summary>
        /// 移动时间间隔 默认1
        /// </summary>
        /// 
        private double _Interval = 1;
        public double Interval
        { 
            get { return _Interval; }
            set { _Interval = value; }
        }
        public void Start()
        {
            if (_PMap != null)
            {
                _PMap.ZoomTo(_Route.Geometry);
            }
            if (_bindLayer.Children.Count > 0) _bindLayer.Children.RemoveAt(0);

            _bindLayer.Children.Add(this);
            ESRI.ArcGIS.Client.Geometry.Polyline line = _Route.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline;
            PCol = line.Paths[0];
            MapPoint pt1 = PCol[0];
            MapPoint pt2 = PCol[1];
            double angle = CalulateXYAnagle(pt1.X, pt1.Y, pt2.X, pt2.Y);
            RotateItemCanvas.Angle = angle;
          

            ElementLayer.SetEnvelope(this, pt1.Extent);
            if (dba == null) {
                dba = new DoubleAnimation();

                Storyboard.SetTarget(dba, this);
                Storyboard.SetTargetProperty(dba, new PropertyPath("X"));
                sb.Children.Add(dba);

                dba1 = new DoubleAnimation();
                Storyboard.SetTarget(dba1, this);
                Storyboard.SetTargetProperty(dba1, new PropertyPath("Y"));

                sb.Children.Add(dba1);
            
            }
            
            
            sb.Completed += new EventHandler(sb_Completed);

            dba.From = pt1.X;
            dba.To = pt2.X;
            dba.Duration = new Duration(TimeSpan.FromSeconds(_Interval));

            dba1.From = pt1.Y;
            dba1.To = pt2.Y;
            dba1.Duration = new Duration(TimeSpan.FromSeconds(_Interval));
            sb.Begin();
        }
        public static double CalulateXYAnagle(double startx, double starty, double endx, double endy)
        {
            double tan = Math.Atan(Math.Abs((endy - starty) / (endx - startx)))  * 180/Math.PI ;

            if (endx > startx && endy > starty)//第一象限
            {
                return -tan;
            }
           else if (endx > startx && endy < starty)//第二象限
            {
                return tan;
            }
           else if (endx < startx && endy > starty)//第三象限
            {
                return tan-180;
            }
            else
            {
                return 180-tan;
            }
            
        }
        private void sb_Completed(object sender, EventArgs e)
        {
            if (count < PCol.Count - 2)
            {
                count++;
                MapPoint pt1 = PCol[count];
                MapPoint pt2 = PCol[count + 1];
                DoubleAnimation db = (DoubleAnimation)(sender as Storyboard).Children[0];
                db.From = pt1.X;
                db.To = pt2.X;

                double angle = CalulateXYAnagle(pt1.X, pt1.Y, pt2.X, pt2.Y);
                RotateItemCanvas.Angle = angle;
                DoubleAnimation db1 = (DoubleAnimation)(sender as Storyboard).Children[1];
                db1.From = pt1.Y;
                db1.To = pt2.Y;

                sb.Begin();
            }
            else
            {
                
            }
        }
        public void Stop()
        {
            sb.Stop();

        }
        public void Pause()
        {
            sb.Pause();
        }
        public void Resume()
        {
            sb.Resume();
        }
    }
}
