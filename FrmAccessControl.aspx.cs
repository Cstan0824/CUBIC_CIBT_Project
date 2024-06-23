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
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;
using System.Web.UI.HtmlControls;
using Microsoft.Ajax.Utilities;
using static OfficeOpenXml.ExcelErrorValue;

namespace CUBIC_CIBT_Project
{
	public partial class FrmAccessControl : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				//Redirect to login page if user dont have loged in session
				if (G_UserLogin.IsNullOrWhiteSpace() || Session["UserDetails"] == null)
				{
					GF_ReturnErrorMessage("Please Login to the account before use access the content.", this.Page, this.GetType(), "~/Frmlogin.aspx");
					return;
				}
				//Get User Details from Server Session
				UserDetails userDetails = GF_GetSession(Session["UserDetails"]?.ToString());
				Dictionary<string, HtmlGenericControl> Access = new Dictionary<string, HtmlGenericControl>()
				{ ["E_AccessC"] = E_AccessC };
				bool HasAccess = GF_DisplayWithAccessibility(userDetails.User_Access, Access);
				if (!HasAccess)
				{
					GF_ReturnErrorMessage("You dont have access to this page, kindly look for adminstration.", this.Page, this.GetType(), "~/Default.aspx");
					return;
				}
			}
		}

		/// <summary>
		/// Handles the "Save" button click event.
		/// Determines if the employee mode is "Create" or "Update" and processes accordingly.
		/// Shows a success modal and redirects to the access control page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{
			string FailedMessage = "";
			if (!F_CheckUsernameExistence(out FailedMessage))
			{
				ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Failed", "closeModal(); showErrorModal('Failed', '" + FailedMessage + "');", true);
				return;
			}

			string ddlMode = "";
			switch (ddlEmpMode.SelectedValue)
			{
				case "C":
					ddlMode = "Created";
					F_CreateEmp();
					break;
				case "U":
					ddlMode = "Updated";
					F_UploadEmp();
					break;
				default:
					break;
			}
			string SuccessMessage = $"The Employee {ddlMode} Successfully.";
			ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Success", "showModal('Success', '" + SuccessMessage + "');", true);

			Response.Redirect("~/FrmAccessControl.aspx"); // Refresh page
		}

		/// <summary>
		/// Handles the change in the employee mode dropdown selection.
		/// Toggles form fields based on the selected mode (Create or Update).
		/// Populates employee IDs if the update mode is selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ddlEmpMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<TextBox> TxtList = new List<TextBox>() { txtEmpUsername, txtPassword };

			bool HasModeSelected = ddlEmpMode.SelectedValue != "";
			bool UpdateMode = ddlEmpMode.SelectedValue == "U";
			bool CreateMode = ddlEmpMode.SelectedValue == "C";

			//Create or Update Mode
			TxtList.ForEach(txt => txt.ReadOnly = !HasModeSelected);
			btnGeneratePassword.Visible = HasModeSelected;

			//Update mode
			EmpIDDrpList.Visible = UpdateMode;
			lblEmpIDtxt.Visible = UpdateMode;

			//Clear the item every time fill in the data to it
			GF_ClearItem(EmpIDDrpList);
			//Display current employees ID
			if (HasModeSelected)
			{
				if (UpdateMode)
				{
					string WhereClause = $"WHERE [EMP_NO] <> '{G_UserLogin}' AND [EMP_STATUS] != 'X' ";
					GF_PopulateDocNo(EmpIDDrpList, new M_Employee(), "EMP_NO", WhereClause);
				}

			}
			else
			{
				GF_ClearInputFeild(Page.Master.FindControl("form1"));
			}
		}

		/// <summary>
		/// Generates a strong password for the user.
		/// The password includes a mix of letters, numbers, and special symbols,
		/// and is displayed in the txtPassword field.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
				.OrderBy(c => Rnd.Next(SpecialSymbols.Length))
				.First()
				.ToString();

			//Combine
			string temp_pass = Num_Pass + Char_Pass + Special_Pass;

			//Shuffle password
			string password = new string(temp_pass.ToCharArray().OrderBy(c => Rnd.Next(temp_pass.Length)).ToArray());
			txtPassword.Text = password;
		}

		/// <summary>
		/// Updates existing employee information and access permissions.
		/// Modifies employee details including username and status.
		/// Hashes the password if provided and updates the database.
		/// </summary>
		private void F_UploadEmp()
		{

			string WhereClause = $"WHERE [EMP_NO] = '{EmpIDDrpList.SelectedValue}' ";
			M_Employee m_Employee = new M_Employee()
			{
				Emp_UserName = txtEmpUsername.Text,
				Emp_Status = char.Parse(rbStatus.SelectedValue),
				Emp_Modified_By = G_UserLogin,
				Emp_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};
			//Update the password if user fill in
			if (txtPassword.Text != string.Empty)
			{
				m_Employee.Emp_Password = GF_HashPassword(txtPassword.Text);
			}
			TableDetails tableDetails = F_GetTableDetails(m_Employee, WhereClause, IsUpdateMethod: true);
			DB_UpdateData(tableDetails);
			//Update access
			F_UpdateAccesss(EmpIDDrpList.SelectedValue);
		}

		/// <summary>
		/// Creates a new employee record with initial details and access permissions.
		/// Generates a new employee ID and hashes the password.
		/// Inserts the new employee record into the database.
		/// </summary>
		private void F_CreateEmp()
		{
			string tempBU = "CS";
			string tempEmpNo = GF_GenerateID(tempBU, "EMP");
			M_Employee m_Employee = new M_Employee()
			{
				Emp_No = tempEmpNo,
				Emp_UserName = txtEmpUsername.Text,
				Emp_Password = GF_HashPassword(txtPassword.Text),
				Emp_Status = char.Parse(rbStatus.SelectedValue),
				Emp_BU = tempBU,
				Emp_IsLogIn = 'N',
				Emp_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Emp_Modified_By = G_UserLogin,
				Emp_Created_Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Emp_Created_By = G_UserLogin,
			};
			TableDetails tableDetails = F_GetTableDetails(m_Employee);
			DB_CreateData(tableDetails);
			//Create Access
			F_CreateAccess(tempEmpNo);
		}

		/// <summary>
		/// Retrieves selected access permissions from the form.
		/// Converts them into a list of data strings based on the access mode.
		/// </summary>
		/// <param name="_EmpNo">Employee number (optional for create mode).</param>
		/// <param name="IsUpdateMode">Flag to indicate if the operation is in update mode.</param>
		/// <returns>A list of selected access permissions, Or return null if no access is checked</returns>
		private List<string> F_GetCheckedAccess(string _EmpNo = "", bool IsUpdateMode = false)
		{
			List<string> Values = new List<string>();
			bool HasAccessChecked = false;
			Dictionary<string, int> accessMapping = GetAccessMapping(); //return the access_no when pass the access_desc to the dictionary
			List<CheckBoxList> checkBoxLists = new List<CheckBoxList>()
			{
			ChkEditAccessAdmin,
			ChkEditAccessMaintenance,
			ChkViewAccessAdmin,
			ChkViewAccessMaintenance,
			ChkViewAccessReport
			};
			checkBoxLists.
				ForEach(
				checkBoxList =>
				{
					checkBoxList.Items.Cast<ListItem>().ForEach(item =>
					{
						if (item.Selected)
						{
							HasAccessChecked = true;
							// Add the value or text of the checked item
							if (IsUpdateMode)
							{
								Values.Add($"'{accessMapping[item.Value]}'");
							}
							else
							{
								Values.Add($"('{_EmpNo}','{accessMapping[item.Value]}')");
							}
						}
					});
				});
			if (HasAccessChecked)
			{
				return Values;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Retrieves the mapping of access descriptions to their corresponding numbers.
		/// Queries the database to get access details and maps descriptions to access numbers.
		/// </summary>
		/// <returns>A dictionary mapping access descriptions to access numbers.</returns>
		private Dictionary<string, int> GetAccessMapping()
		{
			Dictionary<string, int> accessMapping = new Dictionary<string, int>();

			// Assuming you have a method to retrieve all access descriptions and numbers
			TableDetails tableDetails = F_GetTableDetails(new M_Access());
			DataTable accessTable = DB_ReadData(tableDetails);
			foreach (DataRow row in accessTable.Rows)
			{
				int accessNo = Convert.ToInt32(row["ACCESS_NO"]);
				string accessDesc = row["ACCESS_DESC"].ToString();
				accessMapping[accessDesc] = accessNo;
			}

			return accessMapping;
		}

		/// <summary>
		/// Handles the change in the employee ID dropdown selection.
		/// Populates form fields with selected employee details and current access permissions.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void EmpIDDrpList_SelectedIndexChanged(object sender, EventArgs e)
		{
			Dictionary<string, int> accessMapping = GetAccessMapping(); //return the access_no when pass the access_desc to the dictionary

			//Employee Details
			string WhereClause = $"WHERE [EMP_NO] = '{EmpIDDrpList.SelectedValue}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Employee(), WhereClause);
			DataTable Emp_DataTable = DB_ReadData(tableDetails);
			if (Emp_DataTable == null)
			{
				return;
			}
			if (Emp_DataTable.Rows.Count == 0)
			{
				return;
			}

			DataRow row = Emp_DataTable.Rows[0];
			txtEmpUsername.Text = row["EMP_USERNAME"]?.ToString();
			rbStatus.SelectedValue = row["EMP_STATUS"]?.ToString();

			//Access Details

			List<CheckBoxList> checkBoxLists = new List<CheckBoxList>()
			{
			ChkEditAccessAdmin,
			ChkEditAccessMaintenance,
			ChkViewAccessAdmin,
			ChkViewAccessMaintenance,
			ChkViewAccessReport
			};

			DataTable Access_DataTable = F_GetEmpCurrentAccess(EmpIDDrpList.SelectedValue);
			if (Access_DataTable == null)
			{
				return;
			}
			if (Access_DataTable.Rows.Count == 0)
			{
				return;
			}
			var CurrAccess = Access_DataTable.AsEnumerable()
											  .Select(data => data["ACCESS_NO"].ToString())
											  .ToList();

			checkBoxLists.ForEach(checkBoxList =>
			{
				checkBoxList.Items.Cast<ListItem>().ForEach(item =>
				{
					string mappedValue = accessMapping[item.Value].ToString();
					item.Selected = CurrAccess.Contains(mappedValue);
				});
			});
		}

		/// <summary>
		/// Retrieves the current access permissions for a specific employee.
		/// Queries the database for access permissions associated with the employee number.
		/// </summary>
		/// <param name="_EmpNo">The employee number.</param>
		/// <returns>A DataTable containing current access permissions.</returns>
		private DataTable F_GetEmpCurrentAccess(string _EmpNo)
		{
			//Get the current access of the employee
			string WhereClause = $"WHERE [EMP_NO] = '{_EmpNo}' ";
			TableDetails tableDetails = F_GetTableDetails(new T_Employee_Access(), WhereClause);
			return DB_ReadData(tableDetails, "[ACCESS_NO]");
		}

		/// <summary>
		/// Inserts selected access permissions into the database for a new employee.
		/// Retrieves selected access permissions and inserts them as records in the database.
		/// </summary>
		/// <param name="_EmpNo">The employee number.</param>
		private void F_CreateAccess(string _EmpNo)
		{
			//Receive checked access and convert them into a list of data
			List<string> Values = F_GetCheckedAccess(_EmpNo);
			if (Values == null)
			{
				return;
			}
			string Access = string.Join(",", Values);

			//connect with database and insert the data - DB_ReadData() requires multiple sql Insert request
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLInsertCommand = $"INSERT INTO [dbo].[T_EMPLOYEE_ACCESS]([EMP_NO],[ACCESS_NO]) ";
				SQLInsertCommand += $"VALUES{Access};";
				SqlCommand SQLCmd = new SqlCommand(SQLInsertCommand, Conn);
				SQLCmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "F_CreateAccess", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
			return;
		}

		/// <summary>
		/// Updates the access permissions for an existing employee.
		/// Compares new and current access permissions and adds or removes as needed.
		/// </summary>
		/// <param name="_EmpNo">The employee number.</param>
		private void F_UpdateAccesss(string _EmpNo)
		{
			List<string> newAccess = F_GetCheckedAccess(_EmpNo);

			//Get the current access of the employee
			DataTable dataTable = F_GetEmpCurrentAccess(_EmpNo);

			if (dataTable == null)
			{
				return;
			}
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			var CurrAccess = dataTable.AsEnumerable()
											  .Select(data => data["ACCESS_NO"].ToString())
											  .ToList();
			//delete all current access if the newAccess is null
			if (newAccess != null)
			{
				//Remove those access that have currently compared to new access
				for (int i = CurrAccess.Count - 1; i >= 0; i--)
				{
					string accessDesc = CurrAccess[i];
					if (newAccess.Contains(accessDesc))
					{
						// Remove from the Values list
						newAccess.Remove(accessDesc);

						// Remove from the currAccess list
						CurrAccess.Remove(accessDesc);
					}
				}
				//Those access left at Values are the new access to add 
				F_AccessToAdd(newAccess);
			}
			//Those access left at currAccess are the old access to delete
			F_AccessToDelete(_EmpNo, CurrAccess);

			

		}

		/// <summary>
		/// Inserts new access permissions into the database for an existing employee.
		/// </summary>
		/// <param name="_newAccess">A list of new access permissions to add.</param>
		private void F_AccessToAdd(List<string> _newAccess)
		{
			string AccessToAdd = string.Join(",", _newAccess);
			//connect with database and insert the data - DB_ReadData() requires multiple sql Insert request
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLInsertCommand = $"INSERT INTO [dbo].[T_EMPLOYEE_ACCESS] ";
				SQLInsertCommand += $"VALUES{AccessToAdd};";
				SqlCommand SQLCmd = new SqlCommand(SQLInsertCommand, Conn);
				SQLCmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "F_AccessToAdd", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}

		/// <summary>
		/// Deletes old access permissions from the database for an existing employee.
		/// </summary>
		/// <param name="_EmpNo">The employee number.</param>
		/// <param name="_CurrAccess">A list of current access permissions to delete.</param>
		private void F_AccessToDelete(string _EmpNo, List<string> _CurrAccess)
		{
			string AccessToDelete = string.Join(",", _CurrAccess);
			string WhereClause = $"WHERE [EMP_NO] = '{_EmpNo}' AND [ACCESS_NO] IN ({AccessToDelete}) ";
			TableDetails tableDetails = F_GetTableDetails(new T_Employee_Access(), WhereClause);

			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLDeleteCommand = $"DELETE FROM [dbo].[T_EMPLOYEE_ACCESS] ";
				SQLDeleteCommand += $"{tableDetails.WhereOrJoinClause};";
				SqlCommand SQLCmd = new SqlCommand(SQLDeleteCommand, Conn);
				SQLCmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "F_AccessToDelete", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}

		/// <summary>
		/// Checks for the existence of data based on the selected employee mode and username.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if data existence check fails.</param>
		/// <returns>True if data does not exist or mode is not "Create", otherwise false.</returns>
		private bool F_CheckUsernameExistence(out string _FailedMessage)
		{
			if (ddlEmpMode.SelectedValue != "C")
			{
				_FailedMessage = "";
				return true;
			}
			//Check value Existence (Employee Username)
			string SelectData = "[EMP_USERNAME] ";
			string WhereClause = $"WHERE [EMP_USERNAME] = '{txtEmpUsername.Text}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Employee(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);
			if (dataTable == null)
			{
				_FailedMessage = "There\'s an issue while receiving the data, Please try again later.";
				return false;
			}
			if (dataTable.Rows.Count > 0)
			{
				_FailedMessage = "The Employee Username already exists. Please try another username.";
				return false;
			}
			_FailedMessage = "";
			return true;
		}

	}

}