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

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				Dictionary<List<string>, HyperLink> PageAccess = new Dictionary<List<string>, HyperLink>()
				{
					[new List<string>() { "V_ProjR" }] = HPFrmProjReport,
					[new List<string>() { "V_DocM", "E_DocM" }] = HPFrmDocumentMaint,
					[new List<string>() { "E_AccessC" }] = HPFrmAccessControl,
					[new List<string>() { "V_BankStateM", "E_BankStateM" }] = HPFrmBankStateMaint,
					[new List<string>() { "V_ClientM", "E_ClientM" }] = HPFrmClientMaint,
					[new List<string>() { "V_InvM", "E_InvM" }] = HPFrmInvMaint,
					[new List<string>() {"E_ProjM" }] = HPFrmProjMaint
				};
				UserDetails userDetails = JsonConvert.DeserializeObject<UserDetails>(Session["UserDetails"]?.ToString());
				GF_DisplayWithAccessibility(userDetails.User_Access, PageAccess);
			}	
		}
	}
}