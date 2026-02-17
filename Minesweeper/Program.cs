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
    class Program
    {
        public static GameForm gameForm;
        static SettingsMenu settingsMenu;

        public static bool _gameOver, victory;
        public static bool calcFontSize, firstRun = true, userChangedSettings = false;

        public static int nBomb, columns, rows, openBoxes, rFlag, boxSize, topPanelH;

        static bool firstClick;
        static int firstClickX, firstClickY;

        static Box[,] Boxes;

        static Color defaultBoxColor, boxOpenColor = Color.LightGray;

        public static Bitmap flagImageForBox, bombImageForBox;

        static PictureBox flagIcon, restartButton, settingsButton;

        static Label rFlagLabel;

        static void assignDefaultValues()
        {
            _gameOver = false;
            victory = false;
            firstClick = true;
            firstClickX = -1; firstClickY = -1;
            calcFontSize = true;

            if (!userChangedSettings)
            {
                nBomb = 16;
                columns = 10;
                rows = 12;
            }

            openBoxes = 0;
            rFlag = nBomb;
        }

        static void assignBombs()
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

        static void openNearbyBoxes(int x, int y)
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

        static void openNearbyBoxesNumbered(int x, int y)
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

        static void gameOver()
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

        static void box_MousePressed(object sender, MouseEventArgs e)
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
                gameForm.ActiveControl = null;

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

        static void showSettingsMenu(object sender, EventArgs e)
        {
            settingsMenu.Left = gameForm.Left;
            settingsMenu.Top = gameForm.Top;
            settingsMenu.Show();
            gameForm.Hide();
        }

        static void RestartButton_Click(object sender, EventArgs e)
        {
            gameForm.SuspendLayout();

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

            gameForm.ResumeLayout(false);
        }

        static float fontSizeBox = 0.00f;
        public static void initGame()
        {
            gameForm.SuspendLayout();

            assignDefaultValues();

            if (firstRun)
            {
                settingsMenu = new SettingsMenu();
                firstRun = false;
            }

            int boxW = 0, boxH = 0, cr = Math.Max(columns, rows);

            gameForm.ClientSize = new Size((int)(Screen.PrimaryScreen.WorkingArea.Width * 0.9), (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.9));

                boxW = gameForm.ClientSize.Width / columns;
                boxH = gameForm.ClientSize.Height / rows;
                boxSize = Math.Max(boxW, boxH);

                if (boxSize * cr > gameForm.ClientSize.Width || boxSize * cr > gameForm.ClientSize.Height)
                    if (rows > columns) boxSize = gameForm.ClientSize.Height / rows;
                    else boxSize = gameForm.ClientSize.Width / columns;

                topPanelH = boxSize;
           
                gameForm.ClientSize = new Size(boxSize * columns, (boxSize * rows) + topPanelH);
        
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
                    gameForm.Controls.Add(box);
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

            restartButton.Size = new Size(topPanelH, topPanelH);
            restartButton.Left = gameForm.ClientSize.Width / 2 - restartButton.Width / 2;
            restartButton.Top = 0;
            restartButton.Image = Resource1.smileyNormal;
            restartButton.Click += RestartButton_Click;
            restartButton.SizeMode = PictureBoxSizeMode.StretchImage;
            gameForm.Controls.Add(restartButton);

                rFlagLabel.Size = new Size(boxSize*2, topPanelH);

            rFlagLabel.Left = Boxes[columns-2, 0].Left;
            rFlagLabel.Top = 0;
            rFlagLabel.Text = rFlag.ToString();
            rFlagLabel.TextAlign = ContentAlignment.MiddleCenter;

            rFlagLabel.Font = new Font(new FontFamily(GenericFontFamilies.Serif), fontSizeBox, FontStyle.Bold);
            gameForm.Controls.Add(rFlagLabel);

            flagIcon.Size = restartButton.Size;
            flagIcon.Left = rFlagLabel.Left - flagIcon.Width;
            flagIcon.Top = 0;
            flagIcon.Image = Resource1.flag;
            flagIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            gameForm.Controls.Add(flagIcon);

            settingsButton.Size = restartButton.Size;
            settingsButton.Left = 0;
            settingsButton.Top = 0;
            settingsButton.Image = Resource1.cog;
            settingsButton.Click += showSettingsMenu;
            settingsButton.SizeMode = PictureBoxSizeMode.StretchImage;
            gameForm.Controls.Add(settingsButton);

            gameForm.ResumeLayout(true);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            gameForm = new GameForm();

            rFlagLabel = new Label();

           if (File.Exists("minesweeper_settings"))
               try
               {
                   using (StreamReader sr = new StreamReader("minesweeper_settings"))
                   {
                       string[] parameters = sr.ReadToEnd().Split(' ');

                       if (parameters[0] == "1")
                       {
                           userChangedSettings = true;
                           nBomb = int.Parse(parameters[1]);
                           columns = int.Parse(parameters[2]);
                           rows = int.Parse(parameters[3]);
                       }
                       else if (parameters[0] == "0") userChangedSettings = false;
                   }
               }
               catch
               {
                   nBomb = 16;
                   columns = 10;
                   rows = 12;
                   userChangedSettings = false;
                   File.Delete("minesweeper_settings");
               }

            initGame();
            Application.Run(gameForm);
        }
    }

    public partial class GameForm : Form
    {

        public GameForm()
        {
            this.DoubleBuffered = true;

            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = true;
            this.Text = "Minesweeper";
            this.ClientSize = new Size(Screen.PrimaryScreen.WorkingArea.Width / 5, Screen.PrimaryScreen.WorkingArea.Height / 2);
            this.StartPosition = FormStartPosition.Manual;
            
            this.BackColor = Color.White;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Program.flagImageForBox.Dispose();
            Program.bombImageForBox.Dispose();
        }
    }

    class Box : Label
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
