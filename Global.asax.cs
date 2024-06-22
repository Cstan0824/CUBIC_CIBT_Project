using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;

namespace CUBIC_CIBT_Project
{
	public class Global : HttpApplication
	{
		void Application_Start(object sender, EventArgs e)
		{
			// Code that runs on application startup
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

			// ** ADD THIS TO PREVENT JS SCRIPT ERROR
			string JQueryVer = "3.5.1";
			ScriptManager.ScriptResourceMapping.AddDefinition("jquery", new ScriptResourceDefinition
			{
				Path = "~/Scripts/jquery-" + JQueryVer + ".min.js",
				DebugPath = "~/Scripts/jquery-" + JQueryVer + ".js",
				CdnPath = "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-" + JQueryVer + ".min.js",
				CdnDebugPath = "http://ajax.aspnetcdn.com/ajax/jQuery/jquery-" + JQueryVer + ".js",
				CdnSupportsSecureConnection = true,
				LoadSuccessExpression = "window.jQuery"
			});
		}
	}
}