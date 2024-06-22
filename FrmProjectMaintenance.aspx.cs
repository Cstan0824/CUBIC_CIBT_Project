using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalVariable;
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;
using System.Data;
using System.Globalization;


namespace CUBIC_CIBT_Project
{

	public partial class FrmProjectMaintenance : System.Web.UI.Page
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

				//Authenticate and Authorize access
				Dictionary<string, HtmlGenericControl> Access = new Dictionary<string, HtmlGenericControl>()
				{ ["E_ProjM"] = E_ProjM };
				bool HasAccess = GF_DisplayWithAccessibility(userDetails.User_Access, Access);
				if (!HasAccess)
				{
					GF_ReturnErrorMessage("You dont have access to this page, kindly look for adminstration.", this.Page, this.GetType(), "~/Default.aspx");
					return;
				}

				//Get From FrmProjectListing.aspx
				string projNo = Request.QueryString["ProjNo"];
				if (!string.IsNullOrEmpty(projNo))
				{
					F_LoadProjectDetails(projNo);
				}
			}
		}

		/// <summary>
		/// Handles changes in the selected project mode (Create or Update) and updates the UI accordingly.
		/// Enables or disables input fields and populates dropdown lists based on the selected mode.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ddlProjectMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<DropDownList> DrpList = new List<DropDownList> { DrpListCustomerCode, DrpListProjectCode };

			bool HasModeSelected = ddlProjectMode.SelectedValue != "";
			bool UpdateMode = ddlProjectMode.SelectedValue == "U";

			// Create or Update mode
			txtProjectDate.ReadOnly = !HasModeSelected;
			txtProjectName.ReadOnly = !HasModeSelected;

			// Update mode
			lblProjectCodeTxt.Visible = UpdateMode;
			DrpListProjectCode.Visible = UpdateMode;

			// Clear the DrpList before adding data to it
			DrpList.ForEach(GF_ClearItem);
			if (UpdateMode)
			{
				string WhereClause = "WHERE [PROJ_STATUS] != 'X' ";
				// Only display and populate in Update Mode
				GF_PopulateProjCode(DrpListProjectCode, WhereClause);
			}
			if (HasModeSelected)
			{
				GF_PopulateCustCode(DrpListCustomerCode);
			}
			else
			{
				GF_ClearInputFeild(Page.Master.FindControl("form1"));
			}
		}

		/// <summary>
		/// Handles the confirmation and saving of project details.
		/// Checks for required fields, then either creates or updates the project based on the selected mode.
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
			switch (ddlProjectMode.SelectedValue)
			{
				case "U":
					ddlMode = "Updated";
					F_UpdateProj();
					break;
				case "C":
					string tempProjNo = F_CreateProjectCode(txtProjectDate.Text);
					ddlMode = "Created";
					F_CreateProj(tempProjNo);
					F_CreateDocumentHdrs(tempProjNo);
					break;
				default:
					break;
			}

			string SuccessMessage = $"The Project {ddlMode} Successfully.";
			ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Success", "showModal('Success', '" + SuccessMessage + "');", true);
			Response.Redirect("~/FrmProjectMaintenance.aspx"); // Refresh page
		}

		/// <summary>
		/// Handles the selection change of the project code dropdown list.
		/// Populates the form fields with data from the selected project.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ProjectCodeDrpList_SelectedIndexChanged(object sender, EventArgs e)
		{
			string WhereClause = $"WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Project_Master(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails);
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			// Fill in data to the form
			DataRow row = dataTable.Rows[0];
			txtProjectName.Text = row["PROJ_NAME"]?.ToString();
			txtProjectDate.Text = DateTime.Parse(row["PROJ_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			DrpListCustomerCode.SelectedValue = row["CUST_NO"]?.ToString();
			rbStatus.SelectedValue = row["PROJ_STATUS"]?.ToString();
		}

		/// <summary>
		/// Updates the project details in the database with the current form values.
		/// Constructs the SQL update query and executes it based on the project number.
		/// </summary>
		private void F_UpdateProj()
		{
			M_Project_Master proj_master = new M_Project_Master
			{
				// Project Details
				Cust_No = DrpListCustomerCode.SelectedValue,
				Proj_Name = txtProjectName.Text,
				Proj_Date = txtProjectDate.Text,
				Proj_Status = rbStatus.SelectedValue[0],
				Proj_Modified_By = G_UserLogin,
				Proj_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};
			string WhereClause = $"WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			TableDetails tableDetails = F_GetTableDetails(proj_master, WhereClause, IsUpdateMethod: true);
			DB_UpdateData(tableDetails);
		}

		/// <summary>
		/// Creates a new project in the database with the current form values.
		/// Constructs the SQL insert query and executes it, including generating a new project number.
		/// </summary>
		/// <param name="_tempProjNo">The temporary project number generated for the new project.</param>
		private void F_CreateProj(string _tempProjNo)
		{
			string tempBU = "CS";
			M_Project_Master proj_master = new M_Project_Master
			{
				// Project Details
				Proj_No = _tempProjNo,
				Cust_No = DrpListCustomerCode.SelectedValue,
				Proj_Name = txtProjectName.Text,
				Proj_Date = txtProjectDate.Text,
				Proj_BU = tempBU,
				Proj_Status = rbStatus.SelectedValue[0],
				Proj_Created_By = G_UserLogin,
				Proj_Modified_By = G_UserLogin
			};

			TableDetails tableDetails = F_GetTableDetails(proj_master);
			DB_CreateData(tableDetails);
		}

		/// <summary>
		/// Creates document headers (Quotation, Delivery Order, Invoice) for a new project.
		/// Generates codes for each document type and inserts records into the database.
		/// </summary>
		/// <param name="_tempProjNo">The project number for which document headers are created.</param>
		private void F_CreateDocumentHdrs(string _tempProjNo)
		{
			// Create QO Header
			M_Quotation m_Quotation = new M_Quotation
			{
				QO_No = F_CreateQuotationCode(txtProjectDate.Text),
				Proj_No = _tempProjNo
			};
			TableDetails QO_TableDetails = F_GetTableDetails(m_Quotation);
			DB_CreateData(QO_TableDetails);

			// Create DO Header
			M_Delivery_Order_Hdr m_Delivery_Order_Hdr = new M_Delivery_Order_Hdr
			{
				DO_No = F_CreateDeliveryOrderCode(txtProjectDate.Text),
				Proj_No = _tempProjNo
			};
			TableDetails DO_TableDetails = F_GetTableDetails(m_Delivery_Order_Hdr);
			DB_CreateData(DO_TableDetails);

			// Create Invoice Header
			M_Invoice m_Invoice = new M_Invoice
			{
				INV_No = F_CreateInvoiceCode(txtProjectDate.Text),
				Proj_No = _tempProjNo,
				INV_Installment_ID = 4, // Full paid by default
				INV_Balance_Amount = 0
			};
			TableDetails INV_TableDetails = F_GetTableDetails(m_Invoice);
			DB_CreateData(INV_TableDetails);
		}

		/// <summary>
		/// Generates a project code containing the last two digits of the year.
		/// </summary>
		/// <param name="_tempDate">The date string used to extract the year.</param>
		/// <returns>A string representing the project code in the format "CBSyyNNN-ProjectName".</returns>
		private string F_CreateProjectCode(string _tempDate)
		{
			string tempBU = "CS";
			string tempPrefix = "CBS";
			int runningNumber = GF_GetRunningNumber(tempBU, tempPrefix);
			DateTime date = DateTime.Parse(_tempDate);
			int year = date.Year % 100; // Get last 2 digits of the year

			string primaryCode = $"{tempPrefix}{year:D2}{runningNumber:D3}";
			return primaryCode;
		}

		/// <summary>
		/// Generates an invoice code containing the full year, month, and a running number.
		/// </summary>
		/// <param name="_tempDate">The date string used to extract the year and month.</param>
		/// <returns>A string representing the invoice code in the format "INVyyyy-MM-NNNN".</returns>
		private string F_CreateInvoiceCode(string _tempDate)
		{
			string tempBU = "CS";
			string tempPrefix = "INV";
			int runningNumber = GF_GetRunningNumber(tempBU, tempPrefix);
			DateTime date = DateTime.Parse(_tempDate);
			int year = date.Year;
			int month = date.Month;

			string primaryCode = $"{tempPrefix}{year:D4}-{month:D2}-{runningNumber:D4}";
			return primaryCode;
		}

		/// <summary>
		/// Generates a delivery order code containing the full year, month, and a running number.
		/// </summary>
		/// <param name="_tempDate">The date string used to extract the year and month.</param>
		/// <returns>A string representing the delivery order code in the format "BSyyyy-MM-NNNNN".</returns>
		private string F_CreateDeliveryOrderCode(string _tempDate)
		{
			string tempBU = "CS";
			string tempPrefix = "BS";
			int runningNumber = GF_GetRunningNumber(tempBU, tempPrefix);
			DateTime date = DateTime.Parse(_tempDate);
			int year = date.Year;
			int month = date.Month;

			string primaryCode = $"{tempPrefix}{year:D4}-{month:D2}-{runningNumber:D5}";
			return primaryCode;
		}

		/// <summary>
		/// Generates a bank statement code containing the full year, month, and a running number.
		/// </summary>
		/// <param name="_tempDate">The date string used to extract the year and month.</param>
		/// <returns>A string representing the bank statement code in the format "QOyyyy-MM-NNNN".</returns>
		private string F_CreateQuotationCode(string _tempDate)
		{
			string tempBU = "CS";
			string tempPrefix = "QO";

			int RunningNumber = GF_GetRunningNumber(tempBU, tempPrefix);
			DateTime Date = DateTime.Parse(_tempDate);
			int year = Date.Year;
			int month = Date.Month;
			string PrimaryCode = $"{tempPrefix}{year:D4}-{month:D2}-{RunningNumber:D4}";
			return PrimaryCode;
		}

		/// <summary>
		/// Loads project details into the form for editing based on the provided project number.
		/// Changes the form to Update mode and populates fields with existing data.
		/// </summary>
		/// <param name="_tempProjNo">The project number to load details for.</param>
		private void F_LoadProjectDetails(string _tempProjNo)
		{
			//Change to Update mode
			ddlProjectMode.SelectedValue = "U";
			ddlProjectMode_SelectedIndexChanged(ddlProjectMode, EventArgs.Empty);

			string WhereClause = $"WHERE [PROJ_NO] = '{_tempProjNo}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Project_Master(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails);
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			//Fill in data to the form
			DataRow row = dataTable.Rows[0];
			txtProjectName.Text = row["PROJ_NAME"]?.ToString();
			txtProjectDate.Text = DateTime.Parse(row["PROJ_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			DrpListCustomerCode.SelectedValue = row["CUST_NO"]?.ToString();
			rbStatus.SelectedValue = row["PROJ_STATUS"]?.ToString();

			DrpListProjectCode.SelectedValue = _tempProjNo;
		}

		/// <summary>
		/// Checks if a Project's name already exists in the database when in "Create" mode.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if the name exists or if there is an issue retrieving data.</param>
		/// <returns>True if the name does not exist or if the mode is not "Create"; otherwise, false.</returns>
		private bool F_CheckUsernameExistence(out string _FailedMessage)
		{
			if (ddlProjectMode.SelectedValue != "C")
			{
				_FailedMessage = "";
				return true;
			}

			//Check value Existence (Project name)
			string SelectData = "[PROJ_NAME] ";
			string WhereClause = $"WHERE [PROJ_NAME] = '{txtProjectName.Text}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Customer(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);
			if (dataTable == null)
			{
				_FailedMessage = "There\'s an issue while receiving the data, Please try again later.";
				return false;
			}
			if (dataTable.Rows.Count > 0)
			{
				_FailedMessage = "The Project name already exists. Please try another username.";
				return false;
			}
			_FailedMessage = "";
			return true;
		}
	}
}