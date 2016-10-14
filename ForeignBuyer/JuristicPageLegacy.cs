using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web;
using NSoup;
using NSoup.Nodes;
using NSoup.Select;

namespace ForeignBuyer
{
    class JuristicPageLegacy : JuristicPage
    {
        override public string getPath()
        {
            return @"http://www.twse.com.tw/ch/trading/fund/BFI82U/BFI82U_oldtsec.php";
        }

        override public string download(string url, DateTime date)
        {
            string strResult = string.Empty;

            WebResponse objResponse;
            WebRequest objRequest = System.Net.HttpWebRequest.Create(url);

            //if (date.CompareTo(format_2015_start) > 0)
            {//later than
                /*
                report1:day
                input_date:105%2F10%2F11
                mSubmit:%ACd%B8%DF
                yr:2016
                w_date:20161011
                m_date:20161001
                */
                Dictionary<string, string> postParameters = new Dictionary<string, string>();
                //postParameters.Add("report1", "day");
                postParameters.Add("input_date", String.Format("{0}/{1:d2}/{2:d2}", date.Year - 1911, date.Month, date.Day));
                //postParameters.Add("mSubmit", "%ACd%B8%DF");
                //postParameters.Add("yr", date.Year.ToString());
                //postParameters.Add("w_date", String.Format("{0}/{1:d2}/{2:d2}", date.Year, date.Month, date.Day));
                //postParameters.Add("m_date", String.Format("{0}/{1:d2}/{2:d2}", date.Year, date.Month, "01"));
                postParameters.Add("login_btn", "+%ACd%B8%DF+");

                String postData = "";
                Boolean first = true;
                foreach (string key in postParameters.Keys)
                {
                    if (first)
                        first = false;
                    else
                        postData += "&";

                    postData += HttpUtility.UrlEncode(key) + "="
                          + HttpUtility.UrlEncode(postParameters[key]);

                }

                byte[] data = Encoding.ASCII.GetBytes(postData);
                objRequest.Method = "POST";
                objRequest.ContentType = "application/x-www-form-urlencoded";
                objRequest.ContentLength = data.Length;

                Stream requestStream = objRequest.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
            }

            try
            {
                objResponse = objRequest.GetResponse();

                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream(), Encoding.GetEncoding("big5"))) //, Encoding.Unicode
                {
                    strResult = sr.ReadToEnd();
                    // Close and clean up the StreamReader
                    sr.Close();
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
            }
            return strResult;
        }

        override public Boolean parse(DateTime date, String source, ref String[] res)
        {
            DateTime formatChangeDate = new DateTime(2001, 9, 5);
            if(date <= formatChangeDate)
                return parseFormat1(source, ref res);
            else
                return parseFormat2(date, source, ref res);
        }

        private Boolean parseFormat2(DateTime date, String source, ref String[] res)
        {
            Document doc = NSoup.NSoupClient.Parse(source);

            Elements trad_table = doc.Select("center center table tbody");
            if (trad_table.Count <= 2)
                return false;

            DateTime minorChangeDate = new DateTime(2001, 10, 4);
            Elements rows;
            if(date < minorChangeDate)
                rows = trad_table[1].Select("tr");
            else
                rows = trad_table[0].Select("tr");

            if (rows.Count < 5)
                return false;

            for (int i = 2; i < rows.Count; i++)
            {
                Elements tds = rows[i].Select("td");
                if (tds.Count == 0)
                    continue;
                String title = tds[0].Text();
                if (title.StartsWith("外資"))
                {
                    if (tds.Count < 4)
                        continue;
                    for (int idx = 1; idx < 4; idx++)
                        res[idx - 1] = numberString2IntString(tds[idx].Text());
                }
                foreach (Element e in tds)
                {
                    Console.Write(e.Text() + " ");
                }
                Console.WriteLine();
            }

            return true;
        }

        private Boolean parseFormat1(String source, ref String[] res)
        {
            Document doc = NSoup.NSoupClient.Parse(source);

            Elements trad_table = doc.Select(".til_2");
            if (trad_table.Count <= 1)
                return false;

            String tableContent = trad_table[1].Text();
            String[] phrase = tableContent.Split(new char[] {' ' }, StringSplitOptions.RemoveEmptyEntries);


            for (int i = 0; i < phrase.Length; i++)
            {
                if (phrase[i].StartsWith("外國法人"))
                {
                    if ((i + 3) >= phrase.Length)
                        break;

                    for (int idx = 0; idx < 3; idx++)
                        res[idx] = numberString2IntString(phrase[i + 1 + idx])+"0000";
                }
                /*foreach (String e in tds)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine();*/
            }

            return true;
        }

        public static String numberString2IntString(String src)       // 123,456.78  -> 123456
        {
            String res_str;

            String[] segs = src.Split(new char[] { '.' });
            res_str = segs[0].Replace(",", "").Replace(" ", "");

            return res_str;
        }
    }
}

