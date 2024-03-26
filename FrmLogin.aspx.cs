using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.Services.Description;
using System.Web.UI;
using static CUBIC_CIBT_Project.GlobalVariable;
using static CUBIC_CIBT_Project.GlobalProjectClass;
using System.Data;
using System.Linq;
using Microsoft.Ajax.Utilities;


namespace CUBIC_CIBT_Project
{
	public partial class FrmLogin : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				Dictionary<string, HttpCookie> DictCookie = new Dictionary<string, HttpCookie>()
				{ ["TokenSessionID"] = Request.Cookies["TokenSessionID"], ["UserLogin"] = Request.Cookies["UserLogin"] };
				if (DictCookie["UserLogin"] != null)
				{
					txtUsername.Text = DictCookie["UserLogin"]["Username"];
					txtPassword.Text = DictCookie["UserLogin"]["Password"];
				}
				if (Request.Url.ToString().Split('=')[1] == "SignOut")
				{
					HttpContext.Current.Session.Clear();
					DictCookie.ForEach((Cookie) =>
					{
						Cookie.Value.Expires = DateTime.Now.AddDays(-1);
						Response.Cookies.Add(Cookie.Value);
					});
				}
			}
			if (this.Page.User.Identity.IsAuthenticated)
			{
				Response.Redirect(FormsAuthentication.DefaultUrl);
			}
			
		}
		protected void btnLogin_Click(object sender, EventArgs e) {
			//Exit if empty input
			if (txtPassword.Text == string.Empty || txtUsername.Text == string.Empty)
			{
				GF_ReturnErrorMessage("Username or Password cannot be empty.", this.Page,this.GetType());
				return;
			}
			if (ChkRememberMe.Checked)
			{
				HttpCookie UserCookie = new HttpCookie("UserLogin");
				UserCookie.HttpOnly = true;
				UserCookie["Username"] = txtUsername.Text;
				UserCookie["Password"] = txtPassword.Text;
				UserCookie.Expires = DateTime.Now.AddDays(15);
				Response.Cookies.Add(UserCookie);
			}
			else
			{
				HttpCookie UserCookie = Request.Cookies["UserLogin"];
				if (UserCookie != null)
				{
					UserCookie.Expires = DateTime.Now.AddDays(-1);
					Response.Cookies.Add(UserCookie);
					GF_UpdateLogData('X', G_UserLogin); // 'O' - Open , 'X' - Close
				}
			}
			F_Login();
		}
		private void F_Login()
		{
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLSelectCommand = "SELECT [EMP].* [ACC].[ACCESS_DESC]";
				SQLSelectCommand += "FROM [dbo].[T_EMPLOYEE] EMP ";
				SQLSelectCommand += "JOIN [T_EMPLOYEE_ACCESS] EMP_ACC ON [EMP].[EMP_NO] = [EMP_ACC].[EMP_NO] ";
				SQLSelectCommand += "JOIN [T_ACCESS] ACC ON [EMP_ACC].[ACCESS_ID] = [ACC].[ACCESS_ID] WHERE";
				SQLSelectCommand += "[EMP_NO] = '" + txtUsername.Text + "';";

				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, Conn);
				SqlDataReader DataReader = SQLcmd.ExecuteReader();

				if (!DataReader.HasRows)
				{
					GF_ReturnErrorMessage("User Not Found, kindly look for administration", this.Page, this.GetType());
					return;
				}

				DataReader.Read();

				if (!F_VerifyPassword(txtPassword.Text, DataReader["EMP_PASSWORD"]?.ToString()) && txtUsername.Text.Equals(DataReader["EMP_NO"]?.ToString()))
				{
					GF_ReturnErrorMessage("Incorrect with Password or ID kindly try again", this.Page, this.GetType());
					return;
				}

				//Store User Details
				UserDetails userDetails = new UserDetails
				{
					User_Login = DataReader["EMP_NO"]?.ToString(),
					Username = DataReader["EMP_USERNAME"]?.ToString(),
					User_BU = DataReader["EMP_BU"]?.ToString(),
					TokenSessionID = Request.Cookies["SessionID"].ToString() ?? Guid.NewGuid().ToString()
				};
				userDetails.User_Access.AddRange(
					DataReader.Cast<IDataRecord>()
					.Select(row => row["ACCESS_DESC"]?.ToString())
					.ToList());

				
				F_StartSession(userDetails);

				G_UserBU = userDetails.User_BU;
				G_UserLogin = userDetails.User_Login;

				//Update the login details
				GF_UpdateLogData('O', userDetails.User_Login); // 'O' - Open , 'X' - Close

				//Redirect to Default Page
				Response.Redirect("Default.aspx");
			}
			catch (Exception ex)
			{
				GF_InsertAuditLog("-", "Catch error", "GF_InsertAuditLog", "F_Login", ex.ToString());
			}
			finally
			{
				Conn.Dispose();
				Conn.Close();
			}
		}
		private bool F_VerifyPassword(string _EnteredPassword, string _StoredHash)
		{
			byte[] Salt = new byte[16];
			byte[] HashBytes = Convert.FromBase64String(_StoredHash);

			Array.Copy(HashBytes, 0, Salt, 0, 16);

			var pbkdf2 = new Rfc2898DeriveBytes(_EnteredPassword, Salt, 10000);
			var Hash = pbkdf2.GetBytes(20);

			for (int i = 0; i < 20; i++) if (Hash[i] != HashBytes[i + 16]) return false;

			return true;
		}
		private void F_StartSession(UserDetails _UserDetails)
		{
			//Store to session
			Session["UserDetails"] = JsonConvert.SerializeObject(_UserDetails);
			//var Details = JsonConvert.DeserializeObject<UserDetails>(Session["UserDetails"].ToString());
			if (Request.Cookies["TokenSessionID"] != null)
			{
				return;
			}
			//Store Session ID to HttpOnly Cookie
			HttpCookie UserCookie = new HttpCookie("TokenSessionID", _UserDetails.TokenSessionID);
			UserCookie.Secure = true;
			UserCookie.HttpOnly = true;
			UserCookie.Expires = DateTime.Now.AddHours(8);
			Response.Cookies.Add(UserCookie);
		}
	}
}