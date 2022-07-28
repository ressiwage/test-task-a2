using System;
using System.Collections.Generic;
using System.Text.Json;
using RestSharp;
using System.Data.SQLite;
using System.Threading;


namespace test_task_a2
{
    public class Program
    {
        public static void createTable(SQLiteCommand cmd)
        {
            cmd.CommandText = @"CREATE TABLE `deal` (
                                `id` INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE NOT NULL,
                                `declaration_num` VARCHAR(50) UNIQUE  NOT NULL,
                                `seller_name` VARCHAR(350),
                                `seller_inn` VARCHAR(50),
                                `buyer_name` VARCHAR(350),
                                `buyer_inn` VARCHAR(50),
                                `deal_date` DATE NOT NULL,
                                `vol_Pr` FLOAT,
                                `vol_Pk` FLOAT)";
            cmd.ExecuteNonQuery();
        }
        
        public static void clearTable(SQLiteCommand cmd)
        {
            cmd.CommandText = @"DELETE FROM `deal`";
            cmd.ExecuteNonQuery();
        }
        
        public class Content
        {
            public string sellerName { get; set; }
            public string sellerInn { get; set; }
            public string buyerName { get; set; }
            public string buyerInn { get; set; }
            public double woodVolumeBuyer { get; set; }
            public double woodVolumeSeller { get; set; }
            public string dealDate { get; set; }
            public string dealNumber { get; set; }
            public string __typename { get; set; }
        }

        public class Data
        {
            public SearchReportWoodDeal searchReportWoodDeal { get; set; }
        }

        public class Root
        {
            public Data data { get; set; }
        }

        public class SearchReportWoodDeal
        {
            public List<Content> content { get; set; }
            public string __typename { get; set; }
        }

        
        public static void Main(string[] args)
        {
            
            for (int PAGE = 0; PAGE < 12259; PAGE++)
            {
                var client = new RestClient("https://www.lesegais.ru/open-area/graphql");
                var request = new RestRequest("https://www.lesegais.ru/open-area/graphql", Method.Post);
                request.Timeout = -1;
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Origin", "https://www.lesegais.ru");
                request.AddHeader("Referer", "https://www.lesegais.ru/open-area/deal");
                request.AddHeader("Sec-Fetch-Dest", "empty");
                request.AddHeader("Sec-Fetch-Mode", "cors");
                request.AddHeader("Sec-Fetch-Site", "same-origin");
                //"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
                request.AddHeader("sec-ch-ua",
                    "\"Google Chrome\";v=\"105\", \"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"105\"");
                request.AddHeader("sec-ch-ua-mobile", "?0");
                request.AddHeader("sec-ch-ua-platform", "\"Windows\"");
                string body = @"{
" + "\n" +
                              @"    ""query"": ""query SearchReportWoodDeal($size: Int!, $number: Int!, $filter: Filter, $orders: [Order!]) {\n  searchReportWoodDeal(filter: $filter, pageable: {number: $number, size: $size}, orders: $orders) {\n    content {\n      sellerName\n      sellerInn\n      buyerName\n      buyerInn\n      woodVolumeBuyer\n      woodVolumeSeller\n      dealDate\n      dealNumber\n      __typename\n    }\n    __typename\n  }\n}\n"",
" + "\n" +
                              @"    ""variables"": {
" + "\n" +
                              @"        ""size"": 20,
" + "\n" +
                              $@"        ""number"": {PAGE},
" + "\n" +
                              @"        ""filter"": null,
" + "\n" +
                              @"        ""orders"": null
" + "\n" +
                              @"    },
" + "\n" +
                              @"    ""operationName"": ""SearchReportWoodDeal""
" + "\n" +
                              @"}";
                request.AddParameter("application/json", body, ParameterType.RequestBody);

                //Console.WriteLine(body);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                RestResponse response = client.Execute(request);
                //Console.WriteLine(response.Content);

                Root root = JsonSerializer.Deserialize<Root>(response.Content);

                string cs = @"URI=file:test.db";

                using var con = new SQLiteConnection(cs);
                con.Open();

                using var cmd = new SQLiteCommand(con);
                
                foreach (Content content in root.data.searchReportWoodDeal.content)
                {
                    Console.WriteLine(content.buyerInn + "| " +
                                      content.buyerName + "| " +
                                      content.dealDate + "| " +
                                      content.dealNumber + "| " +
                                      content.sellerInn + "| " +
                                      content.sellerName + "| " +
                                      content.woodVolumeBuyer + "| " +
                                      content.woodVolumeSeller + "\n");
                    string parsedDate = content.dealDate; //DD.MM.YYYY -> YYYY-MM-DD
                    

                    cmd.CommandText = $@"INSERT INTO `deal`( 
                   `declaration_num`, 
                   `seller_name`, 
                   `seller_inn`, 
                   `buyer_name`, 
                   `buyer_inn`, 
                   `deal_date`, 
                   `vol_Pr`, 
                   `vol_Pk`) 
                VALUES ('{content.dealNumber}','{content.sellerName}','{content.sellerInn}',
                        '{content.buyerName}','{content.buyerInn}','{parsedDate}',
                        '{content.woodVolumeSeller}','{content.woodVolumeBuyer}')";
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        Console.WriteLine("запись с таким id/номером сделки уже существует");
                    }
                }
                
                Thread.Sleep(1000*60*10);
            }
        }
    }
}