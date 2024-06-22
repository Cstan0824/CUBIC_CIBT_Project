using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;

namespace CUBIC_CIBT_Project
{
	public class GlobalVariable
	{
		public static string G_ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CUBIC_CIBT_PROJECT;Max Pool Size=200";
		public static string G_UserBU { get; set; }
		public static string G_UserLogin { get; set; }
		public static string G_Username { get; set; }

		/// <summary>
		/// Ensures the specified SqlConnection is closed and disposed.
		/// </summary>
		/// <param name="_TempConn">The SqlConnection to check and close.</param>
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
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "GF_CheckConnectionStatus", ex.ToString());
			}
		}

		/// <summary>
		/// Inserts an audit log entry into the AUDIT_LOG table.
		/// </summary>
		/// <param name="_AuditCode">The audit code.</param>
		/// <param name="_AuditType">The type of audit.</param>
		/// <param name="_AuditForm">The form or source of the audit.</param>
		/// <param name="_AuditDesc">The description of the audit.</param>
		/// <param name="_AuditRemark">Additional remarks for the audit.</param>
		public static void GF_InsertAuditLog(string _AuditCode, string _AuditType, string _AuditForm, string _AuditDesc, string _AuditRemark)
		{
			using (SqlConnection Conn = new SqlConnection(G_ConnectionString))
			{
				Conn.Open();

				string SQLInsertCommand = "INSERT INTO [dbo].[AUDIT_LOG] ";
				SQLInsertCommand += "([AUDIT_CODE], [AUDIT_TYPE], [AUDIT_FORM], [AUDIT_DESC], [AUDIT_REMARK], [AUDIT_BU], [AUDIT_CREATED_BY], [AUDIT_CREATED_DATE]) ";
				SQLInsertCommand += "VALUES (@AuditCode, @AuditType, @AuditForm, @AuditDesc, @AuditRemark, @AuditBU, @AuditCreatedBy, GETDATE());";

				using (SqlCommand SQLCmd = new SqlCommand(SQLInsertCommand, Conn))
				{
					SQLCmd.Parameters.AddWithValue("@AuditCode", _AuditCode);
					SQLCmd.Parameters.AddWithValue("@AuditType", _AuditType);
					SQLCmd.Parameters.AddWithValue("@AuditForm", _AuditForm);
					SQLCmd.Parameters.AddWithValue("@AuditDesc", _AuditDesc);
					SQLCmd.Parameters.AddWithValue("@AuditRemark", _AuditRemark);
					SQLCmd.Parameters.AddWithValue("@AuditBU", G_UserBU ?? "-");
					SQLCmd.Parameters.AddWithValue("@AuditCreatedBy", G_UserLogin ?? "-");

					SQLCmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Hashes the given password using PBKDF2 with a random salt.
		/// </summary>
		/// <param name="_Password">The password to hash.</param>
		/// <returns>The hashed password as a base64 string.</returns>
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

		/// <summary>
		/// Displays an error message to the user using JavaScript alert.
		/// </summary>
		/// <param name="_Message">The error message to display.</param>
		/// <param name="_Page">The current web page context.</param>
		/// <param name="_Type">The type of the calling page or control.</param>
		public static void GF_ReturnErrorMessage(string message, Page page, Type type, string redirectUrl = null)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<script type='text/javascript'>");
			sb.Append("window.onload = function() {");
			sb.Append("alert('").Append(message).Append("');");

			if (!string.IsNullOrEmpty(redirectUrl))
			{
				sb.Append("window.location.href = '").Append(page.ResolveUrl(redirectUrl)).Append("';");
			}
			sb.Append("};");
			sb.Append("</script>");
			page.ClientScript.RegisterClientScriptBlock(type, "alert", sb.ToString());
		}

		/// <summary>
		/// Recursively clears text from all TextBox controls within the specified control.
		/// </summary>
		/// <param name="_control">The control containing TextBox elements to clear.</param>
		public static void GF_ClearInputFeild(Control _control)
		{
			_control.Controls.OfType<TextBox>().ToList().ForEach(txtBox => txtBox.Text = string.Empty);
			_control.Controls.OfType<Control>().Where(c => c.Controls.Count > 0).ToList().ForEach(child => GF_ClearInputFeild(child));
		}

		/// <summary>
		/// Adds a default item "--Select One--" to the specified DropDownList.
		/// </summary>
		/// <param name="_droplist">The DropDownList to modify.</param>
		public static void GF_DrpListAddDefaultItem(DropDownList _droplist)
		{
			_droplist.Items.Add(new ListItem("--Select One--", ""));
		}

		/// <summary>
		/// Clears all items from the specified DropDownList.
		/// </summary>
		/// <param name="_droplist">The DropDownList to clear.</param>
		public static void GF_ClearItem(DropDownList _droplist)
		{
			_droplist.Items.Clear();
		}

		/// <summary>
		/// Retrieves the next running number from the system settings for the given business unit and prefix.
		/// </summary>
		/// <param name="_tempBU">The business unit identifier.</param>
		/// <param name="_tempPrefix">The prefix for the running number.</param>
		/// <returns>The next running number as an integer.</returns>
		public static int GF_GetRunningNumber(string _tempBU, string _tempPrefix)
		{
			int RunningNumber = 0;
			string SelectData = "[T_SYS_NEXT]";
			string WhereClause = $"WHERE [T_SYS_PREFIX] = '{_tempPrefix}'";
			WhereClause += $" AND [T_SYS_BU] = '{_tempBU}'";
			WhereClause += $"AND [T_SYS_STATUS] = 'O'";
			TableDetails tableDetails = F_GetTableDetails(new T_SysRun_No(), WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, SelectData);
			if (dataTable == null)
			{
				return 0;
			}
			if (dataTable.Rows.Count == 0)
			{
				return 0;
			}
			DataRow row = dataTable.Rows[0];
			RunningNumber = int.Parse(row["T_SYS_NEXT"]?.ToString());

			//Update the Running Number after receive the data
			GF_UpdateRunningNumber(_tempBU, _tempPrefix);

			return RunningNumber;
		}

		/// <summary>
		/// Updates the running number in the system settings for the given business unit and prefix.
		/// </summary>
		/// <param name="_tempBU">The business unit identifier.</param>
		/// <param name="_tempPrefix">The prefix for the running number.</param>
		private static void GF_UpdateRunningNumber(string _tempBU, string _tempPrefix)
		{
			string WhereClause = $"WHERE [T_SYS_PREFIX] = '{_tempPrefix}' AND [T_SYS_BU] = '{_tempBU}' AND [T_SYS_STATUS] = 'O' ";
			TableDetails tableDetails = F_GetTableDetails(new T_SysRun_No(), WhereClause, IsUpdateMethod: true);
			tableDetails.Column = "[T_SYS_NEXT]";
			tableDetails.RowData = "[T_SYS_NEXT] + 1";
			DB_UpdateData(tableDetails);
		}

		/// <summary>
		/// Updates the login status of an employee based on their employee number.
		/// </summary>
		/// <param name="_isLogin">The login status ('1' for logged in, '0' for logged out).</param>
		/// <param name="_empNo">The employee number.</param>
		public static void GF_UpdateLogData(char _isLogin, string _empNo)
		{
			string WhereClause = $"WHERE [EMP_NO] = '{_empNo}' ";
			M_Employee m_Employee = new M_Employee()
			{
				Emp_IsLogIn = _isLogin,
			};
			TableDetails tableDetails = F_GetTableDetails(m_Employee, WhereClause, IsUpdateMethod: true);
			DB_UpdateData(tableDetails);
		}

		/// <summary>
		/// Verifies if the session token matches the given cookie value.
		/// </summary>
		/// <param name="_Session">The session string.</param>
		/// <param name="_Cookie">The cookie value to compare.</param>
		/// <returns>True if the session token matches the cookie value; otherwise, false.</returns>

		/// <summary>
		/// Retrieves user details from the session string.
		/// </summary>
		/// <param name="_Session">The session string containing user details.</param>
		/// <returns>UserDetails object parsed from the session string.</returns>
		public static UserDetails GF_GetSession(string _Session)
		{
			return JsonConvert.DeserializeObject<UserDetails>(_Session);
		}

		/// <summary>
		/// Sets visibility of controls based on current user access rights using a list of required access keys. - for Site.Master.cs
		/// </summary>
		/// <typeparam name="T">The type of control (must be a Control).</typeparam>
		/// <param name="_CurrentAccess">The list of current user access rights.</param>
		/// <param name="_PageAccess">Dictionary mapping access keys to controls.</param>
		public static void GF_DisplayWithAccessibility<T>(List<string> _CurrentAccess, Dictionary<List<string>, T> _PageAccess) where T : Control
		{
			if (_CurrentAccess == null)
			{
				return;
			}
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

		/// <summary>
		/// Sets visibility of controls based on current user access rights using a dictionary of access keys.
		/// </summary>
		/// <typeparam name="T">The type of control (must be a Control).</typeparam>
		/// <param name="_CurrentAccess">The list of current user access rights.</param>
		/// <param name="_Access">Dictionary mapping access keys to controls.</param>
		/// <returns>return false if no access in the current page</returns>
		public static bool GF_DisplayWithAccessibility<T>(List<string> _CurrentAccess, Dictionary<string, T> _Access) where T : Control
		{
			bool HasAccess = false;
			if (_CurrentAccess == null)
			{
				return false;
			}

			_Access.ForEach(access =>
			{
				if (_CurrentAccess.Contains(access.Key))
				{
					access.Value.Visible = true;
					HasAccess = true;
				}
			});
			return HasAccess;
		}

		/// <summary>
		/// Get the unique identifier from database with the given prefix and running number for the business unit.
		/// </summary>
		/// <param name="_tempBU">The business unit identifier.</param>
		/// <param name="_tempPrefix">The prefix for the identifier.</param>
		/// <returns>The generated unique identifier.</returns>
		public static string GF_GenerateID(string _tempBU, string _tempPrefix)
		{
			return _tempPrefix +
				GF_GetRunningNumber(_tempBU, _tempPrefix).
				ToString().
				PadLeft(8, '0');
		}

		/// <summary>
		/// Populates a DropDownList with project codes and names from the database based on the optional WHERE clause.
		/// </summary>
		/// <param name="_DrpList">The DropDownList to populate.</param>
		/// <param name="_WhereClause">Optional WHERE clause for filtering projects.</param>
		public static void GF_PopulateProjCode(DropDownList _DrpList, string _WhereClause = "")
		{
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLSelectCommand = $"SELECT [PROJ_NAME],[PROJ_NO] FROM [dbo].[M_PROJECT_MASTER] {_WhereClause};";
				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, Conn);
				SqlDataReader DataReader = SQLcmd.ExecuteReader();
				if (!DataReader.HasRows)
				{
					return;
				}
				GF_DrpListAddDefaultItem(_DrpList);

				while (DataReader.Read())
				{
					_DrpList.Items.Add(new ListItem($"{DataReader["PROJ_NAME"]?.ToString()} - {DataReader["PROJ_NO"]?.ToString()}", DataReader["PROJ_NO"]?.ToString()));
				}
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "GF_PopulateProjCode", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}

		/// <summary>
		/// Populates a DropDownList with customer codes and names from the database.
		/// </summary>
		/// <param name="_DrpList">The DropDownList to populate.</param>
		public static void GF_PopulateCustCode(DropDownList _DrpList)
		{
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string WhereClause = "WHERE [CUST_STATUS] != 'X' ";
				string SQLSelectCommand = $"SELECT [CUST_NO],[CUST_NAME] FROM [dbo].[M_CUSTOMER] {WhereClause};";
				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, Conn);
				SqlDataReader DataReader = SQLcmd.ExecuteReader();
				if (!DataReader.HasRows)
				{
					return;
				}
				GF_DrpListAddDefaultItem(_DrpList);
				while (DataReader.Read())
				{
					_DrpList.Items.Add(new ListItem($"{DataReader["CUST_NAME"]?.ToString()} - {DataReader["CUST_NO"]?.ToString()}", DataReader["CUST_NO"]?.ToString()));
				}
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "GF_PopulateCustCode", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}

		/// <summary>
		/// Populates a DropDownList with document numbers from the database based on the specified object and optional WHERE clause.
		/// </summary>
		/// <param name="_DrpList">The DropDownList to populate.</param>
		/// <param name="_Object">The object defining the table to query.</param>
		/// <param name="_SelectData">The column name to select.</param>
		/// <param name="_WhereClause">Optional WHERE clause for filtering documents.</param>
		public static void GF_PopulateDocNo(DropDownList _DrpList, object _Object, string _SelectData, string _WhereClause = "")
		{
			GF_DrpListAddDefaultItem(_DrpList);
			TableDetails tableDetails = F_GetTableDetails(_Object, _WhereClause);
			DataTable dataTable = DB_ReadData(tableDetails, $"[Obj].[{_SelectData}]");
			if(dataTable == null)
			{
				return;
			}
			foreach (DataRow row in dataTable.Rows)
			{
				_DrpList.Items.Add(new ListItem(row[_SelectData]?.ToString(), row[_SelectData]?.ToString()));
			}
		}

		/// <summary>
		/// Generates a unique file identifier based on the original file name by appending a GUID.
		/// </summary>
		/// <param name="_originalFileName">The original file name.</param>
		/// <returns>The generated unique file identifier.</returns>
		public static string GF_GenerateUniqueFileIdentifier(string _originalFileName)
		{
			string UniqueId = Guid.NewGuid().ToString();
			string Extension = Path.GetExtension(_originalFileName);
			return $"{UniqueId}{Extension}";
		}
	}
}