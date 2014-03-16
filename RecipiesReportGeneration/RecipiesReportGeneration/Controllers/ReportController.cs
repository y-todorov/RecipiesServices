using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using RecipiesModelNS;
using Telerik.Reporting.Processing;

namespace RecipiesReportGeneration.Controllers
{
    public class ReportController : ApiController
    {
        // GET api/report
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // POST api/report
        public string Post([FromBody]string value)
        {
            var res = value.Split(new string[] { "EscapeSequence" }, StringSplitOptions.RemoveEmptyEntries);

            string reportdata = res[0];
            string datasource = res[1];

            string result = DownloadReportSerialization(reportdata, datasource);
            return result;
        }

        private string DownloadReportSerialization(string reportSerializationBytes, string reportDataSource)
        {
            ReportProcessor reportProcessor = new ReportProcessor();

            var instanceReportSource = new Telerik.Reporting.InstanceReportSource();
            Telerik.Reporting.Report salesOrderDetailsReport =
                DeserializeReport(reportSerializationBytes);

            List<PurchaseOrderDetail> serDataSource = JsonConvert.DeserializeObject<List<PurchaseOrderDetail>>(reportDataSource);

            salesOrderDetailsReport.DataSource = serDataSource;

            instanceReportSource.ReportDocument = salesOrderDetailsReport;

            RenderingResult result = reportProcessor.RenderReport("PDF", instanceReportSource, null);

            return Convert.ToBase64String(result.DocumentBytes); ;

        }


        private string SerializeReport(Telerik.Reporting.Report reportToSerialize)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Telerik.Reporting.XmlSerialization.ReportXmlSerializer xmlSerializer =
                    new Telerik.Reporting.XmlSerialization.ReportXmlSerializer();

                xmlSerializer.Serialize(ms, reportToSerialize);

                byte[] result = ms.ToArray();

                string resultString = Convert.ToBase64String(result);

                return resultString;
            }
        }

        private Telerik.Reporting.Report DeserializeReport(string reportString)
        {
            byte[] reportBytes = Convert.FromBase64String(reportString);

            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            using (MemoryStream ms = new MemoryStream(reportBytes))
            {
                Telerik.Reporting.XmlSerialization.ReportXmlSerializer xmlSerializer =
                    new Telerik.Reporting.XmlSerialization.ReportXmlSerializer();

                Telerik.Reporting.Report report = (Telerik.Reporting.Report)
                    xmlSerializer.Deserialize(ms);

                return report;
            }
        }
    
    
    }
}
