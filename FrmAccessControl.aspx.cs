using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using static CUBIC_CIBT_Project.GlobalVariable;
using System.Text;
using System.Data.SqlTypes;
using System.Web.UI.WebControls.WebParts;

namespace CUBIC_CIBT_Project
{
	public partial class FrmAccessControl : Page
	{
		StringBuilder TempCombineEditAccess = new StringBuilder();
		StringBuilder TempCombineViewAccess = new StringBuilder();
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				ChkEditAccessMaintenance.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = true);
				ChkViewAccessMaintenance.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = true);
				ChkViewAccessReport.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = true);
			}
			
		}

		protected void ConfirmBtnCreate_Click(object sender, EventArgs e)
		{
			// Append Edit Access use one-many table instead
			TempCombineEditAccess.Append
				(
				string.Join(
				",",
				ChkEditAccessMaintenance.Items.Cast<ListItem>()
				.Concat(ChkEditAccessAdmin.Items.Cast<ListItem>())
				)
				);

			// Append View Access
			TempCombineViewAccess.Append(string.Join(",", ChkViewAccessMaintenance.Items.Cast<ListItem>()));


			if (TempCombineEditAccess.Length > 0)
			{
				TempCombineEditAccess.Length--;
			}
			if (TempCombineViewAccess.Length > 0)
			{
				TempCombineViewAccess.Length--;
			}

			if (ddlEmpMode.SelectedValue != "C")
			{
				F_CreateEmp();
			}
			else
			{
				F_UploadEmpDetails();
			}
			//Save access and details to database
		}
		//Admin Edit
		protected void ChkEditAccessAdmin_SelectedIndexChanged(object sender, EventArgs e)
		{
		}
		//Maintenance View
		protected void ChkViewAccessMaintenance_SelectedIndexChanged(object sender, EventArgs e)
		{ 
		}
		//Maintenance Edit
		protected void ChkEditAccessMaintenance_SelectedIndexChanged(object sender, EventArgs e)
		{
		}
		protected void RadioQuickAccess_SelectedIndexChanged(object sender,EventArgs e)
		{
			F_SetAllChkBtn(false);
			var QuickAccess = RadioQuickAccess.SelectedValue.ToString();
			switch (QuickAccess)
			{
				case "Default":
					ChkEditAccessMaintenance.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = true);
					ChkViewAccessMaintenance.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = true);
					ChkViewAccessReport.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = true);
					break;
				case "Admin":
					F_SetAllChkBtn(true);
					break;
				case "View":
					ChkViewAccessMaintenance.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = true);
					ChkViewAccessReport.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = true);
					break;
				default:
					break;
			}
		}
		protected void ddlEmpMode_SelectedIndexChanged(object sender,EventArgs e) 
		{
			Control myControl = Page.Master.FindControl("form1");
			var SetReadOnly = (ddlEmpMode.SelectedValue == "") ? true : false;

			List<TextBox> TxtList = new List<TextBox>() { txtEmpUsername,txtPassword };

			//Set ReadOnly
			TxtList.ForEach(txt => txt.ReadOnly= SetReadOnly);
			//myControl.Controls.OfType<TextBox>().ToList().ForEach(txtBox => txtBox.Text = string.Empty);
			//passing the action to inside of ForEach to GF_EmptyInputFeild > GF_InputFeild

			//Set Visble
			btnGeneratePassword.Visible = !SetReadOnly;

			//Display Select Input when update mode and Txt Input when Create or Default Mode
			var SetVisible = (ddlEmpMode.SelectedValue == "U");
			EmpIDDrpList.Visible = SetVisible;
			lblEmpIDtxt.Visible = SetVisible;

			//Reset input feild
			if(SetReadOnly) GF_ClearInputFeild(myControl);


		}
		protected void btnGeneratePassword_Click(object sender, EventArgs e) 
		{
			int Total_Len = 12;

			const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			const string Numbers = "0123456789";
			const string SpecialSymbols = "@#_$";

			Random Rnd = new Random();
			int Char_Len = Rnd.Next(Total_Len - 4, Total_Len - 2);

			int Num_Len = Total_Len - Char_Len - 1;

			//Append Character
			string Char_Pass = 
				new string(Enumerable
				.Repeat(Letters, Char_Len)
				.Select(c => c[Rnd.Next(Letters.Length)])
				.ToArray())
				;
			//Append Number
			string Num_Pass = new string(
				Enumerable
				.Repeat(Numbers, Num_Len)
				.Select(c => c[Rnd.Next(Numbers.Length)])
				.ToArray())
				;

			//Append Special Symbol
			string Special_Pass = 
				SpecialSymbols
				.OrderBy(c=>Rnd.Next(SpecialSymbols.Length))
				.First()
				.ToString();

			//Combine
			string temp_pass = Num_Pass + Char_Pass + Special_Pass;

			//Shuffle password
			string password = new string(temp_pass.ToCharArray().OrderBy(c => Rnd.Next(temp_pass.Length)).ToArray());
			txtPassword.Text = password;
		}
		private void F_SetAllChkBtn(bool SetCheck)
		{
			//Page.Master.FindControl("form1")?.Controls.OfType<CheckBoxList>().ToList().ForEach(chkList => chkList.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = SetCheck));
			ChkEditAccessAdmin.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = SetCheck);
			ChkEditAccessMaintenance.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = SetCheck);
			ChkViewAccessMaintenance.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = SetCheck);
			ChkViewAccessReport.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = SetCheck);
			ChkViewAccessAdmin.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = SetCheck);
		}
		private void F_UploadEmpDetails()
		{
			SqlConnection conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(conn);
			conn.Open();
			try
			{
				//Create a one to many relation table
				string SQLSelectCommand = "UPDATE Emp_Table SET (Emp_name,Emp_Access) = (" +
					txtEmpUsername.Text +
					"," + 
					TempCombineEditAccess +
					TempCombineEditAccess + 
					")";
				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, conn);
				SQLcmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				GF_ReturnErrorMessage("Employee details Upload Failed.",this.Page,this.GetType());
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "UploadEmployeeDetails", ex.ToString());
			}
			finally
			{
				conn.Dispose();
				conn.Close();
			}
		}
		private void F_CreateEmp()
		{
			SqlConnection conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(conn);
			conn.Open();
			try
			{
				string SQLSelectCommand = "INSERT INTO Emp_Table() VALUES();";
				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, conn);
				SQLcmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				GF_ReturnErrorMessage("Failed to Create an Employee",this.Page,this.GetType());
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "CreateEmp", ex.ToString());
			}
			finally
			{
				conn.Dispose();
				conn.Close();
			}
		}
		protected void btnCreate_Click(object sender, EventArgs e)
		{
			SqlConnection conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(conn);
			conn.Open();
			try
			{
				string SQLSelectCommand = "SELECT Emp_id FROM Emp_Table;";
				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, conn);
				SqlDataReader DataReader = SQLcmd.ExecuteReader();
				if (!DataReader.HasRows)
				{
					GF_ReturnErrorMessage("User Not Found, kindly look for administration.", this.Page, this.GetType());
					return;
				}
				while (DataReader.Read())
				{
					
				}
				DirectTarget.Attributes["data-bs-target"] = "#ConfirmationModalMessage";
			}
			catch (Exception ex)
			{
				DirectTarget.Attributes["data-bs-target"] = "#ErrorModalMessage";
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "btnCreate_Click", ex.ToString());
			}
			finally
			{
				conn.Dispose();
				conn.Close();
			}
		}

        protected void ChkViewAccessAdmin_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
