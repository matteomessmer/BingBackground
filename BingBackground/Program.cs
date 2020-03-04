using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace Background
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient wc = new WebClient();

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(appdata, "Daily BG\\");
            Directory.CreateDirectory(path + "images\\");
            try
            {
                wc.DownloadFile("http://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US", path + "foto.js");
                StreamReader sr = new StreamReader(path + "foto.js");

                string s = sr.ReadLine();
                sr.Close();

                List<string> tmp = new List<string>();
                tmp.AddRange(s.Split(':'));
                s = "https://www.bing.com" + tmp[tmp.FindIndex(x => x.Contains("url")) + 1].Split('"')[1];

                String filePath = path + "images\\" + DateTime.Now.ToFileTime() + ".jpg";
                wc.DownloadFile(s, filePath);
                Uri u = new Uri(filePath);
                Wallpaper.Set(u, Wallpaper.Style.Tiled);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Errore, connettersi alla rete e riprovare");
                Console.ReadLine();
            }
            finally
            {
                File.Delete(path + "foto.js");
            }

        }
    }
    public sealed class Wallpaper
    {
        Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void Set(Uri uri, Style style)
        {
            System.IO.Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

            System.Drawing.Image img = System.Drawing.Image.FromStream(s);
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
