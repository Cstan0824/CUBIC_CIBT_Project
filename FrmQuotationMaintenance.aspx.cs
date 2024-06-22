using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalVariable;
using System.IO;
using System.Data;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System.Web.UI.HtmlControls;

namespace CUBIC_CIBT_Project
{
	public partial class FrmQuotationMaintenance : System.Web.UI.Page
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
				{ ["E_QoM"] = E_QoM, ["V_QoM"] = V_QoM };
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
		/// Handles the selection change event of DrpListRevisionNo dropdown list.
		/// Loads quotation details into the form fields when a revision number is selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DrpListRevisionNo_SelectedIndexChanged(object sender, EventArgs e)
		{
			string WhereClause = $"WHERE [Obj].[DOC_REVISION_NO] = '{DrpListRevisionNo.SelectedValue}' AND";
			WhereClause += $"[Obj].[DOC_REMARK] = (SELECT [QO_NO] FROM [dbo].[M_QUOTATION] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}') ";

			TableDetails tableDetails = F_GetTableDetails(new T_Document(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails);
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			DataRow row = dataTable.Rows[0];
			txtQuotationDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
		}

		/// <summary>
		/// Handles the selection change event of ddlQuotationMode dropdown list.
		/// Controls the visibility and state of form fields based on selected quotation mode (Create or Update).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ddlQuotationMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<DropDownList> DrpList = new List<DropDownList> { DrpListProjectCode, DrpListRevisionNo };

			bool HasModeSelected = ddlQuotationMode.SelectedValue != "";
			bool CreateMode = ddlQuotationMode.SelectedValue == "C";
			bool UpdateMode = ddlQuotationMode.SelectedValue == "U";

			//Create or Update Mode
			txtQuotationDate.ReadOnly = !HasModeSelected;
			ChooseFileUpload.Visible = HasModeSelected;
			lblFileUpload.Visible = HasModeSelected;

			//Create Mode
			txtRevisionNo.Visible = !HasModeSelected || CreateMode;

			//Update Mode
			DrpListRevisionNo.Visible = UpdateMode;
			txtRevisionNo.ReadOnly = !CreateMode;

			//Clear the DrpList before add the data to it
			DrpList.ForEach(GF_ClearItem);
			if (HasModeSelected)
			{
				string WhereClause = "";
				if (UpdateMode)
				{
					string SelectDocRemark = "(SELECT [DOC_REMARK] FROM [dbo].[T_DOCUMENT] WHERE [DOC_TYPE] = 'QO')";
					string SelectProjNo = $"(SELECT [PROJ_NO] FROM [dbo].[M_QUOTATION] WHERE [QO_NO] IN {SelectDocRemark})";
					WhereClause = $"WHERE [PROJ_STATUS] != 'X' AND [PROJ_NO] IN {SelectProjNo} ";
				}
				GF_PopulateProjCode(DrpListProjectCode, WhereClause);

			}
			else
			{
				GF_ClearInputFeild(Page.Master.FindControl("form1"));
			}

		}

