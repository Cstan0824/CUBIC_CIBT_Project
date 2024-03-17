using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.Services.Description;
using System.Web.UI;
using static CUBIC_CIBT_Project.GlobalVariable;

namespace CUBIC_CIBT_Project
{
	public partial class FrmLogin : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				return;
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

			HttpCookie UsernameCookie = new HttpCookie("Username");
			if (ChkRememberMe.Checked)
			{
				UsernameCookie.Value = txtUsername.Text;
				UsernameCookie.Expires = DateTime.Now.AddDays(15);
				Response.Cookies.Add(UsernameCookie);
			}
			else
			{
				if (Request.Cookies["Username"] != null)
				{
					UsernameCookie.Expires = DateTime.Now.AddDays(-1);
					Response.Cookies.Add(UsernameCookie);
				}
			}
			F_Login();
		}
		private void F_Login()
		{
			var FinalString = F_GenerateToken();
			SqlConnection Conn = new SqlConnection(G_ConnectionString);
			GF_CheckConnectionStatus(Conn);
			Conn.Open();
			try
			{
				string SQLSelectCommand =
					"SELECT EMP_BU, EMP_CODE, EMP_LEVEL,EMP_USERNAME, EMP_PASSWORD " +
					"FROM [dbo].[M_EMP_LOGIN] WHERE" +
					"(EMP_CODE = '" + txtUsername.Text + "' OR EMP_USERNAME = '" + txtUsername.Text + "') ";
				SqlCommand SQLcmd = new SqlCommand(SQLSelectCommand, Conn);
				SqlDataReader DataReader = SQLcmd.ExecuteReader();

				if (!DataReader.HasRows)
				{
					GF_ReturnErrorMessage("User Not Found, kindly look for administration", this.Page, this.GetType());
					return;
				}

				while (DataReader.Read())
				{
					if (!F_VerifyPassword(txtPassword.Text, DataReader["EMP_PASSWORD"].ToString()))
					{
						GF_ReturnErrorMessage("User Not Found, kindly look for administration", this.Page, this.GetType());
						return;
					}

					Session["UserID"] = txtUsername.Text;
					Session["UserLogin"] = DataReader["EMP_CODE"].ToString();
					Session["UserName"] = DataReader["EMP_USERNAME"].ToString();
					Session["BU"] = DataReader["EMP_BU"].ToString();
					Session["UserLevel"] = DataReader["EMP_LEVEL"].ToString();
					Session["TokenSessionID"] = FinalString;

					//Redirect to Home Page
					Response.Redirect("FrmEmpty.aspx?ID=HPFrmEmpty");
				}
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
		private string F_GenerateToken()
		{
			//Generate Token
			var Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var StringChars = new char[8];
			var random = new Random();
			for (int i = 0;
				i < StringChars.Length;
				StringChars[i++] = Chars[random.Next(Chars.Length)])
				;

			var FinalString = new String(StringChars);
			return FinalString;
		}
		private static bool F_VerifyPassword(string Entered_Password, string StoredHash)
		{
			byte[] Salt = new byte[16];
			byte[] HashBytes = Convert.FromBase64String(StoredHash);

			Array.Copy(HashBytes, 0, Salt, 0, 16);

			var pbkdf2 = new Rfc2898DeriveBytes(Entered_Password, Salt, 10000);
			var Hash = pbkdf2.GetBytes(20);

			for (int i = 0; i < 20; i++) if (Hash[i] != HashBytes[i + 16]) return false;

			return true;
		}

	}
}