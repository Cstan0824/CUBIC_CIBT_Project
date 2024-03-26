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
	public partial class FrmInvoiceMaintenance : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				UserDetails userDetails = JsonConvert.DeserializeObject<UserDetails>(Session["UserDetails"]?.ToString());
				Dictionary<string, HtmlGenericControl> Access = new Dictionary<string, HtmlGenericControl>()
				{ ["E_InvM"] = E_InvM, ["V_InvM"] = V_InvM };
				GF_DisplayWithAccessibility(userDetails.User_Access, Access);
			}
		}

        protected void ddlInvoiceMode_SelectedIndexChanged(object sender, EventArgs e)
        {
			List<DropDownList> DrpList = new List<DropDownList> { DrpListCustomerCode, DrpListProjectCode, DrpListSupplierCode,DrpListInvoiceID };
			List<TextBox> txtBox = new List<TextBox>() { txtInvoiceDate, txtBalanceAmount, txtPaidAmount, txtPOAmount, txtPaidAmount,txtRemark,txtPONo };
			bool ReadOnly, SetVisible,SetFileVisible;
			ReadOnly = ddlInvoiceMode.SelectedValue == "";
			SetVisible = ddlInvoiceMode.SelectedValue == "U"; 
			SetFileVisible = ddlInvoiceMode.SelectedValue == "C";

			//SetReadOnly
			txtBox.ForEach(txt => txt.ReadOnly = ReadOnly);

			//Set Visible
			lblInvoiceIDDrpList.Visible = SetVisible;
			DrpListInvoiceID.Visible = SetVisible;

			lblFileUpload.Visible = SetFileVisible;
			ChooseFileUpload.Visible = SetFileVisible;

			lblPOFile.Visible = SetFileVisible;
			filePO.Visible = SetFileVisible;

			DrpList.ForEach(GF_ClearItem);
			if (!ReadOnly)
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
	}
}