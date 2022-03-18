using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDF_Parsing;
using System.Collections.Generic;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DiffTesting
{
    [TestClass]
    public class DiffTesting
    {
        private static TestContext Context;
        [ClassInitialize]
        public static void ClassInit(TestContext con)
        {
            Context = con;
        }

        [ClassCleanup]
        public static void ClassClean()
        {
            using (StreamWriter w = File.AppendText(Context.TestDir + "/../log.txt"))
            {
                LogCleanUp(Context.CurrentTestOutcome.ToString(), w);
            }
        }


        [TestMethod]
        public async Task FirstDiffTestAsync()
        {
            string folderDir = "../../../";
            string correctReportDir = folderDir + "Reports To Compare/Testing - Copy.pdf";
            string OptyNumber = "122906";

            //Making a POST call to OIC to generate CPPCA
            string result = OIC_CPPCAGeneration(OptyNumber).Result;
            Response reportResponse = JsonConvert.DeserializeObject<Response>(result);
            string newURL = reportResponse.documentUrl;

            //Logging the OIC Response to a text file for tracking purposes
            await File.WriteAllTextAsync(Context.TestRunDirectory + "/REST_Response.txt", result);


            using (StreamWriter w = File.AppendText(Context.TestDir + "/../log.txt"))
            {
                //Checking if the Integration failed
                if (reportResponse.Error != null)
                {
                    LogTesting($" Integration Failed: {reportResponse.Error}", w);
                    Assert.IsNull(reportResponse.Error);
                    return;
                }
                //now we have the url, reading in the pdf reports
                List<string> Files = new List<string> { correctReportDir, newURL };
                List<string> parsedText = PdfToParsedText(Files);

                DiffPaneModel diff = InlineDiffBuilder.Diff(parsedText[0], parsedText[1]);
                // DiffReport is a customised object
                DiffReport diffReport = new DiffReport(correctReportDir, newURL);
                diffReport.RunDiffReport(diff);

                //In-test Logging
                string indent = "\n      - ";
                string logMsg = $"{indent}Opty Number: {OptyNumber}{indent}Activity Number: {reportResponse.ActivityNumber}{indent}File Name: {reportResponse.FileName}";
                if (diffReport.totalDiff != 0)
                {
                    //Writing HTML report conditionally
                    await File.WriteAllTextAsync(Context.TestRunDirectory + "/DiffReport.html", diffReport.htmlDiffHeader + diffReport.htmlDiffBody);
                    logMsg += $"{indent}Different lines: {diffReport.insertCounter} Inserted, {diffReport.deleteCounter} Deleted";
                }
                LogTesting(logMsg, w);
                Assert.IsTrue(diffReport.insertCounter + diffReport.deleteCounter == 0);
            }

        }


        public static async Task<string> OIC_CPPCAGeneration(string appNumber, string debugMode = "false")
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = (new AuthenticationHeaderValue("Basic", "ZGF4aW5nQGRlbG9pdHRlLmNvbS5hdTo1U3pKZERWNTdYczR5VDg="));
            var endPoint = new Uri("https://oic-projectx-qa-afgoc-sy.integration.ocp.oraclecloud.com/ic/api/integration/v1/flows/rest/CP_PCAGENERATION/1.0/GenerateReport");
            var newPost = new Post()
            {
                ApplicationNumber = appNumber,
                DebugMode = debugMode
            };
            var newPostJson = JsonConvert.SerializeObject(newPost);
            var payload = new StringContent(newPostJson, Encoding.UTF8, "application/json");
            var result = client.PostAsync(endPoint, payload).Result.Content.ReadAsStringAsync().Result;
            //Response reportResponse = JsonConvert.DeserializeObject<Response>(result);
            return result;

            //var result = client.PostAsync(endPoint, payload).Result.Content.ReadAsStringAsync().Result;
            //return reportResponse;

        }

        public static List<string> PdfToParsedText(List<string> Files)
        {
            List<string> parsedText = new List<string> { };

            //Read in and Parse the pdf reports
            for (int i = 0; i < Files.Count; i++)
            {
                var pdfDoc = new PdfDocument(new PdfReader(Files[i]));
                var numPages = pdfDoc.GetNumberOfPages();
                string wholeDoc = "";

                for (int pageNo = 1; pageNo <= numPages; pageNo++)
                {
                    var page = pdfDoc.GetPage(pageNo);
                    var allText = PdfTextExtractor.GetTextFromPage(page);
                    if (allText.LastIndexOf("Generated on ") != -1) allText = allText.Remove(allText.LastIndexOf("Generated on ")).TrimEnd('\n');
                    //wholeDoc += $"---------------{pageNo}\n" + allText + "\n";
                    wholeDoc += allText + "\n";
                }
                wholeDoc = wholeDoc.TrimEnd('\n');
                parsedText.Add(wholeDoc);
            }

            return parsedText;
        }

        public static void LogCleanUp(string status, TextWriter w)
        {
            w.WriteLine($" Status :{status}");
            w.WriteLine("-------------------------------");
        }

        public static void LogTesting(string logMsg, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine($" Log :{logMsg}");
        }

    }
}
