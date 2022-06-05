using System.Net;



    // See https://aka.ms/new-console-template for more information
    Console.WriteLine("Hello, World!");


    //string url = @"https://home.treasury.gov/resource-center/data-chart-center/interest-rates/pages/xml?data=[value]&field_tdr_date_value=[all]";
    string url = @"https://home.treasury.gov/resource-center/data-chart-center/interest-rates/pages/xml?data=daily_treasury_yield_curve&field_tdr_date_value=all";
    string targetFile = "result.xml";

    //string iterator = "&page=[xxx]";

    WebClient client = new WebClient();
    client.DownloadFile(url, targetFile);



    int i = 0;

    string newurl = string.Format("{0}&page=[{1}]", url, i);

    client.DownloadFile(newurl, targetFile);


    Console.WriteLine("Done");


