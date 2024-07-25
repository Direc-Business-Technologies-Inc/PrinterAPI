using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using ZXing;

namespace PrinterAPI.Controllers
{
	public class PrintController : ApiController
	{

		// GET api/<controller>
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/<controller>/5
		public string Get(int id)
		{
			return "value";
		}

		public HttpResponseMessage Get(string Header, string args, string PrinterName, string FilePath, string Database)
		{
			//bool result = false;
			try
			{

				string path = FilePath;

				string tmpFile = Path.GetTempFileName();
				//TextFile tf = new TextFile(path);
				//File.WriteAllText(tmpFile, Header + args.TrimEnd('\n'));
				//File.WriteAllText(Database, Header + args.TrimEnd('\n'));
				File.WriteAllText(Database, Header + args.Replace("nextLine", "\n"));

				//// Split the header and args strings into arrays
				//string[] headerArray = Header.Split(',');
				//string[] argsArray = args.Remove(args.Length - 1, 1).Split(',');

				//// Create a new DataTable
				//DataTable dataTable = new DataTable();

				//// Add columns to the DataTable
				//foreach (string columnName in headerArray)
				//{
				//	dataTable.Columns.Add(columnName);
				//}

				//// Add rows to the DataTable
				//for (int i = 0; i < argsArray.Length; i += headerArray.Length)
				//{
				//	DataRow newRow = dataTable.NewRow();
				//	for (int j = 0; j < headerArray.Length; j++)
				//	{
				//		newRow[j] = argsArray[i + j];
				//	}
				//	dataTable.Rows.Add(newRow);
				//}

				ReportDocument cryRpt = new ReportDocument();

				cryRpt.Load(path);

				var setting = new System.Drawing.Printing.PrinterSettings();

				cryRpt.PrintOptions.PrinterName = PrinterName;

				//cryRpt.SetDataSource(Database);

				cryRpt.Refresh();

				//cryRpt.PrintToPrinter(1, false, 0, 0); // set start and end page to 0 to print all
				Stream crStream = cryRpt.ExportToStream(ExportFormatType.PortableDocFormat); // set start and end page to 0 to print all
																							 //cryRpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, "Report");
				crStream.Seek(0, SeekOrigin.Begin);

				// Create an HttpResponseMessage with the stream content
				HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
				response.Content = new StreamContent(crStream);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf"); // Change content type if needed
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
				response.Content.Headers.ContentDisposition.FileName = "report.pdf"; // Change the filename as desired

				cryRpt.Dispose();
				cryRpt.Close();

				return response;


				//result = true;
				//return crStream;
			}
			catch (Exception ex)
			{

				throw;
			}
		}

		public HttpResponseMessage Get(string FilePath)
		{
			//bool result = false;
			try
			{

				string path = FilePath;

				ReportDocument cryRpt = new ReportDocument();

				cryRpt.Load(path);

				var setting = new System.Drawing.Printing.PrinterSettings();

				cryRpt.Refresh();

				//cryRpt.PrintToPrinter(1, false, 0, 0); // set start and end page to 0 to print all
				Stream crStream = cryRpt.ExportToStream(ExportFormatType.PortableDocFormat); // set start and end page to 0 to print all
																							 //cryRpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, "Report");
				crStream.Seek(0, SeekOrigin.Begin);

				// Create an HttpResponseMessage with the stream content
				HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
				response.Content = new StreamContent(crStream);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf"); // Change content type if needed
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
				response.Content.Headers.ContentDisposition.FileName = "report.pdf"; // Change the filename as desired

				cryRpt.Dispose();
				cryRpt.Close();

				return response;

			}
			catch (Exception ex)
			{

				throw;
			}
		}

