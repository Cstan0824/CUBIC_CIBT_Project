using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalVariable;

namespace CUBIC_CIBT_Project
{
	public partial class FrmProjectMaintenance : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected void ddlProjectMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool ReadOnly, SetVisible, SetDisplayDrpList;
			List<DropDownList> DrpList = new List<DropDownList> { DrpListCustomerCode, DrpListProjectCode, DrpListSupplierCode };

			ReadOnly = ddlProjectMode.SelectedValue == "";
			SetVisible = ddlProjectMode.SelectedValue == "U";
			SetDisplayDrpList = !ReadOnly;
			//Set ReadOnly
			txtProjectDate.ReadOnly = ReadOnly;
			txtRemark.ReadOnly = ReadOnly;

			//Set Visible
			lblProjectID.Visible = SetVisible;
			DrpListProjectID.Visible = SetVisible;

			DrpList.ForEach(GF_ClearItem);
			if (SetDisplayDrpList)
			{
				DrpList.ForEach(GF_DrpListAddDefaultItem);
			}

			//Clear Input Feild
			if (ReadOnly) GF_ClearInputFeild(Page.Master.FindControl("form1"));
		}

		protected void BtnSave_Click(object sender, EventArgs e)
		{

		}

		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{

		}
	}
}