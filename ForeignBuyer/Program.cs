using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ForeignBuyer
{
    class Program
    {
        static String fileName = "foreign_buyer.log";
        static Boolean updateMode = true;

        static void Main(string[] args)
        {
            Boolean showArgsOnly = false;
            parseArg(args, ref showArgsOnly);
            if (showArgsOnly)
                return;

            DateTime latest = new DateTime();
            if( getLatestDataDate(ref latest) == false)
            {
                latest = new DateTime(2016, 10, 1);
            }
            String previous_data = getPreviousData();

            JuristicPage juristic = new JuristicPage();

            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.GetEncoding("big5")))
            {
                DateTime date = DateTime.Today;
                while (!date.Equals(latest))
                {
                    String page_html = juristic.download(juristic.getPath(), date);

                    String[] res = new String[3];
                    Boolean success = juristic.parse(page_html, ref res);

                    sw.Write(date.ToShortDateString() + " ");
                    Console.Write(date.ToShortDateString() + " ");

                    if (success)
                    {
                        foreach (String str in res)
                        {
                            sw.Write(str + " ");
                            Console.Write(str + " ");
                        }
                    }
                    sw.WriteLine();
                    Console.WriteLine();

                    date = date.AddDays(-1);
                }

                sw.Write(previous_data);
            }
                

            
            
            /*using (StreamWriter sw = new StreamWriter("test.html", false, Encoding.GetEncoding("big5")))
            {
                sw.Write(page_html);
                sw.Close();
            }*/

            
        }

        static void parseArg(string[] args, ref Boolean showArgsOnly)
        {
            for (int index = 0; index < args.Count(); index++)
            {
                if (args[index].StartsWith("-"))
                {
                    char cmd = args[index].ToUpper()[1];
                    if (cmd == 'F')
                    {
                        fileName = args[index + 1];
                        index++;
                    }
                    else if (cmd == 'R')
                    {
                        updateMode = false;
                    }
                    else if (cmd == 'H')
                    {
                        Console.WriteLine("-h          : Print help message.");
                        Console.WriteLine("-f filename : Assign output file name.");
                        showArgsOnly = true;
                    }
                }
            }
        }

        static Boolean getLatestDataDate(ref DateTime date)
        {
            Boolean res = false;
            if (!File.Exists(fileName))
                return false;

            using (StreamReader sw = new StreamReader(fileName, Encoding.GetEncoding("big5")))
            {
                //try 2 times
                for (int i = 0; i < 2; i++)
                {
                    String line = sw.ReadLine();
                    if (line != null)
                    {
                        String[] segs = line.Split(new char[] { ' ', '\t' });
                        date = DateTime.Parse(segs[0]);
                        if (segs.Length >= 4)
                        {
                            res = true;
                            break;
                        }
                    }
                }
            }
            return res;
        }

        static String getPreviousData()
        {
            String res = "";

            if (!File.Exists(fileName))
                return "";

            using (StreamReader sw = new StreamReader(fileName, Encoding.GetEncoding("big5")))
            {
                res = sw.ReadToEnd();
            }
            return res;
        }
    }
}
