using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CUBIC_CIBT_Project
{
	public partial class FrmProjectListing : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			DataTable dt = new DataTable();
			DocMRepeater.DataSource = dt;
			DocMRepeater.DataBind();
		}

        protected void BtnConfirmSave_Click(object sender, EventArgs e)
        {

        }
    }
}