using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CUBIC_CIBT_Project
{
	public class GlobalVariable
	{


		public static string G_ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CUBIC_CIBT;Max Pool Size=200";

		public string G_LogIn
		{
			get; set;
		}


		public static void GF_CheckConnectionStatus(SqlConnection _TempConn)
		{
			try
			{
				if (_TempConn.State == ConnectionState.Open)
				{
					_TempConn.Dispose();
					_TempConn.Close();
				}
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "GF_CheckConnectionStatus", ex.ToString().Replace("'", ""));
			}
		}

		public static void GF_InsertAuditLog(string _AuditLog, string _AuditType, string _AuditForm, string _AuditDesc, string _AuditRemark)
		{
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			Conn.Open();

			string SQLInsertCommand = "INSERT INTO Audit_Log VALUES('{_AuditLog}');";
			SqlCommand SQLCmd = new SqlCommand(SQLInsertCommand, Conn);
			SQLCmd.ExecuteNonQuery();

			Conn.Dispose();
			Conn.Close();
		}

		public static string GF_HashPassword(string _Password)
		{
			byte[] Salt;
			new RNGCryptoServiceProvider().GetBytes(Salt = new byte[16]);

			var pbkdf2 = new Rfc2898DeriveBytes(_Password, Salt, 10000);
			byte[] Hash = pbkdf2.GetBytes(20);

			byte[] HashBytes = new byte[36];
			Array.Copy(Salt, 0, HashBytes, 0, 16);
			Array.Copy(Hash, 0, HashBytes, 16, 20);

			return Convert.ToBase64String(HashBytes);
		}

		public static void GF_ReturnErrorMessage(string _Message, Page _Page,Type _Type)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<script type='text/javascript'>" +
				"window.onload= () => {alert('" +
				_Message +
				"')}" +
				"</script>");
			_Page.ClientScript.RegisterClientScriptBlock(_Type, "alert", sb.ToString());
		}
		public static void GF_ClearInputFeild(Control control)
		{
			control.Controls.OfType<TextBox>().ToList().ForEach(txtBox => txtBox.Text = string.Empty);
			control.Controls.OfType<Control>().Where(c => c.Controls.Count > 0).ToList().ForEach(child => GF_ClearInputFeild(child));
		}

		public static void GF_DrpListAddDefaultItem(DropDownList droplist)
		{
			droplist.Items.Add(new ListItem("--Select One--", ""));
		}

		public static void GF_ClearItem(DropDownList droplist)
		{
			droplist.Items.Clear();
		}
	}
}