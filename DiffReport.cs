using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace PDF_Parsing
{
    public class DiffReport
    {
        public DiffReport(string correctReport, string newReport)
        {
            htmlDiffHeader = "<!DOCTYPE html><html><header><h1>CP/PCA Difference Report</h1>";
            htmlDiffBody = "<body><h2>Diff Report:</h2><p>";
            insertCounter = 0;
            deleteCounter = 0;
            correctReportDir = correctReport;
            newURL = newReport;
        }

        public int insertCounter { get; set; }
        public int deleteCounter { get; set; }
        public int totalDiff { get; set; }
        public string htmlDiffHeader { get; set; }
        public string htmlDiffBody { get; set; }
        public string correctReportDir { get; set; }
        public string newURL { get; set; }

        public void RunDiffReport(DiffPaneModel diff)
        {
            //Goes through each line in Diff while constructing a report
            foreach (var line in diff.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        htmlDiffBody += "<span style=\"color: green; font-size: 18px;\"><b>+ " + line.Text + "</b></span>";
                        insertCounter++;
                        break;
                    case ChangeType.Deleted:
                        htmlDiffBody += "<span style=\"color: red; font-size: 18px;\"><b>- " + line.Text + "</b></span>";
                        deleteCounter++;
                        break;
                    default:
                        htmlDiffBody += line.Text;
                        break;
                }
                htmlDiffBody += "<br>";
            }
            htmlDiffBody += "</p></body></html>";

            //Writes the header of the html diff report
            totalDiff = insertCounter + deleteCounter;
            if (totalDiff != 0)
            {
                htmlDiffHeader += $"\n\n<h2>Total inserted lines: {insertCounter}<br>Total deleted lines: {deleteCounter}</h2>";
            }
            else
            {
                htmlDiffHeader += "<h2 style = \"color: green; \">The generated report is correct! (It is the same as the correct report)</h2>";
            }
            htmlDiffHeader += $"<p>Correct Report: {correctReportDir}. <br>Tested Report: {newURL}</p></header><hr>";

        }
    }
}
