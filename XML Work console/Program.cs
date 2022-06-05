using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

using System.Xml;
using System.IO;

namespace XML_Work_console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //The SaveAsXML method needs some work and some cleanup. It was planned to be used to 'normalize' the xml output.

            string base_url = @"https://home.treasury.gov/resource-center/data-chart-center/interest-rates/pages/xml?data=daily_treasury_yield_curve&field_tdr_date_value=all";

            string result = "result.xml";

            DeleteFile(result);

            for (int page = 1; page < 50; page++)
            {
                string temp = result;

                Console.WriteLine(page);
                string url = IteratePaginatedUrl(base_url, page);

                DownloadFile(url, IterateTempFile(page));

                if (FindEmptyTag(OpenXml(IterateTempFile(page))))
                {
                    // We're doing it this way, because we won't know the previous page is the last one
                    // until we check the current page. We don't want to remove the final line from the last page.
                    Console.WriteLine(String.Format("We found one on page {0}!", page));

                    DropHead(IterateTempFile(page - 1), 6);
                    ConcatFile(temp, IterateTempFile(page - 1));
                    DeleteFile(IterateTempFile(page));
                    DeleteFile(IterateTempFile(page - 1));
                    break;
                }
                else
                {
                    if (page == 2)
                    {
                        DropTail(IterateTempFile(page - 1), 1);
                        ConcatFile(temp, IterateTempFile(page - 1));
                        DeleteFile(IterateTempFile(page - 1));
                    }
                    else if (page > 2)
                    {
                        DropHeadAndTail(IterateTempFile(page -1 ), 6, 1);
                        ConcatFile(temp, IterateTempFile(page - 1));
                        DeleteFile(IterateTempFile(page - 1));
                    }
                    
                }

            }

        }

        private static void DeleteFile(string result)
        {
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }
        public static void DownloadFile(string url, string destination)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(url, destination);
        }
        public static XmlDocument OpenXml(string file)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using(XmlReader xmlReader = new XmlTextReader(file))
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }
        public static string IteratePaginatedUrl(string original_url, int iteration)
        {
            return string.Format("{0}&page={1}", original_url, iteration);
        }
        public static string IterateTempFile(int iteration)
        {
            return string.Format("{0}{1}.{2}", "temp", iteration, "xml");
        }

        public static void SaveAsXml(string source, string destination)
        {
            XmlDocument xmlDoc = new XmlDocument();
            
            XmlReader xmlReader = new XmlTextReader(source);

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.NewLineChars = "\n";
            xmlWriterSettings.NewLineHandling = NewLineHandling.None;
            //xmlWriterSettings.Indent = true;
            //xmlWriterSettings.IndentChars = "";
            //xmlWriterSettings.NewLineOnAttributes = false;
            xmlWriterSettings.Encoding = Encoding.UTF8;

            XmlWriter xmlWriter = XmlWriter.Create(destination, xmlWriterSettings);

            //xmlWriter.Settings.NewLineChars = "\n";
            
            //xmlWriter.Settings.Encoding = Encoding.UTF8;


            xmlDoc.Load(xmlReader);
            xmlDoc.WriteTo(xmlWriter);
            //xmlDoc.Save(destination);
            
            xmlReader.Close();
            xmlWriter.Close();
        }

        public static void ConcatFile(string segment1, string segment2)
        {
            StreamWriter file1 = new StreamWriter(segment1, true);
            file1.NewLine = "\n";
            file1.AutoFlush = true;
            StreamReader file2 = new StreamReader(segment2);

            // we need to drop the last 1 lines of the first file
            // we need to drop the first 6 lines of the second file

            // This will be very mess IO wise, unless we read all of both into memory first
            // question, do we thrash IO, or do we thrash memory?
            // SSIS is more memory constrained

            string s = "";
            while ((s = file2.ReadLine()) != null){
                file1.WriteLine(s);
            }
            file1.Close();
            file2.Close();
        }
        public static List<string> getLines(string file)
        {
            List<string> lines = new List<string>();

            using (StreamReader sr = new StreamReader(file))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }
        public static void DropTail(string file, int lineCount)
        {
            List<string> lines = getLines(file);

            using (StreamWriter sw = new StreamWriter(file, false))
            {
                sw.NewLine = "\n";
                sw.AutoFlush = true; ;
                for (int i = 0; i < lines.Count - lineCount; i++)
                {
                    sw.WriteLine(lines[i]);
                }
            }
        }
        public static void DropHead(string file, int lineCount)
        {
            List<string> lines = getLines(file);

            using (StreamWriter sw = new StreamWriter(file, false))
            {
                sw.NewLine = "\n";
                sw.AutoFlush = true; ;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (i >= lineCount)
                    {
                        sw.WriteLine(lines[i]);
                    }
                }
            }
        }
        public static void DropHeadAndTail(string file, int head, int tail)
        {
            List<string> lines = getLines(file);

            using (StreamWriter sw = new StreamWriter(file, false))
            {
                sw.NewLine = "\n";
                sw.AutoFlush = true; ;
                for (int i = 0; i < lines.Count - tail; i++)
                {
                    if (i >= head)
                    {
                        sw.WriteLine(lines[i]);
                    }
                }
            }
        }

        public static bool FindEmptyTag(XmlDocument xmlDocument)
        {
            bool tag_exists = false;
            // We're assuming the XmlDocument has been loaded.
            foreach (XmlNode node in xmlDocument.ChildNodes)
            {
                if (node.Name == "feed")
                {
                    foreach (XmlNode node1 in node.ChildNodes)
                    {
                        if (node1.Name == "entry")
                        {
                            // We found an "entry" tag
                            tag_exists = true;
                            if (!node1.HasChildNodes)
                            {
                                // and the tag is empty
                                Console.WriteLine("We found an empty tag!");

                                return true;
                            }
                            

                            //}
                        }
                    }
                    break;
                }
                
            }
            if (tag_exists)
            {
                // no empty tag found
                return false;
            }
            else
            {
                // no tag was found at all!
                return true;
            }
        }
    }
}
