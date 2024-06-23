using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
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
	public partial class FrmInvoiceMaintenance : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				// Redirect to login page if user dont have loged in session
				if (G_UserLogin.IsNullOrWhiteSpace() || Session["UserDetails"] == null)
				{
					GF_ReturnErrorMessage("Please Login to the account before use access the content.", this.Page, this.GetType(), "~/Frmlogin.aspx");
					return;
				}
				//Get User Details from Server Session
				UserDetails userDetails = GF_GetSession(Session["UserDetails"]?.ToString());

				//Authenticate and Authorize access
				Dictionary<string, HtmlGenericControl> Access = new Dictionary<string, HtmlGenericControl>()
				{ ["E_InvM"] = E_InvM, ["V_InvM"] = V_InvM };
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

		/// <summary>
		/// Event handler for the selection change in the invoice mode dropdown list.
		/// Adjusts UI elements and populates project codes based on the selected mode (Create or Update).
		/// </summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		protected void ddlInvoiceMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<DropDownList> DrpList = new List<DropDownList> { DrpListProjectCode, DrpListRevisionNo };
			List<TextBox> txtBox = new List<TextBox> { txtInvoiceDate, txtPaidAmount, txtRevisionNo };
			bool HasModeSelected = ddlInvoiceMode.SelectedValue != "";
			bool UpdateMode = ddlInvoiceMode.SelectedValue == "U";
			bool CreateMode = ddlInvoiceMode.SelectedValue == "C";

			// Create or Update Mode
			txtBox.ForEach(txt => txt.ReadOnly = !HasModeSelected);
			lblFileUpload.Visible = HasModeSelected;
			ChooseFileUpload.Visible = HasModeSelected;

			// Create Mode
			txtRevisionNo.Visible = !HasModeSelected || CreateMode;

			// Update Mode
			DrpListRevisionNo.Visible = UpdateMode;

			// Clear the DrpList before adding data to it
			DrpList.ForEach(GF_ClearItem);
			if (HasModeSelected)
			{
				string WhereClause = "WHERE [PROJ_STATUS] != 'X'";
				if (UpdateMode)
				{
					string SelectDocRemark = "(SELECT [DOC_REMARK] FROM [dbo].[T_DOCUMENT] WHERE [DOC_TYPE] = 'INV')";
					string SelectProjNo = $"(SELECT [PROJ_NO] FROM [dbo].[M_INVOICE] WHERE [INV_NO] IN {SelectDocRemark})";
					WhereClause += $" AND [PROJ_NO] IN {SelectProjNo} ";
				}
				GF_PopulateProjCode(DrpListProjectCode, WhereClause);
			}
			else
			{
				GF_ClearInputFeild(Page.Master.FindControl("form1"));
			}
		}

		/// <summary>
		/// Event handler for the Save button click.
		/// Validates the form and either creates or updates an invoice based on the selected mode.
		/// </summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{
			string FailedMessage = "";
			if (!F_CheckRevisionNoExistence(out FailedMessage))
			{
				ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Failed", "closeModal(); showErrorModal('Failed', '" + FailedMessage + "');", true);
				return;
			}
			if (!F_CheckBalanceAmount(out FailedMessage))
			{
				ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Failed", "closeModal(); showErrorModal('Failed', '" + FailedMessage + "');", true);
				return;
			}
			string ddlMode = "";
			switch (ddlInvoiceMode.SelectedValue)
			{
				case "U":
					ddlMode = "Updated";
					F_UploadInv();
					break;
				case "C":
					ddlMode = "Created";
					F_CreateInv();
					break;
				default:
					break;
			}
			string SuccessMessage = $"The Invoice {ddlMode} Successfully.";
			ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Success", "showModal('Success', '" + SuccessMessage + "');", true);
			Response.Redirect("~/FrmInvoiceMaintenance.aspx"); //Refresh page
		}

		/// <summary>
		/// Event handler for the selection change in the project code dropdown list.
		/// Populates the revision number dropdown list based on the selected project code.
		/// </summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		protected void DrpListProjectCode_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool HasModeSelected = DrpListProjectCode.SelectedValue != "";
			GF_ClearItem(DrpListRevisionNo);
			if (!HasModeSelected)
			{
				return;
			}
			string WhereClause = "WHERE [DOC_STATUS] != 'X' AND [DOC_REMARK] = ";
			WhereClause += $"(SELECT [INV_NO] FROM [dbo].[M_INVOICE] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}') ";
			GF_PopulateDocNo(DrpListRevisionNo, new T_Document(), "DOC_REVISION_NO", WhereClause);
		}

		/// <summary>
		/// Event handler for the selection change in the revision number dropdown list.
		/// Retrieves and displays invoice details based on the selected revision number.
		/// </summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		protected void DrpListRevisionNo_SelectedIndexChanged(object sender, EventArgs e)
		{
			string WhereClause = $"WHERE [DOC_REVISION_NO] = '{DrpListRevisionNo.SelectedValue}' AND ";
			WhereClause += $"[DOC_REMARK] = (SELECT [INV_NO] FROM [dbo].[M_INVOICE] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}') ";
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new T_Document(), WhereClause));
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			DataRow row = dataTable.Rows[0];
			txtInvoiceDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
		}

		/// <summary>
		/// Method to update an existing invoice.
		/// Uploads the invoice file, updates document details, and saves the updated data to the database.
		/// </summary>
		private void F_UploadInv()
		{
			T_Document t_Document = new T_Document()
			{
				Doc_Date = txtInvoiceDate.Text,
				Doc_Status = char.Parse(rbStatus.SelectedValue),
				Doc_Modified_By = G_UserLogin,
				Doc_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};
			if (ChooseFileUpload.HasFile)
			{
				//Delete the file that upload at this doc No recently
				F_CheckFileExistence(DrpListRevisionNo.SelectedValue);

				// Create the Server Folder Path
				string UploadPath = "~/Documents/Invoice/";
				string serverFolderPath = Server.MapPath(UploadPath);

				// File Name - generator
				string oriFileName = Path.GetFileName(ChooseFileUpload.FileName);
				string FileName = GF_GenerateUniqueFileIdentifier(oriFileName);

				// Assign it to object for storing to database
				t_Document.Doc_Upl_File_Name = FileName;
				t_Document.Doc_Upl_Path = UploadPath;

				// Save File to server Folder
				ChooseFileUpload.PostedFile.SaveAs(Path.Combine(serverFolderPath, FileName));
			}

			// Where Clause
			string WhereClause = "WHERE [DOC_REVISION_NO] = ";
			WhereClause += $"(SELECT [INV_NO] FROM [dbo].[M_INVOICE] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}')";

			// Create Query
			TableDetails tableDetails = F_GetTableDetails(t_Document, WhereClause, IsUpdateMethod: true);
			// Update Data to database
			F_UpdateBalanceAmount();
			DB_UpdateData(tableDetails);
		}

		/// <summary>
		/// Method to create a new invoice.
		/// Retrieves necessary data, generates document numbers, and saves the data to the database.
		/// </summary>
		private void F_CreateInv()
		{
			// Get NO
			string tempDocNo = GF_GenerateID("CS", "DOC");
			DataTable dataTable = F_GetPONo();
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			DataRow row = dataTable.Rows[0];
			string tempINVNo = row["INV_NO"]?.ToString();
			// Create object for query
			T_Document t_Document = new T_Document()
			{
				Doc_No = tempDocNo,
				Doc_Remark = tempINVNo,
				Doc_Revision_No = txtRevisionNo.Text,
				Doc_Date = txtInvoiceDate.Text,
				Doc_Type = "INV",
				Doc_Status = char.Parse(rbStatus.SelectedValue),
				Doc_BU = "CS",
				Doc_Created_By = G_UserLogin,
				Doc_Created_Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Doc_Modified_By = G_UserLogin,
				Doc_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};
			if (ChooseFileUpload.HasFile)
			{
				// Create the Server Folder Path
				string UploadPath = "~/Documents/Invoice/";
				string serverFolderPath = Server.MapPath(UploadPath);

				// File Name - generator
				string oriFileName = Path.GetFileName(ChooseFileUpload.FileName);
				string FileName = GF_GenerateUniqueFileIdentifier(oriFileName);

				// Assign it to object for storing to database
				t_Document.Doc_Upl_File_Name = FileName;
				t_Document.Doc_Upl_Path = UploadPath;

				// Save File to server Folder
				ChooseFileUpload.PostedFile.SaveAs(Path.Combine(serverFolderPath, FileName));
			}
			// Update the balance
			F_UpdateBalanceAmount();
			// Create Query
			TableDetails tableDetails = F_GetTableDetails(t_Document);
			// Insert data to database
			DB_CreateData(tableDetails);
		}

		/// <summary>
		/// Method to retrieve the invoice number based on the selected project code.
		/// </summary>
		/// <returns>DataTable containing the invoice number.</returns>
		private DataTable F_GetPONo()
		{
			string WhereClause = $"WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			string SelectData = "[INV_NO]";

			TableDetails tableDetails = F_GetTableDetails(new M_Invoice(), WhereClause);
			return DB_ReadData(tableDetails, SelectData);
		}

		/// <summary>
		/// Method to update the balance amount for the selected project code.
		/// Adds the paid amount to the current balance amount in the database.
		/// </summary>
		private void F_UpdateBalanceAmount()
		{
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				decimal paidAmount = decimal.Parse(txtPaidAmount.Text, CultureInfo.InvariantCulture);
				string SQlUpdateCommand = $"UPDATE [dbo].[M_INVOICE] ";
				SQlUpdateCommand += $"SET [INV_BALANCE_AMOUNT] = [INV_BALANCE_AMOUNT] + {paidAmount.ToString(CultureInfo.InvariantCulture)} ";
				SQlUpdateCommand += $"WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}';";
				SqlCommand SQLCmd = new SqlCommand(SQlUpdateCommand, Conn);
				SQLCmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "F_UpdateBalanceAmount", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}

		/// <summary>
		/// Method to display the invoice data in a repeater control.
		/// Retrieves and binds the data to the control based on the provided criteria.
		/// </summary>
		private void F_DisplayDataTable()
		{
			string WhereClause = "WHERE [Doc].[DOC_TYPE] = 'INV' AND [Doc].[DOC_STATUS] != 'X' ";
			string JoinClause = "JOIN [dbo].[T_DOCUMENT] Doc ON [Doc].[DOC_REMARK] = [Obj].[INV_NO]";
			string SelectData = "[Doc].[DOC_NO], [Doc].[DOC_REVISION_NO], [Doc].[DOC_DATE], [Doc].[DOC_STATUS], [Doc].[DOC_UPL_PATH], ";
			SelectData += "[Obj].[PROJ_NO], [Obj].[INV_NO] ";
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new M_Invoice(), $"{JoinClause} {WhereClause}"), SelectData);
			INVMRepeater.DataSource = dataTable;
			INVMRepeater.DataBind();
		}

		/// <summary>
		/// Event handler for the delete document button click.
		/// Deletes the selected document from the database and refreshes the page.
		/// </summary>
		///<param name="sender">The object that raises the event.</param>
		///<param name="e">The event arguments.</param>
		protected void DeleteDoc_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string WhereClause = $"WHERE [DOC_NO] = '{btn.CommandArgument}' ";
			string Status = "DOC_STATUS";
			DB_DeleteData(F_GetTableDetails(new T_Document(), WhereClause), Status);
			Response.Redirect("~/FrmInvoiceMaintenance.aspx"); //Refresh page
		}

		/// <summary>
		/// Event handler for the edit document button click.
		/// Retrieves and displays the document details for editing and sets the form to update mode.
		/// </summary>
		///<param name="sender">The object that raises the event.</param>
		///<param name="e">The event arguments.</param>
		protected void EditDoc_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string WhereClause = $"WHERE [DOC_NO] = '{btn.CommandArgument}' ";

			DataTable dataTable = F_ReceiveINVData(WhereClause);
			if (dataTable == null)
			{
				return;
			}
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			// Change to update mode before assigning any value to the form
			ddlInvoiceMode.SelectedValue = "U";
			ddlInvoiceMode_SelectedIndexChanged(ddlInvoiceMode, EventArgs.Empty);

			DataRow row = dataTable.Rows[0];
			txtInvoiceDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
		}

		///<summary>
		/// Retrieves Invoice data based on the provided Document number.
		///</summary>
		///<param name="_WhereClause">The Document number used to fetch Invoice data.</param>
		///<returns>A DataTable containing Invoice data.</returns>
		private DataTable F_ReceiveINVData(string _WhereClause)
		{
			TableDetails tableDetails = F_GetTableDetails(new T_Document(), _WhereClause);
			return DB_ReadData(tableDetails);
		}

		/// <summary>
		/// Retrieves the URL of a Invoice file based on the document number.
		/// </summary>
		/// <param name="_DocNo">Document number used to retrieve the Invoice file information.</param>
		/// <returns>
		/// The URL of the Invoice file if found; otherwise, returns "#" indicating a dummy link.
		/// </returns>
		protected string F_GetInvoiceFileUrl(string _DocNo)
		{
			string WhereClause = $"WHERE [DOC_NO] = '{_DocNo}' ";

			DataTable dataTable = F_ReceiveINVData(WhereClause);
			// Return a dummy link if the file is not found
			if (dataTable == null)
			{
				return "#";
			}
			if (dataTable.Rows.Count == 0)
			{
				return "#";
			}
			DataRow row = dataTable.Rows[0];
			string fileName = row["DOC_UPL_FILE_NAME"]?.ToString();
			string virtualPath = Path.Combine(row["DOC_UPL_PATH"]?.ToString(), fileName);
			return ResolveUrl(virtualPath);
		}

		/// <summary>
		/// Determines whether to show a link based on the provided document upload path.
		/// </summary>
		/// <param name="_DocUplPath">The document upload path to evaluate.</param>
		/// <returns>True if the document upload path is null or empty; otherwise, false.</returns>
		protected bool F_ShowLink(string _DocUplPath)
		{
			return string.IsNullOrEmpty(_DocUplPath);
		}

		/// <summary>
		/// Checks the existence of a file associated with the given document revision number.
		/// Deletes the file from the server if it exists.
		/// </summary>
		/// <param name="_RevisionNo">The document revision number to check.</param>
		private void F_CheckFileExistence(string _RevisionNo)
		{
			string WhereClause = $"WHERE [Obj].[DOC_REVISION_NO] = '{_RevisionNo}' AND ";
			WhereClause += $"[Obj].[DOC_REMARK] = (SELECT [DO_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}') ";
			DataTable dataTable = F_ReceiveINVData(WhereClause);
			if (dataTable == null)
			{
				return;
			}
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			DataRow row = dataTable.Rows[0];
			string UplPath = row["DOC_UPL_PATH"]?.ToString();
			string FileName = row["DOC_UPL_FILE_NAME"].ToString();
			string VirtualPath = Path.Combine(UplPath, FileName);
			string AbslutePath = Server.MapPath(VirtualPath);
			if (string.IsNullOrEmpty(UplPath))
			{
				return;
			}
			if (!File.Exists(AbslutePath))
			{
				return;
			}
			GF_DeleteFileFromServer(AbslutePath);
		}

		/// <summary>
		/// Checks if a document revision number already exists in the database for a given project when in "Create" mode.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if the revision number exists or if there is an issue retrieving data.</param>
		/// <returns>True if the revision number does not exist or if the mode is not "Create"; otherwise, false.</returns>
		private bool F_CheckRevisionNoExistence(out string _FailedMessage)
		{
			if (ddlInvoiceMode.SelectedValue != "C")
			{
				_FailedMessage = "";
				return true;
			}

			//Check value Existence (Document Revision No)
			string SelectData = "[DOC_REVISION_NO] ";
			string WhereClause = $"WHERE [DOC_REVISION_NO] = '{txtRevisionNo.Text}' AND [DOC_REMARK] = ";
			WhereClause += $"(SELECT [INV_NO] FROM [dbo].[M_INVOICE] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}') ";
			TableDetails tableDetails = F_GetTableDetails(new T_Document(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);

			if (dataTable == null)
			{
				_FailedMessage = "There\'s an issue while receiving the data, Please try again later.";
				return false;
			}
			if (dataTable.Rows.Count > 0)
			{
				_FailedMessage = "The Revision No already exists in this project. Please try another Revision No.";
				return false;
			}
			_FailedMessage = "";
			return true;
		}

		/// <summary>
		/// Validates if the balance amount is sufficient for the payment amount specified in `txtPaidAmount`.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if validation fails.</param>
		/// <returns>True if the balance amount is valid and sufficient; otherwise, false.</returns>
		private bool F_CheckBalanceAmount(out string _FailedMessage)
		{
			decimal TotalPaidAmount = F_GetTotalPaidAmount(out _FailedMessage);
			if (TotalPaidAmount == -1)
			{
				return false;
			}

			decimal ReceivedAmount = F_GetReceivedAmount(out _FailedMessage);
			if (ReceivedAmount == -1)
			{
				return false;
			}
			//Ensure the Amount haven't balanced
			decimal ReceiableAmount = TotalPaidAmount - ReceivedAmount;
			if (ReceiableAmount <= 0)
			{
				_FailedMessage = "The Total Paid Amount has already Balanced.";
				return false;
			}
			//Ensure the Balanced amount doesnt exceed the limit after receive the amount.
			decimal ToPaidAmount = decimal.Parse(txtPaidAmount.Text, CultureInfo.InvariantCulture);
			if (ReceiableAmount - ToPaidAmount < 0)
			{
				_FailedMessage = $"The To Paid Amount has already exceed the Balanced Amount, You only require {ReceiableAmount - ToPaidAmount} To finish the payment.";
				return false;
			}
			_FailedMessage = "";
			return true;
		}

		/// <summary>
		/// Retrieves the total paid amount for a specific project.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if data retrieval fails.</param>
		/// <returns>The total paid amount as a decimal. Returns -1 if there is an error.</returns>
		private decimal F_GetTotalPaidAmount(out string _FailedMessage)
		{
			string SelectData = "[PO_TOTAL_PAID_AMOUNT] ";
			string WhereClause = $"WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Purchase_Order(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);

			if (dataTable == null)
			{
				_FailedMessage = "There\'s an issue while receiving the data, Please try again later.";
				return -1;
			}
			if (dataTable.Rows.Count == 0)
			{
				_FailedMessage = "Kindly Create the PO document before create the invoices.";
				return -1;
			}
			DataRow row = dataTable.Rows[0];
			_FailedMessage = "";
			return decimal.Parse(row["PO_TOTAL_PAID_AMOUNT"]?.ToString(), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Retrieves the received amount (balance amount) for invoices related to a specific project.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if data retrieval fails.</param>
		/// <returns>The received amount as a decimal. Returns -1 if there is an error.</returns>
		private decimal F_GetReceivedAmount(out string _FailedMessage)
		{
			string WhereClause = $"WHERE [Obj].[PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Invoice(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, "[INV_BALANCE_AMOUNT]");
			if (dataTable == null)
			{
				_FailedMessage = "There\'s an issue while receiving the data, Please try again later.";
				return -1;
			}
			if (dataTable.Rows.Count == 0)
			{
				_FailedMessage = "There\'s an issue while receiving the data, Please try again later.";
				return -1;
			}
			DataRow row = dataTable.Rows[0];
			_FailedMessage = "";
			return decimal.Parse(row["INV_BALANCE_AMOUNT"]?.ToString(), CultureInfo.InvariantCulture);
		}
	}
}