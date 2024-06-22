using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalVariable;
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;
using System.Data;
using System.Net.NetworkInformation;
using System.Globalization;
using Microsoft.Ajax.Utilities;
using System.Web.UI.HtmlControls;
namespace CUBIC_CIBT_Project
{
	public partial class FrmProjectListingDet : System.Web.UI.Page
	{
		//HTTP: GET method 
		private readonly string ProjNo = HttpContext.Current.Request.QueryString["ProjNo"];
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
				{ ["V_ProjR"] = V_ProjR };
				bool HasAccess = GF_DisplayWithAccessibility(userDetails.User_Access, Access);
				if (!HasAccess)
				{
					GF_ReturnErrorMessage("You dont have access to this page, kindly look for adminstration.", this.Page, this.GetType(), "~/Default.aspx");
					return;
				}

				//Display Table and Project Details
				F_DisplayDataTable();
				F_DisplayProjectDetails();
			}
		}


		/// <summary>
		/// Handles the deletion of a document when the Delete button is clicked.
		/// Deletes the document from the database based on the document number.
		/// </summary>
		/// <param name="sender">The source of the event, typically the Delete button.</param>
		/// <param name="e">Event data.</param>
		protected void DeleteDoc_Click(object sender, EventArgs e)
		{
			T_Document t_Document = new T_Document();
			Button btn = (Button)sender;
			string WhereClause = $"WHERE [DOC_NO] = '{btn.CommandArgument}' ";
			TableDetails tableDetails = F_GetTableDetails(t_Document, WhereClause);
			DB_DeleteData(tableDetails, "[DOC_STATUS]");

			Response.Redirect("~/FrmProjectListingDet.aspx"); //Refresh page
		}

		/// <summary>
		/// Redirects the user to the appropriate document maintenance page when the Edit button is clicked.
		/// Constructs the URL based on the document type and the document number.
		/// </summary>
		/// <param name="sender">The source of the event, typically the Edit button.</param>
		/// <param name="e">Event data.</param>
		protected void EditDoc_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string DocNo = btn.CommandArgument.ToString();
			Dictionary<string, string> FrmDocumentMaintenance = new Dictionary<string, string>()
			{
				["PO"] = "FrmPurchaseOrderMaintenance.aspx",
				["DO"] = "FrmDeliveryOrderMaintenance.aspx",
				["QO"] = "FrmQuotationMaintenance.aspx",
				["INV"] = "FrmInvoiceMaintenance.aspx"
			};
			string currentDocument = DocRemark.SelectedValue;
			Response.Redirect($"~/{FrmDocumentMaintenance[currentDocument]}?DocNo={DocNo}");
		}

		/// <summary>
		/// Initiates the download process for a document when the Download button is clicked.
		/// The actual implementation of the download mechanism is required based on the storage system.
		/// </summary>
		/// <param name="sender">The source of the event, typically the Download button.</param>
		/// <param name="e">Event data.</param>
		protected void DownloadDoc_Click(object sender, EventArgs e)
		{
			// Implementation for document download goes here.
		}

		/// <summary>
		/// Updates the displayed data table based on the selected document remark.
		/// Refreshes the list of documents when the selected document type changes.
		/// </summary>
		/// <param name="sender">The source of the event, typically the DocRemark dropdown list.</param>
		/// <param name="e">Event data.</param>
		protected void DocRemark_SelectedIndexChanged(object sender, EventArgs e)
		{
			F_DisplayDataTable();
		}

		/// <summary>
		/// Constructs and executes a SQL query to retrieve documents based on the selected document remark.
		/// Binds the retrieved data to the `DocMRepeater` control for display.
		/// </summary>
		private void F_DisplayDataTable()
		{
			string WhereClause = "WHERE [DOC_REMARK] IN ";
			switch (DocRemark.SelectedValue)
			{
				case "INV":
					WhereClause += $"(SELECT [INV_NO] FROM [dbo].[M_INVOICE] WHERE [PROJ_NO] = '{ProjNo}')";
					break;
				case "DO":
					WhereClause += $"(SELECT [DO_NO] FROM [dbo].[M_DELIVERY_ORDER_HDR] WHERE [PROJ_NO] = '{ProjNo}')";
					break;
				case "PO":
					WhereClause += $"(SELECT [PO_NO] FROM [dbo].[M_PURCHASE_ORDER] WHERE [PROJ_NO] = '{ProjNo}')";
					break;
				case "QO":
					WhereClause += $"(SELECT [QO_NO] FROM [dbo].[M_QUOTATION] WHERE [PROJ_NO] = '{ProjNo}')";
					break;
				default:
					break;
			}

			DataTable dt = DB_ReadData(F_GetTableDetails(new T_Document(), WhereClause));
			DocMRepeater.DataSource = dt;
			DocMRepeater.DataBind();
		}

		/// <summary>
		/// Retrieves and displays detailed project information including financial data and customer information.
		/// Uses project number to query the database and populate project details on the UI.
		/// </summary>
		private void F_DisplayProjectDetails()
		{
			string WhereClause = $"WHERE [PROJ_NO] = '{ProjNo}' ";
			string JoinClause = "JOIN [dbo].[M_CUSTOMER] CUST ON [CUST].[CUST_NO] = [Obj].[CUST_NO] ";
			TableDetails tableDetails = F_GetTableDetails(new M_Project_Master(), $"{JoinClause} {WhereClause}");
			DataTable dataTable = DB_ReadData(tableDetails);
			if (dataTable.Rows.Count == 0)
			{
				return;
			}
			DataRow row = dataTable.Rows[0];
			decimal TotalPaidAmount = F_GetTotalPaidAmount();
			decimal ReceivedAmount = F_GetReceivedAmount();
			decimal ReceiableAmount = TotalPaidAmount - ReceivedAmount;

			lblProjectDate.Text = DateTime.Parse(row["PROJ_DATE"]?.ToString()).ToString("yyyy-MM-dd");
			lblProjectName.Text = row["PROJ_NAME"]?.ToString();
			lblProjectNo.Text = row["PROJ_NO"]?.ToString();
			lblReceiableAmount.Text = $"RM{ReceiableAmount}";
			lblReceivedAmount.Text = $"RM{ReceivedAmount}";
			lblCustomerName.Text = row["CUST_NAME"]?.ToString();
			lblCustomerPhoneNumber.Text = row["CUST_PHONE_NUMBER"]?.ToString();
			lblProjectCreatedBy.Text = row["PROJ_CREATED_BY"]?.ToString();
			lblProjectCreatedDate.Text = DateTime.Parse(row["PROJ_CREATED_DATE"]?.ToString()).ToString("yyyy-MM-dd");

			hiddProjStatus.Value = row["PROJ_STATUS"]?.ToString();
			hiddReceivedAmount.Value = ReceivedAmount.ToString();
			hiddTotalPaidAmount.Value = TotalPaidAmount.ToString();
		}

		/// <summary>
		/// Calculates and returns the total received amount for the project.
		/// Queries the database for invoice balance amounts related to the project.
		/// </summary>
		/// <returns>Total received amount as a decimal value.</returns>
		private decimal F_GetReceivedAmount()
		{
			string WhereClause = $"WHERE [Obj].[PROJ_NO] = '{ProjNo}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Invoice(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, "[INV_BALANCE_AMOUNT]");
			if (dataTable.Rows.Count == 0)
			{
				return 0;
			}
			DataRow row = dataTable.Rows[0];
			decimal ReceiableAmount = decimal.Parse(row["INV_BALANCE_AMOUNT"]?.ToString(), CultureInfo.InvariantCulture);
			return ReceiableAmount;
		}

		/// <summary>
		/// Calculates and returns the total paid amount for the project.
		/// Queries the database for purchase order total paid amounts related to the project.
		/// </summary>
		/// <returns>Total paid amount as a decimal value.</returns>
		private decimal F_GetTotalPaidAmount()
		{
			string WhereClause = $"WHERE [Obj].[PROJ_NO] = '{ProjNo}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Purchase_Order(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, "[PO_TOTAL_PAID_AMOUNT]");
			if (dataTable.Rows.Count == 0)
			{
				return 0;
			}
			DataRow row = dataTable.Rows[0];
			decimal TotalPaidAmount = decimal.Parse(row["PO_TOTAL_PAID_AMOUNT"]?.ToString(), CultureInfo.InvariantCulture);
			return TotalPaidAmount;
		}

	}
}