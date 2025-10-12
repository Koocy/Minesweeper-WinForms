using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Text;

namespace Minesweeper
{
    public class Box : Label
    {
        public bool isBomb = false, isOpen = false, isFlag = false;
        public int nearby = 0;
        public int x, y;

        public Box() : base()
        {
            this.TabStop = false;
        }
    }

    public partial class Form1 : Form
    {
        public static bool victory;

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        static Bitmap flagImageForBox;
        static Bitmap bombImageForBox;

        static PictureBox flagIcon;
        static PictureBox restartButton;

        static int nBomb;
        static int columns, rows;
        static Box[,] Boxes;
        static int rFlag, sFlag;
        static Label showRFlag;
        static int boxW, boxH;
        static int topPanelH, gameAreaH;

        static Color defaultBoxColor;
        static Color boxOpenColor = Color.LightGray;

        static void assignDefaultValues()
        {
            victory = false;

            restartButton = new PictureBox();
            flagIcon = new PictureBox();
            showRFlag = new Label();

            nBomb = 1;
            rFlag = nBomb;
            sFlag = 0;
            columns = 10;
            rows = 12;

            Boxes = new Box[columns, rows];

            topPanelH = 80;
        }

        static void assignBombs()
        {
            int bombs = 0;
            Random random = new Random();

            while (bombs < nBomb)
            {
                int rnd1 = random.Next(0, columns);
                int rnd2 = random.Next(0, rows);

                if (!Boxes[rnd1, rnd2].isBomb)
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
                                Boxes[nx, ny].nearby++;
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
            box.BackColor = boxOpenColor;
            if (box.nearby > 0)
            {
                box.Text = box.nearby.ToString();

                switch(box.nearby)
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

        void initGameWindow()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = true;
            this.Text = "Minesweeper";
            this.ClientSize = new Size(600, 800);
            this.StartPosition = FormStartPosition.Manual;
            this.Left = 10; this.Top = 10;

            gameAreaH = this.ClientSize.Height - topPanelH;
            boxW = this.ClientSize.Width / columns;
            boxH = gameAreaH / rows;

            flagImageForBox = new Bitmap(Resource1.flag, new Size(boxW, boxH));
            bombImageForBox = new Bitmap(Resource1.bomb, new Size(boxW, boxH));

            restartButton.Size = new Size(80, 80);
            restartButton.Left = this.ClientSize.Width / 2 - restartButton.Width / 2;
            restartButton.Top = 0;
            restartButton.Image = Resource1.smileyNormal;
            restartButton.Click += RestartButton_Click;
            this.Controls.Add(restartButton);

            flagIcon.Size = new Size(80, 80);
            flagIcon.Left = this.ClientSize.Width - flagIcon.Width - boxW;
            flagIcon.Top = 0;
            flagIcon.Image = Resource1.flag;
            this.Controls.Add(flagIcon);

            showRFlag.Size = new Size(80, 80);
            showRFlag.Left = flagIcon.Right;
            showRFlag.Top = 0;
            showRFlag.Text = rFlag.ToString();
            using (var g = showRFlag.CreateGraphics())
            {
                var fontSize = ((Math.Min(showRFlag.Width, showRFlag.Height) * (72f / g.DpiY)) - 1) / 2;
                showRFlag.TextAlign = ContentAlignment.MiddleCenter;
                showRFlag.Font = new Font(new FontFamily(GenericFontFamilies.Serif), fontSize, FontStyle.Bold);
            }
            this.Controls.Add(showRFlag);

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
                    using (var g = box.CreateGraphics())
                    {
                        var fontSize = (Math.Min(boxW, boxH) * (72f / g.DpiY)) - 1;
                        box.TextAlign = ContentAlignment.MiddleCenter;
                        box.Font = new Font(new FontFamily(GenericFontFamilies.SansSerif), fontSize, FontStyle.Bold);
                    }
                    box.BorderStyle = BorderStyle.FixedSingle;

                    box.MouseClick += boxMousePressed;

                    Boxes[j, i] = box;

                    this.Controls.Add(box);
                    if (i == 0 && j == 0) defaultBoxColor = box.BackColor;

                    posx += boxW;
                }
                posx = 0;
                posy += boxH;
            }
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            this.Controls.Clear();

            assignDefaultValues();
            initGameWindow();
            assignBombs();
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
                        b.BackColor = boxOpenColor;
                        b.Image = bombImageForBox;
                    }
                    else if (b.nearby > 0)
                    {
                        b.Text = b.nearby.ToString();
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
                    b.Click += (s, e) => { return; };
                    if (!b.isOpen) b.BackColor = boxOpenColor;
                }
            }
        }

        void boxMousePressed(object sender, MouseEventArgs e)
        {
            Box box = (Box)sender;

            if (e.Button == MouseButtons.Left)
            {
                this.ActiveControl = null;

                if (box.isFlag) return;

                if (box.isBomb)
                {
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
                }

                if (sFlag == nBomb)
                {
                    victory = true;
                    gameOver();
                    return;
                }
            }
        }

        public Form1()
        {
            SetProcessDPIAware();
            InitializeComponent();

            assignDefaultValues();
            initGameWindow();
            assignBombs();
        }
    }
}
