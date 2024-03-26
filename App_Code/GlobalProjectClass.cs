using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CUBIC_CIBT_Project
{
	public class GlobalProjectClass
	{
		public class UserDetails
		{
			public string User_Login { get; set; }
			public string Username { get; set; }
			public string User_BU { get; set; }
			public string TokenSessionID { get; set; }
			public List<string> User_Access { get; set; }
		}
	}
	
}