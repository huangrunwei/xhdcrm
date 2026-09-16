using System;

namespace formatfromtext
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var file1 = System.IO.File.Open("1.txt", System.IO.FileMode.Open);

            //using (var stream = new System.IO.StreamReader(file1))
            //{
            //    while (!stream.EndOfStream)
            //    {
            //        string txt = stream.ReadLine();

            //        //Console.WriteLine($"{i}读取源数据：{txt}");

            //        //s += txt;
            //        string[] s = txt.Split(',');
            //        for (int j = 0; j < s.Length; j++)
            //        {
            //            var ss = s[j].Replace("N'", "").Replace("'", "").Replace(" ", "");;
            //            s[j] = ss;
            //        }
            //        var items = $"menus.Add(new Sys_Menu {{ id = \"{s[0] }\", Menu_name = \"{s[1]}\", parentid = \"{s[2]}\", Menu_url = \"{s[4]}\", Menu_icon = \"{s[5]}\", Menu_order = {s[6]} }});";
            //        Console.WriteLine(items);
            //    }
            //}
            //file1.Close();

            //Console.WriteLine();
            //Console.WriteLine("=================================================");
            //Console.WriteLine();

            //var file2= System.IO.File.Open("2.txt", System.IO.FileMode.Open);

            //using (var stream = new System.IO.StreamReader(file2))
            //{
            //    while (!stream.EndOfStream)
            //    {
            //        string txt = stream.ReadLine();

            //        //Console.WriteLine($"{i}读取源数据：{txt}");

            //        //s += txt;
            //        string[] s = txt.Split(',');
            //        for (int j = 0; j < s.Length; j++)
            //        {
            //            var ss = s[j].Replace("N'", "").Replace("'", "").Replace(" ", "");;
            //            s[j] = ss;
            //        }
            //        var items = $"button.Add(new Sys_Button {{ id= \"{s[0] }\",Btn_name= \"{s[1]}\",Btn_icon= \"{s[3]}\",Btn_handler= \"{s[4]}\" ,Btn_order={s[5]},Menu_id= \"{s[6]}\" }});";
            //        Console.WriteLine(items);
            //    }
            //}
            //file2.Close();

            //Console.WriteLine();
            //Console.WriteLine("=================================================");
            //Console.WriteLine();

            //var file3 = System.IO.File.Open("3.txt", System.IO.FileMode.Open);

            //using (var stream = new System.IO.StreamReader(file3))
            //{
            //    while (!stream.EndOfStream)
            //    {
            //        string txt = stream.ReadLine();

            //        //Console.WriteLine($"{i}读取源数据：{txt}");

            //        //s += txt;
            //        string[] s = txt.Split(',');
            //        for (int j = 0; j < s.Length; j++)
            //        {
            //            var ss = s[j].Replace("N'", "").Replace("'", "").Replace(" ", "");
            //            s[j] = ss;
            //        }
            //        var items = $" provinces.Add(new Sys_Param_Provinces {{ id = \"{s[0]}\", Provinces = \"{s[1]}\", Provinces_order = {s[2]}, Provinces_type = \"sys\" }});";
            //        Console.WriteLine(items);
            //    }
            //}
            //file3.Close();

            Console.WriteLine();
            Console.WriteLine("=================================================");
            Console.WriteLine();

            var file4 = System.IO.File.Open("4.txt", System.IO.FileMode.Open);

            using (var stream = new System.IO.StreamReader(file4))
            {
                while (!stream.EndOfStream)
                {
                    string txt = stream.ReadLine();

                    //Console.WriteLine($"{i}读取源数据：{txt}");

                    //s += txt;
                    string[] s = txt.Split(',');
                    for (int j = 0; j < s.Length; j++)
                    {
                        var ss = s[j].Replace("N'", "").Replace("'", "").Replace(" ", "");
                        s[j] = ss;
                    }
                    var items = $"City.Add(new Sys_Param_City {{ id=\"{s[0]}\",Provinces_id=\"{s[1]}\",City=\"{s[2]}\",City_order={s[3]},City_type=\"sys\" }});";
                    Console.WriteLine(items);
                }
            }
            file4.Close();

            Console.ReadLine();
        }
    }
}