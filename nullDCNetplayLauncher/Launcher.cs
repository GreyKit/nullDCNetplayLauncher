﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace nullDCNetplayLauncher
{
    public class Launcher
    {
        public static string rootDir = GetApplicationExecutableDirectoryName() + "\\";

        public Launcher()
        {
        }

        public static void LaunchNullDC(string RomPath, bool isHost = false)
        {
            string WorkDir = rootDir + "nulldc_rom_launcher\\";
            string RomDir = rootDir + "nulldc-1-0-4-en-win\\roms\\";
            string launchArgs = "\"" + WorkDir + "nulldc_rom_launcher.ahk" + "\" " + "\"" + RomDir + RomPath + "\"";
            if (isHost == true)
                launchArgs += " host";
            Console.WriteLine("\"" + WorkDir + "AutoHotkeyU32.exe" + "\" " + launchArgs);
            Process.Start("\"" + WorkDir + "AutoHotkeyU32.exe" + "\"", launchArgs);
        }

        // written by MarioBrotha
        // this fork mostly revolves around abusing this method
        public static void UpdateCFGFile(bool netplayEnabled = true,
            bool isHost = false,
            string hostAddress = "0.0.0.0",
            string hostPort = "27886",
            string frameDelay = "1")
        {
            string EmuDir = rootDir + "nulldc-1-0-4-en-win\\";
            string CfgPath = EmuDir + "nullDC.cfg";

            string enabled = "Enabled=" + (netplayEnabled ? 1 : 0).ToString();
            string hosting = "Hosting=" + (isHost ? 1 : 0).ToString();
            string hostip = "Host=" + hostAddress;
            string portcfg = "Port=" + hostPort;
            string delaycfg = "Delay=" + frameDelay;

            string[] lines = File.ReadAllLines(CfgPath);

            using (StreamWriter writer = new StreamWriter(CfgPath))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    //section netplay                   
                    if (lines[i].Contains("[Netplay]"))
                    {
                        writer.WriteLine("[Netplay]");
                        //rewriting all the lines in this section
                        writer.WriteLine(enabled);
                        writer.WriteLine(hosting);
                        writer.WriteLine(hostip);
                        writer.WriteLine(portcfg);
                        writer.WriteLine(delaycfg);
                        i = i + 5;
                    }
                    else
                        writer.WriteLine(lines[i]);
                }
            }
        }

        public static string GetApplicationExecutableDirectoryName()
        {
            var LauncherPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            return Directory.GetParent(LauncherPath).ToString();
        }

        public static string GetApplicationConfigurationDirectoryName()
        {
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        }

        public static string ExtractRomNameFromPath(string path)
        {
            List<string> splitPath = path.Split(Path.DirectorySeparatorChar).ToList();
            return splitPath.ElementAt(splitPath.Count - 2);
        }

        public static string ExtractRelativeRomPath(string path)
        {
            List<string> splitPath = path.Split(Path.DirectorySeparatorChar).ToList();
            return String.Join("\\", Enumerable.Reverse(splitPath).Take(2).Reverse().ToList<string>());
        }
    }
}