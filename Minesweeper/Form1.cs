using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Minesweeper
{
    public partial class GameForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        SettingsMenu settingsMenu;

        bool _gameOver, victory, firstClick, createGraphics;
        int firstClickX, firstClickY;
        public static bool userChangedSettings = false;

        Bitmap flagImageForBox;
        Bitmap bombImageForBox;

        PictureBox flagIcon;
        PictureBox restartButton;
        PictureBox settingsButton;

        Label showRFlag = new Label();
        Label gameOverResultShow = new Label();

        //The following values can be set by user
        public static int nBomb, columns, rows;
        //

        //Calculate these values
        Box[,] Boxes;
        int openBoxes;
        int rFlag, sFlag;
        int boxW, boxH;
        //

        int topPanelH, gameAreaH;

        Color defaultBoxColor;
        Color boxOpenColor = Color.LightGray;

        void assignDefaultValues()
        {
            _gameOver = false;
            victory = false;
            firstClick = true;
            firstClickX = -1; firstClickY = -1;
            createGraphics = true;

            if (!userChangedSettings)
            {
                nBomb = 16;
                columns = 10;
                rows = 12;
            }

            openBoxes = 0;
            rFlag = nBomb;
            sFlag = 0;

            topPanelH = 80;
        }

        void assignBombs()
        {
            int bombs = 0;
            Random random = new Random();

            while (bombs < nBomb)
            {
                int rnd1 = random.Next(0, columns);
                int rnd2 = random.Next(0, rows);

                if (!Boxes[rnd1, rnd2].isBomb && !(firstClickX == rnd1 && firstClickY == rnd2))
                {
                    Boxes[rnd1, rnd2].isBomb = true;

                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            if (x == 0 && y == 0) continue;

                            int nx = rnd1 + x;
                            int ny = rnd2 + y;

                            if (nx >= 0 && nx < columns && ny >= 0 && ny < rows)
                            {
                                Boxes[nx, ny].nearbyBombs++;
                            }
                        }
                    }

                    bombs++;
                }
            }
        }
    
        void openNearbyBoxes(int x, int y)
        {
            if (x < 0 || x > columns - 1 || y < 0 || y > rows - 1) return;

            Box box = Boxes[x, y];

            if (box.isOpen || box.isFlag || box.isBomb) return;

            box.isOpen = true;
            openBoxes++;
            Console.WriteLine("open boxes: " + openBoxes);
            Console.WriteLine("rows*columns: " + rows * columns);
            Console.WriteLine("Boxes: " + Boxes.Length);
            Console.WriteLine("Bombs: " + nBomb);
            Console.WriteLine("rflag: " + rFlag);
            Console.WriteLine("sFlag: " + sFlag);
            box.BackColor = boxOpenColor;
            if (box.nearbyBombs > 0)
            {
                box.Text = box.nearbyBombs.ToString();

                switch(box.nearbyBombs)
                {
                    case 1:
                        box.ForeColor = Color.Blue;
                        break;
                    case 2:
                        box.ForeColor = Color.Red;
                        break;
                    case 3:
                        box.ForeColor = Color.Green;
                        break;
                    case 4:
                        box.ForeColor = Color.Brown;
                        break;
                    case 5:
                        box.ForeColor = Color.Purple;
                        break;
                    case 6:
                        box.ForeColor = Color.Yellow;
                        break;
                    case 7:
                        box.ForeColor = Color.Turquoise;
                        break;
                    case 8:
                        box.ForeColor = Color.Pink;
                        break;
                }

                return;
            }

            for (int Y = -1; Y <= 1; Y++)
            {
                for (int X = -1; X <= 1; X++)
                {
                    if (X == 0 && Y == 0) continue;

                    int nX = x + X;
                    int nY = y + Y;

                    if (nX < 0 || nX > columns - 1 || nY < 0 || nY > rows - 1 || Boxes[nX, nY].isOpen || Boxes[nX, nY].isFlag || Boxes[nX, nY].isBomb) continue;

                    openNearbyBoxes(nX, nY);
                }
            }
        }

        void openNearbyBoxesNumbered(int x, int y)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j == 0 && i == 0) continue;

                    int nx = x + j;
                    int ny = y + i;

                    if (nx < 0 || nx >= columns || ny < 0 || ny >= rows) continue;

                    if (!Boxes[nx, ny].isOpen && !Boxes[nx, ny].isFlag)
                    {
                        if (Boxes[nx, ny].isBomb)
                        {
                            _gameOver = true;
                            victory = false;
                            gameOver();
                            return;
                        }

                        else
                        {
                            openNearbyBoxes(nx, ny);
                        }
                    }
                }
            }
        }

        float fontSizeBox = 0.00f;
        public void initGame()
        {
            this.SuspendLayout();

            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = true;
            this.Text = "Minesweeper";
            this.ClientSize = new Size(600, 800);
            this.StartPosition = FormStartPosition.Manual;
            if (!userChangedSettings)
            {
                this.Left = 10;
                this.Top = 10;
            }
            this.BackColor = Color.White;

            assignDefaultValues();

            gameAreaH = this.ClientSize.Height - topPanelH;
            boxW = this.ClientSize.Width / columns;
            boxH = gameAreaH / rows;

            flagImageForBox = new Bitmap(Resource1.flag, new Size(boxW, boxH));
            bombImageForBox = new Bitmap(Resource1.bomb, new Size(boxW, boxH));

            flagIcon = new PictureBox();
            restartButton = new PictureBox();
            settingsButton = new PictureBox();

            restartButton.Size = new Size(80, 80);
            restartButton.Left = this.ClientSize.Width / 2 - restartButton.Width / 2;
            restartButton.Top = 0;
            restartButton.Image = Resource1.smileyNormal;
            restartButton.Click += RestartButton_Click;
            this.Controls.Add(restartButton);

            flagIcon.Size = new Size(80, 80);
            flagIcon.Left = this.ClientSize.Width - flagIcon.Width - 60;
            flagIcon.Top = 0;
            flagIcon.Image = Resource1.flag;
            this.Controls.Add(flagIcon);

            showRFlag.Size = new Size(80, 80);
            showRFlag.Left = flagIcon.Right;
            showRFlag.Top = 0;
            showRFlag.Text = rFlag.ToString();
            showRFlag.TextAlign = ContentAlignment.MiddleCenter;
            float fontSizeRFlag = 0.00f;
            if (createGraphics)
            {
                using (var g = showRFlag.CreateGraphics())
                {
                    fontSizeRFlag = ((Math.Min(showRFlag.Width, showRFlag.Height) * (72f / g.DpiY)) - 1) / 2;
                }
            }
            showRFlag.Font = new Font(new FontFamily(GenericFontFamilies.Serif), fontSizeRFlag, FontStyle.Bold);
            this.Controls.Add(showRFlag);

            gameOverResultShow.Text = "You Lose";
            gameOverResultShow.Left = 10;
            gameOverResultShow.Font = new Font(new FontFamily(GenericFontFamilies.Serif), 16, FontStyle.Bold);
            gameOverResultShow.AutoSize = true;
            gameOverResultShow.Top = topPanelH / 2 - gameOverResultShow.Height / 2;
            this.Controls.Add(gameOverResultShow);
            gameOverResultShow.Hide();

            settingsButton.Size = new Size(80, 80);
            settingsButton.Left = gameOverResultShow.Right + 10;
            settingsButton.Top = 0;
            settingsButton.Image = Resource1.cog;
            settingsButton.Click += showSettingsMenu;
            this.Controls.Add(settingsButton);

            Boxes = new Box[columns, rows];
            int posx = 0, posy = topPanelH;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Box box = new Box();
                    box.x = j; box.y = i;

                    box.Size = new Size(boxW, boxH);
                    box.Left = posx; box.Top = posy;

                    box.Text = "";
                    box.TextAlign = ContentAlignment.MiddleCenter;

                    if (createGraphics)
                    {
                        using (var g = box.CreateGraphics())
                        {
                            fontSizeBox = (Math.Min(boxW, boxH) * (72f / g.DpiY)) - 1;
                        }

                        createGraphics = false;
                    }

                    box.Font = new Font(new FontFamily(GenericFontFamilies.SansSerif), fontSizeBox, FontStyle.Bold);
                    box.BorderStyle = BorderStyle.FixedSingle;

                    box.MouseClick += box_MousePressed;

                    Boxes[j, i] = box;
                    this.Controls.Add(box);
                    if (i == 0 && j == 0) defaultBoxColor = box.BackColor;

                    posx += boxW;
                }
                posx = 0;
                posy += boxH;
            }

            if (!userChangedSettings)
            settingsMenu = new SettingsMenu();

            this.ResumeLayout(false); ;
            this.PerformLayout();

            this.Show();
            this.BringToFront();
        }

        void showSettingsMenu(object sender, EventArgs e)
        {
            settingsMenu.Show();
            this.Hide();
        }

        void RestartButton_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            assignDefaultValues();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Boxes[j, i].isBomb = false;
                    Boxes[j, i].isOpen = false;
                    Boxes[j, i].isFlag = false;
                    Boxes[j, i].nearbyBombs = 0;
                    Boxes[j, i].Text = "";
                    Boxes[j, i].BackColor = defaultBoxColor;
                    Boxes[j, i].Image = null;
                    Boxes[j, i].Enabled = true;
                    Boxes[j, i].nearbyFlags = 0;
                }
            }

            restartButton.Image = Resource1.smileyNormal;
            showRFlag.Text = rFlag.ToString();
            gameOverResultShow.Hide();

            this.ResumeLayout(false);
        }

        void gameOver()
        {
            if (!victory)
            {
                gameOverResultShow.Text = "You Lose";

                restartButton.Image = Resource1.smileySad;

                foreach (Box b in Boxes)
                {
                    if (b.isBomb)
                    {
                        b.Image = bombImageForBox;
                    }
                    else if (b.nearbyBombs > 0)
                    {
                        b.Text = b.nearbyBombs.ToString();
                        b.BackColor = boxOpenColor;
                    }

                    b.Enabled = false;
                }
            }

            else if (victory)
            {
                gameOverResultShow.Text = "You Win!";

                restartButton.Image = Resource1.smileyHappy;

                foreach (Box b in Boxes)
                {
                    if (!b.isOpen) b.BackColor = boxOpenColor;
                    if (b.isBomb && !b.isFlag) b.Image = bombImageForBox;
                }
            }

            gameOverResultShow.Show();
        }

        void box_MousePressed(object sender, MouseEventArgs e)
        {
            if (_gameOver)
            {
                return;
            }

            Box box = (Box)sender;

            if (firstClick)
            {
                firstClickX = box.x;
                firstClickY = box.y;
                firstClick = false;
                assignBombs();
            }

            if (e.Button == MouseButtons.Left)
            {
                this.ActiveControl = null;

                if (box.isFlag) return;

                else if (box.isOpen && box.nearbyBombs > 0)
                {
                    if (box.nearbyFlags == box.nearbyBombs) openNearbyBoxesNumbered(box.x, box.y);
                }

                if (box.isBomb)
                {
                    _gameOver = true;
                    victory = false;
                    gameOver();
                    return;
                }

                else
                {
                    openNearbyBoxes(box.x, box.y);
                }
            }

            else if (e.Button == MouseButtons.Right)
            {
                if (box.isFlag == false && box.isOpen == false && rFlag > 0)
                {
                    box.isFlag = true;
                    box.BackColor = boxOpenColor;
                    box.Image = flagImageForBox;

                    rFlag--;
                    showRFlag.Text = rFlag.ToString();

                    if (box.isBomb)
                        sFlag++;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (j == 0 && i == 0) continue;

                            int nx = box.x + j;
                            int ny = box.y + i;

                            if (nx < 0 || nx >= columns || ny < 0 || ny >= rows) continue;

                            Boxes[nx, ny].nearbyFlags++;
                        }
                    }
                }
                else if (box.isFlag)
                {
                    box.isFlag = false;
                    box.Image = null;
                    box.BackColor = defaultBoxColor;
                    rFlag++;
                    showRFlag.Text = rFlag.ToString();

                    if (box.isBomb)
                        sFlag--;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (j == 0 && i == 0) continue;

                            int nx = box.x + j;
                            int ny = box.y + i;

                            if (nx < 0 || nx >= columns || ny < 0 || ny >= rows) continue;

                            Boxes[nx, ny].nearbyFlags--;
                        }
                    }
                }
            }
            else return;

            if (openBoxes == Boxes.Length - nBomb)
            {
                victory = true;
                _gameOver = true;
                gameOver();
                return;
            }
        }

        public GameForm()
        {  
            SetProcessDPIAware();
            this.DoubleBuffered = true;

            initGame();
        }
    }

    public partial class SettingsMenu : Form
    {
        Label makeLabel(string Text, float fontSize, int Left, int Top, bool setWidth, int? Width)
        {
            Label lbl = new Label();
            lbl.Text = Text;
            lbl.Font = new Font(new FontFamily(GenericFontFamilies.Serif), fontSize);
            lbl.AutoSize = true;
            lbl.Width = setWidth ? (int)Width : (int)fontSize * Text.ToCharArray().Length;
            lbl.Height += 20;
            lbl.Left = Left; lbl.Top = Top;
            return lbl;
        }

        NumericUpDown makeNumericUpDown(int Value, int Increment, int Minimum, int Maximum, float fontSize, int Width, int Left, int Top)
        {
            NumericUpDown nud = new NumericUpDown();
            nud.Value = Value;
            nud.Increment = Increment;
            nud.Minimum = Minimum;
            nud.Maximum = Maximum;
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

        async void hideWarningLabelAfterAWhile()
        {
            if (warningType == 1)
                await Task.Delay(2500);
            else if (warningType == 2)
                await Task.Delay(4000); 
            warningLabel.Hide();
        }

        void saveSettings(object sender, EventArgs e)
        {
            if (GameForm.nBomb != (int)nBombSelect.Value || GameForm.columns != (int)columnsSelect.Value || GameForm.rows != (int)rowsSelect.Value)
            {
                if (warningLabel.Visible) warningLabel.Hide();

                if (nBombSelect.Value >= (columnsSelect.Value * rowsSelect.Value))
                {
                    warningLabel.Text = "Number of bombs cant't be higher than or equal to number of total boxes (rows times columns)";
                    warningType = 2;
                    if (!warningLabel.Visible) warningLabel.Show();
                    hideWarningLabelAfterAWhile();
                    return;
                }

                DialogResult choice = MessageBox.Show("Changing the settings will start a new game. Do you really want to save?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (choice == DialogResult.Yes)
                {
                    GameForm.userChangedSettings = true;

                    GameForm.nBomb = (int)nBombSelect.Value;
                    GameForm.columns = (int)columnsSelect.Value;
                    GameForm.rows = (int)rowsSelect.Value;

                    this.Hide();

                    Program.gameForm.Controls.Clear();
                    Program.gameForm.initGame();
                }
            }

            else
            {
                warningLabel.Text = "No changes!";
                warningType = 1;
                if (!warningLabel.Visible) warningLabel.Show();
                hideWarningLabelAfterAWhile();
            }
        }

        void hideSettingsMenu(object sender, EventArgs e)
        {
            if (GameForm.nBomb != (int)nBombSelect.Value || GameForm.columns != (int)columnsSelect.Value || GameForm.rows != (int)rowsSelect.Value)
            {
                DialogResult choice = MessageBox.Show("You have unsaved changes. Save settings?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (choice == DialogResult.Yes)
                {
                    saveSettings(sender, e);
                }

                else if (choice == DialogResult.No)
                {
                    nBombSelect.Value = GameForm.nBomb;
                    columnsSelect.Value = GameForm.columns;
                    rowsSelect.Value = GameForm.rows;

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
        int warningType;
        public SettingsMenu()
        {
            this.DoubleBuffered = true;

            this.ClientSize = new Size(600, 800);
            this.Text = "Settings";
            this.StartPosition = FormStartPosition.Manual;
            this.Left = 10;
            this.Top = 10;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ShowInTaskbar = true;
            this.ControlBox = false;

            Label nBombLabel = makeLabel("Number of bombs:", 16, 0, 0, false, null);
            nBombSelect = makeNumericUpDown(GameForm.nBomb, 1, 1, 100, nBombLabel.Font.Size, 120, nBombLabel.Right + 10, 0);
            nBombSelect.KeyPress += numericUpDown_KeyPress;

            Label columnsLabel = makeLabel("Columns:", 16, 0, nBombSelect.Bottom + 10, false, null);
            columnsSelect = makeNumericUpDown(GameForm.columns, 1, 5, 25, columnsLabel.Font.Size, nBombSelect.Width, nBombSelect.Left, columnsLabel.Top);
            columnsSelect.KeyPress += numericUpDown_KeyPress;

            Label rowsLabel = makeLabel("Rows:", 16, 0, columnsSelect.Bottom + 10, false, null);
            rowsSelect = makeNumericUpDown(GameForm.rows, 1, 5, 25, rowsLabel.Font.Size, columnsSelect.Width, columnsSelect.Left, rowsLabel.Top);
            rowsSelect.KeyPress += numericUpDown_KeyPress;

            warningLabel = makeLabel("No changes!", 16, 0, rowsSelect.Bottom + 10, true, this.ClientSize.Width);
            warningLabel.MaximumSize = new Size(warningLabel.Width, 0);
            warningLabel.ForeColor = Color.Red;
            warningLabel.Hide();
            warningType = 1;

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
    }

    public class Box : Label
    {
        public bool isBomb = false, isOpen = false, isFlag = false;
        public int nearbyBombs = 0, nearbyFlags = 0;
        public int x, y;

        public Box() : base()
        {
            this.TabStop = false;
        }
    }
}
