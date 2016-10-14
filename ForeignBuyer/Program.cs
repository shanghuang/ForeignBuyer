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
                latest = new DateTime(2000, 3, 13);
            }
            String previous_data = getPreviousData();

            DateTime newFormatStart = new DateTime(2004, 4, 7);

            JuristicPageNew juristicNew = new JuristicPageNew();
            JuristicPageLegacy juristicLegacy = new JuristicPageLegacy();

            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.GetEncoding("big5")))
            {
                DateTime date = DateTime.Today;
                //DateTime date = new DateTime(2001, 10, 31);

                while (!date.Equals(latest))
                {
                    JuristicPage juristic;
                    if (date >= newFormatStart)
                        juristic = juristicNew;
                    else
                        juristic = juristicLegacy;

                    String page_html = juristic.download(juristic.getPath(), date);

                    //debug
                    //DbgSaveFile( date.ToShortDateString()+".html",page_html);

                    String[] res = new String[3];
                    Boolean success = juristic.parse(date, page_html, ref res);

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

        static void DbgSaveFile(String filename, String content)
        {
            filename = filename.Replace('/', '_');
            using (StreamWriter sw = new StreamWriter(filename, false, Encoding.GetEncoding("big5")))
            {
                sw.Write(content);
                sw.Close();
            }
        }
    }
}
