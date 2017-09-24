using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using cc.RealTimeAnalyzer.Models;

namespace WebApp.Controllers
{
    public class DataHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }        
    }
}