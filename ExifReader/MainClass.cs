using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExifReader
{
    public class MainClass
    {

        [STAThread]
        public static void Main()
        {

            string path = "";

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.FileName;
                }
            }

            if (path != "")
            {
                Console.WriteLine(Read(path));
                Console.ReadKey();
            }     
        }



        public static string Read(string path)
        {
            Bitmap bmp;

            try
            {
               bmp  = new Bitmap(path);
            }
            catch(Exception e)
            {
                return e.ToString();
            }

            if (bmp is null) return null;



            string output = "";

            Dictionary<int, string> dictionary = new Dictionary<int, string>();

           
            string types = Properties.Resources.types;


            string[] rows = types.Split('\n');

            foreach (string row in rows)
            {
                string[] parts = row.Split(',');
                if (parts[0].Length <= 2 | !parts[0].StartsWith("0x")) continue;               
                if (int.TryParse(parts[0].Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int id))
                {
                    dictionary.Add(id, parts[1].Trim().Substring(11));
                }
            }

            

            foreach (int id in bmp.PropertyIdList)
            {
                PropertyItem item = bmp.GetPropertyItem(id);

                string value = "";

                switch(item.Type)
                {
                    case 1:
                        value = "byte array";
                        break;
                    case 2:
                        value = Encoding.ASCII.GetString(item.Value);
                        break;
                    case 3:
                        value = BitConverter.ToInt16(item.Value, 0).ToString();
                        break;
                    case 4:
                        value = BitConverter.ToInt32(item.Value, 0).ToString();
                        break;
                    case 5:
                        {
                            if (item.Value is null) continue;

                            for (int i=0; i< item.Value?.Length / 4; i++)
                            {
                                uint number = BitConverter.ToUInt32(item.Value, i);

                                value += ((i % 2 == 0) ? "" : "/") + number + ((i % 2 == 0) ? "" : "; ");
                            }
 
                        }
                        break;
                    case 6:
                        value = "byte array";
                        break;
                    case 7:

                        if (item.Value is null) continue;
                        for (int i = 0; i < item.Value.Length / 8; i++)
                        {
                            value += BitConverter.ToUInt64(item.Value, i) + " ";
                        }

                         break;
                    
                    case 10:

                        if (item.Value is null) continue;
                        for (int i = 0; i < item.Value.Length / 8; i++)
                        {
                            ulong number = BitConverter.ToUInt64(item.Value, i);

                            value += ((i % 2 == 0) ? "" : "/") + number + ((i % 2 == 0) ? "" : "; ");
                        }
                        break;
                    default:
                        value = "Error!";
                        break;                
                }
                
                if (dictionary.TryGetValue(item.Id, out string type))
                {
                    output +=  type + " = " + value + "\n\n";
                }


            }


            return output;
        }

    }
}
