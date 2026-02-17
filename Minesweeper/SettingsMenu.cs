using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class SettingsMenu : Form
    {
        Label makeLabel(string Text, float fontSize, int Left, int Top, bool setWidth, int? Width)
        {
            Label lbl = new Label();
            lbl.Text = Text;
            lbl.Font = new Font(new FontFamily(GenericFontFamilies.Serif), fontSize);
            lbl.AutoSize = true;
            lbl.Width = setWidth ? Width.Value : (int)fontSize * Text.ToCharArray().Length;
            lbl.Height += 20;
            lbl.Left = Left; lbl.Top = Top;
            return lbl;
        }

        NumericUpDown makeNumericUpDown(int Value, int Increment, int Minimum, int Maximum, float fontSize, int Width, int Left, int Top)
        {
            NumericUpDown nud = new NumericUpDown();

            nud.Increment = Increment;
            nud.Minimum = Minimum;
            nud.Maximum = Maximum;
            nud.Value = Value;

            nud.Font = new Font(new FontFamily(GenericFontFamilies.Serif), fontSize);
            nud.Width = Width;
            nud.Left = Left;
            nud.Top = Top;
            TextBox editBox = nud.Controls.OfType<TextBox>().First();
            editBox.SelectionStart = editBox.Text.Length;
            editBox.SelectionLength = 0;
            return nud;
        }

        Button makeButton(Color backColor, Color textColor, Color borderColor, string Text, float fontSize, int width, int height, int left, int top)
        {
            Button btn = new Button();
            btn.BackColor = backColor;
            btn.ForeColor = textColor;
            btn.Text = Text;
            btn.Font = new Font(new FontFamily(GenericFontFamilies.Serif), fontSize);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = borderColor;
            btn.FlatAppearance.BorderSize = 4;
            btn.Width = width;
            btn.Height = height;
            btn.Left = left;
            btn.Top = top;
            return btn;
        }

        void numericUpDown_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.' || e.KeyChar == ',') e.Handled = true;
        }

        static Task Delay(int ms)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            System.Threading.Timer timer = new System.Threading.Timer(_ =>
            {
                tcs.SetResult(null);
            }, null, ms, System.Threading.Timeout.Infinite);
            return tcs.Task;
        }

        Task hideWarningLabelAfterAWhile()
        {
            return Delay(3500).ContinueWith(t =>
            {
                if (warningLabel.InvokeRequired)
                    warningLabel.Invoke((MethodInvoker)(() => warningLabel.Hide()));
                else
                    warningLabel.Hide();
            });
        }

        void saveSettings(object sender, EventArgs e)
        {
            if (warningLabel.Visible) warningLabel.Hide();

            if (nBombSelect.Value >= (columnsSelect.Value * rowsSelect.Value))
            {
                warningLabel.Text = "Number of bombs cant't be higher than or equal to number of total boxes (rows times columns)";
                if (!warningLabel.Visible) warningLabel.Show();
                hideWarningLabelAfterAWhile();
                return;
            }

            Program.userChangedSettings = true;

            Program.nBomb = (int)nBombSelect.Value;
            Program.columns = (int)columnsSelect.Value;
            Program.rows = (int)rowsSelect.Value;

            using (StreamWriter sw = new StreamWriter("minesweeper_settings"))
            {
                sw.Write("1 ");
                sw.Write(nBombSelect.Value + " ");
                sw.Write(columnsSelect.Value + " ");
                sw.Write(rowsSelect.Value + " ");
            }

            this.Hide();

            Program.gameForm.Controls.Clear();
            Program.initGame();
            Program.gameForm.Show();
        }

        void hideSettingsMenu(object sender, EventArgs e)
        {
            if (Program.nBomb != (int)nBombSelect.Value || Program.columns != (int)columnsSelect.Value || Program.rows != (int)rowsSelect.Value)
            {
                DialogResult choice = MessageBox.Show("You have unsaved changes. Save settings?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (choice == DialogResult.Yes)
                {
                    saveSettings(sender, e);
                }

                else if (choice == DialogResult.No)
                {
                    nBombSelect.Value = Program.nBomb;
                    columnsSelect.Value = Program.columns;
                    rowsSelect.Value = Program.rows;

                    if (warningLabel.Visible) warningLabel.Hide();
                    Program.gameForm.Show();
                    this.Hide();
                }
            }

            else
            {
                if (warningLabel.Visible) warningLabel.Hide();
                Program.gameForm.Show();
                this.Hide();
            }
        }

        NumericUpDown nBombSelect;
        NumericUpDown rowsSelect;
        NumericUpDown columnsSelect;
        Label warningLabel;
        public SettingsMenu()
        {
            this.DoubleBuffered = true;
            this.ShowInTaskbar = false;
            this.FormClosed += SettingsMenu_FormClosed;

            Rectangle Screen = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;

            this.ClientSize = new Size(Screen.Width / 2, Screen.Height / 2);
            this.Text = "Settings";
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ShowInTaskbar = true;
            this.ControlBox = false;

            Label nBombLabel = makeLabel("Number of bombs:", 16, 0, 0, false, null);
            nBombSelect = makeNumericUpDown(Program.nBomb, 1, 1, 400, nBombLabel.Font.Size, 120, nBombLabel.Right + 10, 0);
            nBombSelect.KeyPress += numericUpDown_KeyPress;

            Label columnsLabel = makeLabel("Columns:", 16, 0, nBombSelect.Bottom + 10, false, null);
            columnsSelect = makeNumericUpDown(Program.columns, 1, 5, 50, columnsLabel.Font.Size, nBombSelect.Width, nBombSelect.Left, columnsLabel.Top);
            columnsSelect.KeyPress += numericUpDown_KeyPress;

            Label rowsLabel = makeLabel("Rows:", 16, 0, columnsSelect.Bottom + 10, false, null);
            rowsSelect = makeNumericUpDown(Program.rows, 1, 5, 50, rowsLabel.Font.Size, columnsSelect.Width, columnsSelect.Left, rowsLabel.Top);
            rowsSelect.KeyPress += numericUpDown_KeyPress;

            warningLabel = makeLabel("", 16, 0, rowsSelect.Bottom + 10, true, this.ClientSize.Width);
            warningLabel.MaximumSize = new Size(warningLabel.Width, 0);
            warningLabel.ForeColor = Color.Red;

            Button saveButton = makeButton(Color.Green, Color.White, Color.Black, "SAVE", 16, 120, 60, 0, 0);
            saveButton.Left = this.ClientSize.Width / 2 - saveButton.Width - 3;
            saveButton.Top = this.ClientSize.Height - saveButton.Height - 5;
            saveButton.Click += saveSettings;

            Button cancelButton = makeButton(Color.LightGray, Color.DarkRed, Color.Red, "CANCEL", 11, 120, 60, 0, 0);
            cancelButton.Left = this.ClientSize.Width / 2 + 3;
            cancelButton.Top = saveButton.Top;
            cancelButton.Click += hideSettingsMenu;

            this.Controls.Add(nBombLabel);
            this.Controls.Add(nBombSelect);
            this.Controls.Add(columnsLabel);
            this.Controls.Add(columnsSelect);
            this.Controls.Add(rowsLabel);
            this.Controls.Add(rowsSelect);
            this.Controls.Add(warningLabel);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

        void SettingsMenu_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