		public HttpResponseMessage Get(string FilePath, string Database, string Company, string DateFrom, string DateTo, string SalesMan, string Area)
		{
			try
			{
				HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

				//Get Connection String from Main Project
				string conString = ConfigurationManager.ConnectionStrings[Database].ToString();

				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(conString);

				// Access individual values
				string dataSource = builder.DataSource;
				string initialCatalog = builder.InitialCatalog;
				string userID = builder.UserID;
				string password = builder.Password;

				string path = FilePath;

				string tmpFile = Path.GetTempFileName();

				ReportDocument cryRpt = new ReportDocument();
				TableLogOnInfos crtableLogoninfos = new TableLogOnInfos();
				TableLogOnInfo crtableLogoninfo = new TableLogOnInfo();
				ConnectionInfo crConnectionInfo = new ConnectionInfo();

				cryRpt.Load(path);

				ParameterFieldDefinitions crParameterdef = cryRpt.DataDefinition.ParameterFields;

				var logonProperties = new DbConnectionAttributes();

				logonProperties.Collection.Set("Connection String", $@"Data Source={dataSource};Initial Catalog={initialCatalog};User ID={userID};Password={password};"); //FOR DB CONNECTION

				logonProperties.Collection.Set("UseDSNProperties", false);
				var connectionAttributes = new DbConnectionAttributes();
				connectionAttributes.Collection.Set("Database DLL", "crdb_odbc.dll");
				connectionAttributes.Collection.Set("QE_DatabaseName", String.Empty);
				connectionAttributes.Collection.Set("QE_DatabaseType", "ODBC (RDO)");
				connectionAttributes.Collection.Set("QE_LogonProperties", logonProperties);
				connectionAttributes.Collection.Set("QE_ServerDescription", dataSource); //IP ADDRESS OR SERVERNAME
				connectionAttributes.Collection.Set("QE_SQLDB", false);
				connectionAttributes.Collection.Set("SSO Enabled", false);

				crConnectionInfo.ServerName = dataSource; //IP ADDRESS

				crConnectionInfo.DatabaseName = initialCatalog; //DB NAME

				crConnectionInfo.UserID = userID;
				crConnectionInfo.Password = password;
				crConnectionInfo.Attributes = connectionAttributes;
				crConnectionInfo.Type = ConnectionInfoType.CRQE;
				crConnectionInfo.IntegratedSecurity = false;

				foreach (Table CrTable in cryRpt.Database.Tables)
				{
					crtableLogoninfo = CrTable.LogOnInfo;
					crtableLogoninfo.ConnectionInfo = crConnectionInfo;
					CrTable.ApplyLogOnInfo(crtableLogoninfo);
				}

				var setting = new System.Drawing.Printing.PrinterSettings();

				cryRpt.Refresh();


				//foreach (var param in Parameters)
				//{
				//    cryRpt.SetParameterValue(param.Key, param.Value); //SET PARAMETER
				//}
				cryRpt.SetParameterValue("areacodes", Area); //SET PARAMETER
				cryRpt.SetParameterValue("CompanyInitials", Company); //SET PARAMETER
				cryRpt.SetParameterValue("df", DateFrom); //SET PARAMETER
				cryRpt.SetParameterValue("dt", DateTo); //SET PARAMETER
				cryRpt.SetParameterValue("salesmans", SalesMan); //SET PARAMETER

				//cryRpt.PrintToPrinter(1, false, 0, 0); // set start and end page to 0 to print all
				Stream crStream = cryRpt.ExportToStream(ExportFormatType.PortableDocFormat); // set start and end page to 0 to print all
																							 //cryRpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, "Report");
				crStream.Seek(0, SeekOrigin.Begin);

				// Create an HttpResponseMessage with the stream content
				response.Content = new StreamContent(crStream);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf"); // Change content type if needed
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
				response.Content.Headers.ContentDisposition.FileName = "report.pdf"; // Change the filename as desired

				cryRpt.Dispose();
				cryRpt.Close();

				return response;

			}
			catch (Exception ex)
			{

				throw;
			}
		}

