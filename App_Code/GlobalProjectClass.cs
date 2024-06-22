using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using static CUBIC_CIBT_Project.GlobalVariable;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
using Newtonsoft.Json.Linq;
using System.Web.UI.WebControls;


namespace CUBIC_CIBT_Project
{
	public class GlobalProjectClass
	{
		//Stores User Details to server session for authentication and authorization
		public class UserDetails
		{
			public string User_Login { get; set; }
			public string Username { get; set; }
			public string User_BU { get; set; }
			public List<string> User_Access { get; set; }
		}
		//Function that related to database
		public static class DataStructure
		{
			//Any function interact with Database start with DB_

			/// <summary>
			/// Inserts data into the specified database table based on TableDetails.
			/// </summary>
			/// <param name="_tableDetails">Details of the table and data to be inserted.</param>
			public static void DB_CreateData(TableDetails _tableDetails)
			{
				//connect with database and insert the data
				SqlConnection Conn = new SqlConnection(G_ConnectionString);
				GF_CheckConnectionStatus(Conn);
				Conn.Open();
				try
				{
					string SQLInsertCommand = $"INSERT INTO {_tableDetails.Table} ";
					SQLInsertCommand += $"VALUES ({_tableDetails.RowData});";
					SqlCommand SQLCmd = new SqlCommand(SQLInsertCommand, Conn);
					SQLCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "DB_CreateData", ex.ToString());
				}
				finally
				{
					Conn.Dispose();
					Conn.Close();
				}
			}

			/// <summary>
			/// Updates data in the specified database table based on TableDetails.
			/// </summary>
			/// <param name="_tableDetails">Details of the table, data, and update conditions.</param>
			/// <param name="_tempDelimeter">Temporary delimiter used in data processing (optional).</param>
			/// <param name="AvoidSpecialChar">Flag indicating whether to avoid special characters (optional).</param>
			public static void DB_UpdateData(TableDetails _tableDetails,char _tempDelimeter = '|', bool AvoidSpecialChar = false)
			{
				SqlConnection Conn = new SqlConnection(G_ConnectionString);
				GF_CheckConnectionStatus(Conn);
				Conn.Open();
				string setClause = "";
				try
				{
					//Query Set up
					string[] columns = _tableDetails.Column.Split(',');
					string[] values = _tableDetails.RowData.Split(',');
					if (AvoidSpecialChar)
					{
						for (int i = 0; i < columns.Length; i++)
						{
							values[i] = values[i].Replace(_tempDelimeter, ',');
						}
					}

					for (int i = 0; i < columns.Length; i++)
					{
						
						setClause += $"{columns[i]} = {values[i]},";
					}
					setClause = setClause.TrimEnd(',');

					string SQlUpdateCommand = $"UPDATE {_tableDetails.Table} ";
					SQlUpdateCommand += $"SET {setClause} ";
					SQlUpdateCommand += $"{_tableDetails.WhereOrJoinClause};";
					SqlCommand SQLCmd = new SqlCommand(SQlUpdateCommand, Conn);
					SQLCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "DB_UpdateData", ex.ToString());
				}
				finally
				{
					Conn.Dispose();
					Conn.Close();
				}

				//connect with database and update the data
			}

