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
        enum FormAction
        {
            Editing,
            Logging
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = Editor.fullname;

            if (!File.Exists(Settings.ini))
                Raw.Store(Settings.ini, Settings.default_raw);

            Settings.Initialize();

            // Filling Combo box with Maps loaded from settings.ini
            foreach (KeyValuePair<string, Dictionary<string, string>> item in Settings.Data)
                if (item.Key.StartsWith("map", StringComparison.OrdinalIgnoreCase))
                    foreach (KeyValuePair<string, string> subItem in item.Value)
                        if (subItem.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
                            comboBoxLoadedMap.Items.Add(subItem.Value);

            int sel = Settings.GetSelected();
            comboBoxLoadedMap.SelectedIndex = sel - 1;

            // Output Format not implemented yet
            outputFormatLabel.Enabled = false;
            outputFotmatIII_rBtn.Enabled = false;
            outputFormatVC_rBtn.Enabled = false;
            outputFormatSA_rBtn.Enabled = false;
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            Editor.PatchIDEs = patchIdeCheckBox.Checked;
            Editor.IgnoreLODs = ignoreLOD_checkBox.Checked;

            try
            {
                Editor.offset = UInt32.Parse(IDoffsetTextBox.Text);
            }
            catch
            {
                MessageBox.Show("Offset field has an invalid value, only positive integer numbers are accepted.",
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
                MessageBox.Show("The path to input files does not exist. Make sure you put it in the correct place and try again.",
                    "Input folder error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                EnableForm(false, FormAction.Editing);
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

            List<string> ipl_raw = Raw.Get(ipl), ide_raw = Raw.Get(ide);

            Editor.Ids = Editor.GetAllIds(ide, ide_raw);

            int startID = Editor.GetStartID(Editor.Ids),
                finalID = Editor.GetFinalID(Editor.Ids);

            Editor.Interval = finalID - startID;
            Editor.Progress = startID;
            Editor.PercentageCompleted = 0;

            LogIds.Log("Before editing");
            LogIds.Log(Editor.Ids);

            // Fix Ide Subroutine
            if (Editor.PatchIDEs)
                for (int i = 0; i < ide_raw.Count; i++)
                    ide_raw[i] = Editor.FixIde(ide_raw[i]);

            // Batch Id re-conversion in IDE / IPL files
            Editor.BatchIdsReConversion(ide, ide_raw, ipl_raw, startID, bgWorker);

            // Checking if IPL is in diferent format than output, and converting if necessary
            // And also, re-assigning LODs (if format is San Andreas)
            Editor.PatchAllIpl(ipl, ipl_raw, bgWorker);

            // Editing IPL coordinates
            if (Editor.xOff != 0 || Editor.yOff != 0 || Editor.zOff != 0)
                Editor.FixIplCoordinates(ipl, ipl_raw, bgWorker);

            Editor.Ids = Editor.GetAllIds(ide, ide_raw);
            LogIds.Log("After editing");
            LogIds.Log(Editor.Ids);
            //LogIds.EndLogging("editor_ids.log");

            // Building new IDE / IPL files
            bgWorker.ReportProgress(100, "100 %\nStoring.");
            for (int i = 0; i < ide_raw.Count; i++)
            {
                Editor.CreateDirectoryOf(out_ide[i]);
                Raw.Store(out_ide[i], ide_raw[i]);
            }
            for (int i = 0; i < ipl_raw.Count; i++)
            {
                Editor.CreateDirectoryOf(out_ipl[i]);
                Raw.Store(out_ipl[i], ipl_raw[i]);
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableForm(true, FormAction.Editing);
            MessageBox.Show("Editing process completed successfully!",
                "IDE/IPL editing", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private void EnableForm(bool flag, FormAction action)
        {
            switch (action)
            {
                default:
                case FormAction.Editing:
                    editProgressBar.Visible = !flag;
                    labelProgressStatus.Visible = !flag;
                    break;
                case FormAction.Logging:
                    logProgressLabel.Visible = !flag;
                    loggingProgressBar.Visible = !flag;
                    break;
            }
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
            ignoreLOD_checkBox.Enabled = flag;
            comboBoxLoadedMap.Enabled = flag;
        }

        private void patchIdeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (patchIdeCheckBox.Checked)
            {
                DialogResult patchIdeDialogResult = MessageBox.Show("You have selected option 'Patch IDEs'." + "\r\n"
                    + "" + "\r\n"
                    + "Before continuing, you must know this is a poorly developed routine, and can have unexpected results." + "\r\n"
                    + "All it does is replace Item Definition lines that have this structure:" + "\r\n"
                    + "" + "\r\n"
                    + "ID, ModelName, TextureName, DrawDist, Flags" + "\r\n"
                    + "" + "\r\n"
                    + "With this new one:" + "\r\n"
                    + "" + "\r\n"
                    + "ID, ModelName, TextureName, ObjectCount, DrawDist, Flags" + "\r\n"
                    + "" + "\r\n"
                    + "Adding a 1 as value to Object Count always." + "\r\n"
                    + "" + "\r\n"
                    + "But IDE lines can have more data than that. Actually Wiki modding pages describe IDE lines as:" + "\r\n"
                    + "ID, ModelName, TextureName, ObjectCount, DrawDist, [DrawDist2, ...], Flags" + "\r\n"
                    + "" + "\r\n"
                    + "Which means that if you have a file with more data than previous lines, you may have loss of data." + "\r\n"
                    + "So, only proceed with conversion if you really know what you're doing." + "\r\n"
                    + "",
                    "Patch IDE confirmation dialog", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (patchIdeDialogResult == DialogResult.Cancel)
                    patchIdeCheckBox.Checked = false;
            }
        }

        private void ignoreLOD_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ignoreLOD_checkBox.Checked)
            {
                DialogResult ignoreLODDialogResult = MessageBox.Show("You have selected option 'Ignore LODs build'." + "\r\n"
                    + "" + "\r\n"
                    + "Checking this option will reduce conversion time, but in consequence IPL files will not do LOD recognition, which means map LODs models will not work appropriately." + "\r\n"
                    + "" + "\r\n"
                    + "Only check this option if you make sure your IPL file has no LODs at all." + "\r\n"
                    + "" + "\r\n"
                    + "If you have no idea, just leave this field always unchecked." + "\r\n"
                    + "Maintain option checked?" + "\r\n"
                    + "",
                    "Patch IDE confirmation dialog", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (ignoreLODDialogResult == DialogResult.Cancel)
                    ignoreLOD_checkBox.Checked = false;
            }
        }

        private void generateIdReportButton_Click(object sender, EventArgs e)
        {
            string path_to_files = "";

            if (inputRadioButton.Checked)
            {
                if (!Directory.Exists(inputTextBox.Text))
                {
                    MessageBox.Show("The path to input files does not exist. Make sure you put it in the correct place and try again.",
                        "Input folder error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                path_to_files = inputTextBox.Text;
            }
            else if (outputRadioButton.Checked)
            {
                if (!Directory.Exists(outputTextBox.Text))
                {
                    MessageBox.Show("The path to output files does not exist. Make sure you put it in the correct place and try again.",
                        "Output folder error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                path_to_files = outputTextBox.Text;
            }

            List<string> ide = Settings.GetAllFilesFrom(path_to_files, "*.ide"),
                ipl = Settings.GetAllFilesFrom(path_to_files, "*.ipl");

            List<string> ipl_raw = Raw.Get(ipl), ide_raw = Raw.Get(ide);

            Editor.Ids = Editor.GetAllIds(ide, ide_raw);

            int startID = Editor.GetStartID(Editor.Ids),
                finalID = Editor.GetFinalID(Editor.Ids);

            Editor.Interval = finalID - startID;
            Editor.Progress = startID;
            Editor.PercentageCompleted = 0;

            LogIds.Init();
            LogIds.Log(String.Format("Map '{0}' Ids Report:\r\n=================",
                Settings.Data["Map"+Settings.GetSelected()]["name"]));

            if (ignoreMissingCheckBox.Checked)
                LogIds.Log(Editor.Ids);
            else
                LogIds.LogWithMissingIds(Editor.Ids);

            LogIds.EndLogging("id_report.log");

            MessageBox.Show(String.Format("Report File '{0}' successfully generated.", "id_report.log"),
                "IDE/IPL logging", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void idReportBgWorker_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void idReportBgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            logProgressLabel.Text = e.UserState as string;
            loggingProgressBar.Value = e.ProgressPercentage;
        }

        private void idReportBgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableForm(true, FormAction.Logging);
            MessageBox.Show(String.Format("Report File '{0}' successfully generated.", ""),
                "IDE/IPL logging", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
