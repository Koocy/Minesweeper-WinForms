using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Minesweeper
{
    public class Game : Form
    {
        public Color defaultBoxColor = Color.White, boxOpenColor = Color.LightGray;
        public Bitmap flagImageForBox, bombImageForBox;
        PictureBox flagIcon, restartButton, settingsButton;
        Label rFlagLabel;

        public bool _gameOver, victory;

        public int nBomb, columns, rows, openBoxes, rFlag, boxSize, topPanelH;

        bool firstClick;
        int firstClickX, firstClickY;

        bool calcFontSize;
        float fontSizeBox = 0.00f;

        Box[,] Boxes;

        SettingsMenu settingsMenu;

        void assignDefaultValues()
        {
            _gameOver = false;
            victory = false;
            firstClick = true;
            firstClickX = -1; firstClickY = -1;
            calcFontSize = true;

            int[] settings = GetCurrentSettings();

            nBomb = settings[0];
            columns = settings[1];
            rows = settings[2];

            openBoxes = 0;
            rFlag = nBomb;
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
            box.BackColor = boxOpenColor;
            if (box.nearbyBombs > 0)
            {
                box.Text = box.nearbyBombs.ToString();

                switch (box.nearbyBombs)
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

        void gameOver()
        {
            if (!victory)
            {
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
                restartButton.Image = Resource1.smileyHappy;

                foreach (Box b in Boxes)
                {
                    if (!b.isOpen) b.BackColor = boxOpenColor;
                    if (b.isBomb && !b.isFlag) b.Image = bombImageForBox;
                }
            }
        }

        void Restart()
        {
            SuspendLayout();

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
            rFlagLabel.Text = rFlag.ToString();

            ResumeLayout(false);
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
                ActiveControl = null;

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
                    rFlagLabel.Text = rFlag.ToString();

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
                    rFlagLabel.Text = rFlag.ToString();

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

        void RestartButton_Click(object sender, EventArgs e)
        {
            Restart();
        }

        public int[] GetCurrentSettings()
        {
            if (File.Exists("minesweeper_settings"))
            {
                try
                {
                    using (StreamReader sr = new StreamReader("minesweeper_settings"))
                    {
                        string[] parameters = sr.ReadToEnd().Split(' ');

                        int _nBomb = int.Parse(parameters[0]), _columns = int.Parse(parameters[1]), _rows = int.Parse(parameters[2]);

                        return new int[] { _nBomb, _columns, _rows };
                    }
                }
                catch
                {
                    File.Delete("minesweeper_settings");
                    return new int[] { 16, 10, 12 };
                }
            }

            else return new int[] { 16, 10, 12 };
        }

        public void initGame()
        {
            SuspendLayout();

            assignDefaultValues();

            int boxW = 0, boxH = 0, cr = Math.Max(columns, rows);

            ClientSize = new Size((int)(Screen.PrimaryScreen.WorkingArea.Width * 0.9), (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.9));

            boxW = ClientSize.Width / columns;
            boxH = ClientSize.Height / rows;
            boxSize = Math.Max(boxW, boxH);

            if (boxSize * cr > ClientSize.Width || boxSize * cr > ClientSize.Height)
                if (rows >= columns) boxSize = ClientSize.Height / rows;
                else boxSize = ClientSize.Width / columns;

            topPanelH = boxSize;

            ClientSize = new Size(boxSize * columns, (boxSize * rows) + topPanelH);

            Boxes = new Box[columns, rows];
            int posx = 0, posy = topPanelH;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Box box = new Box();
                    box.x = j; box.y = i;

                    box.Size = new Size(boxSize, boxSize);

                    box.Left = posx; box.Top = posy;

                    box.Text = "";
                    box.TextAlign = ContentAlignment.MiddleCenter;

                    if (calcFontSize)
                    {
                        using (var g = box.CreateGraphics())
                        {
                            fontSizeBox = boxSize * 72f / g.DpiY - 4;
                        }

                        calcFontSize = false;
                    }

                    box.Font = new Font(new FontFamily(GenericFontFamilies.SansSerif), fontSizeBox, FontStyle.Bold);
                    box.BorderStyle = BorderStyle.FixedSingle;

                    box.MouseClick += box_MousePressed;

                    Boxes[j, i] = box;
                    Controls.Add(box);
                    if (i == 0 && j == 0) defaultBoxColor = box.BackColor;

                    posx += boxSize;
                }
                posx = 0;
                posy += boxSize;
            }

            flagImageForBox = new Bitmap(Resource1.flag, new Size(boxSize, boxSize));
            bombImageForBox = new Bitmap(Resource1.bomb, new Size(boxSize, boxSize));

            flagIcon = new PictureBox();
            restartButton = new PictureBox();
            settingsButton = new PictureBox();
            rFlagLabel = new Label();

            restartButton.Size = new Size(topPanelH, topPanelH);
            restartButton.Left = ClientSize.Width / 2 - restartButton.Width / 2;
            restartButton.Top = 0;
            restartButton.Image = Resource1.smileyNormal;
            restartButton.Click += RestartButton_Click;
            restartButton.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(restartButton);

            rFlagLabel.Size = new Size(boxSize * 2, topPanelH);
            rFlagLabel.Left = Boxes[columns - 2, 0].Left;
            rFlagLabel.Top = 0;
            rFlagLabel.Text = rFlag.ToString();
            rFlagLabel.TextAlign = ContentAlignment.MiddleCenter;

            rFlagLabel.Font = new Font(new FontFamily(GenericFontFamilies.Serif), fontSizeBox, FontStyle.Bold);
            Controls.Add(rFlagLabel);

            flagIcon.Size = restartButton.Size;
            flagIcon.Left = rFlagLabel.Left - flagIcon.Width;
            flagIcon.Top = 0;
            flagIcon.Image = Resource1.flag;
            flagIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(flagIcon);

            settingsButton.Size = restartButton.Size;
            settingsButton.Left = 0;
            settingsButton.Top = 0;
            settingsButton.Image = Resource1.cog;
            settingsButton.Click += showSettingsMenu;
            settingsButton.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(settingsButton);

            ResumeLayout(true);
        }

        void showSettingsMenu(object sender, EventArgs e)
        {
            settingsMenu.Left = this.Left;
            settingsMenu.Top = this.Top;
            settingsMenu.Show();
            this.Hide();
        }

        public Game()
        {
            this.DoubleBuffered = true;

            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = true;
            this.Text = "Minesweeper";
            this.ClientSize = new Size(Screen.PrimaryScreen.WorkingArea.Width / 5, Screen.PrimaryScreen.WorkingArea.Height / 2);
            this.StartPosition = FormStartPosition.Manual;

            this.BackColor = Color.White;

            int[] settings = GetCurrentSettings();
            nBomb = settings[0];
            columns = settings[1];
            rows = settings[2];

            initGame();

            settingsMenu = new SettingsMenu(this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            flagImageForBox.Dispose();
            bombImageForBox.Dispose();
            Application.Exit();
        }
    }
}
