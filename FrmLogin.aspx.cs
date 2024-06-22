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
using static CUBIC_CIBT_Project.GlobalProjectClass;
using static CUBIC_CIBT_Project.GlobalProjectClass.DataStructure;
using static CUBIC_CIBT_Project.GlobalVariable;
using System.Data;
using System.Linq;
using Microsoft.Ajax.Utilities;
using System.Data.Common;


namespace CUBIC_CIBT_Project
{
	public partial class FrmLogin : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				//Cookie Session Process
				HttpCookie httpCookie = Request.Cookies["UserLogin"];
				if (httpCookie != null)
				{
					txtUsername.Text = httpCookie["Username"];
					txtPassword.Text = httpCookie["Password"];
				}
			}
			if (this.Page.User.Identity.IsAuthenticated)
			{
				Response.Redirect(FormsAuthentication.DefaultUrl);
			}
		}
		/// <summary>
		/// Event handler for the login button click event.
		/// Validates user input, handles "Remember Me" cookie logic, and attempts user login.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnLogin_Click(object sender, EventArgs e)
		{
			// Check Empty Value
			if (txtPassword.Text == string.Empty || txtUsername.Text == string.Empty)
			{
				GF_ReturnErrorMessage("Username or Password cannot be empty.", this.Page, this.GetType());
				return;
			}
			if (ChkRememberMe.Checked)
			{
				// Save cookie
				HttpCookie UserCookie = new HttpCookie("UserLogin")
				{
					HttpOnly = true
				};
				UserCookie["Username"] = txtUsername.Text;
				UserCookie["Password"] = txtPassword.Text;
				UserCookie.Expires = DateTime.Now.AddDays(15);
				Response.Cookies.Add(UserCookie);
			}
			else
			{
				// Destroy cookie
				HttpCookie UserCookie = Request.Cookies["UserLogin"];
				if (UserCookie != null)
				{
					UserCookie.Expires = DateTime.Now.AddDays(-1);
					Response.Cookies.Add(UserCookie);
					GF_UpdateLogData('C', G_UserLogin); // 'O' - Open , 'C' - Close
				}
			}
			F_Login();
		}

		/// <summary>
		/// Attempts user login by verifying credentials and setting up a user session.
		/// </summary>
		private void F_Login()
		{
			List<string> tempUserAccess = new List<string>();

			string WhereClause = $"WHERE [Obj].[EMP_STATUS] != 'X' AND [Obj].[EMP_NO] = '{txtUsername.Text}' ";
			string JoinClause = "LEFT JOIN [dbo].[T_EMPLOYEE_ACCESS] EMP_ACC ON [Obj].[EMP_NO] = [EMP_ACC].[EMP_NO] ";
			JoinClause += "LEFT JOIN [dbo].[M_ACCESS] ACC ON [EMP_ACC].[ACCESS_NO] = [ACC].[ACCESS_NO] ";
			
			TableDetails tableDetails = F_GetTableDetails(new M_Employee(), $"{JoinClause} {WhereClause}");
			DataTable dataTable = DB_ReadData(tableDetails);

			if (dataTable == null)
			{
				GF_ReturnErrorMessage("User Not Found, kindly look for administration 1 ", this.Page, this.GetType());
				return;
			}

			if (dataTable.Rows.Count == 0)
			{
				GF_ReturnErrorMessage("User Not Found, kindly look for administration 2 ", this.Page, this.GetType());
				return;
			}

			DataRow row = dataTable.Rows[0];

			if (!F_VerifyPassword(txtPassword.Text, row["EMP_PASSWORD"]?.ToString()) && txtUsername.Text.Equals(row["EMP_NO"]?.ToString()))
			{
				GF_ReturnErrorMessage("Incorrect Password or ID, kindly try again", this.Page, this.GetType());
				return;
			}

			// Store User Details
			UserDetails userDetails = new UserDetails()
			{
				User_Login = row["EMP_NO"]?.ToString(),
				Username = row["EMP_USERNAME"]?.ToString(),
				User_BU = row["EMP_BU"]?.ToString(),
				User_Access = new List<string>()
			};
			tempUserAccess.Add(row["ACCESS_DESC"]?.ToString());
			// Achieve the remaining access descriptions
			tempUserAccess.AddRange(
			dataTable.AsEnumerable() 
			.Select(data => data["ACCESS_DESC"]?.ToString())
			.Where(accessDesc => accessDesc != null)
			.ToList()
			);

			userDetails.User_Access.AddRange(tempUserAccess);

			F_StartSession(userDetails); // Store the details to session

			G_UserLogin = row["EMP_NO"]?.ToString();
			G_UserBU = row["EMP_BU"]?.ToString();
			Response.Redirect("~/Default.aspx"); // Redirect to home page
		}

		/// <summary>
		/// Verifies the entered password against the stored password hash using PBKDF2 hashing.
		/// </summary>
		/// <param name="_EnteredPassword">The entered password.</param>
		/// <param name="_StoredHash">The stored hash.</param>
		/// <returns>True if the password matches; otherwise, false.</returns>
		private bool F_VerifyPassword(string _EnteredPassword, string _StoredHash)
		{
			byte[] Salt = new byte[16];
			byte[] HashBytes = Convert.FromBase64String(_StoredHash);

			Array.Copy(HashBytes, 0, Salt, 0, 16);

			var pbkdf2 = new Rfc2898DeriveBytes(_EnteredPassword, Salt, 10000);
			var Hash = pbkdf2.GetBytes(20);

			for (int i = 0; i < 20; i++)
				if (Hash[i] != HashBytes[i + 16])
					return false;

			return true;
		}

		/// <summary>
		/// Starts a user session by storing user details in the session and setting a session ID cookie.
		/// </summary>
		/// <param name="_UserDetails">The user details to store in the session.</param>
		private void F_StartSession(UserDetails _UserDetails)
		{
			// Store to session
			Session["UserDetails"] = JsonConvert.SerializeObject(_UserDetails);

			// Save cookie
			HttpCookie UserCookie = new HttpCookie("UserLogin")
			{
				HttpOnly = true
			};
			UserCookie["Username"] = txtUsername.Text;
			UserCookie["Password"] = txtPassword.Text;
			UserCookie.Expires = DateTime.Now.AddDays(15);
			Response.Cookies.Add(UserCookie);
		}
	}
}