		public HttpResponseMessage Get(string FilePath, string Parameter, string Type)
		{
			try
			{
				HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

				//Get Connection String from Main Project
				string conString = ConfigurationManager.ConnectionStrings["Sap"].ToString();

				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(conString);

				// Access individual values
				string dataSource = builder.DataSource;
				string initialCatalog = builder.InitialCatalog;
				string userID = builder.UserID;
				string password = builder.Password;

				string path = FilePath;

				string tmpFile = Path.GetTempFileName();

				ReportDocument cryRpt = new ReportDocument(), crSubreportDocument;
				TableLogOnInfos crtableLogoninfos = new TableLogOnInfos();
				TableLogOnInfo crtableLogoninfo = new TableLogOnInfo();
				ConnectionInfo crConnectionInfo = new ConnectionInfo();

				cryRpt.Load(path);

				ParameterFieldDefinitions crParameterdef = cryRpt.DataDefinition.ParameterFields;

				var logonProperties = new DbConnectionAttributes();

				logonProperties.Collection.Set("Connection String", $@"Data Source={dataSource};Initial Catalog={initialCatalog};User ID={userID};Password={password};"); //FOR DB CONNECTION

				logonProperties.Collection.Set("UseDSNProperties", false);
				var connectionAttributes = new DbConnectionAttributes();
				connectionAttributes.Collection.Set("Database DLL", "crdb_odbc.dll");
				connectionAttributes.Collection.Set("QE_DatabaseName", String.Empty);
				connectionAttributes.Collection.Set("QE_DatabaseType", "ODBC (RDO)");
				connectionAttributes.Collection.Set("QE_LogonProperties", logonProperties);
				connectionAttributes.Collection.Set("QE_ServerDescription", dataSource); //IP ADDRESS OR SERVERNAME
				connectionAttributes.Collection.Set("QE_SQLDB", false);
				connectionAttributes.Collection.Set("SSO Enabled", false);

				crConnectionInfo.ServerName = dataSource; //IP ADDRESS

				crConnectionInfo.DatabaseName = initialCatalog; //DB NAME

				crConnectionInfo.UserID = userID;
				crConnectionInfo.Password = password;
				crConnectionInfo.Attributes = connectionAttributes;
				crConnectionInfo.Type = ConnectionInfoType.CRQE;
				crConnectionInfo.IntegratedSecurity = false;

				foreach (Table CrTable in cryRpt.Database.Tables)
				{
					crtableLogoninfo = CrTable.LogOnInfo;
					crtableLogoninfo.ConnectionInfo = crConnectionInfo;
					CrTable.ApplyLogOnInfo(crtableLogoninfo);
				}

				var setting = new System.Drawing.Printing.PrinterSettings();

				Sections crSections;
				ReportObjects crReportObjects;
				SubreportObject crSubreportObject;


				Database crDatabase;
				crDatabase = cryRpt.Database;
				Tables crTables;
				crTables = crDatabase.Tables;

				// THIS STUFF HERE IS FOR REPORTS HAVING SUBREPORTS 
				// set the sections object to the current report's section 
				crSections = cryRpt.ReportDefinition.Sections;
				// loop through all the sections to find all the report objects 
				foreach (Section crSection in crSections)
				{
					crReportObjects = crSection.ReportObjects;

					//loop through all the report objects in there to find all subreports 
					foreach (ReportObject crReportObject in crReportObjects)
					{
						if (crReportObject.Kind == ReportObjectKind.SubreportObject)
						{
							crSubreportObject = (SubreportObject)crReportObject;
							//open the subreport object and logon as for the general report 
							crSubreportDocument = crSubreportObject.OpenSubreport(crSubreportObject.SubreportName);
							crDatabase = crSubreportDocument.Database;
							crTables = crDatabase.Tables;
							foreach (Table aTable in crTables)
							{
								crtableLogoninfo = aTable.LogOnInfo;
								crtableLogoninfo.ConnectionInfo = crConnectionInfo;
								aTable.ApplyLogOnInfo(crtableLogoninfo);
							}
						}
					}
				}

				cryRpt.Refresh();

				ParameterFields parameterList = cryRpt.ParameterFields;
				foreach (ParameterField param in parameterList)
				{
					if (Type == "Documents" || Type == "Schedules")
					{
						if (FilePath.Contains("Product and Process Specs"))
						{
							cryRpt.SetParameterValue("DocKey@", Parameter); //SET PARAMETER
						}
						else
						{
							param.CurrentValues.AddValue(Parameter);
						}

					}
				}
				//if (Type == "Documents")
				//{
				//	cryRpt.SetParameterValue("DocKey@", Parameter); //SET PARAMETER
				//}
				//else if (Type == "Schedules")
				//{
				//	cryRpt.SetParameterValue("df", Parameter); //SET PARAMETER
				//}

				//cryRpt.PrintToPrinter(1, false, 0, 0); // set start and end page to 0 to print all
				Stream crStream = cryRpt.ExportToStream(ExportFormatType.PortableDocFormat); // set start and end page to 0 to print all
																							 //cryRpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, "Report");
				crStream.Seek(0, SeekOrigin.Begin);

				// Create an HttpResponseMessage with the stream content
				response.Content = new StreamContent(crStream);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf"); // Change content type if needed
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
				response.Content.Headers.ContentDisposition.FileName = "report.pdf"; // Change the filename as desired

				cryRpt.Dispose();
				cryRpt.Close();

				return response;

			}
			catch (Exception ex)
			{

				throw;
			}
		}

		// POST api/<controller>
		public HttpResponseMessage Post(string Header, string args, string PrinterName, string FilePath, string Database)
		{
			//bool result = false;
			try
			{

				string path = FilePath;

				string tmpFile = Path.GetTempFileName();
				File.WriteAllText(Database, Header + args.Replace("nextLine", "\n"));


				ReportDocument cryRpt = new ReportDocument();

				cryRpt.Load(path);

				var setting = new System.Drawing.Printing.PrinterSettings();

				cryRpt.PrintOptions.PrinterName = PrinterName;

				//cryRpt.SetDataSource(Database);

				cryRpt.Refresh();

				//cryRpt.PrintToPrinter(1, false, 0, 0); // set start and end page to 0 to print all
				Stream crStream = cryRpt.ExportToStream(ExportFormatType.PortableDocFormat); // set start and end page to 0 to print all
																							 //cryRpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, "Report");
				crStream.Seek(0, SeekOrigin.Begin);

				// Create an HttpResponseMessage with the stream content
				HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
				response.Content = new StreamContent(crStream);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf"); // Change content type if needed
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");
				response.Content.Headers.ContentDisposition.FileName = "report.pdf"; // Change the filename as desired

				cryRpt.Dispose();
				cryRpt.Close();

				return response;


				//result = true;
				//return crStream;
			}
			catch (Exception ex)
			{

				throw;
			}
		}

		// PUT api/<controller>/5
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/<controller>/5
		public void Delete(int id)
		{
		}
	}
}