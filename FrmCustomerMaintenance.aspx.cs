using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;
using static CUBIC_CIBT_Project.GlobalVariable;
namespace CUBIC_CIBT_Project
{
	public partial class FrmCustomerMaintenance : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				//Redirect to login page if user dont have loged in session
				if (G_UserLogin.IsNullOrWhiteSpace() || Session["UserDetails"] == null)
				{
					GF_ReturnErrorMessage("Please Login to the account before use access the content.", this.Page, this.GetType(), "~/Frmlogin.aspx");
					return;
				}
				//Get User Details from Server Session
				UserDetails userDetails = GF_GetSession(Session["UserDetails"]?.ToString());

				//Authenticate and Authorize access
				Dictionary<string, HtmlGenericControl> Access = new Dictionary<string, HtmlGenericControl>()
				{ ["E_CustomerM"] = E_CustomerM, ["V_CustomerM"] = V_CustomerM };
				bool HasAccess = GF_DisplayWithAccessibility(userDetails.User_Access, Access);
				if (!HasAccess)
				{
					GF_ReturnErrorMessage("You dont have access to this page, kindly look for adminstration.", this.Page, this.GetType(), "~/Default.aspx");
					return;
				}

				//Display Table
				F_DisplayDataTable();
			}
		}

		///<summary>
		/// Event handler for the Customer Mode dropdown list selection change.
		/// Handles visibility and read-only properties based on the selected mode ("Create" or "Update").
		///</summary>
		///<param name="sender">The object that raises the event.</param>
		///<param name="e">The event arguments.</param>
		protected void ddlCustomerMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool HasModeSelected = ddlCustomerMode.SelectedValue != "";
			bool UpdateMode = ddlCustomerMode.SelectedValue == "U";

			// Create or Update mode
			txtCustomerUsername.ReadOnly = !HasModeSelected;
			txtCustomerPhoneNumber.ReadOnly = !HasModeSelected;

			// Update Mode
			lblCustomerID.Visible = UpdateMode;
			DrpListCustomerID.Visible = UpdateMode;

			// Clear the DrpList before adding data to it
			GF_ClearItem(DrpListCustomerID);
			if (HasModeSelected)
			{
				GF_PopulateCustCode(DrpListCustomerID);
			}
			else
			{
				GF_ClearInputFeild(Page.Master.FindControl("form1"));
			}
		}

		///<summary>
		/// Event handler for the "Confirm Save" button click.
		/// Validates input data, checks for data existence, and performs either customer creation or update.
		/// Displays success or error messages via modal dialogs.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{
			string FailedMessage = "";
			if (!F_CheckUsernameExistence(out FailedMessage))
			{
				ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Failed", "closeModal(); showErrorModal('Failed', '" + FailedMessage + "');", true);
				return;
			}
			if (!F_CheckPhoneNumberExistence(out FailedMessage))
			{
				ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Failed", "closeModal(); showErrorModal('Failed', '" + FailedMessage + "');", true);
				return;
			}

			string ddlMode = "";
			switch (ddlCustomerMode.SelectedValue)
			{
				case "C":
					ddlMode = "Created";
					F_CreateCustomer();
					break;
				case "U":
					ddlMode = "Updated";
					F_UpdateCustomer();
					break;
				default:
					break;
			}

			string SuccessMessage = $"The Customer {ddlMode} was successful.";
			ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Success", "showModal('Success', '" + SuccessMessage + "');", true);

			// Refresh page
			Response.Redirect("~/FrmCustomerMaintenance.aspx");
		}

		///<summary>
		/// Event handler for the Customer ID dropdown list selection change.
		/// Retrieves and displays customer details (username, phone number, status) based on the selected ID.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		protected void DrpListCustomerID_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (DrpListCustomerID.SelectedValue == "")
			{
				return;
			}

			string WhereClause = $"WHERE [CUST_NO] = '{DrpListCustomerID.SelectedValue}' ";
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new M_Customer(), WhereClause));

			if (dataTable.Rows.Count == 0)
			{
				GF_ReturnErrorMessage("No Revision Found", this.Page, this.GetType());
				return;
			}

			DataRow row = dataTable.Rows[0];
			txtCustomerUsername.Text = row["CUST_NAME"]?.ToString();
			txtCustomerPhoneNumber.Text = row["CUST_PHONE_NUMBER"]?.ToString();
			rbStatus.SelectedValue = row["CUST_STATUS"]?.ToString();
		}

		///<summary>
		/// Event handler for the "Edit Customer" button click.
		/// Retrieves customer details and sets the form in update mode based on the selected customer ID.
		///</summary>
		///<param name="sender">The object that raises the event.</param>
		///<param name="e">The event arguments.</param>
		protected void EditCustomer_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			DrpListCustomerID.SelectedValue = "U"; // Assuming 'U' is the value for update mode
			string WhereClause = $"WHERE [CUST_NO] = '{btn.CommandArgument}'";
			TableDetails tableDetails = F_GetTableDetails(new M_Customer(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails);

			if (dataTable.Rows.Count == 0)
			{
				GF_ReturnErrorMessage("No Revision Found", this.Page, this.GetType());
				return;
			}

			// Change to update mode
			ddlCustomerMode.SelectedValue = "U";
			ddlCustomerMode_SelectedIndexChanged(ddlCustomerMode, EventArgs.Empty);

			// Fill in data to the form
			DataRow row = dataTable.Rows[0];
			txtCustomerUsername.Text = row["CUST_NAME"]?.ToString();
			txtCustomerPhoneNumber.Text = row["CUST_PHONE_NUMBER"]?.ToString();
			rbStatus.SelectedValue = row["CUST_STATUS"]?.ToString();
		}

		///<summary>
		/// Event handler for the "Delete Customer" button click.
		/// Deletes the customer record based on the selected customer ID and refreshes the page.
		///</summary>
		///<param name="sender">The object that raises the event.</param>
		///<param name="e">The event arguments.</param>
		protected void DeleteCustomer_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string WhereClause = $"WHERE [CUST_NO] = '{btn.CommandArgument}'";
			TableDetails tableDetails = F_GetTableDetails(new M_Customer(), WhereClause);
			DB_DeleteData(tableDetails, "[CUST_STATUS]");

			// Refresh page
			Response.Redirect("~/FrmCustomerMaintenance.aspx");
		}

		///<summary>
		/// Displays active customer data in the Repeater control on the page.
		/// Filters out customers with status 'X' (deleted).
		///</summary>
		private void F_DisplayDataTable()
		{
			string WhereClause = "WHERE [CUST_STATUS] != 'X' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Customer(), WhereClause);
			CustMRepeater.DataSource = DB_ReadData(tableDetails);
			CustMRepeater.DataBind();
		}

		///<summary>
		/// Creates a new customer record based on the input data.
		/// Uses a generated customer code and current user information for logging.
		///</summary>
		private void F_CreateCustomer()
		{
			string tempBU = "CS";

			M_Customer m_Customer = new M_Customer
			{
				Cust_No = F_CreateCustomerCode(),
				Cust_Name = txtCustomerUsername.Text,
				Cust_Phone_Number = txtCustomerPhoneNumber.Text,
				Cust_Status = rbStatus.SelectedValue,
				Cust_BU = tempBU,
				Cust_Created_By = G_UserLogin,
				Cust_Modified_By = G_UserLogin,
				Cust_Created_Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Cust_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};

			TableDetails tableDetails = F_GetTableDetails(m_Customer);
			DB_CreateData(tableDetails);
		}

		///<summary>
		/// Updates an existing customer record based on the selected customer ID.
		/// Updates customer details and logs the modification with current user information.
		///</summary>
		private void F_UpdateCustomer()
		{
			M_Customer m_Customer = new M_Customer
			{
				Cust_Name = txtCustomerUsername.Text,
				Cust_Phone_Number = txtCustomerPhoneNumber.Text,
				Cust_Status = rbStatus.SelectedValue,
				Cust_Modified_By = G_UserLogin,
				Cust_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};

			string WhereClause = $"WHERE [CUST_NO] = '{DrpListCustomerID.SelectedValue}' ";
			TableDetails tableDetails = F_GetTableDetails(m_Customer, WhereClause, IsUpdateMethod: true);
			DB_UpdateData(tableDetails);
		}


		/// <summary>
		/// Generates an invoice code containing the full year, month, and a running number.
		/// </summary>
		/// <param name="_tempDate">The date string used to extract the year and month.</param>
		/// <returns>A string representing the invoice code in the format "INVyyyy-MM-NNNN".</returns>
		private string F_CreateCustomerCode()
		{
			string tempBU = "CS";
			string tempPrefix = "CUS";
			int runningNumber = GF_GetRunningNumber(tempBU, tempPrefix);
			DateTime date = DateTime.Now;
			int year = date.Year;
			int month = date.Month;

			string primaryCode = $"{tempPrefix}{year:D4}-{month:D2}-{runningNumber:D4}";
			return primaryCode;
		}

		/// <summary>
		/// Checks if a customer's phone number already exists in the database when in "Create" mode.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if the phone number exists or if there is an issue retrieving data.</param>
		/// <returns>True if the phone number does not exist or if the mode is not "Create"; otherwise, false.</returns>
		private bool F_CheckPhoneNumberExistence(out string _FailedMessage)
		{
			if (ddlCustomerMode.SelectedValue != "C")
			{
				_FailedMessage = "";
				return true;
			}

			//Check value Existence (Customers Phone Number)
			string SelectData = "[CUST_PHONE_NUMBER] ";
			string WhereClause = $"WHERE [CUST_PHONE_NUMBER] = '{txtCustomerPhoneNumber.Text}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Customer(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);
			if (dataTable == null)
			{
				_FailedMessage = "There\'s an issue while receiving the data, Please try again later.";
				return false;
			}
			if (dataTable.Rows.Count > 0)
			{
				_FailedMessage = "The Customer Phone Number already exists. Please try another username.";
				return false;
			}
			_FailedMessage = "";
			return true;
		}

		/// <summary>
		/// Checks if a customer's username already exists in the database when in "Create" mode.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if the username exists or if there is an issue retrieving data.</param>
		/// <returns>True if the username does not exist or if the mode is not "Create"; otherwise, false.</returns>
		private bool F_CheckUsernameExistence(out string _FailedMessage)
		{
			if (ddlCustomerMode.SelectedValue != "C")
			{
				_FailedMessage = "";
				return true;
			}

			//Check value Existence (Customers Username)
			string SelectData = "[CUST_NAME] ";
			string WhereClause = $"WHERE [CUST_NAME] = '{txtCustomerUsername.Text}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Customer(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);
			if (dataTable == null)
			{
				_FailedMessage = "There\'s an issue while receiving the data, Please try again later.";
				return false;
			}
			if (dataTable.Rows.Count > 0)
			{
				_FailedMessage = "The Customer Username already exists. Please try another username.";
				return false;
			}
			_FailedMessage = "";
			return true;
		}
	}
}