		/// <summary>
		/// Handles the selection change event of DrpListProjectCode dropdown list.
		/// Populates the revision number dropdown list based on the selected project code.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DrpListProjectCode_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool HasModeSelected = DrpListProjectCode.SelectedValue != "";
			GF_ClearItem(DrpListRevisionNo);
			if (!HasModeSelected)
			{
				return;
			}
			string SelectDoNo = $"(SELECT [QO_NO] FROM [dbo].[M_QUOTATION] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}')";
			string WhereClause = $"WHERE [DOC_STATUS] != 'X' AND [DOC_REMARK] IN {SelectDoNo} ";
			GF_PopulateDocNo(DrpListRevisionNo, new T_Document(), "DOC_REVISION_NO", WhereClause);
		}

		/// <summary>
		/// Initiates the process of saving a new or updated quotation based on the selected mode.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{
			string FailedMessage = "";
			if (!F_CheckRevisionNoExistence(out FailedMessage))
			{
				ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Failed", "closeModal(); showErrorModal('Failed', '" + FailedMessage + "');", true);
				return;
			}
			string ddlMode = "";
			switch (ddlQuotationMode.SelectedValue)
			{
				case "U":
					ddlMode = "Update";
					F_UpdateQuotation();
					break;
				case "C":
					ddlMode = "Create";
					F_CreateQuotation();
					break;
				default:
					break;
			}
			string SuccessMessage = $"Quotation {ddlMode} Successfully";
			ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Success", "showModal('Success', '" + SuccessMessage + "');", true);
			Response.Redirect("~/FrmQuotationMaintenance.aspx"); // Refresh page
		}

		/// <summary>
		/// Creates a new quotation and related document entry in the database.
		/// </summary>
		private void F_CreateQuotation()
		{
			string tempBU = "CS";
			DataTable dataTable = F_GetQoNo();
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			DataRow row = dataTable.Rows[0];
			string tempQONo = row["QO_NO"]?.ToString();

			M_Quotation m_Quotation = new M_Quotation()
			{
				QO_No = tempQONo,
				Proj_No = DrpListProjectCode.SelectedValue
			};
			T_Document t_Document = new T_Document()
			{
				Doc_No = GF_GenerateID(tempBU, "DOC"),
				Doc_BU = tempBU,
				Doc_Status = char.Parse(rbStatus.SelectedValue),
				Doc_Revision_No = DrpListRevisionNo.SelectedValue,
				Doc_Remark = tempQONo,
				Doc_Type = "QO",
				Doc_Date = DateTime.Parse(txtQuotationDate.Text).ToString("yyyy-MM-dd"),
				Doc_Modified_By = G_UserLogin,
				Doc_Created_By = G_UserLogin
			};
			if (ChooseFileUpload.HasFile)
			{
				//Create the Server Folder Path
				string UploadPath = "~/Documents/Quotation/";
				string serverFolderPath = Server.MapPath(UploadPath);

				//File Name - generator
				string oriFileName = Path.GetFileName(ChooseFileUpload.FileName);
				string FileName = GF_GenerateUniqueFileIdentifier(oriFileName);

				//Assign it to object for storing to database
				t_Document.Doc_Upl_File_Name = FileName;
				t_Document.Doc_Upl_Path = UploadPath;

				//Save File to server Folder
				ChooseFileUpload.PostedFile.SaveAs(Path.Combine(serverFolderPath, FileName));
			}
			//Create the table details for query
			TableDetails Doc_TableDetails = F_GetTableDetails(t_Document);
			TableDetails QO_TableDetails = F_GetTableDetails(m_Quotation);

			//Create Data
			DB_CreateData(Doc_TableDetails);
			DB_CreateData(QO_TableDetails);
		}

		/// <summary>
		/// Updates an existing quotation and related document entry in the database.
		/// </summary>
		private void F_UpdateQuotation()
		{
			T_Document t_Document = new T_Document()
			{
				Doc_Date = txtQuotationDate.Text,
				Doc_Status = rbStatus.SelectedValue[0],
				Doc_Modified_By = G_UserLogin,
				Doc_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};
			if (ChooseFileUpload.HasFile)
			{
				//Create the Server Folder Path
				string UploadPath = "~/Documents/Quotation/";
				string serverFolderPath = Server.MapPath(UploadPath);

				//File Name - generator
				string oriFileName = Path.GetFileName(ChooseFileUpload.FileName);
				string FileName = GF_GenerateUniqueFileIdentifier(oriFileName);

				//Assign it to object for storing to database
				t_Document.Doc_Upl_File_Name = FileName;
				t_Document.Doc_Upl_Path = UploadPath;

				//Save File to server Folder
				ChooseFileUpload.PostedFile.SaveAs(Path.Combine(serverFolderPath, FileName));
			}
			string DO_SelectClause = $"(SELECT [QO_NO] FROM [dbo].[M_QUOTATION] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}')";
			string WhereClause = $"WHERE [DOC_REVISION_NO] = '{DrpListRevisionNo.SelectedValue}' AND [DOC_REMARK] = {DO_SelectClause}";
			TableDetails tableDetails = F_GetTableDetails(t_Document, WhereClause, IsUpdateMethod: true);
			DB_UpdateData(tableDetails);
		}

		/// <summary>
		/// Deletes a document entry from the database based on the provided document number.
		/// </summary>
		/// <param name="sender">The sender object (usually a Button).</param>
		/// <param name="e">The event arguments.</param>
		protected void DeleteDoc_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string WhereClause = $"WHERE [DOC_NO] = '{btn.CommandArgument}' ";
			string Status = "DOC_STATUS";
			DB_DeleteData(F_GetTableDetails(new T_Document(), WhereClause), Status);
			Response.Redirect("~/FrmQuotationMaintenance.aspx"); // Refresh page
		}

		/// <summary>
		/// Loads quotation details into the form fields for editing based on the selected document.
		/// Changes the form to Update mode and populates fields with existing data.
		/// </summary>
		/// <param name="sender">The sender object (usually a Button).</param>
		/// <param name="e">The event arguments.</param>
		protected void EditDoc_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			//Join Document with DO Details table
			string JoinClause = "JOIN [dbo].[M_QUOTATION] QO ON [QO].[DOC_NO] = [Obj].[DOC_REMARK] ";
			string WhereClause = $"WHERE [DOC_NO] = '{btn.CommandArgument}' ";
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new T_Document(), $"{JoinClause} {WhereClause}"));
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			//Change to update mode before assign any value to the form
			ddlQuotationMode.SelectedValue = "U";
			ddlQuotationMode_SelectedIndexChanged(ddlQuotationMode, EventArgs.Empty);

			//Fill in the value to the form
			DataRow row = dataTable.Rows[0];
			txtQuotationDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
			DrpListProjectCode.SelectedValue = row["PROJ_NO"]?.ToString();
		}

		///<summary>
		/// Event handler for the Download button click, allows user to download the selected Document file.
		///</summary>
		///<param name="sender">The object that raises the event.</param>
		///<param name="e">The event arguments.</param>
		protected void DownloadDoc_Click(object sender, EventArgs e)
		{

		}

		/// <summary>
		/// Displays a list of quotation data in a repeater control for viewing purposes.
		/// </summary
		private void F_DisplayDataTable()
		{
			string WhereClause = "WHERE [Doc].[DOC_TYPE] = 'QO' AND [Doc].[DOC_STATUS] != 'X' ";
			string JoinClause = "JOIN [dbo].[T_DOCUMENT] Doc ON [Doc].[DOC_REMARK] = [Obj].[QO_NO]";
			string SelectData = "[Doc].[DOC_NO], [Doc].[DOC_DATE], [Doc].[DOC_STATUS], [Doc].[DOC_REVISION_NO], ";
			SelectData += "[Obj].[Proj_No] ";
			TableDetails tableDetails = F_GetTableDetails(new M_Quotation(), $"{JoinClause} {WhereClause}");
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);

			QOMRepeater.DataSource = dataTable;
			QOMRepeater.DataBind();
		}

		/// <summary>
		/// Retrieves the quotation number associated with the selected project code.
		/// </summary>
		/// <returns>A DataTable containing the quotation number.</returns>
		private DataTable F_GetQoNo()
		{
			string WhereClause = $"WHERE [Obj].[PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			string SelectData = "[QO_NO]";
			TableDetails tableDetails = F_GetTableDetails(new M_Quotation(), WhereClause);
			return DB_ReadData(tableDetails, SelectData);
		}

		/// <summary>
		/// Checks if a document revision number already exists in the database for a given project when in "Create" mode.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if the revision number exists or if there is an issue retrieving data.</param>
		/// <returns>True if the revision number does not exist or if the mode is not "Create"; otherwise, false.</returns>
		private bool F_CheckRevisionNoExistence(out string _FailedMessage)
		{
			if (ddlQuotationMode.SelectedValue != "C")
			{
				_FailedMessage = "";
				return true;
			}

			//Check value Existence (Document Revision No)
			string SelectData = "[DOC_REVISION_NO] ";
			string WhereClause = $"WHERE [DOC_REVISION_NO] = '{txtRevisionNo.Text}' AND [DOC_REMARK] = ";
			WhereClause += $"(SELECT [DO_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
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
	}
}