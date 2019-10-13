using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace RemoveDesign
{
    class Program
    {
        static int Main(string[] args)
        {
            string DTS = "www.microsoft.com/SqlServer/Dts";
            List<KeyValuePair<string,string>> RemoveAttributes = new List<KeyValuePair<string,string>>
            {
                new KeyValuePair<string, string>("VersionBuild","1"),
                new KeyValuePair<string, string>("CreationDate",new DateTime(1970,1,1).ToShortDateString()),
                new KeyValuePair<string, string>("CreatorName",""),
                new KeyValuePair<string, string>("CreatorComputerName",""),
                new KeyValuePair<string, string>("VersionGUID",Guid.Empty.ToString())
            };

            if (args.Length % 2 != 0)
            {
                TextWriter ew = Console.Error;
                ew.WriteLine("You have to provide a property for every argument");
                return 2;
            }
            List<KeyValuePair<string, string>> Arguments = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < args.Length; i += 2)
            {
                var NewArg = new KeyValuePair<string, string>(args[i].Substring(1, args[i].Length - 1).ToLower(), args[i + 1]);
                Arguments.Add(NewArg);
            }
            if (!Arguments.Any(p => p.Key == "folder"))
            {
                Arguments.Add(new KeyValuePair<string, string>("folder", Directory.GetCurrentDirectory()));
            }
            string folder = Arguments.Single(p => p.Key == "folder").Value;
            if (!Directory.Exists(Arguments.First(p => p.Key == "folder").Value))
            {
                Console.WriteLine("Invalid directory: " + folder);
                return 1;
            }
            Console.WriteLine("Watching folder " + folder + " for changes. Ctrl + C to exit");

            bool running = false;
            if(Arguments.Any(p => p.Key.ToLower() == "watch"))
            {
                running = Arguments.Single(p => p.Key == "watch").Value.ToLower() == "true" ? true : false;
            }
            
            /* Process files */
            do
            {

                var Files = Directory.GetFiles(folder, "*.dtsx");

                foreach (var File in Files)
                {
                    bool Modified = false;
                    XmlDocument doc = new XmlDocument();
                    doc.Load(File);

                    /* Remove attributes from Executable */ 
                    var Executable = doc.GetElementsByTagName("Executable", DTS).Item(0);
                    foreach(var RemoveAttribute in RemoveAttributes)
                    {
                        /* If the attribute doesn't exists... */
                        var Attribute = Executable.Attributes[RemoveAttribute.Key, DTS];
                        if (Attribute == null) continue;

                        if (Attribute.Value != RemoveAttribute.Value)
                        {
                            Executable.Attributes[RemoveAttribute.Key, DTS].Value = RemoveAttribute.Value;
                            Modified = true;
                        }
                    }
                    
                    /* Remove design */
                    var Design = doc.GetElementsByTagName("DesignTimeProperties", DTS);
                    if (Design.Count > 0)
                    {
                        var DesignNode = Design.Item(0);
                        DesignNode.ParentNode.RemoveChild(DesignNode);
                        
                        Modified = true;
                    }
                    if (Modified)
                    {
                        Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Modified file {File}");
                        using (TextWriter tw = new StreamWriter(File, false, System.Text.Encoding.UTF8))
                        {
                            doc.Save(tw);
                        }
                    }
                }
                Thread.Sleep(3000);
            } while (running);
            return 0;
        }
    }
}
