using System.Drawing;
using System;
using System.Windows.Forms;

namespace mineSweeper
{
    class Tiles
    {
        public PictureBox Pb { get; }
        public Point coords;
        bool isHiden;
        bool isMark;
        public readonly bool isBomb;

        /// <summary>
        /// Скрытый рисунок клетки.
        /// </summary>
        Image faceOfPB;

        /// <summary>
        /// Добавление бомбы на форму
        /// </summary>
        /// <param name="_pb"></param>
        public Tiles(PictureBox _pb)
        {
            isHiden = true;
            isMark = false;
            isBomb = true;

            Pb = _pb;
            Pb.SizeMode = PictureBoxSizeMode.Zoom;
            Pb.AccessibleDescription = "Bomb";

            faceOfPB = GetImage("tiles/bomb.png");
            Pb.Image = GetImage("tiles/hide.png");

            Pb.SizeMode = PictureBoxSizeMode.Zoom;

        }

        /// <summary>
        /// Добавление клетки на форму
        /// </summary>
        /// <param name="_pb"></param>
        /// <param name="_coords">Координаты клетки</param>
        /// <param name="digit"></param>
        public Tiles(PictureBox _pb, Point _coords, int digit)
        {
            coords = _coords;

            isHiden = true;
            isMark = false;
            isBomb = false;

            Pb = _pb;
            Pb.SizeMode = PictureBoxSizeMode.Zoom;
            faceOfPB = (Image)new Bitmap(Pb.Width, Pb.Height);
            Graphics g = Graphics.FromImage(faceOfPB);
            if (digit != 0)
            {
                g.DrawString(
                    "" + digit,
                    new Font("Snap ITC", 10),
                    new SolidBrush(Color.Black),
                    new Point(1, 1));
                Pb.AccessibleDescription = "Digit";
            }
            else
            {
                g.DrawString(
                    "",
                    new Font("Snap ITC", 10),
                    new SolidBrush(Color.Black),
                    new Point(10, 10));
                Pb.AccessibleDescription = "Empty";
            }
            g.Dispose();

            Pb.Image = GetImage("tiles/hide.png");

            Pb.SizeMode = PictureBoxSizeMode.Zoom;
        }

        /// <summary>
        /// Скрыто ли содержимое клетки.
        /// </summary>
        public bool IsHiden
        {
            get => isHiden;
            set => isHiden = HideEvent(value);
        }
        private bool HideEvent(bool value)
        {
            if (value) return value;

            Pb.Image = faceOfPB;

            return false;
        }

        /// <summary>
        /// Помечена ли клеточка флагом.
        /// </summary>
        public bool IsMarked
        {
            get => isMark;
            set => isMark = MarkEvent(value);
        }
        private bool MarkEvent(bool value)
        {
            if (value)
            {
                Pb.Image = GetImage("tiles/flag.png");
                return true;
            }
            Pb.Image = GetImage("tiles/hide.png");
            return value;
        }

        /// <summary>
        /// Возвращает картинку из файла
        /// </summary>
        /// <param name="path">путь к файлу</param>
        /// <returns></returns>
        private Image GetImage(string path)
        {
            try
            {
                return Image.FromFile(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " is not found");
                Application.Exit();
                return null;
            }
        }

        internal void openCells()
        {
            //если угадали
            if (isBomb & isMark)
                Pb.Image = GetImage("tiles/guessBomb.png");
            //если  не угадали
            if (!isBomb & isMark)
                Pb.Image = GetImage("tiles/notGuessBomb.png");
            //если не отметили бомбу
            if (isBomb & !isMark)
                Pb.Image = GetImage("tiles/bomb.png");
            if (!isBomb & !isMark)
                Pb.Image = faceOfPB;
        }
        internal void boomBomb()
        {
            Pb.Image = GetImage("tiles/boom_Bomb.png");
        }
    }
}
