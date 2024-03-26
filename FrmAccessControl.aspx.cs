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
using Newtonsoft.Json;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using System.Web.UI.HtmlControls;

namespace CUBIC_CIBT_Project
{
	public partial class FrmAccessControl : Page
	{
		//List<string> Accessibility = new List<string>();
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				UserDetails userDetails = JsonConvert.DeserializeObject<UserDetails>(Session["UserDetails"]?.ToString());
				Dictionary<string, HtmlGenericControl> Access = new Dictionary<string, HtmlGenericControl>()
				{ ["E_AccessC"] = E_AccessC };
				GF_DisplayWithAccessibility(userDetails.User_Access, Access);

				List<CheckBoxList> chkBoxList = new List<CheckBoxList>()
				{  ChkEditAccessMaintenance, ChkViewAccessReport, ChkViewAccessMaintenance };
				chkBoxList.ForEach(chkList => chkList.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = true));
			}
		}

		protected void ConfirmBtnCreate_Click(object sender, EventArgs e)
		{
			if (ddlEmpMode.SelectedValue == "C")
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

			List<TextBox> TxtList = new List<TextBox>() { txtEmpUsername, txtPassword };

			//Set ReadOnly
			TxtList.ForEach(txt => txt.ReadOnly= SetReadOnly);

			//Set Visble
			btnGeneratePassword.Visible = !SetReadOnly;

			//Display Select Input when update mode and Txt Input when Create or Default Mode
			var SetVisible = (ddlEmpMode.SelectedValue == "U");
			EmpIDDrpList.Visible = SetVisible;
			lblEmpIDtxt.Visible = SetVisible;

			//Reset input feild
			if(SetReadOnly) GF_ClearInputFeild(myControl);

			//Display current employees ID
			if (SetVisible)
			{
				GF_DrpListAddDefaultItem(EmpIDDrpList);
				F_PopulateEmp();
			}
			else
			{
				GF_ClearItem(EmpIDDrpList);
			}
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
			List<CheckBoxList> chkBoxList = new List<CheckBoxList>() 
			{ ChkEditAccessAdmin, ChkEditAccessMaintenance, ChkViewAccessAdmin, ChkViewAccessReport, ChkViewAccessMaintenance };
			chkBoxList.ForEach(chkList=>chkList.Items.Cast<ListItem>().ToList().ForEach(item => item.Selected = SetCheck));
		}
		private void F_UploadEmpDetails()
		{
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				//Create a one to many relation table
				string SQLSelectCommand = "UPDATE T_EMPLOYEE SET (EMP_USERNAME,EMP_MODIFIED_DATE,EMP_MODIFIED_BY)";
				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, Conn);
				SQLcmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				GF_ReturnErrorMessage("Employee details Upload Failed.",this.Page,this.GetType());
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "UploadEmployeeDetails", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}
		//Debug
		private void F_CreateEmp()
		{
			UserDetails userDetails = JsonConvert.DeserializeObject<UserDetails>(Session["UserDetails"]?.ToString());
			string EmpNO = F_GenerateEmpID(userDetails.User_BU, "EMP");
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLInsertCommand = "INSERT INTO T_EMPLOYEE()";
				SQLInsertCommand += $"VALUES('{EmpNO}',);";
				SqlCommand SQLcmd = new SqlCommand(SQLInsertCommand, Conn);
				SQLcmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				GF_ReturnErrorMessage("Failed to Create an Employee",this.Page,this.GetType());
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "CreateEmp", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}
		//Debug
		protected void btnCreate_Click(object sender, EventArgs e)
		{
			//check if the check box is not all empty
			if (txtPassword.Text == string.Empty || txtEmpUsername.Text == string.Empty || ddlEmpMode.SelectedValue == "")
			{
				DirectTarget.Attributes["data-bs-target"] = "#ErrorModalMessage";
			}
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLSelectCommand = $"SELECT EMP_NO FROM [T_EMPLOYEE] WHERE [EMP_NO]='{txtEmpUsername}';";
				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, Conn);
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
				Conn.Dispose();
				Conn.Close();
			}
		}

        protected void ChkViewAccessAdmin_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

		private void F_PopulateEmp()
		{
			UserDetails userDetails = JsonConvert.DeserializeObject<UserDetails>(Session["UserDetails"]?.ToString());

			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLSelectCommand = $"SELECT [EMP_NO] FROM [T_EMPLOYEE] WHERE [EMP_NO] <> {userDetails.User_Login};";
				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, Conn);
				SqlDataReader DataReader = SQLcmd.ExecuteReader();
				if (!DataReader.HasRows)
				{
					GF_ReturnErrorMessage("No Employee Found", this.Page, this.GetType());
					return;
				}
				while (DataReader.Read())
				{
					EmpIDDrpList.Items.Add(new ListItem(DataReader["EMP_NO"]?.ToString(), DataReader["EMP_NO"]?.ToString()));
				}
			}
			catch (Exception ex)
			{
				GF_ReturnErrorMessage("Failed to Populate Employee", this.Page, this.GetType());
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "CreateEmp", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}

		private string F_GenerateEmpID(string _tempBU,string _tempPrefix)
		{
			return _tempPrefix + GF_GetRunningNumber(_tempBU, _tempPrefix).ToString().PadLeft(8, '0');
		}

		protected void EmpIDDrpList_SelectedIndexChanged(object sender, EventArgs e)
		{
			//SqlConnection Conn = new SqlConnection(G_ConnectionString);
			//GF_CheckConnectionStatus(Conn);
			//Conn.Open();
			//try
			//{
			//	string SQLSelectCommand = $"SELECT [EMP_USERNAME] FROM [T_EMPLOYEE] WHERE [EMP_NO] = '{EmpIDDrpList.SelectedValue}';";
			//	SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, Conn);
			//	SqlDataReader DataReader = SQLcmd.ExecuteReader();
			//	if (!DataReader.HasRows)
			//	{
			//		GF_ReturnErrorMessage("No Employee Found", this.Page, this.GetType());
			//		return;
			//	}
			//	DataReader.Read();
			//	txtEmpUsername.Text = DataReader["EMP_USERNAME"]?.ToString();
			//}
			//catch (Exception ex)
			//{
			//	GF_ReturnErrorMessage("Failed to Populate Employee", this.Page, this.GetType());
			//	GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "CreateEmp", ex.ToString());
			//}
			//finally
			//{
			//	Conn.Dispose();
			//	Conn.Close();
			//}
		}

		
	}
}


//myControl.Controls.OfType<TextBox>().ToList().ForEach(txtBox => txtBox.Text = string.Empty);
//passing the action to inside of ForEach to GF_EmptyInputFeild > GF_InputFeild