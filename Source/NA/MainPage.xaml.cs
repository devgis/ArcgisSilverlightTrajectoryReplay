

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
using ESRI.ArcGIS.Client.Tasks;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;

namespace NA
{
    public partial class MainPage : UserControl
    {
        
        RouteTask routeTask;
        List<Graphic> _stops = new List<Graphic>();
        List<Graphic> _barriers = new List<Graphic>();
        RouteParameters _routeParams = new RouteParameters();
        GraphicsLayer stopsGraphicsLayer = null;
        GraphicsLayer routeGraphicsLayer = null;

        public MainPage()
        {
            InitializeComponent();
            RoutingBarriers();
        }



        private void map1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

          
        }

           public void RoutingBarriers()
        {
            InitializeComponent();

            routeTask =
                new RouteTask("http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route");
            routeTask.SolveCompleted += new EventHandler<RouteEventArgs>(_routeTask_SolveCompleted);
            routeTask.Failed += new EventHandler<TaskFailedEventArgs>(_routeTask_Failed);
            stopsGraphicsLayer = MyMap.Layers["MyStopsGraphicsLayer"] as GraphicsLayer;
            routeGraphicsLayer = MyMap.Layers["MyRouteGraphicsLayer"] as GraphicsLayer;

        }

           void _routeTask_SolveCompleted(object sender, RouteEventArgs e)
           {
               routeGraphicsLayer.Graphics.Clear();

               RouteResult routeResult = e.RouteResults[0];

               Graphic lastRoute = routeResult.Route;

               routeGraphicsLayer.Graphics.Add(lastRoute);
               ElementLayer elelay = MyMap.Layers["MoveCarLayer"] as ElementLayer;
               ESRI.ArcGIS.Client.Geometry.Polyline Pline = lastRoute.Geometry as ESRI.ArcGIS.Client.Geometry.Polyline;
                            //自定义元素
               Flag mele = new Flag();                                       
               mele.BindMap = MyMap;
               mele.bindLayer = elelay;
               mele.Interval = 1; //自己设置每段间隔运行的时间，这个功能不是很完善，应该是设置速度才合适，不过通过可以在扩展Flag的功能，暴露出来设置速度的方法。
               mele.Route = lastRoute;
               mele.Start();

           }

           void _routeTask_Failed(object sender, TaskFailedEventArgs e)
           {
               string errorMessage = "Routing error: ";
               errorMessage += e.Error.Message;
               foreach (string detail in (e.Error as ServiceException).Details)
                   errorMessage += "," + detail;

               MessageBox.Show(errorMessage);

               stopsGraphicsLayer.Graphics.RemoveAt(stopsGraphicsLayer.Graphics.Count - 1);

           }

           private void MyMap_MouseClick(object sender, Map.MouseEventArgs e)
           {
               
               SimpleMarkerSymbol mark = new SimpleMarkerSymbol();                          
               Graphic stop = new Graphic() { Geometry = e.MapPoint,Symbol=mark };
               stopsGraphicsLayer.Graphics.Add(stop);
               if (stopsGraphicsLayer.Graphics.Count > 1)
               {
                   if (routeTask.IsBusy)
                   {
                       routeTask.CancelAsync();
                       stopsGraphicsLayer.Graphics.RemoveAt(stopsGraphicsLayer.Graphics.Count - 1);
                   }
                   routeTask.SolveAsync(new RouteParameters()
                   {
                       Stops = stopsGraphicsLayer,
                    //   UseTimeWindows = false,
                       UseHierarchy = false,
                       OutSpatialReference = MyMap.SpatialReference
                   });
               }
           }



           
    }
}
