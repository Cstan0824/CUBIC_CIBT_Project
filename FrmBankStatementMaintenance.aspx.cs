using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalVariable;

namespace CUBIC_CIBT_Project
{
	public partial class FrmBankStatementMaintenance : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (ChooseFileUpload.HasFile)
			{
				txtRemark.ReadOnly = false;
			}
		}

        protected void DrpListProjectCode_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

		protected void DrpListBankType_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool SetDisplay,ReadOnly;
			List<DropDownList> DrpList = new List<DropDownList> { DrpListCustomerCode, DrpListProjectCode, DrpListSupplierCode };

			SetDisplay = DrpListBankType.SelectedValue != "";
			ReadOnly = SetDisplay;
			//Set ReadOnly
			txtRemark.ReadOnly = ReadOnly;
			txtStatementDate.ReadOnly = ReadOnly;

			//Set Visible
			ChooseFileUpload.Visible= SetDisplay;
			lblFileUpload.Visible= SetDisplay;

			//Set to display the Drp List
			DrpList.ForEach(GF_ClearItem);
			if (SetDisplay)
			{
				DrpList.ForEach(GF_DrpListAddDefaultItem);
			}
			if (ReadOnly)
			{
				GF_ClearInputFeild(Page.Master.FindControl("form1"));
			}
		}

		protected void BtnSave_Click(object sender, EventArgs e)
		{

		}

		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{

		}
		private void F_DrpListAddDefaultItem(DropDownList droplist)
		{
			droplist.Items.Add(new ListItem("--Select One--", ""));
		}

		private void F_ClearItem(DropDownList droplist)
		{
			droplist.Items.Clear();
		}
	}
}