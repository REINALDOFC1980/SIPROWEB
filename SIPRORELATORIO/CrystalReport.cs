
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Web;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;




namespace SIPRORELATORIO
{
    public class CrystalReport
    {
        public HttpResponseMessage Act_EmitirRelatorio(string relatorio, string[] parametro)
        {
            string ServerName = ConfigurationManager.AppSettings["rpt_servername"];
            string DatabaseName = ConfigurationManager.AppSettings["rpt_databasename"];
            string UserName = ConfigurationManager.AppSettings["rpt_username"];
            string Password = ConfigurationManager.AppSettings["rpt_password"];
            string rptfile = ConfigurationManager.AppSettings["rpt_pathrpt"] + relatorio;

            ReportDocument report = new ReportDocument();

            try
            {
                report.Load(rptfile);
                report.DataSourceConnections[0].SetConnection(ServerName, DatabaseName, UserName, Password);

                // Definir os valores dos parâmetros usando um loop
                for (int i = 0; i < parametro.Length; i++)
                {
                    report.SetParameterValue(i, parametro[i]);
                }

                Stream stream = report.ExportToStream(ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "Report.pdf"
                };

                return  response;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = ex.Message
                };
            }
            finally
            {
                report.Close();
                report.Dispose();
            }
        }
    }
}
