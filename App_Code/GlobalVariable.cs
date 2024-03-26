using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;

namespace CUBIC_CIBT_Project
{
	public class GlobalVariable
	{


		public static string G_ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CUBIC_CIBT;Max Pool Size=200";
		public static string G_UserBU {  get; set; }
		public static string G_UserLogin{ get; set; }
		public static string G_Username {  get; set; }
		public static  List<string> G_Accessibility {  get; set; }
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
		public static void GF_InsertAuditLog(string _AuditCode, string _AuditType, string _AuditForm, string _AuditDesc, string _AuditRemark)
		{
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			Conn.Open();

			string SQLInsertCommand = "INSERT INTO [dbo].[AUDIT_LOG]";
	
			SQLInsertCommand += "([AUDIT_BU],";
			SQLInsertCommand += "[AUDIT_CODE],";
			SQLInsertCommand += "[AUDIT_TYPE],";
			SQLInsertCommand += "[AUDIT_FORM],";
			SQLInsertCommand += "[AUDIT_DESC],";
			SQLInsertCommand += "[AUDIT_REMARK],";
			SQLInsertCommand += "[AUDIT_CREATE_BY],";
			SQLInsertCommand += "[AUDIT_CREATE_DATE])";

			SQLInsertCommand += "VALUES(";
			SQLInsertCommand += $"'{G_UserBU}',";
			SQLInsertCommand += $"'{_AuditCode}',";
			SQLInsertCommand += $"'{_AuditType}',";
			SQLInsertCommand += $"'{_AuditForm}',";
			SQLInsertCommand += $"'{_AuditDesc}',";
			SQLInsertCommand += $"'{_AuditRemark}',";
			SQLInsertCommand += $"'{G_UserLogin}',";
			SQLInsertCommand += "GETDATE());";

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
		public static void GF_ClearInputFeild(Control _control)
		{
			_control.Controls.OfType<TextBox>().ToList().ForEach(txtBox => txtBox.Text = string.Empty);
			_control.Controls.OfType<Control>().Where(c => c.Controls.Count > 0).ToList().ForEach(child => GF_ClearInputFeild(child));
		}
		public static void GF_DrpListAddDefaultItem(DropDownList _droplist)
		{
			_droplist.Items.Add(new ListItem("--Select One--", ""));
		}
		public static void GF_ClearItem(DropDownList _droplist)
		{
			_droplist.Items.Clear();
		}
		public static int GF_GetRunningNumber(string _tempBU,string _tempPrefix)
		{
			int RunningNum = 0;
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SqlSelectCommand = "SELECT T_SYS_NEXT1 FROM [dbo].[SYSRUN_BU] ";
				SqlSelectCommand += $"WHERE T_SYS_PREFIX = {_tempPrefix} AND T_SYS_BU = {_tempBU}";

				SqlCommand SqlCmd = new SqlCommand(SqlSelectCommand,Conn);
				SqlDataReader DataReader = SqlCmd.ExecuteReader();
				if (!DataReader.HasRows)
				{
					return 0;
				}
				DataReader.Read();
				RunningNum = (int)DataReader["T_SYS_NEXT1"];
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "GF_DrpListAddDefaultItem", ex.ToString().Replace("'", ""));
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
				GF_UpdateRunningNumber(_tempBU, _tempPrefix);
			}
			return RunningNum;
		}
		public static void GF_UpdateRunningNumber(string _tempBU, string _tempPrefix)
		{
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SqlUpdateCommand = "Update[dbo].[SYSRUN_BU] Set [T_SYS_Next1] = [T_SYS_Next1] + 1 ";
				SqlUpdateCommand += $"WHERE [T_SYS_PREFIX] = {_tempPrefix} AND [T_SYS_BU] = {_tempBU} ";

				SqlCommand SQLCmd = new SqlCommand(SqlUpdateCommand, Conn);
				SQLCmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "GF_DrpListAddDefaultItem", ex.ToString().Replace("'", ""));
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}
		public static void GF_UpdateLogData(char _isLogin, string _empNo)
		{
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLUpdateCommand = "UPDATE [T_EMPLOYEE] EMP";
				SQLUpdateCommand += "JOIN [T_EMP_LOGIN] LOG ON [EMP].[EMP_LOG_ID] = [LOG].[EMP_LOG_ID]";
				SQLUpdateCommand += $"SET [LOG].[EMP_ISLOGIN] = '{_isLogin}' WHERE [EMP].[EMP_NO] = '{_empNo}';";
				SqlCommand SQLCmd = new SqlCommand(SQLUpdateCommand, Conn);
				SQLCmd.ExecuteNonQuery();

			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "F_UpdateLoginData", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}
		public static bool GF_GetSession(string _Session,string _Cookie)
		{
			return _Cookie.Equals(JsonConvert.DeserializeObject<UserDetails>(_Session).TokenSessionID); 
		}
		public static void GF_DisplayWithAccessibility<T>(List<string> _CurrentAccess, Dictionary<List<string>, T> _PageAccess) where T : Control
		{
			_PageAccess.ForEach(access =>
			{
				access.Key.ForEach(key =>
				{
					if (_CurrentAccess.Contains(key))
					{
						access.Value.Visible = true;
					}
				});
			});
		}
		public static void GF_DisplayWithAccessibility<T>(List<string> _CurrentAccess, Dictionary<string, T> _Access) where T : Control
		{
			_Access.ForEach(access =>
			{
				if (_CurrentAccess.Contains(access.Key))
				{
					access.Value.Visible = true;
				}
			});
		}

	}
}