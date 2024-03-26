using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalVariable;
namespace CUBIC_CIBT_Project
{
	public partial class FrmClientMaintenance : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			UserDetails userDetails = JsonConvert.DeserializeObject<UserDetails>(Session["UserDetails"]?.ToString());
			Dictionary<string, HtmlGenericControl> Access = new Dictionary<string, HtmlGenericControl>()
			{ ["E_ClientM"] = E_ClientM, ["V_ClientM"] = V_ClientM };
			GF_DisplayWithAccessibility(userDetails.User_Access, Access);
		}
		protected void ddlClientMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool ReadOnly, SetVisible;
			ReadOnly = ddlClientMode.SelectedValue == "";
			SetVisible = ddlClientMode.SelectedValue == "U";

			txtClientUsername.ReadOnly = ReadOnly;
			DrpListClientID.Visible = SetVisible;
			lblClientID.Visible = SetVisible;
		}

		protected void BtnSave_Click(object sender, EventArgs e)
		{

		}

		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{

		}
	}
}