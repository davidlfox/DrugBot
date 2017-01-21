using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Routing;

namespace DrugBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var instrumentationKey = WebConfigurationManager.AppSettings["ApplicationInsightsInstrumentationKey"];

            if (!string.IsNullOrWhiteSpace(instrumentationKey))
            {
                Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
            }
        }
    }
}
