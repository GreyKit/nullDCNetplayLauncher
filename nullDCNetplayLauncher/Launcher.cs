﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace nullDCNetplayLauncher
{
    public class Launcher
    {
        public static string rootDir = GetDistributionRootDirectoryName() + "\\";

        public Launcher()
        {
        }

        public static int GuessDelay(string IP)
        {
            int delay = -1;
            long avgResponseTime;

            List<long> responseTimes = new List<long>();
            for (int i = 0; i < 10; i++) {
                try
                {
                    Ping ping = new Ping();
                    PingReply pingReply = ping.Send(IP, 1000);
                    if (pingReply.Status == 0)
                    {
                        long responseTime = pingReply.RoundtripTime;
                        responseTimes.Add(responseTime);
                        Console.WriteLine(responseTime);
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }
            }

            if (responseTimes.Count > 0)
            {
                avgResponseTime = (long)responseTimes.Average();

                if (avgResponseTime < 25)
                {
                    delay = 1;
                }
                else if (avgResponseTime < 60)
                {
                    delay = 2;
                }
                else if (avgResponseTime < 100)
                {
                    delay = 3;
                }
                else if (avgResponseTime < 130)
                {
                    delay = 4;
                }
                else if (avgResponseTime < 155)
                {
                    delay = 5;
                }
                else if (avgResponseTime < 180)
                {
                    delay = 6;
                }
                else if (avgResponseTime > 180)
                {
                    delay = 7;
                }
            }
            return delay;
        }

        public static void KillAntiMicro()
        {
            var amInstances = Process.GetProcessesByName("antimicro");
            if (amInstances.Length > 0)
            {
                foreach (Process amInstance in amInstances)
                {
                    amInstance.Kill();
                    amInstance.WaitForExit();
                    amInstance.Dispose();
                }
            }
        }

        public static void LaunchAntiMicro(bool hidden=false)
        {
            string hiddenArg = "";
            KillAntiMicro();
            if (hidden)
            {
                hiddenArg = " --hidden";
            }
            Process.Start(Launcher.rootDir + "antimicro\\antimicro.exe", $"{hiddenArg} --profile {Launcher.rootDir}\\antimicro\\profiles\\nulldc.gamecontroller.amgp");
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

        public static string GetDistributionRootDirectoryName()
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


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hWnd, int dwAttribute, out Rect lpRect, int cbAttribute);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }



        public static IntPtr NullDCWindowHandle()
        {
            Process[] processes = Process.GetProcessesByName("nullDC_Win32_Release-NoTrace");
            Process ndc = processes[0];
            IntPtr ptr = ndc.MainWindowHandle;
            return ptr;
        }

        public static Point NullDCWindowDimensions()
        {
            Rect ndcWindowDim = new Rect();
            if (Environment.OSVersion.Version.Major < 6)
            {
                GetWindowRect(NullDCWindowHandle(), ref ndcWindowDim);
            }
            else
            {
                if ((DwmGetWindowAttribute(NullDCWindowHandle(), DWMWA_EXTENDED_FRAME_BOUNDS, out ndcWindowDim, Marshal.SizeOf(ndcWindowDim)) != 0))
                {
                    //GetWindowRect(Launcher.NullDCWindowHandle(), ref ndcWindowDim);
                }
            }

            var ndcWinX = ndcWindowDim.Right - ndcWindowDim.Left;
            var ndcWinY = ndcWindowDim.Bottom - ndcWindowDim.Top;

            return new Point(ndcWinX, ndcWinY);
        }

    }
}
