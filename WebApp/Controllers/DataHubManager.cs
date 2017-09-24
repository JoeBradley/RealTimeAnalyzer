using cc.RealTimeAnalyzer.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Controllers
{
    public class DataHubManager
    {
        private static readonly Lazy<DataHubManager> lazy = new Lazy<DataHubManager>(() => new DataHubManager(GlobalHost.ConnectionManager.GetHubContext<DataHub>()));

        private IHubContext _context;

        private DataHubManager(IHubContext context)
        {
            _context = context;
        }

        public static DataHubManager Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public void SetData(AggregateData data) {
            _context.Clients.All.setData(data);
        }
    }
}