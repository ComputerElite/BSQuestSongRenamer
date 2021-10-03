using ComputerUtils.ConsoleUi;
using ComputerUtils.FileManaging;
using ComputerUtils.Logging;
using ComputerUtils.StringFormatters;
using ComputerUtils.Updating;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BSQuestSongRenamer
{
    class Program
    {
        public static Updater updater = new Updater("1.0", "https://github.com/ComputerElite/BSQuestSongRenamer", "BSQuestSongRenamer");
        static void Main(string[] args)
        {
            SetupExceptionHandlers();
            Logger.displayLogInConsole = true;
            Logger.SetLogFile(AppDomain.CurrentDomain.BaseDirectory + "Log.log");
            if (args.Length >= 1 && args[0] == "--update") updater.Update();
            updater.UpdateAssistant();
            Converter c = new Converter();
            c.Start();
        }

        public static void SetupExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            HandleExtenption((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                HandleExtenption(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        public static void HandleExtenption(Exception e, string source)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Logger.Log("An unhandled exception has occured:\n" + e.ToString(), LoggingType.Crash);
            if(!Logger.displayLogInConsole) Console.WriteLine("\n\nAn unhandled exception has occured. Check the log for more info and send it to ComputerElite for the (probably) bug to get fix. Press any key to close out.");
            Console.ReadKey();
            Logger.Log("Exiting cause of unhandled exception.");
            Environment.Exit(0);
        }
    }

    class Converter
    {
        public static string exe = AppDomain.CurrentDomain.BaseDirectory;
        public void Start()
        {
            string dir = ConsoleUiController.QuestionString("Drag and drop the directory with all your songs here:").Replace("\"", "");
            FileManager.RecreateDirectoryIfExisting(exe + "songs");
            foreach (string f in Directory.GetDirectories(dir))
            {
                Convert(f);
            }
            Process.Start(exe + "songs");
        }

        public void Convert(string song)
        {
            Logger.Log("converting " + song);
            FileManager.DirectoryCopy(song, exe + "songs\\custom_level_" + GetCustomLevelHash(song), true);
        }

        public static string CreateSha1FromBytes(byte[] input)
        {
            // Use input string to calculate MD5 hash
            using (var sha1 = SHA1.Create())
            {
                var inputBytes = input;
                var hashBytes = sha1.ComputeHash(inputBytes);

                return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            }
        }

        public static string GetCustomLevelHash(string path)
        {
            byte[] combinedBytes = new byte[0];
            combinedBytes = combinedBytes.Concat(File.ReadAllBytes(Directory.GetFiles(path).FirstOrDefault(x => x.ToLower().EndsWith("info.dat")))).ToArray();
            var json = JSON.Parse(File.ReadAllText(path + "\\info.dat"));

            for (int i = 0; i < json["_difficultyBeatmapSets"].Count; i++)
            {
                for (int i2 = 0; i2 < json["_difficultyBeatmapSets"][i]["_difficultyBeatmaps"].Count; i2++)
                    if (File.Exists(path + "\\" + json["_difficultyBeatmapSets"][i]["_difficultyBeatmaps"][i2]["_beatmapFilename"]))
                        combinedBytes = combinedBytes.Concat(File.ReadAllBytes(path + "\\" + json["_difficultyBeatmapSets"][i]["_difficultyBeatmaps"][i2]["_beatmapFilename"])).ToArray();
            }

            return CreateSha1FromBytes(combinedBytes.ToArray()).ToLower();
        }
    }
}
