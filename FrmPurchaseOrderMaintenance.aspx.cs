using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
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
	public partial class FrmPurchaseOrderMaintenance : System.Web.UI.Page
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
				{ ["V_PoM"] = V_PoM, ["E_PoM"] = E_PoM };
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
		/// Handles the selection change event of ddlPurchaseOrderMode dropdown list.
		/// Depending on selected mode (Create or Update), enables or disables form fields
		/// and populates dropdown lists with relevant data.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ddlPurchaseOrderMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			List<DropDownList> DrpList = new List<DropDownList> { DrpListProjectCode, DrpListPaymentMethod };

			bool HasModeSelected = ddlPurchaseOrderMode.SelectedValue != "";
			bool UpdateMode = ddlPurchaseOrderMode.SelectedValue == "U";
			//Create or Update
			txtPurchaseOrderDate.ReadOnly = !HasModeSelected;
			txtTotalPaidAmount.ReadOnly = !HasModeSelected;
			ChooseFileUpload.Visible = HasModeSelected;
			lblFileUpload.Visible = HasModeSelected;

			//Create
			DrpList.ForEach(GF_ClearItem);
			if (HasModeSelected)
			{
				//Update: Display Project that has PO document
				//Create: Display Project that dont have PO document
				string InOrNotIn = UpdateMode ? "" : "NOT";
				string SelectDocRemark = "(SELECT [DOC_REMARK] FROM [dbo].[T_DOCUMENT] WHERE [DOC_TYPE] = 'PO')";
				string SelectProjNo = $"(SELECT [PROJ_NO] FROM [dbo].[M_PURCHASE_ORDER] WHERE [PO_NO] IN {SelectDocRemark})";
				string WhereClause = $"WHERE [PROJ_STATUS] != 'X' AND [PROJ_NO] {InOrNotIn} IN {SelectProjNo} ";

				GF_PopulateProjCode(DrpListProjectCode, WhereClause);
				GF_PopulateDocNo(DrpListPaymentMethod, new M_Inv_Installment(), "INSTALLMENT_PERIOD");
			}
			else
			{
				GF_ClearInputFeild(Page.Master.FindControl("form1"));
			}
		}

		/// <summary>
		/// Handles the selection change event of DrpListProjectCode dropdown list.
		/// Loads purchase order details into the form fields when a project is selected
		/// in Update mode.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DrpListProjectCode_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool UpdateMode = ddlPurchaseOrderMode.SelectedValue == "U";
			if (!UpdateMode)
			{
				return;
			}
			//Ensure theres a document added recently
			string JoinClause = "JOIN [dbo].[M_PURCHASE_ORDER] PO ON [PO].[PO_NO] = [Obj].[DOC_REMARK] ";
			JoinClause += "JOIN [dbo].[M_INVOICE] INV ON [INV].[PROJ_NO] = [PO].[PROJ_NO] ";
			string WhereClause = $"WHERE [PO].[PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new T_Document(), $"{JoinClause} {WhereClause}"));
			if (dataTable.Rows.Count == 0)
			{
				GF_ClearInputFeild(Page.Master.FindControl("form1"));
				DrpListPaymentMethod.SelectedIndex = 0;
				return;
			}
			DataRow row = dataTable.Rows[0];
			txtPurchaseOrderDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
			txtTotalPaidAmount.Text = row["PO_TOTAL_PAID_AMOUNT"]?.ToString();
			DrpListPaymentMethod.SelectedIndex = int.Parse(row["INV_INSTALLMENT_ID"]?.ToString());
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

			Response.Redirect("~/FrmPurchaseOrderMaintenance.aspx"); // Refresh page
		}

		/// <summary>
		/// Loads purchase order details into the form fields for editing based on the selected document.
		/// Changes the form to Update mode and populates fields with existing data.
		/// </summary>
		/// <param name="sender">The sender object (usually a Button).</param>
		/// <param name="e">The event arguments.</param>
		protected void EditDoc_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string WhereClause = $"WHERE [Obj].[DOC_NO] = '{btn.CommandArgument}' ";
			DataTable dataTable = F_ReceivePOData(WhereClause);
			if (dataTable == null)
			{
				return;

			}
			if (dataTable.Rows.Count == 0)
			{
				return;
			}

			//Change to update mode before assign any value to the form
			ddlPurchaseOrderMode.SelectedValue = "U";
			ddlPurchaseOrderMode_SelectedIndexChanged(ddlPurchaseOrderMode, EventArgs.Empty);

			DataRow row = dataTable.Rows[0];
			txtPurchaseOrderDate.Text = DateTime.Parse(row["DOC_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			rbStatus.SelectedValue = row["DOC_STATUS"]?.ToString();
			txtTotalPaidAmount.Text = row["PO_TOTAL_PAID_AMOUNT"]?.ToString();
			DrpListPaymentMethod.SelectedIndex = int.Parse(row["INV_INSTALLMENT_ID"]?.ToString());
			DrpListProjectCode.SelectedValue = row["PROJ_NO"]?.ToString();
		}



		/// <summary>
		/// Initiates the process of saving a new or updated purchase order based on the selected mode.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{
			string FailedMessage = "";
			if (!F_CheckPOExistence(out FailedMessage))
			{
				ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Failed", "closeModal(); showErrorModal('Failed', '" + FailedMessage + "');", true);
				return;
			}
			string ddlMode = "";
			switch (ddlPurchaseOrderMode.SelectedValue)
			{
				case "U":
					ddlMode = "Updated";
					F_UpdatePO();
					break;
				case "C":
					ddlMode = "Created";
					F_CreatePO();
					break;
				default:
					break;
			}
			string SuccessMessage = $"The Bank Statement {ddlMode} Successfully.";
			ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Modal_Success", "showModal('Success', '" + SuccessMessage + "');", true);
			Response.Redirect("~/FrmPurchaseOrderMaintenance.aspx"); // Refresh page
		}

		/// <summary>
		/// Creates a new purchase order and related document entry in the database.
		/// </summary>
		private void F_CreatePO()
		{
			//Get Primary Code
			string tempPONo = F_CreatePurchaseOrderCode(txtPurchaseOrderDate.Text);
			string tempDocNo = GF_GenerateID("CS", "DOC");

			//Create object for query
			T_Document t_Document = new T_Document()
			{
				Doc_No = tempDocNo,
				Doc_Remark = tempPONo,
				Doc_Revision_No = "-",
				Doc_Date = txtPurchaseOrderDate.Text,
				Doc_Type = "PO",
				Doc_Status = char.Parse(rbStatus.SelectedValue),
				Doc_BU = "CS",
				Doc_Created_By = G_UserLogin,
				Doc_Created_Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Doc_Modified_By = G_UserLogin,
				Doc_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};
			M_Purchase_Order m_Purchase_Order = new M_Purchase_Order()
			{
				PO_No = tempPONo,
				Proj_No = DrpListProjectCode.SelectedValue,
				PO_Total_Paid_Amount = decimal.Parse(txtTotalPaidAmount.Text, CultureInfo.InvariantCulture),

			};
			if (ChooseFileUpload.HasFile)
			{
				//Create the Server Folder Path
				string UploadPath = "~/Documents/Purchase Order/";
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

			//Create Query
			TableDetails PO_TableDetails = F_GetTableDetails(m_Purchase_Order);
			TableDetails Doc_TableDetails = F_GetTableDetails(t_Document);

			//INSERT data into database
			DB_CreateData(PO_TableDetails);
			DB_CreateData(Doc_TableDetails);
			F_UpdatePaymentMethod();
		}

		/// <summary>
		/// Updates an existing purchase order and related document entry in the database.
		/// </summary>
		private void F_UpdatePO()
		{
			//Create object for query
			T_Document t_Document = new T_Document()
			{
				Doc_Date = txtPurchaseOrderDate.Text,
				Doc_Status = char.Parse(rbStatus.SelectedValue),
				Doc_Modified_By = G_UserLogin,
				Doc_Modified_Date = DateTime.Now.ToString("yyyy-MM-dd")
			};
			M_Purchase_Order m_Purchase_Order = new M_Purchase_Order()
			{
				PO_Total_Paid_Amount = decimal.Parse(txtTotalPaidAmount.Text)
			};
			if (ChooseFileUpload.HasFile)
			{
				//Delete the file that upload at this doc No recently
				F_CheckFileExistence(DrpListProjectCode.SelectedValue);

				//Create the Server Folder Path
				string UploadPath = "~/Documents/Purchase Order/";
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
			//WhereClause 
			string PO_WhereClause = $"WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			string DocWhereClause = $"WHERE [DOC_REMARK] = ";
			DocWhereClause += $"(SELECT [PO_NO] FROM [dbo].[M_PURCHASE_ORDER] WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}') ";

			//Create Query
			TableDetails PO_TableDetails = F_GetTableDetails(m_Purchase_Order, PO_WhereClause, IsUpdateMethod: true);
			TableDetails Doc_TableDetails = F_GetTableDetails(t_Document, DocWhereClause, IsUpdateMethod: true);

			//Update data into database
			DB_UpdateData(PO_TableDetails);
			DB_UpdateData(Doc_TableDetails);
			F_UpdatePaymentMethod();
		}

		/// <summary>
		/// Updates the payment method information associated with a purchase order.
		/// </summary>
		private void F_UpdatePaymentMethod()
		{
			//Update the Payment method at Invoice Hdr table
			string WhereClause = $"WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			M_Invoice m_Invoice = new M_Invoice()
			{
				INV_Installment_ID = DrpListPaymentMethod.SelectedIndex
			};
			TableDetails tableDetails = F_GetTableDetails(m_Invoice, WhereClause, IsUpdateMethod: true);
			DB_UpdateData(tableDetails);
		}

		/// <summary>
		/// Displays a list of purchase order data in a repeater control for viewing purposes.
		/// </summary>
		private void F_DisplayDataTable()
		{
			string JoinClause = "JOIN [dbo].[T_DOCUMENT] Doc ON [Doc].[DOC_REMARK] = [Obj].[PO_NO]";
			string WhereClause = "WHERE [Doc].[DOC_TYPE] = 'PO' AND [Doc].[DOC_STATUS] != 'X' ";
			string SelectData = "[Doc].[DOC_NO], [Doc].[DOC_DATE], [Doc].[DOC_STATUS], [Doc].[DOC_UPL_PATH], ";
			SelectData += "[Obj].[PO_TOTAL_PAID_AMOUNT], [Obj].[PROJ_NO] ";
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new M_Purchase_Order(), $"{JoinClause} {WhereClause}"), SelectData);
			POMRepeater.DataSource = dataTable;
			POMRepeater.DataBind();
		}

		/// <summary>
		/// Generates a purchase order code containing the full year and a running number.
		/// </summary>
		/// <param name="_tempDate">The date string used to extract the year.</param>
		/// <returns>A string representing the purchase order code in the format "BSyyyy-NNN".</returns>
		private string F_CreatePurchaseOrderCode(string _tempDate)
		{
			string tempBU = "CS";
			string tempPrefix = "PO";
			int runningNumber = GF_GetRunningNumber(tempBU, tempPrefix);
			DateTime date = DateTime.Parse(_tempDate);
			int year = date.Year;

			string primaryCode = $"{tempPrefix}{year:D4}-{runningNumber:D3}";
			return primaryCode;
		}

		///<summary>
		/// Retrieves Purchase Order data based on the provided Document number.
		///</summary>
		///<param name="_WhereClause">The Document number used to fetch Purchase Order data.</param>
		///<returns>A DataTable containing Purchase Order data.</returns>
		private DataTable F_ReceivePOData(string _WhereClause)
		{
			string JoinClause = "JOIN [dbo].[M_PURCHASE_ORDER] PO ON [PO].[PO_NO] = [Obj].[DOC_REMARK] ";
			JoinClause += "JOIN [dbo].[M_INVOICE] INV ON [INV].[PROJ_NO] = [PO].[PROJ_NO] ";
			TableDetails tableDetails = F_GetTableDetails(new T_Document(), $"{JoinClause} {_WhereClause}");
			return DB_ReadData(tableDetails);
		}

		/// <summary>
		/// Retrieves the URL of a Purchase Order file based on the document number.
		/// </summary>
		/// <param name="_DocNo">Document number used to retrieve the Purchase Order file information.</param>
		/// <returns>
		/// The URL of the Purchase Order file if found; otherwise, returns "#" indicating a dummy link.
		/// </returns>
		protected string F_GetPurchaseOrderFileUrl(string _DocNo)
		{
			string WhereClause = $"WHERE [Obj].[DOC_NO] = '{_DocNo}' ";
			DataTable dataTable = F_ReceivePOData(WhereClause);
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
		/// <param name="_ProjNo">The document revision number to check.</param>
		private void F_CheckFileExistence(string _ProjNo)
		{
			string WhereClause = $"WHERE [Obj].[DOC_REMARK] = (SELECT [DO_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [PROJ_NO] = '{_ProjNo}') ";
			DataTable dataTable = F_ReceivePOData(WhereClause);
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

		/// <summary>
		/// Checks the existence of a Purchase Order (PO) within a specific project.
		/// </summary>
		/// <param name="_FailedMessage">Out parameter to return error messages if the check fails.</param>
		/// <returns>
		/// True if the Purchase Order does not exist in the project and the mode is not "Create" (ddlPurchaseOrderMode.SelectedValue != "C"); otherwise, returns false.
		/// </returns>
		private bool F_CheckPOExistence(out string _FailedMessage)
		{
			if (ddlPurchaseOrderMode.SelectedValue != "C")
			{
				_FailedMessage = "";
				return true;
			}

			//Check value Existence (Purchase Order)
			string SelectData = "[PO_NO] ";
			string WhereClause = $"WHERE [PROJ_NO] = '{DrpListProjectCode.SelectedValue}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Purchase_Order(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);

			if (dataTable == null)
			{
				_FailedMessage = "There\'s an issue while receiving the data, Please try again later.";
				return false;
			}
			if (dataTable.Rows.Count > 0)
			{
				_FailedMessage = "The Purchase Order already exists in this project.";
				return false;
			}
			_FailedMessage = "";
			return true;
		}

	}
}