using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalVariable;

namespace CUBIC_CIBT_Project
{
	public partial class SiteMaster : MasterPage
	{
		public string UserLogin { get; set; }
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				//Redirect to login page if user dont have loged in session
				if (G_UserLogin.IsNullOrWhiteSpace() || Session["UserDetails"] == null)
				{
					Response.Redirect("~/Frmlogin.aspx");
					return;
				}

				Dictionary<List<string>, HyperLink> PageAccess = new Dictionary<List<string>, HyperLink>()
				{
					[new List<string>() { "V_ProjR" }] = HPFrmProjReport,
					[new List<string>() { "E_AccessC" }] = HPFrmAccessControl,
					[new List<string>() { "V_BankStateM", "E_BankStateM" }] = HPFrmBankStateMaint,
					[new List<string>() { "V_CustomerM", "E_CustomerM" }] = HPFrmCustomerMaint,
					[new List<string>() { "V_InvM", "E_InvM" }] = HPFrmInvMaint,
					[new List<string>() { "E_ProjM" }] = HPFrmProjMaint,
					[new List<string>() { "E_DoM","V_DoM" }] = HPFrmDoMaint,
					[new List<string>() { "E_PoM","V_PoM" }] = HPFrmPoMaint,
					[new List<string>() { "E_QoM","V_QoM" }] = HPFrmQoMaint,

				};

				//Authenticate and Authorize User Access
				UserDetails userDetails = GF_GetSession(Session["UserDetails"]?.ToString());
				GF_DisplayWithAccessibility(userDetails.User_Access, PageAccess);

				//Display User Login
				lblLoginNameSideNav.Text = userDetails.Username;
			}	
		}
	}
}