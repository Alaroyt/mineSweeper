using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace mineSweeper
{
    public partial class GameForm : Form
    {
        private readonly int _SizeOfCell = 25;
        static List<Tiles> images = new List<Tiles>();

        bool isLoose = false;
        /// <summary>
        /// how many bombs
        /// </summary>
        int countOfBomb;
        /// <summary>
        /// how many cells are selected
        /// </summary>
        int currentCount;
        /// <summary>
        /// how many bombs are guessed
        /// </summary>
        int correctCount;

        int _n = 20;
        int _bomb = 25;

        public GameForm()
        {
            InitializeComponent();
            _Genmap(_n, _bomb);
            MessageBox.Show("Left click - open the cell, Right click - mark the cells");
        }

        static int[,] Bomb;

        /// <summary>
        /// Генерация карты.
        /// </summary>
        /// <param name="N">Размер поля NxN</param>
        /// <param name="howManyBombsGenerate">Количество бомб на поле</param>
        void _Genmap(int N, int howManyBombsGenerate)
        {
            #region Дизайн формы

            //Размеры формы
            ClientSize = new Size(N * _SizeOfCell, N * _SizeOfCell);

            //MaximumSize = new Size(Width, Height);
            //MinimumSize = new Size(Width, Height);

            //Размеры сетки
            int _Widht = _SizeOfCell * N;
            int _Height = _SizeOfCell * N;

            //Генерация сетки
            for (int i = 0; i <= _Height / _SizeOfCell; i++)
            {
                PictureBox field = new PictureBox
                {
                    Size = new Size(_Widht, 1),
                    Location = new Point(0, _SizeOfCell * i),
                    BackColor = Color.Green
                };
                Controls.Add(field);
            }
            for (int i = 0; i <= _Widht / _SizeOfCell; i++)
            {
                PictureBox field = new PictureBox
                {
                    Size = new Size(1, _Height),
                    Location = new Point(_SizeOfCell * i, 0),
                    BackColor = Color.Green
                };
                Controls.Add(field);
            }
            #endregion

            #region Генерация бомб
            countOfBomb = howManyBombsGenerate;

            //Рандомные точки
            List<int> ind = new List<int>();
            Random rnd = new Random();
            for (int i = 1; i <= howManyBombsGenerate; i++)
            {
                int temp = rnd.Next(0, N * N);
                if (isCellIsBomb(ind, temp))
                    i--;
                ind.Add(temp);
            }

            Bomb = new int[N, N];

            for (int p = 1, i = 0; i < N; i++)
            {
                for (int j = 0; j < N; p++, j++)
                {
                    if (isCellIsBomb(ind, p))
                    {
                        PlaceBombAndMarkAround(i, j);
                    }
                }
            }
            #endregion

            #region Расстановка бомб\цифр
            for (int p = 0, i = 0; i < N; i++)
            {
                for (int j = 0; j < N; p++, j++)
                {

                    PictureBox field = new PictureBox
                    {
                        Size = new Size(_SizeOfCell, _SizeOfCell),
                        Location = new Point(i * _SizeOfCell, j * _SizeOfCell)
                    };

                    field.AccessibleDefaultActionDescription = "" + p;

                    field.MouseClick += (newClick);

                    if (Bomb[i, j] == -1)
                    {
                        images.Add(new Tiles(field));
                        Controls.Add(images[images.Count - 1].Pb);
                    }
                    else
                    {
                        images.Add(new Tiles(field, new Point(i, j), Bomb[i, j]));
                        Controls.Add(images[images.Count - 1].Pb);
                    }

                    Controls.Add(field);
                }
            }
            #endregion
        }

        /// <summary>
        /// Обработка нажатия на клетку.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newClick(object sender, MouseEventArgs e)
        {
            if (!isLoose)
            {
                var t = images[Convert.ToInt32(((PictureBox)sender).AccessibleDefaultActionDescription)];
                if (e.Button == MouseButtons.Left)
                {
                    if (!t.IsMarked)
                    {
                        if (t.Pb.AccessibleDescription == "Empty")
                            openAround(t);
                        else
                        {
                            if (t.Pb.AccessibleDescription == "Bomb")
                            {
                                t.IsHiden = false;
                                openAllCells();
                                isLoose = true;
                                t.boomBomb();
                            }
                            else
                                t.IsHiden = false;
                        }
                    }
                }
                else
                {
                    if (t.IsHiden)
                    {
                        if (!t.IsMarked)
                        {
                            if (currentCount < countOfBomb)
                            {
                                t.IsMarked = !t.IsMarked;
                                currentCount++;
                                if (t.isBomb)
                                    correctCount++;
                            }
                        }
                        else
                        {
                            t.IsMarked = !t.IsMarked;
                            currentCount--;
                            if (t.isBomb)
                                correctCount--;
                        }
                        if (correctCount == countOfBomb)
                        {
                            openAllCells();
                            MessageBox.Show("Поздравляю, вы победили!");
                            Application.Restart();
                        }

                    }
                }
            }
            else
            {
                MessageBox.Show("Вы проиграли :(\nВы угадали " + correctCount + " из " + countOfBomb + " бомб.");
                Application.Restart();
            }
            Text = currentCount + "/" + countOfBomb;
        }

        static List<Tiles> TilesWhichMayOpen;
        static List<Tiles> Expired;
        private void openAround(Tiles tiles)
        {
            Expired = new List<Tiles>();

            TilesWhichMayOpen = new List<Tiles>();
            TilesWhichMayOpen.Add(tiles);

            for (; ; )
            {
                TilesWhichMayOpen[0].IsHiden = !TilesWhichMayOpen[0].IsHiden;

                var temparr = (emptyCellsAround(TilesWhichMayOpen[0]));
                foreach (var el in temparr)
                    TilesWhichMayOpen.Add(el);

                Expired.Add(TilesWhichMayOpen[0]);
                TilesWhichMayOpen.RemoveAt(0);
                if (TilesWhichMayOpen.Count == 0) break;
            }

            var temp = new List<Tiles>();
            for (int i = 0; i <= Expired.Count - 1; i++)
            {
                var temptemp = digitCellsAround(Expired[i]);

                foreach (var el in temptemp) el.IsHiden = !el.IsHiden;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns>Массив пустых клеток, которые находятся рядом с переданной клеткой</returns>
        private List<Tiles> emptyCellsAround(Tiles tiles)
        {
            List<Tiles> emptyCells = new List<Tiles>();

            #region Поиск пустых клеточек рядом с пустой клеточкой
            int i = tiles.coords.X;
            int j = tiles.coords.Y;

            List<int[]> vs = new List<int[]>();
            vs.Add(new int[] { i - 1, j - 1 });
            vs.Add(new int[] { i - 1, j });
            vs.Add(new int[] { i - 1, j + 1 });
            vs.Add(new int[] { i, j - 1 });
            vs.Add(new int[] { i, j + 1 });
            vs.Add(new int[] { i + 1, j - 1 });
            vs.Add(new int[] { i + 1, j });
            vs.Add(new int[] { i + 1, j + 1 });

            foreach (int[] el in vs)
            {
                try
                {
                    if (Bomb[el[0], el[1]] == 0)
                    {
                        var ttile = findTilesByCoords(el[0], el[1]);
                        if (ttile.IsHiden & !ttile.IsMarked & !Expired.Contains(ttile) & !TilesWhichMayOpen.Contains(ttile))
                            emptyCells.Add(ttile);
                    }
                }
                catch { /*lierally nothing*/ }
            }
            #endregion

            return emptyCells;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tiles"></param>
        /// <returns>массив клеточек с цифрами, который находятся рядом с пустыми клетками, который нужно открыть</returns>
        private List<Tiles> digitCellsAround(Tiles tiles)
        {
            List<Tiles> digitCells = new List<Tiles>();

            #region Поиск клеточек с цифрой рядом с пустой клеточкой
            int i = tiles.coords.X;
            int j = tiles.coords.Y;

            List<int[]> vs = new List<int[]>();
            vs.Add(new int[] { i - 1, j - 1 });
            vs.Add(new int[] { i - 1, j });
            vs.Add(new int[] { i - 1, j + 1 });
            vs.Add(new int[] { i, j - 1 });
            vs.Add(new int[] { i, j + 1 });
            vs.Add(new int[] { i + 1, j - 1 });
            vs.Add(new int[] { i + 1, j });
            vs.Add(new int[] { i + 1, j + 1 });

            foreach (int[] el in vs)
            {
                try
                {

                    var ttile = findTilesByCoords(el[0], el[1]);
                    if (ttile.IsHiden & !ttile.IsMarked & !Expired.Contains(ttile) & !TilesWhichMayOpen.Contains(ttile))
                        digitCells.Add(ttile);
                }
                catch { /*lierally nothing*/ }
            }
            #endregion

            return digitCells;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1">i</param>
        /// <param name="v2">j</param>
        /// <returns>Клеточку, найденую по координатам (i,j)</returns>
        private static Tiles findTilesByCoords(int v1, int v2)
        {

            foreach (var t in images.Where(t => t.coords == new Point(v1, v2)))
            { return t; };

            throw new Exception("something wrong");
        }

        /// <summary>
        /// Обводка бомб цифрами.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private static void PlaceBombAndMarkAround(int i, int j)
        {
            Bomb[i, j] = -1;
            List<int[]> vs = new List<int[]>();
            vs.Add(new int[] { i - 1, j - 1 });
            vs.Add(new int[] { i - 1, j });
            vs.Add(new int[] { i - 1, j + 1 });
            vs.Add(new int[] { i, j - 1 });
            vs.Add(new int[] { i, j + 1 });
            vs.Add(new int[] { i + 1, j - 1 });
            vs.Add(new int[] { i + 1, j });
            vs.Add(new int[] { i + 1, j + 1 });

            foreach (int[] el in vs)
            {
                try
                {
                    if (Bomb[el[0], el[1]] != -1)
                        Bomb[el[0], el[1]]++;
                }
                catch { /*lierally nothing*/ }
            }
        }

        /// <summary>
        /// Является ли клеточка бомбой.
        /// </summary>
        /// <param name="ind">Лист с номерами клеточек с бомбами</param>
        /// <param name="p">Номер клеточки</param>
        /// <returns></returns>
        bool isCellIsBomb(List<int> ind, int p)
        {
            foreach (int el in ind)
            {
                if (p == el)
                    return true;
            }
            return false;
        }

        void openAllCells()
        {
            foreach (var el in images)
            {
                el.openCells();
            }
        }
    }
}
