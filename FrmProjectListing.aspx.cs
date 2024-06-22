using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalVariable;
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;
using Microsoft.Ajax.Utilities;

namespace CUBIC_CIBT_Project
{
	public partial class FrmProjectListing : System.Web.UI.Page
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
				{ ["V_ProjR"] = V_ProjR };
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
		/// Handles the deletion of a project.
		/// Deletes the project data from the database and refreshes the project listing page.
		/// </summary>
		/// <param name="sender">The event source (the delete button).</param>
		/// <param name="e">The event data.</param>
		protected void DeleteProject_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string WhereClause = $"WHERE [PROJ_NO] = '{btn.CommandArgument}' ";
			TableDetails tableDetails = F_GetTableDetails(new M_Project_Master(), WhereClause);
			DB_DeleteData(tableDetails, "[PROJ_STATUS]");

			Response.Redirect("~/FrmProjectListing.aspx"); // Refresh page
		}

		/// <summary>
		/// Handles the editing of a project.
		/// Redirects users to the project maintenance page for editing.
		/// </summary>
		/// <param name="sender">The event source (the edit button).</param>
		/// <param name="e">The event data.</param>
		protected void EditProject_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			string ProjNo = btn.CommandArgument.ToString();
			// Redirect users to the edit document page 
			Response.Redirect($"~/FrmProjectMaintenance.aspx?ProjNo={HttpUtility.UrlEncode(ProjNo)}");
		}

		/// <summary>
		/// Displays the project data table.
		/// Retrieves and binds project data to the repeater control for display.
		/// </summary>
		private void F_DisplayDataTable()
		{
			string WhereClause = "WHERE [PROJ_STATUS] != 'X' ";
			DataTable dataTable = DB_ReadData(F_GetTableDetails(new M_Project_Master(), WhereClause));
			ProjMRepeater.DataSource = dataTable;
			ProjMRepeater.DataBind();
		}

	}
}