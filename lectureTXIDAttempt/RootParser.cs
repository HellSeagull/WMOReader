using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMOReader
{
    class RootParser
    {

        public void SelectWMORoot(string path)
        {

            //Sorting Filedata for WMO
            string binaryPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string exeName = Path.GetFileName(binaryPath);
            binaryPath = Directory.GetCurrentDirectory();
            var file = File.ReadAllLines(path);
            if (!File.Exists(binaryPath + @"\tempProcess.dat"))
            {
                File.Create(binaryPath + @"\tempProcess.dat").Close();
            }
            else
            {
                File.Delete(binaryPath + @"\tempProcess.dat");
            }

            StreamWriter sw = File.AppendText(binaryPath + @"\tempProcess.dat");

            foreach (string s in file)
            {
                if (s.Contains(".wmo"))
                {
                    sw.WriteLine(s);
                }
            }

            sw.Close();

            List<string>[] parse = new List<string>[2];
            parse[0] = new List<string>();
            parse[1] = new List<string>();

            var file_3 = File.ReadAllLines(binaryPath + @"\tempProcess.dat").Select(s => s.Split(' ')).ToArray();

            binaryPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            exeName = Path.GetFileName(binaryPath);
            binaryPath = Directory.GetCurrentDirectory() + @"\WMO_Root";

            for (Int32 i = 0; i < file_3.Length; i++)
            {
                parse[0].Add(file_3[i][0]);
                parse[1].Add(file_3[i][2]);
            }

            if (!File.Exists(binaryPath))
            {
                File.Create(binaryPath).Close();
            }
            else
            {
                File.Delete(binaryPath);
            }

            sw = File.AppendText(binaryPath);

            for (Int32 i = 0; i < parse[0].Count; i++)
            {
                var line = parse[1].ElementAt(i);
                var linelength = line.Length;
                var lastSlashPosition = line.LastIndexOf('/');
                var name = line.Substring(lastSlashPosition + 1, linelength - lastSlashPosition - 1);
                sw.WriteLine(name + "," + parse[0].ElementAt(i).Substring(0, 10).TrimStart('0'));
            }

            sw.Close();
            sw.Dispose();

            binaryPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            exeName = Path.GetFileName(binaryPath);
            binaryPath = Directory.GetCurrentDirectory();

            if (File.Exists(binaryPath + @"\tempProcess.dat"))
            {
                File.Delete(binaryPath + @"\tempProcess.dat");
            }

        }

    }
}
