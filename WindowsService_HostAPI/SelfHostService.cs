using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace WindowsService_HostAPI
{
    partial class SelfHostService : ServiceBase
    {
        public static List<DirToCheck> dirToCheckList;

        HttpSelfHostServer server;

        public SelfHostService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            dirToCheckList = new List<DirToCheck>();
            var config = new HttpSelfHostConfiguration("http://localhost:8080");

            config.Routes.MapHttpRoute(
               name: "GET",
               routeTemplate: "api/{controller}/{id}",
               defaults: new { id = RouteParameter.Optional }
             );
            config.Routes.MapHttpRoute(
                name: "POST",
                routeTemplate: "api/{controller}"
            );


            server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
        }

        protected override void OnStop()
        {
            server.CloseAsync().Wait();
            server.Dispose();
        }
    }
}
