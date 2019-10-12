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

            Console.WriteLine("Watching folder " + Arguments.Single(p => p.Key == "folder").Value + " for changes. Ctrl + C to exit");
            

            bool running = true;
            do
            {

                var Files = Directory.GetFiles(Arguments.First(p => p.Key == "folder").Value, "*.dtsx");

                foreach (var File in Files)
                {
                    bool Modified = false;
                    XmlDocument doc = new XmlDocument();

                    doc.Load(File);
                    var Executable = doc.GetElementsByTagName("Executable", DTS).Item(0);
                    if(Executable.Attributes["VersionGUID", DTS] != null)
                    {
                        Executable.Attributes.Remove(Executable.Attributes["VersionGUID", DTS]);
                        Modified = true;
                    }
                    var Attributes = Executable.Attributes;

                    foreach (XmlAttribute Attribute in Attributes)
                    {
                        if (Attribute.Name == "DTS:VersionBuild" && Attribute.Value != "1") { Attribute.Value = "1"; Modified = true; }
                        if (Attribute.Name == "DTS:CreationDate" && Attribute.Value != "2099-01-01") { Attribute.Value = "2099-01-01";Modified = true; }
                        if (Attribute.Name == "DTS:CreatorName" && Attribute.Value != "") { Attribute.Value = ""; Modified = true; }
                        if (Attribute.Name == "DTS:CreatorComputerName" && Attribute.Value != "") { Attribute.Value = ""; Modified = true; }
                    }
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
