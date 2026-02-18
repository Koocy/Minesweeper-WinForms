using System.Windows.Forms;

namespace Minesweeper
{
    class Box : Label
    {
        public bool isBomb = false, isOpen = false, isFlag = false;
        public int nearbyBombs = 0, nearbyFlags = 0;
        public int x, y;

        public Box()
            : base()
        {
            this.TabStop = false;
        }
    }
}
