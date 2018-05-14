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
            this.Text = Archivos.fullname;

            if (!File.Exists(Settings.ini))
                Archivos.StoreRaw(Settings.ini, Settings.default_raw);

            Settings.Entry = Int32.Parse(Settings.GetFromIni("DefaultEntry")[0]);
            Settings.UpdateSettings();

            // Filling Combo box with Maps loaded from settings.ini
            int i = 1;
            string[] loadedMap = Settings.GetFromIni("Map" + i.ToString());
            while (loadedMap.Length != 0)
            {
                comboBoxLoadedMap.Items.Add(loadedMap[0]);
                i++;
                loadedMap = Settings.GetFromIni("Map" + i.ToString());
            }
            comboBoxLoadedMap.SelectedIndex = Settings.Entry - 1;

            // Input path
            pathTextBox.Text = Settings.GetFromIni("DefaultEntryPath")[0];
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            if (Archivos.CheckFiles(pathTextBox.Text))
            {
                EnableForm(false);
                editProgressBar.Value = 0;
                labelProgressStatus.Text = "0 %\nStarting.";
                bgWorker.RunWorkerAsync();
            }
            else
                MessageBox.Show("One of more files are missing. Unable to make conversion.", "Error!");
        }
        private void browseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                while (Settings.StoreInIni("DefaultEntryPath", new string[1] { folderBrowserDialog1.SelectedPath }) == false)
                {
                    File.Delete(Settings.ini);
                    Archivos.StoreRaw(Settings.ini, Settings.default_raw);
                    MessageBox.Show(
                        "Un error ha ocurrido mientras se trataba de almacenar datos de configuración. " +
                        "El archivo \"settings.ini\" ha sido reconstruido y pudiera haberse producido pérdida de información.",
                        "settings.ini error");
                }
                pathTextBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] ide = Settings.GetFromIni(Settings.Ide),
                ipl = Settings.GetFromIni(Settings.Ipl);

            string[] ipl_raw = Archivos.GetRaw(pathTextBox.Text, ipl),
                ide_raw = Archivos.GetRaw(pathTextBox.Text, ide);

            Archivos.Ids = Archivos.GetAllIds(ide, ide_raw);

            int startID = Archivos.GetStartID(Archivos.Ids), //Editor.getStartID(ide_raw[0]),
                finalID = Archivos.GetFinalID(Archivos.Ids), //Editor.getFinalID(ide_raw[ide_raw.Length - 1]),
                interval = finalID - startID,
                offset = Int32.Parse(IDoffsetTextBox.Text),
                progress = startID,//labelProgressStatus,
                percentageComplete = 0;

            LogIds.Log("Before editing");
            LogIds.Log(Archivos.Ids);

            // Editor.FixIde will reconvert ide lines that previously had this structure:
            // ID, ModelName, TextureName, DrawDist, Flags
            // to this new structure:
            // ID, ModelName, TextureName, ObjectCount, DrawDist, Flags
            // Inserting always "1" in ObjectCount
            // But... it is really necessary?
            // Besides, unexpected issues may occur when there are present 2 DrawDists

            /*for (int i = 0; i < ide_raw.Length; i++)
                ide_raw[i] = Editor.FixIde(ide_raw[i]);*/

            // Batch Id re-conversion in IDE / IPL files
            for (int i = 0; i < ide_raw.Length; i++)
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
                                line[j] = (Id + offset - startID).ToString() + line[j].Substring(line[j].IndexOf(','));
                                dummy = line[j].Split(',');
                                newExpr = dummy[0] + "," + dummy[1];
                                for (int i2 = 0; i2 < ipl_raw.Length; i2++)
                                    ipl_raw[i2] = ipl_raw[i2].Replace(oldExpr, newExpr);
                                progress++;
                                percentageComplete = (int)(100 * (float)progress / (float)(offset + interval));
                                percentageComplete = (percentageComplete > 100) ? 100 : percentageComplete;
                                bgWorker.ReportProgress(percentageComplete, percentageComplete.ToString() + " %\nProcessing: " + ide[i]);
                            }
                            break;
                    }
                }
                ide_raw[i] = String.Join("\r\n", line);
            }

            // Checking if IPL is in diferent format than output, and converting if necessary
            // And also, re-assigning LODs (if format is San Andreas)
            Archivos.PatchAllIpl(ipl, ipl_raw, bgWorker, percentageComplete);

            // Editing IPL coordinates
            string xTxt = XtextBox.Text, yTxt = YtextBox.Text, zTxt = ZtextBox.Text;
            if (
                !xTxt.Equals("0") && !xTxt.StartsWith("0.") ||
                !yTxt.Equals("0") && !yTxt.StartsWith("0.") ||
                !zTxt.Equals("0") && !zTxt.StartsWith("0.")
                )
            {
                for (int i = 0; i < ipl_raw.Length; i++)
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
                                        posx += Decimal.Parse(xTxt, NumberStyles.Any, CultureInfo.InvariantCulture);
                                        posy += Decimal.Parse(yTxt, NumberStyles.Any, CultureInfo.InvariantCulture);
                                        posz += Decimal.Parse(zTxt, NumberStyles.Any, CultureInfo.InvariantCulture);
                                        dummy[3] = " " + posx.ToString().Replace(",",".");
                                        dummy[4] = " " + posy.ToString().Replace(",", ".");
                                        dummy[5] = " " + posz.ToString().Replace(",", ".");
                                        line[j] = String.Join(",", dummy);
                                    }
                                    LogCoord.LogCoordinates(dummy[3], dummy[4], dummy[5]);
                                    LogCoord.WriteCoordErrorLine(dummy[0] + ", " + dummy[1], xTxt, yTxt, zTxt);
                                }
                                break;
                        }
                    }
                    percentageComplete = (int)(100 * (float)progress / (float)(offset + interval));
                    bgWorker.ReportProgress(percentageComplete, percentageComplete.ToString() + " %\nEditing coordinates of: " + ipl[i]);
                    ipl_raw[i] = String.Join("\r\n", line);
                }
                LogCoord.EndLogging("coordinate_change.log");
            }

            Archivos.Ids = Archivos.GetAllIds(ide, ide_raw);
            LogIds.Log("After editing");
            LogIds.Log(Archivos.Ids);
            //LogIds.EndLogging("editor_ids.log");

            // Building new IDE / IPL files
            bgWorker.ReportProgress(100, "100 %\nStoring.");
            for (int i = 0; i < ide_raw.Length; i++)
            {
                Archivos.CreateDirectoryOf(Path.Combine("output", ide[i]));
                Archivos.StoreRaw(Path.Combine("output", ide[i]), ide_raw[i]);
            }
            for (int i = 0; i < ipl_raw.Length; i++)
            {
                Archivos.CreateDirectoryOf(Path.Combine("output", ipl[i]));
                Archivos.StoreRaw(Path.Combine("output", ipl[i]), ipl_raw[i]);
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableForm(true);
            MessageBox.Show("Editing process completed successfully!", "IDE/IPL editing");
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            labelProgressStatus.Text = e.UserState as string;
            editProgressBar.Value = e.ProgressPercentage;
        }
        private void comboBoxLoadedMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Entry = Int32.Parse((comboBoxLoadedMap.SelectedIndex + 1).ToString());
            Settings.UpdateSettings();
            Settings.StoreInIni("DefaultEntry", new string[1] { (comboBoxLoadedMap.SelectedIndex + 1).ToString() });
        }
        private void EnableForm(bool flag)
        {
            editProgressBar.Visible = !flag;
            labelProgressStatus.Visible = !flag;
            editButton.Enabled = flag;
            pathTextBox.Enabled = flag;
            browseButton.Enabled = flag;
            IDoffsetTextBox.Enabled = flag;
            XtextBox.Enabled = flag;
            YtextBox.Enabled = flag;
            ZtextBox.Enabled = flag;
        }
    }
}
