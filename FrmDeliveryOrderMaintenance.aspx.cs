using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;
using static CUBIC_CIBT_Project.GlobalVariable;

namespace CUBIC_CIBT_Project
{
	public partial class FrmDeliveryOrderMaintenance : System.Web.UI.Page
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
				{ ["E_DoM"] = E_DoM, ["V_DoM"] = V_DoM };
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
		/// Event handler for the selection change in the revision number dropdown list.
		/// Retrieves and displays delivery order details based on the selected revision number.
		/// </summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		protected void DrpListRevisionNo_SelectedIndexChanged(object sender, EventArgs e)
		{
			string JoinClause = "JOIN [dbo].[M_DELIVERY_ORDER_DET] DO ON [DO].[DOC_NO] = [Obj].[DOC_NO] ";
			string WhereClause = $"WHERE [Obj].[DOC_REVISION_NO] = '{DrpListRevisionNo.SelectedValue}' AND ";
			WhereClause += $"[Obj].[DOC_REMARK] = (SELECT [DO_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}') ";
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new T_Document(), $"{JoinClause} {WhereClause}"));
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			//Fill the data to form
			DataRow row = dataTable.Rows[0];
			txtDeliveryOrderDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			txtArrivalDate.Text = DateTime.Parse(row["DO_ARRIVAL_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
			txtDeliveryAddress.Text = row["DO_ADDRESS"]?.ToString();
		}

		/// <summary>
		/// Event handler for the selection change in the delivery order mode dropdown list.
		/// Adjusts UI elements based on the selected mode (Create or Update).
		/// </summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		protected void ddlDeliveryOrderMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<DropDownList> DrpList = new List<DropDownList> { DrpListProjectCode, DrpListRevisionNo };

			bool HasModeSelected = ddlDeliveryOrderMode.SelectedValue != "";
			bool CreateMode = ddlDeliveryOrderMode.SelectedValue == "C";
			bool UpdateMode = ddlDeliveryOrderMode.SelectedValue == "U";

			//Create or Update Mode
			txtArrivalDate.ReadOnly = !HasModeSelected;
			txtDeliveryAddress.ReadOnly = !HasModeSelected;
			txtDeliveryOrderDate.ReadOnly = !HasModeSelected;
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
					string SelectDocRemark = "(SELECT [DOC_REMARK] FROM [dbo].[T_DOCUMENT] WHERE [DOC_TYPE] = 'DO')";
					string SelectProjNo = $"(SELECT [PROJ_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [DO_NO] IN {SelectDocRemark})";
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
			string SelectDoNo = $"(SELECT [DO_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}')";
			string WhereClause = $"WHERE [DOC_STATUS] != 'X' [DOC_REMARK] IN {SelectDoNo} ";
			GF_PopulateDocNo(DrpListRevisionNo, new T_Document(), "DOC_REVISION_NO", WhereClause);
		}

		/// <summary>
		/// Event handler for the Delete button click on a document.
		/// Deletes the selected document based on the provided document number.
		/// </summary>
		///<param name="sender">The object that raises the event.</param>
		///<param name="e">The event arguments.</param>
		protected void DeleteDoc_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string WhereClause = $"WHERE [DOC_NO] = '{btn.CommandArgument}' ";
			string Status = "DOC_STATUS";
			DB_DeleteData(F_GetTableDetails(new T_Document(), WhereClause), Status);
			Response.Redirect("~/FrmDeliveryOrderMaintenance.aspx"); //Refresh page
		}

		/// <summary>
		/// Event handler for the Edit button click on a document.
		/// Retrieves and populates the form with details of the selected document for editing.
		/// </summary>
		///<param name="sender">The object that raises the event.</param>
		///<param name="e">The event arguments.</param>
		protected void EditDoc_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string WhereClause = $"[Obj].[DOC_NO] = {btn.CommandArgument}";
			DataTable dataTable = F_ReceiveDOData(WhereClause);
			if(dataTable == null)
			{
				return;
			}
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			//Change to update mode before assign any value to the form
			ddlDeliveryOrderMode.SelectedValue = "U";
			ddlDeliveryOrderMode_SelectedIndexChanged(ddlDeliveryOrderMode, EventArgs.Empty);

			//Fill in the value to the form
			DataRow row = dataTable.Rows[0];
			txtDeliveryOrderDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
			txtArrivalDate.Text = row["DO_ARRIVAL_DATE"]?.ToString();
			txtDeliveryAddress.Text = row["DO_ADDRESS"]?.ToString();
			DrpListProjectCode.SelectedValue = row["PROJ_NO"]?.ToString();
		}

		/// <summary>
		/// Event handler for the Save button click.
		/// Validates the form and either creates or updates a delivery order based on the selected mode.
		/// </summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{
			string FailedMessage = "";
			if (!F_CheckRevisionNoExistence(out FailedMessage)){
				ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Failed", "closeModal(); showErrorModal('Failed', '" + FailedMessage + "');", true);
				return;
			}

			string ddlMode = "";
			switch (ddlDeliveryOrderMode.SelectedValue)
			{
				case "U":
					ddlMode = "Updated";
					F_UpdateDO();
					break;
				case "C":
					ddlMode = "Created";
					F_CreateDO();
					break;
				default:
					break;
			}
			string SuccessMessage = $"The Delivery Order {ddlMode} Successfully.";
			ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Success", "showModal('Success', '" + SuccessMessage + "');", true);
			Response.Redirect("~/FrmDeliveryOrderMaintenance.aspx"); //Refresh page
		}

		/// <summary>
		/// Method to create a new delivery order.
		/// Retrieves necessary data, generates document numbers, and saves the data to the database.
		/// </summary>
		private void F_CreateDO()
		{
			//Get DO_NO from M_DELIVERY_ORDER_HDR
			DataTable dataTable = F_GetDONo();
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			DataRow row = dataTable.Rows[0];
			string tempDONo = row["DO_NO"]?.ToString();
			//Get DOC No
			string tempDocNo = GF_GenerateID("CS", "DOC");
			//Create object for query
			T_Document t_Document = new T_Document()
			{
				Doc_No = tempDocNo,
				Doc_Remark = tempDONo,
				Doc_Revision_No = txtRevisionNo.Text,
				Doc_Date = DateTime.Parse(txtDeliveryOrderDate.Text).ToString("yyyy-MM-dd"),
				Doc_Type = "DO",
				Doc_Status = char.Parse(rbStatus.SelectedValue),
				Doc_BU = "CS",
				Doc_Created_By = G_UserLogin,
				Doc_Created_Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Doc_Modified_By = G_UserLogin,
				Doc_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};

			if (ChooseFileUpload.HasFile)
			{
				//Create the Server Folder Path
				string UploadPath = "~/Documents/Delivery Order/";
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

			M_Delivery_Order_Det m_Delivery_Order_Det = new M_Delivery_Order_Det()
			{
				Doc_No = tempDocNo,
				DO_Address = txtDeliveryAddress.Text,
				DO_Arrival_Date = DateTime.Parse(txtArrivalDate.Text).ToString("yyyy-MM-dd")
			};
			//Create Query
			TableDetails DO_TableDetails = F_GetTableDetails(m_Delivery_Order_Det);
			TableDetails Doc_TableDetails = F_GetTableDetails(t_Document);
			//INSERT data into database
			DB_CreateData(Doc_TableDetails);
			DB_CreateData(DO_TableDetails);

		}

		/// <summary>
		/// Method to update an existing delivery order.
		/// Retrieves necessary data, updates the document details, and saves the updated data to the database.
		/// </summary>
		private void F_UpdateDO()
		{
			T_Document t_Document = new T_Document()
			{
				Doc_Date = DateTime.Parse(txtDeliveryOrderDate.Text).ToString("yyyy-MM-dd"),
				Doc_Status = char.Parse(rbStatus.SelectedValue),
				Doc_Modified_By = G_UserLogin,
				Doc_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};

			if (ChooseFileUpload.HasFile)
			{
				//Delete the file that upload at this doc No recently
				F_CheckFileExistence(DrpListRevisionNo.SelectedValue);

				//Create the Server Folder Path
				string UploadPath = "~/Documents/Delivery Order/";
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

			M_Delivery_Order_Det m_Delivery_Order_Det = new M_Delivery_Order_Det()
			{
				DO_Address = txtDeliveryAddress.Text.Replace(',', '|'), //convert the '.' to '|' temporary to avoid any error insertion
				DO_Arrival_Date = DateTime.Parse(txtArrivalDate.Text).ToString("yyyy-MM-dd")
			};

			string DO_SelectClause = $"(SELECT [DO_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}')";
			string DO_WhereClause = $"WHERE [DOC_NO] IN (SELECT [DOC_NO] FROM [dbo].[T_DOCUMENT] WHERE [DOC_REMARK] IN {DO_SelectClause})";
			string Doc_WhereClause = $"WHERE [DOC_REVISION_NO] = '{DrpListRevisionNo.SelectedValue}' AND [DOC_REMARK] = {DO_SelectClause} ";

			//Create Query
			TableDetails DO_TableDetails = F_GetTableDetails(m_Delivery_Order_Det, DO_WhereClause, IsUpdateMethod: true);
			TableDetails Doc_TableDetails = F_GetTableDetails(t_Document, Doc_WhereClause, IsUpdateMethod: true);
			//Avoid the ',' inside the address will get split when it comes in DB_UpdateData
			DB_UpdateData(DO_TableDetails, AvoidSpecialChar: true);
			DB_UpdateData(Doc_TableDetails);
		}

		/// <summary>
		/// Method to retrieve the delivery order number based on the selected project code.
		/// </summary>
		/// <returns>DataTable containing the delivery order number.</returns>
		private DataTable F_GetDONo()
		{
			string WhereClause = $"WHERE [Obj].[PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			string SelectData = "[DO_NO]";
			TableDetails tableDetails = F_GetTableDetails(new M_Delivery_Order_Hdr(), WhereClause);
			return DB_ReadData(tableDetails, SelectData);
		}

		/// <summary>
		/// Method to display data in the DataTable control.
		/// Retrieves delivery order data and binds it to the DataTable for display.
		/// </summary>
		private void F_DisplayDataTable()
		{
			string WhereClause = "WHERE [Doc].[DOC_TYPE] = 'DO' AND [Doc].[DOC_STATUS] != 'X' ";

			//Inner Join T_DOCUMENT, M_DELIVERY_ORDER_DET, M_DELIVERY_ORDER_HDR
			string JoinClause = "JOIN [dbo].[T_DOCUMENT] Doc ON [Doc].[DOC_REMARK] = [Obj].[DO_NO] ";
			JoinClause += "JOIN [dbo].[M_DELIVERY_ORDER_DET] Det ON [Doc].[DOC_NO] = [Det].[DOC_NO] ";

			string SelectData = "[Doc].[DOC_NO], [Doc].[DOC_DATE], [Doc].[DOC_STATUS], [Doc].[DOC_REVISION_NO], [Doc].[DOC_UPL_PATH], ";
			SelectData += "[Det].[DO_ARRIVAL_DATE], [Det].[DO_ADDRESS], ";
			SelectData += "[Obj].[PROJ_NO] ";

			DataTable dataTable = DB_ReadData(F_GetTableDetails(new M_Delivery_Order_Hdr(), $"{JoinClause} {WhereClause}"), SelectData);

			DOMRepeater.DataSource = dataTable;
			DOMRepeater.DataBind();
		}

		///<summary>
		/// Retrieves Delivery Order data based on the provided Document number.
		///</summary>
		///<param name="_WhereClause">The Document number used to fetch Delivery Order data.</param>
		///<returns>A DataTable containing Delivery Order data.</returns>
		private DataTable F_ReceiveDOData(string _WhereClause)
		{
			//Join Document with DO Details table
			string JoinClause = "JOIN [dbo].[M_DELIVERY_ORDER_DET] DET ON [DET].[DOC_NO] = [Obj].[DOC_NO] ";
			JoinClause += "JOIN [dbo].[M_DELIVERY_ORDER_HDR] HDR ON [HDR].[DO_NO] = [Obj].[DOC_REMARK] ";
			return DB_ReadData(F_GetTableDetails(new T_Document(), $"{JoinClause} {_WhereClause}"));
		}

		/// <summary>
		/// Retrieves the URL of a Delivery Order file based on the document number.
		/// </summary>
		/// <param name="_DocNo">Document number used to retrieve the Delivery Order file information.</param>
		/// <returns>
		/// The URL of the Delivery Order file if found; otherwise, returns "#" indicating a dummy link.
		/// </returns>
		protected string F_GetDeliveryOrderFileUrl(string _DocNo)
		{
			string WhereClause = $"[Obj].[DOC_NO] = {_DocNo}";
			DataTable dataTable = F_ReceiveDOData(WhereClause);
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
		/// Checks if a document revision number already exists in the database for a given project when in "Create" mode.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter that holds the error message if the revision number exists or if there is an issue retrieving data.</param>
		/// <returns>True if the revision number does not exist or if the mode is not "Create"; otherwise, false.</returns>
		private bool F_CheckRevisionNoExistence(out string _FailedMessage)
		{
			if(ddlDeliveryOrderMode.SelectedValue != "C")
			{
				_FailedMessage = "";
				return true;
			}
			//Check value Existence (Document Revision No)
			string SelectData = "[DOC_REVISION_NO] ";
			string WhereClause = $"WHERE [DOC_REVISION_NO] = '{txtRevisionNo.Text}' AND [DOC_REMARK] = ";
			WhereClause += $"(SELECT [DO_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}') ";
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
		/// Checks the existence of a file associated with the given document revision number.
		/// Deletes the file from the server if it exists.
		/// </summary>
		/// <param name="_RevisionNo">The document revision number to check.</param>
		private void F_CheckFileExistence(string _RevisionNo)
		{
			string WhereClause = $"WHERE [Obj].[DOC_REVISION_NO] = '{_RevisionNo}' AND ";
			WhereClause += $"[Obj].[DOC_REMARK] = (SELECT [DO_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}') ";
			DataTable dataTable = F_ReceiveDOData(WhereClause);
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
			string FileName = row["DOC_UPL_FILE_NAME"]?.ToString();

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