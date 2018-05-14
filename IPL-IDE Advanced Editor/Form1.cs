using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPL_IDE_Advanced_Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = Editor.fullname;

            Settings.Initialize();

            if (!File.Exists(Settings.ini))
                Editor.StoreRaw(Settings.ini, Settings.default_raw);

            // Filling Combo box with Maps loaded from settings.ini
            foreach (KeyValuePair<string, Dictionary<string, string>> item in Settings.Data)
                if (item.Key.StartsWith("map", StringComparison.OrdinalIgnoreCase))
                    foreach (KeyValuePair<string, string> subItem in item.Value)
                        if (subItem.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
                            comboBoxLoadedMap.Items.Add(subItem.Value);

            int sel = Settings.GetSelected();
            comboBoxLoadedMap.SelectedIndex = sel - 1;

            // Path to files
            inputTextBox.Text = Settings.Data["Map" + sel]["InputPath"];
            outputTextBox.Text = Settings.Data["Map" + sel]["OutputPath"];
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            try
            {
                Editor.offset = Int32.Parse(IDoffsetTextBox.Text);
            }
            catch
            {
                MessageBox.Show("Offset field has an invalid value, only integer numbers are accepted.",
                    "Invalid Value on Offset Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Editor.xOff = Decimal.Parse(XtextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture);
                Editor.yOff = Decimal.Parse(YtextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture);
                Editor.zOff = Decimal.Parse(ZtextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Coordinates Offset Fields have invalid values. Only integers or decimal numbers (points as decimal-separator) are accepted.",
                    "Invalid Value on coordinates offsets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(inputTextBox.Text))
                MessageBox.Show("The path to input files does not exist. Make sure you put in the correct place and try again.",
                    "Input folder error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                EnableForm(false);
                editProgressBar.Value = 0;
                labelProgressStatus.Text = "0 %\nStarting.";
                bgWorker.RunWorkerAsync();
            }
        }
        private void browseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                int sel = Settings.GetSelected();
                if (sender == inputBrowseButton)
                {
                    Settings.Data["Map" + sel]["InputPath"] = folderBrowserDialog1.SelectedPath;
                    inputTextBox.Text = folderBrowserDialog1.SelectedPath;
                }
                else if (sender == outputBrowseButton)
                {
                    Settings.Data["Map" + sel]["OutputPath"] = folderBrowserDialog1.SelectedPath;
                    outputTextBox.Text = folderBrowserDialog1.SelectedPath;
                }
                Settings.Save();
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> ide = Settings.GetAllFilesFrom(inputTextBox.Text, "*.ide"),
                ipl = Settings.GetAllFilesFrom(inputTextBox.Text, "*.ipl");

            List<string> out_ide = Editor.CreateOutputPaths(ide, inputTextBox.Text, outputTextBox.Text),
                        out_ipl = Editor.CreateOutputPaths(ipl, inputTextBox.Text, outputTextBox.Text);

            List<string> ipl_raw = Editor.GetRaw(ipl),
                ide_raw = Editor.GetRaw(ide);

            Editor.Ids = Editor.GetAllIds(ide, ide_raw);

            int startID = Editor.GetStartID(Editor.Ids),
                finalID = Editor.GetFinalID(Editor.Ids),
                interval = finalID - startID,
                progress = startID,
                percentageComplete = 0;

            LogIds.Log("Before editing");
            LogIds.Log(Editor.Ids);

            // Editor.FixIde will reconvert ide lines that previously had this structure:
            // ID, ModelName, TextureName, DrawDist, Flags
            // to this new structure:
            // ID, ModelName, TextureName, ObjectCount, DrawDist, Flags
            // Inserting always "1" in ObjectCount
            // But... it is really necessary?
            // Besides, unexpected issues may occur when there are present 2 DrawDists
            if (patchIdeCheckBox.Checked)
                for (int i = 0; i < ide_raw.Count; i++)
                    ide_raw[i] = Editor.FixIde(ide_raw[i]);

            // Batch Id re-conversion in IDE / IPL files
            for (int i = 0; i < ide_raw.Count; i++)
            {
                string[] line = Regex.Split(ide_raw[i], "\r\n");    // ide_raw[i].Split(new [] { '\r', '\n' });
                int stat = 0;
                for (int j = 0; j < line.Length; j++)
                {
                    switch (stat)
                    {
                        case 0:
                            if (line[j].Equals("objs") || line[j].Equals("tobj")) stat = 1;
                            break;
                        case 1:
                            if (line[j].Equals("end")) stat = 0;
                            else
                            {
                                string oldExpr, newExpr;
                                string[] dummy = line[j].Split(',');
                                int Id;
                                /*if (dummy[0].StartsWith("#"))
                                    dummy[0] = dummy[0].Substring(dummy[0].IndexOf("#"));*/
                                try
                                {
                                    Id = Int32.Parse(dummy[0]);
                                }
                                catch
                                {
                                    throw new Exception(
                                        String.Format("Error: invalid Id value: '{0}' on file '{1}', line {2}",
                                        dummy[0], ide[i], j + 1));
                                }
                                oldExpr = dummy[0] + "," + dummy[1];
                                line[j] = (Id + Editor.offset - startID).ToString() + line[j].Substring(line[j].IndexOf(','));
                                dummy = line[j].Split(',');
                                newExpr = dummy[0] + "," + dummy[1];
                                for (int i2 = 0; i2 < ipl_raw.Count; i2++)
                                    ipl_raw[i2] = ipl_raw[i2].Replace(oldExpr, newExpr);
                                progress++;
                                percentageComplete = (int)(100 * (float)progress / (float)(Editor.offset + interval));
                                percentageComplete = (percentageComplete > 100) ? 100 : percentageComplete;
                                bgWorker.ReportProgress(percentageComplete, percentageComplete.ToString() + " %\nProcessing: " + ide[i].Split('\\').ToList().Last());
                            }
                            break;
                    }
                }
                ide_raw[i] = String.Join("\r\n", line);
            }

            // Checking if IPL is in diferent format than output, and converting if necessary
            // And also, re-assigning LODs (if format is San Andreas)
            Editor.PatchAllIpl(ipl, ipl_raw, bgWorker, percentageComplete);

            // Editing IPL coordinates
            if (Editor.xOff != 0 || Editor.yOff != 0 || Editor.zOff != 0)
            {
                for (int i = 0; i < ipl_raw.Count; i++)
                {
                    LogCoord.ReportFile(ipl[i]);
                    string[] line = Regex.Split(ipl_raw[i], "\r\n");
                    int stat = 0;
                    for (int j = 0; j < line.Length; j++)
                    {
                        switch (stat)
                        {
                            case 0:
                                if (line[j].Equals("inst")) stat = 1;
                                break;
                            case 1:
                                if (line[j].Equals("end")) stat = 2;
                                else
                                {
                                    string[] dummy = line[j].Split(',');
                                    LogCoord.InitCoordinates();
                                    LogCoord.LogCoordinates(dummy[3], dummy[4], dummy[5]);
                                    if (dummy.Length > 1)
                                    {
                                        decimal posx, posy, posz;
                                        posx = Decimal.Parse(dummy[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                                        posy = Decimal.Parse(dummy[4], NumberStyles.Any, CultureInfo.InvariantCulture);
                                        posz = Decimal.Parse(dummy[5], NumberStyles.Any, CultureInfo.InvariantCulture);
                                        posx += Editor.xOff;
                                        posy += Editor.yOff;
                                        posz += Editor.zOff;
                                        dummy[3] = " " + posx.ToString().Replace(",",".");
                                        dummy[4] = " " + posy.ToString().Replace(",", ".");
                                        dummy[5] = " " + posz.ToString().Replace(",", ".");
                                        line[j] = String.Join(",", dummy);
                                    }
                                    LogCoord.LogCoordinates(dummy[3], dummy[4], dummy[5]);
                                    LogCoord.WriteCoordErrorLine(dummy[0] + ", " + dummy[1], Editor.xOff, Editor.yOff, Editor.zOff);
                                }
                                break;
                        }
                    }
                    percentageComplete = (int)(100 * (float)progress / (float)(Editor.offset + interval));
                    bgWorker.ReportProgress(percentageComplete, percentageComplete.ToString() + " %\nEditing coordinates of: " + ipl[i]);
                    ipl_raw[i] = String.Join("\r\n", line);
                }
                LogCoord.EndLogging("coordinate_change.log");
            }

            Editor.Ids = Editor.GetAllIds(ide, ide_raw);
            LogIds.Log("After editing");
            LogIds.Log(Editor.Ids);
            //LogIds.EndLogging("editor_ids.log");

            // Building new IDE / IPL files
            bgWorker.ReportProgress(100, "100 %\nStoring.");
            for (int i = 0; i < ide_raw.Count; i++)
            {
                Editor.CreateDirectoryOf(out_ide[i]);
                Editor.StoreRaw(out_ide[i], ide_raw[i]);
            }
            for (int i = 0; i < ipl_raw.Count; i++)
            {
                Editor.CreateDirectoryOf(out_ipl[i]);
                Editor.StoreRaw(out_ipl[i], ipl_raw[i]);
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableForm(true);
            MessageBox.Show("Editing process completed successfully!",
                "IDE/IPL editing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            labelProgressStatus.Text = e.UserState as string;
            editProgressBar.Value = e.ProgressPercentage;
        }
        private void comboBoxLoadedMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = comboBoxLoadedMap.SelectedIndex + 1;
            Settings.Data["General"]["DefaultSelected"] = selected.ToString();
            Settings.Save();

            inputTextBox.Text = Settings.Data["Map" + selected]["InputPath"];
            outputTextBox.Text = Settings.Data["Map" + selected]["OutputPath"];
        }
        private void EnableForm(bool flag)
        {
            editProgressBar.Visible = !flag;
            labelProgressStatus.Visible = !flag;
            editButton.Enabled = flag;
            inputTextBox.Enabled = flag;
            outputTextBox.Enabled = flag;
            inputBrowseButton.Enabled = flag;
            outputBrowseButton.Enabled = flag;
            IDoffsetTextBox.Enabled = flag;
            XtextBox.Enabled = flag;
            YtextBox.Enabled = flag;
            ZtextBox.Enabled = flag;
            patchIdeCheckBox.Enabled = flag;
            comboBoxLoadedMap.Enabled = flag;
        }
    }
}
