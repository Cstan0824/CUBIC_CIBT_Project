using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CUBIC_CIBT_Project
{
	public partial class FrmSupplierMaintenance : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{

		}

        protected void ddlSupplierMode_SelectedIndexChanged(object sender, EventArgs e)
        {
			bool ReadOnly,SetVisible;
			ReadOnly = ddlSupplierMode.SelectedValue == "";
			SetVisible = ddlSupplierMode.SelectedValue == "U";

			txtSupplierUsername.ReadOnly = ReadOnly;
			DrpListSupplierID.Visible = SetVisible;
			lblSupplierID.Visible = SetVisible;

		}
		protected void BtnSave_Click(object sender, EventArgs e)
		{

		}

		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{

		}
	}
}