			/// <summary>
			/// Updates the status field of records in the specified database table instead of deleting them.
			/// </summary>
			/// <param name="_tableDetails">Details of the table and update conditions.</param>
			/// <param name="_Status">Name of the status field to be updated.</param>
			public static void DB_DeleteData(TableDetails _tableDetails,string _Status)
			{
				//Update the status instead of delete the data
				SqlConnection Conn = new SqlConnection(G_ConnectionString);
				GF_CheckConnectionStatus(Conn);
				Conn.Open();
				try
				{
					string SQLDeleteCommand = $"UPDATE {_tableDetails.Table}";
					SQLDeleteCommand += $"SET {_Status} = ";
					SQLDeleteCommand += $"'X' ";
					SQLDeleteCommand += $"{_tableDetails.WhereOrJoinClause};";
					SqlCommand SQLCmd = new SqlCommand(SQLDeleteCommand, Conn);
					SQLCmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "DB_DeleteData", ex.ToString());

				}
				finally
				{
					Conn.Dispose();
					Conn.Close();
				}
			}

			/// <summary>
			/// Reads data from the specified database table based on TableDetails.
			/// </summary>
			/// <param name="_tableDetails">Details of the table and query conditions.</param>
			/// <param name="_SelectData">Columns to select (default is "*").</param>
			/// <returns>DataTable containing the retrieved data.</returns>
			public static DataTable DB_ReadData(TableDetails _tableDetails, string _SelectData = "*")
			{
				
				SqlConnection Conn = new SqlConnection(G_ConnectionString);
				GF_CheckConnectionStatus(Conn);
				Conn.Open();
				try
				{
					//SELECT * instead of calling _tableDetails.Column
					//Available for single/multiple table query
					DataTable tb = new DataTable();
					string SQLSelectCommand = $"SELECT {_SelectData} ";
					SQLSelectCommand += $"FROM {_tableDetails.Table} Obj ";
					SQLSelectCommand += $"{_tableDetails.WhereOrJoinClause};";
					SqlDataAdapter DataAdapter = new SqlDataAdapter(SQLSelectCommand, Conn);
					DataAdapter.Fill(tb);
					return (tb);
				}
				catch(Exception ex)
				{
					GF_InsertAuditLog("-", "Catch Error", "GF_InsertAuditLog", "DB_ReadData", ex.ToString());
					return null;
				}
				finally
				{
					Conn.Dispose();
					Conn.Close();
				}
				
			}

			/// <summary>
			/// Converts property values of an object to their corresponding SQL string representation.
			/// </summary>
			/// <param name="_Property">PropertyInfo object representing a property of the object.</param>
			/// <param name="_Object">Object from which to retrieve property values.</param>
			/// <returns>String representation of the property value for SQL queries.</returns>
			private static string F_IsStringTypeProperty(PropertyInfo _Property, object _Object)
			{
				return _Property.PropertyType == typeof(string) || _Property.PropertyType == typeof(char)
					? $"'{_Property.GetValue(_Object, null)}'"
					: $"{_Property.GetValue(_Object, null)}";
			}

			/// <summary>
			/// Generates TableDetails for a given object, preparing it for database operations.
			/// </summary>
			/// <param name="_Object">Object for which to generate TableDetails.</param>
			/// <param name="_WhereOrJoinClause">Additional WHERE or JOIN clauses for queries (optional).</param>
			/// <param name="IsUpdateMethod">Flag indicating if the method is used for updating data (optional).</param>
			/// <returns>TableDetails object containing table, columns, and row data for SQL operations.</returns>
			public static TableDetails F_GetTableDetails(object _Object, string _WhereOrJoinClause = "",bool IsUpdateMethod = false)
			{
				TableDetails tableDetails = new TableDetails
				{
					Table = $"[dbo].[{_Object.GetType().Name}]", 
					WhereOrJoinClause = _WhereOrJoinClause
				};
				_Object.GetType().
					GetProperties().
				ForEach((Property) =>
				{
					var value = Property.GetValue(_Object, null);
					//Check null-value if IsUpdateMethod
					if (!IsUpdateMethod || value != null)
					{
						tableDetails.Column += $"[{Property.Name}],";
						tableDetails.RowData += $"{F_IsStringTypeProperty(Property, _Object)},";
					}
					});
				//remove ',' at the end of the string
				tableDetails.Column = tableDetails.Column.TrimEnd(',');
				tableDetails.RowData = tableDetails.RowData.TrimEnd(',');
				return tableDetails;
			}
		}

		//Object for better SQL Query Processing - Store SQL details by converting object to query 
		public class TableDetails
		{
			public string Table { get; set; }
			public string Column { get; set; }
			public string RowData { get; set; }
			public string WhereOrJoinClause { get; set; } = string.Empty;
		}

		//Entities
		public class T_Document
		{
			public string Doc_No { get; set; }
			public string Doc_Remark { get; set; }
			public string Doc_Type { get; set; }
			public string Doc_Revision_No { get; set; }
			public string Doc_Desc { get; set; }
			public string Doc_Upl_File_Name { get; set; }
			public string Doc_Upl_Path { get; set; }
			public string Doc_Date { get; set; }
			public string Doc_BU { get; set; }
			public char Doc_Status { get; set; }
			public string Doc_Created_By { get; set; }
			public string Doc_Created_Date { get; set; }
			public string Doc_Modified_By { get; set; }
			public string Doc_Modified_Date { get; set; } 
		}
		public class M_Delivery_Order_Hdr
		{
			public string DO_No { get; set; }
			public string Proj_No { get; set; }
		}
		public class M_Delivery_Order_Det
		{
			public string Doc_No { get; set; }
			public string DO_Address { get; set; }
			public string DO_Arrival_Date { get; set; }
		}
		public class M_Bank_Statement
		{
			public string BS_No { get; set; }
			public int BS_Type_ID { get; set; }
		}
		public class M_BS_Type
		{
			public int BS_Type_ID { get; set; }
			public string Bank_Type { get; set; }
		}
		public class M_Quotation
		{
			public string QO_No { get; set; }
			public string Proj_No { get; set; }
		}
		public class M_Purchase_Order
		{
			public string PO_No { get; set; }
			public string Proj_No { get; set; }
			public decimal PO_Total_Paid_Amount { get; set; } 
		}
		public class M_Invoice
		{
			public string INV_No { get; set; }
			public string Proj_No { get; set; }
			public int INV_Installment_ID { get; set; }
			public decimal INV_Balance_Amount { get; set; }
		}
		public class M_Project_Master
		{
			public string Proj_No { get; set; }
			public string Cust_No { get; set; }
			public string Proj_Name { get; set; }
			public string Proj_Date { get; set; }
			public string Proj_BU {  get; set; }
			public char Proj_Status { get; set; }
			public string Proj_Created_By { get; set; }
			public string Proj_Created_Date { get; set; }
			public string Proj_Modified_By { get; set; }
			public string Proj_Modified_Date { get; set; }
		}
		public class M_Customer
		{
			public string Cust_No { get; set; }
			public string Cust_Name { get; set; }
			public string Cust_Phone_Number { get; set; }
			public string Cust_BU { get; set; }
			public string Cust_Status { get; set; }
			public string Cust_Created_By { get; set; }
			public string Cust_Created_Date { get; set; }
			public string Cust_Modified_By { get; set; }
			public string Cust_Modified_Date { get; set; }
		}
		public class M_Inv_Installment
		{
			public int Inv_Installment_ID { get; set; }
			public string Installment_Period { get; set; }
		}
		public class M_Employee
		{
			public string Emp_No { get; set; }
			public string Emp_UserName { get; set; }
			public string Emp_Password { get; set; }
			public char Emp_IsLogIn { get; set; }
			public string Emp_BU { get; set; }
			public char Emp_Status { get; set; }
			public string Emp_Created_By { get; set; }
			public string Emp_Created_Date { get; set; }
			public string Emp_Modified_By { get; set; }
			public string Emp_Modified_Date { get; set; }
		}
		public class T_Employee_Access
		{
			public string Emp_No { get; set; }
			public int Access_No { get; set; }
		}
		public class M_Access
		{
			public int Access_No { get; set; }
			public string Access_Desc { get; set; }
		}
		public class T_SysRun_No
		{
			public string T_Sys_BU { get; set; }
			public string T_Sys_Prefix { get; set; }
			public string T_Sys_Desc { get; set; }
			public int T_Sys_Next { get; set; }
			public char T_Sys_Status{ get; set; }
		}
	}
	
}