﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nullDCNetplayLauncher
{
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }

        string[] cfgLines;
        string[] rlLines;

        string player1_old;
        string backup_old;
        string player2_old;

        string p1Entry;
        string backupEntry;
        string p2Entry;

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            cfgLines = File.ReadAllLines(Launcher.rootDir + "nulldc-1-0-4-en-win\\nullDC.cfg");
            player1_old = cfgLines.Where(s => s.Contains("player1=")).ToList().First();
            backup_old = cfgLines.Where(s => s.Contains("backup=")).ToList().First();
            player2_old = cfgLines.Where(s => s.Contains("player2=")).ToList().First();

            Dictionary<string, string> InputOptions = new Dictionary<string, string>();
            InputOptions[""] = "";
            InputOptions["Keyboard"] = "keyboard";
            InputOptions["Joystick 1"] = "joy1";
            InputOptions["Joystick 2"] = "joy2";

            cboPlayer1.DataSource = new BindingSource(InputOptions, null);
            cboPlayer1.DisplayMember = "Key";
            cboPlayer1.ValueMember = "Value";

            cboBackup.DataSource = new BindingSource(InputOptions, null);
            cboBackup.DisplayMember = "Key";
            cboBackup.ValueMember = "Value";

            cboPlayer2.DataSource = new BindingSource(InputOptions, null);
            cboPlayer2.DisplayMember = "Key";
            cboPlayer2.ValueMember = "Value";

            p1Entry = player1_old.Split('=')[1];
            backupEntry = backup_old.Split('=')[1];
            p2Entry = player2_old.Split('=')[1];

            cboPlayer1.SelectedValue = p1Entry;
            cboBackup.SelectedValue = backupEntry;
            cboPlayer2.SelectedValue = p2Entry;

            rlLines = File.ReadAllLines(Launcher.rootDir + "nulldc_rom_launcher\\nulldc_rom_launcher.ahk");
            var window_old = rlLines.Where(s => s.Contains("; apply window settings")).ToList().First();
            if (window_old.Contains("WinMaximize"))
            {
                rdoStartMax.Checked = true;
            }
            else if (window_old.Contains("WinMove"))
            {
                rdoCustomSize.Checked = true;
            }
            else 
            {
                rdoDefault.Checked = true;
            }

            string launcherText = File.ReadAllText(Launcher.rootDir + "nullDCNetplayLauncher\\launcher.cfg");
            if (launcherText.Contains("launch_antimicro=1"))
            {
                chkEnableMapper.Checked = true;
            }
            else
            {
                chkEnableMapper.Checked = false;
            }
        }

        private void btnLaunchAntiMicro_Click(object sender, EventArgs e)
        {
            Launcher.LaunchAntiMicro();
        }

        private void btnEditCFG_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", Launcher.rootDir + "nulldc-1-0-4-en-win\\nullDC.cfg");
        }

        private void btnOpenQKO_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Launcher.rootDir + "nulldc-1-0-4-en-win\\qkoJAMMA");
        }

        private void btnSaveInput_Click(object sender, EventArgs e)
        {
            string launcherText = File.ReadAllText(Launcher.rootDir + "nullDCNetplayLauncher\\launcher.cfg");
            if (chkEnableMapper.Checked)
            {
                launcherText = launcherText.Replace("launch_antimicro=0", "launch_antimicro=1");
                if (Process.GetProcessesByName("antimicro").Length == 0)
                {
                    Process.Start(Launcher.rootDir + "antimicro\\antimicro.exe", " --hidden --profile " + Launcher.rootDir + "\\antimicro\\profiles\\nulldc.gamecontroller.amgp");
                }
            }
            else
            {
                launcherText = launcherText.Replace("launch_antimicro=1", "launch_antimicro=0");
            }
            File.WriteAllText(Launcher.rootDir + "nullDCNetplayLauncher\\launcher.cfg", launcherText);

            string cfgText = File.ReadAllText(Launcher.rootDir + "nulldc-1-0-4-en-win\\nullDC.cfg");
            cfgText = cfgText.Replace(player1_old, "player1=" + cboPlayer1.SelectedValue);
            cfgText = cfgText.Replace(backup_old, "backup=" + cboBackup.SelectedValue);
            cfgText = cfgText.Replace(player2_old, "player2=" + cboPlayer2.SelectedValue);
            File.WriteAllText(Launcher.rootDir + "nulldc-1-0-4-en-win\\nullDC.cfg", cfgText);

            // reload from file
            cfgLines = File.ReadAllLines(Launcher.rootDir + "nulldc-1-0-4-en-win\\nullDC.cfg");
            player1_old = cfgLines.Where(s => s.Contains("player1=")).ToList().First();
            backup_old = cfgLines.Where(s => s.Contains("backup=")).ToList().First();
            player2_old = cfgLines.Where(s => s.Contains("player2=")).ToList().First();

            MessageBox.Show("Input Settings Successfully Saved");
        }

        private void chkEnableMapper_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableMapper.Checked)
            {
                cboPlayer1.Text = "Keyboard";
                cboPlayer1.Enabled = false;
            }
            else
            {
                cboPlayer1.Enabled = true;
            }
        }

        private void btnJoyCpl_Click(object sender, EventArgs e)
        {
            Process.Start("joy.cpl");
        }

        private void btnSaveWindow_Click(object sender, EventArgs e)
        {
            string rlText = File.ReadAllText(Launcher.rootDir + "nulldc_rom_launcher\\nulldc_rom_launcher.ahk");
            string result = "";
            var regex = new Regex(@"^.*; apply window settings.*$", RegexOptions.Multiline);
            if (rdoStartMax.Checked)
                result = regex.Replace(rlText, "\tWinMaximize, ahk_class ndc_main_window ; apply window settings ");
            else if (rdoCustomSize.Checked)
                result = regex.Replace(rlText, $"\tWinMove, ahk_class ndc_main_window,,,, {txtWindowX.Text}, {txtWindowY.Text} ; apply window settings ");
            else
                result = regex.Replace(rlText, "\t; apply window settings ");
            File.WriteAllText(Launcher.rootDir + "nulldc_rom_launcher\\nulldc_rom_launcher.ahk", result);
            MessageBox.Show("Window Settings Successfully Saved");
        }

        private void btnGrabWindowSize_Click(object sender, EventArgs e)
        {
            rdoCustomSize.Checked = true;
            Point ndcWin = Launcher.NullDCWindowDimensions();
            txtWindowX.Text = ndcWin.X.ToString();
            txtWindowY.Text = ndcWin.Y.ToString();
        }
    }
}
