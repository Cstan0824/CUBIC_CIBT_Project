using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalVariable;

namespace CUBIC_CIBT_Project
{
	public partial class FrmProjectListing : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				UserDetails userDetails = JsonConvert.DeserializeObject<UserDetails>(Session["UserDetails"]?.ToString());
				Dictionary<string, HtmlGenericControl> Access = new Dictionary<string, HtmlGenericControl>()
				{["V_ProjR"] = V_ProjR };
				GF_DisplayWithAccessibility(userDetails.User_Access, Access);
			}
			DataTable dt = new DataTable();
			DocMRepeater.DataSource = dt;
			DocMRepeater.DataBind();
		}

        protected void BtnConfirmSave_Click(object sender, EventArgs e)
        {

        }
    }
}