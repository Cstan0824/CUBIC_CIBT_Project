using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using WebGrease.Css.Extensions;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;
using static CUBIC_CIBT_Project.GlobalVariable;

namespace CUBIC_CIBT_Project
{
	public partial class FrmBankStatementMaintenance : System.Web.UI.Page
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
				{ ["V_BankStateM"] = V_BankStateM, ["E_BankStateM"] = E_BankStateM };
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
		/// Event handler for the submit button used to update or create bank statements.
		/// Depending on the selected mode (Update or Create), calls corresponding methods and displays success message.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{
			string ddlMode = "";
			switch (ddlBankStateMode.SelectedValue)
			{
				case "U":
					ddlMode = "Updated";
					F_UpdateBankStatement();
					break;
				case "C":
					ddlMode = "Created";
					F_CreateBankStatement();
					break;
				default:
					break;
			}
			string SuccessMessage = $"The Bank Statement {ddlMode} Successfully.";
			ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Success", "showModal('Success', '" + SuccessMessage + "');", true);
			// Refresh page
			Response.Redirect("~/FrmBankStatementMaintenance.aspx");
		}

		/// <summary>
		/// Event handler for the dropdown list selection change event.
		/// Handles mode selection (Create/Update) and adjusts UI elements accordingly.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ddlBankStateMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<DropDownList> DrpList = new List<DropDownList> { DrpListBankType, DrpListRemark };

			bool HasModeSelected = ddlBankStateMode.SelectedValue != "";
			bool UpdateMode = ddlBankStateMode.SelectedValue == "U";

			// Create or Update mode
			txtStatementDate.ReadOnly = !HasModeSelected;
			ChooseFileUpload.Visible = HasModeSelected;
			lblFileUpload.Visible = HasModeSelected;

			// Update mode
			DrpListRemark.Visible = UpdateMode;
			lblRemark.Visible = UpdateMode;

			// Set to display the Drp List
			DrpList.ForEach(GF_ClearItem);
			if (HasModeSelected)
			{
				string JoinClause = "JOIN [dbo].[T_DOCUMENT] Doc ON [Doc].[DOC_REMARK] = [Obj].[BS_NO] ";
				string WhereClause = "WHERE [Doc].[DOC_STATUS] != 'X' ";
				GF_PopulateDocNo(DrpListRemark, new M_Bank_Statement(), "BS_NO", $"{JoinClause} {WhereClause}");
				F_PopulateBankType();
			}
			else
			{
				GF_ClearInputFeild(Page.Master.FindControl("form1"));
			}
		}

		/// <summary>
		/// Event handler for the dropdown list selection change event.
		/// Fills the form with data related to the selected Statement Remark.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DrpListRemark_SelectedIndexChanged(object sender, EventArgs e)
		{
			// JOIN Document table with Bank Statement table to receive document details
			string JoinClause = "JOIN [dbo].[T_Document] Doc ON [Doc].[DOC_REMARK] = [Obj].[BS_NO] ";
			string WhereClause = $"WHERE [Obj].[BS_NO] = '{DrpListRemark.SelectedValue}' ";
			string SelectData = "[Obj].[BS_NO], [Obj].[BS_TYPE_ID], "; // Bank Statement
			SelectData += "[Doc].[DOC_DATE], [Doc].[DOC_STATUS], [Doc].[DOC_UPL_PATH], [Doc].[DOC_UPL_FILE_NAME] "; // Document

			M_Bank_Statement m_Bank_Statement = new M_Bank_Statement();
			TableDetails tableDetails = F_GetTableDetails(m_Bank_Statement, $"{JoinClause} {WhereClause}");
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);

			if (dataTable.Rows.Count == 0)
			{
				GF_ReturnErrorMessage("No Revision Found", this.Page, this.GetType());
				return;
			}
			DataRow row = dataTable.Rows[0];
			txtStatementDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
			DrpListBankType.SelectedValue = row["BS_TYPE_ID"]?.ToString();
		}

		/// <summary>
		/// Populates the Bank Type dropdown list.
		/// </summary>
		private void F_PopulateBankType()
		{
			TableDetails tableDetails = F_GetTableDetails(new M_BS_Type());
			DataTable dataTable = DB_ReadData(tableDetails, "[BS_TYPE_ID], [BANK_TYPE]");
			GF_DrpListAddDefaultItem(DrpListBankType);
			foreach (DataRow row in dataTable.Rows)
			{
				DrpListBankType.Items.Add(new ListItem(row["BANK_TYPE"]?.ToString(), row["BS_TYPE_ID"]?.ToString()));
			}
		}

		/// <summary>
		/// Creates a new bank statement record based on user-filled data.
		/// Generates a unique bank statement code and uploads associated document if chosen.
		/// </summary>
		private void F_CreateBankStatement()
		{
			string tempBSNo = F_CreateBankStatementCode(txtStatementDate.Text);

			// Document Object
			T_Document t_Document = new T_Document()
			{
				Doc_No = GF_GenerateID("CS", "DOC"),
				Doc_Remark = tempBSNo,
				Doc_Desc = "-",
				Doc_Revision_No = "-",
				Doc_Date = txtStatementDate.Text,
				Doc_Type = "BS",
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
				string UploadPath = "~/Documents/Bank Statement/";
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

			M_Bank_Statement m_Bank_Statement = new M_Bank_Statement()
			{
				BS_No = tempBSNo,
				BS_Type_ID = int.Parse(DrpListBankType.SelectedValue)
			};

			// Create the table details for query
			TableDetails Doc_TableDetails = F_GetTableDetails(t_Document);
			TableDetails BS_TableDetails = F_GetTableDetails(m_Bank_Statement);

			// Create Data
			DB_CreateData(Doc_TableDetails);
			DB_CreateData(BS_TableDetails);
		}

		/// <summary>
		/// Updates an existing bank statement record based on user-filled data.
		/// Updates document details and bank statement type.
		/// </summary>
		private void F_UpdateBankStatement()
		{
			// Assign the input form to the object
			T_Document t_Document = new T_Document()
			{
				Doc_Date = txtStatementDate.Text,
				Doc_Status = rbStatus.SelectedValue[0],
				Doc_Modified_By = G_UserLogin,
				Doc_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};

			if (ChooseFileUpload.HasFile)
			{
				//Delete the file that upload at this doc No recently
				F_CheckFileExistence(DrpListRemark.SelectedValue);

				// Create the Server Folder Path
				string UploadPath = "~/Documents/Bank Statement/";
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

			M_Bank_Statement m_Bank_Statement = new M_Bank_Statement()
			{
				BS_Type_ID = int.Parse(DrpListBankType.SelectedValue)
			};

			// Ensures only update the selected RemarkNo 
			string Doc_WhereClause = $"WHERE [DOC_REMARK] = '{DrpListRemark.SelectedValue}' ";
			string BS_WhereClause = $"WHERE [BS_NO] = '{DrpListRemark.SelectedValue}' ";

			// Create the table details for query
			TableDetails Doc_TableDetails = F_GetTableDetails(t_Document, Doc_WhereClause, IsUpdateMethod: true);
			TableDetails BS_TableDetails = F_GetTableDetails(m_Bank_Statement, BS_WhereClause, IsUpdateMethod: true);

			// Update - execute the query
			DB_UpdateData(BS_TableDetails);
			DB_UpdateData(Doc_TableDetails);
		}

		/// <summary>
		/// Deletes the selected document based on its document number.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DeleteDoc_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;

			string WhereClause = $"WHERE [DOC_NO] = '{btn.CommandArgument}' ";
			TableDetails tableDetails = F_GetTableDetails(new T_Document(), WhereClause);
			DB_DeleteData(tableDetails, "[DOC_STATUS]");
			Response.Redirect("~/FrmBankStatementMaintenance.aspx"); // Refresh page
		}

		/// <summary>
		/// Event handler for the Edit button click event.
		/// Sets the dropdown mode to Update and fetches bank statement data based on the selected document.
		/// </summary>
		///<param name="sender">The object that raises the event.</param>
		///<param name="e">The event arguments.</param>
		protected void EditDoc_Click(object sender, EventArgs e)
		{
			// run the Even handler for ddl Mode
			ddlBankStateMode.SelectedValue = "U";
			ddlBankStateMode_SelectedIndexChanged(ddlBankStateMode, EventArgs.Empty);

			Button btn = (Button)sender;
			string WhereClause = $"WHERE [Obj].[DOC_NO] = '{btn.CommandArgument}' ";
			DataTable dataTable = F_ReceiveBSData(WhereClause);
			if (dataTable.Rows.Count == 0)
			{
				GF_ReturnErrorMessage("No Revision Found", this.Page, this.GetType());
				return;
			}
			DataRow row = dataTable.Rows[0];
			txtStatementDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");

			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
			DrpListBankType.SelectedValue = row["BS_TYPE_ID"]?.ToString();
			DrpListRemark.SelectedValue = row["BS_NO"]?.ToString();
		}



		///<summary>
		/// Retrieves Bank Statement data based on the provided Document number.
		///</summary>
		///<param name="_WhereClause">The Document number used to fetch Bank Statement data.</param>
		///<returns>A DataTable containing Bank Statement data.</returns>
		private DataTable F_ReceiveBSData(string _WhereClause)
		{
			string JoinClause = "JOIN [dbo].[M_BANK_STATEMENT] BS ON [Obj].[DOC_REMARK] = [BS].[BS_NO]";
			TableDetails tableDetails = F_GetTableDetails(new T_Document(), $"{JoinClause} {_WhereClause}");
			return DB_ReadData(tableDetails);
		}

		///<summary>
		/// Displays Bank Statement data in the Repeater control on the page.
		///</summary>
		private void F_DisplayDataTable()
		{
			string JoinClause = "JOIN [dbo].[T_DOCUMENT] Doc ON [Doc].[DOC_REMARK] = [Obj].[BS_NO]";
			string WhereClause = "WHERE [Doc].[DOC_TYPE] = 'BS' AND [Doc].[DOC_STATUS] != 'X' ";
			string SelectData = "[Doc].[DOC_NO], [Doc].[DOC_DATE], [Doc].[DOC_STATUS],[Doc].[DOC_UPL_PATH], ";
			SelectData += "[Obj].[BS_NO], [Obj].[BS_TYPE_ID] ";
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new M_Bank_Statement(), $"{JoinClause} {WhereClause}"), SelectData);
			DocMRepeater.DataSource = dataTable;
			DocMRepeater.DataBind();
		}

		/// <summary>
		/// Retrieves the URL of a bank statement file based on the document number.
		/// </summary>
		/// <param name="_DocNo">Document number used to retrieve the bank statement file information.</param>
		/// <returns>
		/// The URL of the bank statement file if found; otherwise, returns "#" indicating a dummy link.
		/// </returns>
		protected string F_GetBankStatementFileUrl(string _DocNo)
		{
			string WhereClause = $"WHERE [Obj].[DOC_NO] = '{_DocNo}' ";
			DataTable dataTable = F_ReceiveBSData(WhereClause);
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

		///<summary>
		/// Retrieves the Bank Type based on the provided Bank Statement Type ID.
		///</summary>
		///<param name="_BSTypeID">The Bank Statement Type ID used to fetch Bank Type.</param>
		///<returns>The Bank Type as a string.</returns>
		protected string F_DisplayBankType(int _BSTypeID)
		{
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new M_BS_Type()));
			if (dataTable.Rows.Count == 0)
			{
				return null;
			}
			DataRow row = dataTable.Rows[_BSTypeID - 1];
			return row["BANK_TYPE"]?.ToString();
		}

		/// <summary>
		/// Generates a bank statement code containing the full year, month, and a running number.
		/// </summary>
		/// <param name="_tempDate">The date string used to extract the year and month.</param>
		/// <returns>A string representing the bank statement code in the format "BSyyyy-MM-NNNN".</returns>
		private string F_CreateBankStatementCode(string _tempDate)
		{
			string tempBU = "CS";
			string tempPrefix = "BS";
			int runningNumber = GF_GetRunningNumber(tempBU, tempPrefix);
			DateTime date = DateTime.Parse(_tempDate);
			int year = date.Year;
			int month = date.Month;

			string primaryCode = $"{tempPrefix}{year:D4}-{month:D2}-{runningNumber:D4}";
			return primaryCode;
		}

		/// <summary>
		/// Checks the existence of a file associated with the given document Remark.
		/// Deletes the file from the server if it exists.
		/// </summary>
		/// <param name="_RevisionNo">The document Remark to check.</param>
		private void F_CheckFileExistence(string _Remark)
		{
			string WhereClause = $"[Obj].[DOC_REMARK] = '{_Remark}'";
			DataTable dataTable = F_ReceiveBSData(WhereClause);
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
	}
}