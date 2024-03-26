using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalVariable;
using OfficeOpenXml;
using System.Data.SqlDoc;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
namespace CUBIC_CIBT_Project
{
	public partial class FrmDocumentMaintenance : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				UserDetails userDetails = JsonConvert.DeserializeObject<UserDetails>(Session["UserDetails"]?.ToString());
				Dictionary<string, HtmlGenericControl> Access = new Dictionary<string, HtmlGenericControl>()
				{ ["E_DocM"] = E_DocM, ["V_DocM"] = V_DocM };
				GF_DisplayWithAccessibility(userDetails.User_Access, Access);
			}
			DataTable dt = new DataTable();
			DocMRepeater.DataSource = dt;
			DocMRepeater.DataBind();
		}
		protected void ddlDocumentMode_SelectedIndexChanged(object sender, EventArgs e)
		{
		
			bool ReadOnly, SetVisible, SetDisplayDrpList, SetVisibleFileType,SetDisplayFile;
			
			List<DropDownList> DrpList = new List<DropDownList>{DrpListDocumentID,DrpListCustomerCode,DrpListProjectCode,DrpListSupplierCode};
			List<TextBox> TextBoxList = new List<TextBox>{ txtDocumentDesc,txtDocumentDate, txtRevisionNo };
			Dictionary<string, string> DrpListDocsType = new Dictionary<string, string>()
			{
				{"", "--Select One--"},
				{ "DO" , "--Delivery Order--"},
				{"QT", "--Quotation--"},
				{"INV", "--Invoice--"}
			};

			//Set Read Only
			ReadOnly = (ddlDocumentMode.SelectedValue == "");
			SetDisplayFile = (ddlDocumentMode.SelectedValue == "C" && DrpListDocumentType.SelectedValue !="");
			TextBoxList.ForEach(txt => txt.ReadOnly = ReadOnly);

			//Set Display File
			lblFileUpload.Visible = SetDisplayFile;
			ChooseFileUpload.Visible = SetDisplayFile;
			//Set Visible
			SetDisplayDrpList = !ReadOnly;
			SetVisible = (ddlDocumentMode.SelectedValue == "U");
			SetVisibleFileType = (ddlDocumentMode.SelectedValue == "C");
			
			
			//Only Display if Update Details
			lblDocumentIDDrpList.Visible = SetVisible;
			DrpListDocumentID.Visible = SetVisible;

			//Only display if Create Document
			lblDocumentTypeDropList.Visible = SetVisibleFileType;
			DrpListDocumentType.Visible = SetVisibleFileType;

			
			//DrpList item setup
			DrpList.ForEach(GF_ClearItem);
			if (SetDisplayDrpList)
			{
				DrpList.ForEach(GF_DrpListAddDefaultItem);
			}


			DrpListDocumentType.Items.Clear();
			if (SetVisibleFileType)
			{
				DrpListDocsType.ForEach(item => DrpListDocumentType.Items.Add(new ListItem(item.Value, item.Key)));
			}

			//Clear Input Feild
			if (ReadOnly) GF_ClearInputFeild(Page.Master.FindControl("form1"));
		}
		protected void BtnConfirmSave_Click(object sender, EventArgs e)
		{

		}

		protected void DrpListDocumentType_SelectedIndexChanged(object sender, EventArgs e)
		{
			var SetVisible = (DrpListDocumentType.SelectedValue != "");
			lblFileUpload.Visible = SetVisible;
			ChooseFileUpload.Visible = SetVisible;
		}
		protected void BtnSave_Click(object sender, EventArgs e)
		{
			if (DrpListCustomerCode.SelectedValue == "" && DrpListSupplierCode.SelectedValue == "")
			{
				GF_ReturnErrorMessage("Document Must be related to either Customer or Supplier.",this.Page,this.GetType());
				return;
			}
			if (DrpListDocumentType.SelectedValue == "")
			{
				GF_ReturnErrorMessage("Please Select the Document Type and Submit a document file.", this.Page, this.GetType());
				return;
			}
			if(DrpListProjectCode.SelectedValue == "")
			{
				GF_ReturnErrorMessage("Document Must be related to a Project.", this.Page, this.GetType());
				return;
			}

			var CheckError = true;
			SqlConnection conn = new SqlConnection(G_ConnectionString);
			conn.Open();
			try
			{
				string SQlSelectCommand = "";
				SqlCommand SQLcmd = new SqlCommand(SQlSelectCommand,conn);
				SqlDataReader dataReader = SQLcmd.ExecuteReader();
				if (!dataReader.HasRows)
				{
					GF_ReturnErrorMessage("Data Not Found", this.Page, this.GetType());
					return;
				} 
				
				while (dataReader.Read())
				{
					
				}
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "BtnSave_Click", ex.ToString());
			}
			finally
			{

			}
			DirectTarget.Attributes["data-bs-target"] = CheckError? "#ConfirmationModalMessage" : "#ErrorModalMessage";
		}

		private bool F_ValidFile()
		{
			if (!ChooseFileUpload.HasFile)
			{
				return false;
			}
			string File = ChooseFileUpload.PostedFile.FileName.ToUpper();
			bool isExcelFile = File.EndsWith(".XLS") || File.EndsWith(".XLSX");
			if (!isExcelFile)
			{
				return false;
			}
			return true;
		}

		private DataTable F_ImportFromExcel()
		{
			DataTable dt = new DataTable();
			if (!F_ValidFile())
			{
				GF_ReturnErrorMessage("No File Found Please Try Again.",this.Page,this.GetType());
				return dt; //Return Empty DataTable
			}

			//Get the uploaded file
			HttpPostedFile uploadedFile = ChooseFileUpload.PostedFile;

			using (var package = new ExcelPackage(uploadedFile.InputStream))
			{
				ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

				// Assuming the first row contains column headers, adjust the starting row index accordingly
				int startRow = worksheet.Dimension.Start.Row;

				// Loop through the rows and columns to read the data
				for (int row = startRow; row <= worksheet.Dimension.End.Row; row++)
				{
					if (row == startRow)
					{
						// Add columns to the DataTable from the header row
						for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
						{
							dt.Columns.Add(worksheet.Cells[row, col].Text);
						}
					}
					else
					{
						// Add data rows to the DataTable
						DataRow dataRow = dt.NewRow();
						for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
						{
							dataRow[col - 1] = worksheet.Cells[row, col].Text;
						}
						dt.Rows.Add(dataRow);
					}
				}
			}


			return dt;
		}

		protected void ProjectCodeDrpList_SelectedIndexChanged(object sender, EventArgs e)
		{

		}
	}
}