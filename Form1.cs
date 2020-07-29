using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simplex_Method
{
    public partial class Form1 : Form
    {
        double[,] A;//Коэффициенты при x в неравенствах
        double[,,] R;
        double[] B;//Правая часть неравенств
        double[] C;//Коэффициенты при x в функции цели
        double[] M;//Последняя строка М-метода
        int re_j, re_i;//Индекс i разр-го элемента, индекс j разр-го элемента
        int r, c, m_i,r_n;//к-во строк, столбцов, индекс максимума/минимума, количество решений
        double m;//максимальный/минимальный элемент
        Double Aij;
        bool button3_was_pressed = false;
        bool problem_type_min = false;//В ComboBox выбран тип задачи на минимум
        bool M_method = false;//Используется M-method в текущей итерации или нет
        bool B_less_than_zero = false;//Есть среди B_i отрицательные или нет (определить при задании массива)
        Color default_color;
        public Form1()
        {
            InitializeComponent();
            comboBox1.Items.Add("Задача на MAX");
            comboBox1.Items.Add("Задача на MIN");
            comboBox1.SetBounds(10, 10, 150, 30);
            //comboBox1 запретить ввод текста
            comboBox1.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Вводим ограничения (матрицу) в 2-мерный массив и функцию цели
            //Проверить, во всех ли ограничениях стоит знак равенства
            //Балансовые переменные начинаются с номера lenght(X)+1
            //При введении новой балансовой - увеличиваем её номер
            //List column-ов (чтобы динамически создавать)

            //создадим таблицу вывода товаров с колонками 
            //Название, Цена, Остаток

            First_menu(sender, e);

            #region Пример использования DataGridView
            /* for (int i = 0; i < 5; ++i)
            {
                //Добавляем строку, указывая значения колонок поочереди слева направо
                dataGridView1.Rows.Add("Пример 1, Товар " + i, i * 1000, i);
            }*/

            /*for (int i = 0; i < 5; ++i)
            {
                //Добавляем строку, указывая значения каждой ячейки по имени (можно использовать индекс 0, 1, 2 вместо имен)
                dataGridView1.Rows.Add();
                dataGridView1["name", dataGridView1.Rows.Count - 1].Value = "Пример 2, Товар " + i;
                dataGridView1["price", dataGridView1.Rows.Count - 1].Value = i * 1000;
                dataGridView1["count", dataGridView1.Rows.Count - 1].Value = i;
            }*/

            //А теперь просто пройдемся циклом по всем ячейкам
            /*for (int i = 0; i < dataGridView1.Rows.Count; ++i)
            {
                for (int j = 0; j < dataGridView1.Columns.Count; ++j)
                {
                    //Значения ячеек хряняться в типе object
                    //это позволяет хранить любые данные в таблице
                    object o = dataGridView1[j, i].Value;
                }
            }*/
            #endregion
        }
        private void button1_Click(object sender, EventArgs e)//Посчитать||Далее_0
        {
            button1.Hide();
            comboBox1.Hide();
            label1.Hide();
            int i, j;
            r = dataGridView1.Rows.Count;
            c = dataGridView1.Columns.Count - 1;
            A = new double[r, c];
            B = new double[r - 1];
            C = new double[c];
            M = new double[c];
            int x_i = 0;
            int r_i = 1;

            #region Добавление базисных переменных
            for (i = 0; i < r; i++)//Обход по строкам. Подбириаем базисные переменные в зависимости от условий + записываем массив А (запись массива под вопросом)
            {
                if (Convert.ToString(dataGridView1[c - 1, i].Value) == "≥")
                {
                    for (j = 0; j < c - 1; j++)
                    {
                        dataGridView1[j, i].Value = -Convert.ToDouble(dataGridView1[j, i].Value);
                        A[i, j] = Convert.ToDouble(dataGridView1[j, i].Value);
                    }
                    dataGridView1[j + 1, i].Value = -Convert.ToDouble(dataGridView1[j + 1, i].Value);
                    A[i, j] = Convert.ToDouble(dataGridView1[j + 1, i].Value);
                    if (A[i, c - 1] < 0)
                        B_less_than_zero = true;
                    dataGridView1.Rows[i].HeaderCell.Value = "x" + (c + x_i++);
                }
                if (Convert.ToString(dataGridView1[c - 1, i].Value) == "≤")//≤", "=", "≥"
                {
                    for (j = 0; j < c - 1; j++)
                        A[i, j] = Convert.ToDouble(dataGridView1[j, i].Value);
                    dataGridView1.Rows[i].HeaderCell.Value = "x" + (c + x_i++);
                    A[i, j] = Convert.ToDouble(dataGridView1[j + 1, i].Value);
                }
                if (Convert.ToString(dataGridView1[c - 1, i].Value) == "=")
                {
                    if (!M_method)
                    {
                        M_method = true;
                        dataGridView1.Rows.Add();
                        dataGridView1.Rows[r].HeaderCell.Value = "M";
                    }
                    for (j = 0; j < c - 1; j++)
                    {
                        A[i, j] = Convert.ToDouble(dataGridView1[j, i].Value);
                        M[j] -= A[i, j];
                    }
                    dataGridView1.Rows[i].HeaderCell.Value = "r" + (r_i++);
                    A[i, j] = Convert.ToDouble(dataGridView1[j + 1, i].Value);
                    M[j] -= A[i, j];
                }
                if (A[i, c - 1] < 0)//В каждой строке проверяем B_i на положительность
                    B_less_than_zero = true;
                if (i == r - 1)
                {
                    dataGridView1.Rows[i].HeaderCell.Value = "L";
                    for (j = 0; j < c; j++)
                        if (j != c - 1)
                        {
                            if (comboBox1.SelectedIndex == 0)
                            {
                                dataGridView1[j, i].Value = -Convert.ToDouble(dataGridView1[j, i].Value);                                
                            }
                            if (comboBox1.SelectedIndex == 1)
                                problem_type_min = true;
                            A[i, j] = Convert.ToDouble(dataGridView1[j, i].Value);
                        }
                        else
                        {
                            if (comboBox1.SelectedIndex == 0)
                                dataGridView1[j, i].Value = -Convert.ToDouble(dataGridView1[j, i].Value);
                            A[i, j] = Convert.ToDouble(dataGridView1[j + 1, i].Value);
                        }
                }
            }
            #endregion

            dataGridView1.Columns.RemoveAt(Convert.ToInt16(dataGridView1.Columns.Count) - 2);
            //Одним из способов выбрать re_i, re_j
            m = 0;
            m_i = 0;

            #region Простой СМ
            if (!(B_less_than_zero | M_method))
            {
                for (j = 0; j < c - 1; j++)
                {
                    if (A[r - 1, j] < m)
                    {
                        m = A[r - 1, j];
                        re_j = j;
                    }
                }
                if (m == 0)
                {
                    MessageBox.Show("Нет решения." + Environment.NewLine + "Все коэффициенты целевой функции больше или равны нулю.");
                    button3.Enabled = false;
                }
                if (button3.Enabled)
                {
                    m = 999999999;
                    for (i = 0; i < r - 1; i++)
                    {
                        if (A[i, re_j] > 0)
                        {
                            if ((A[i, c - 1] / A[i, re_j]) < m)
                            {
                                m = A[i, c - 1] / A[i, re_j];
                                re_i = i;
                            }
                        }
                    }
                    if (m == 999999999)
                    {
                        button3.Enabled = false;
                        if (problem_type_min)
                            A[r - 1, c - 1] = -1 * A[r - 1, c - 1];
                        MessageBox.Show("Невозможно дальше улучшать"+Environment.NewLine+"Ответ: "+Convert.ToString(A[r-1,c-1]));
                    }
                }
            }
            #endregion

            #region Отрицательные B_i
            //Если есть B_i меньше нуля, то среди них выбираем наибольший по модулю. Это будет разрешающая строка.             
            if (B_less_than_zero)
            {
                if (r < dataGridView1.Rows.Count)//Надо додумать. Можно не удалять строку, потому что потом может пригодиться.
                    dataGridView1.Rows.RemoveAt(r);
                for (i = 0; i < r - 1; i++)
                {
                    if (A[i, c - 1] < m)
                    {
                        m = A[i, c - 1];
                        re_i = i;
                    }
                    if (Convert.ToString(dataGridView1.Rows[i].HeaderCell) == "L")//Если есть M строка, то менять от 0 до r будет неправильно
                        break;
                }
                m = 0;
                //В разрешающей строке находим среди отрицательных элементов наибольший по модулю. Это будет разрешающий столбец.
                for (j = 0; j < c - 1; j++)//Столбец B_i не трогаем
                {
                    if (A[re_i, j] < m)
                    {
                        m = A[re_i, j];
                        re_j = j;
                    }
                }
                if (m == 0)
                {
                    MessageBox.Show("Нет отрицательных элементов." + Environment.NewLine + "Решения нет." + Environment.NewLine + "Условия несовместны.");
                    button3.Enabled = false;
                }
                M_method = false;//Уточнить преимущество
            }
            #endregion

            #region М-метод
            //Метод искусственного базиса. Добавляем строку M.
            //Каждый элемент строки - сумма по столбцу из строк, где есть искусственный базис, со знаком минус.
            if (M_method)
            {
                /*for (i = 0; i < r; i++)
                    for (j = 0; j < c; j++)
                        dataGridView1[j, i].Value = A[i, j];*/
                for (j = 0; j < c; j++)
                    dataGridView1[j, i].Value = M[j];
                for (j = 0; j < c - 1; j++)//Столбец B_i не трогаем
                {
                    if (M[j] < m)
                    {
                        m = M[j];
                        re_j = j;
                    }
                }
                if (m == 0)
                {
                    if (problem_type_min)
                        A[r - 1, c - 1] = -1 * A[r - 1, c - 1];
                    MessageBox.Show("Нет отрицательных элементов." + Environment.NewLine + "Дальше улучшать нельзя."
                        + Environment.NewLine + "Ответ:" + Convert.ToString(A[r - 1, c - 1]));
                    button3.Enabled = false;
                }
                if (button3.Enabled)
                {
                    m = 999999999;
                    for (i = 0; i < r - 1; i++)
                    {
                        if (Convert.ToString(dataGridView1.Rows[i].HeaderCell) == "L")//Если есть M строка, то менять от 0 до r будет неправильно
                            break;
                        if (A[i, re_j] > 0)
                        {
                            if (A[i, c - 1] / A[i, re_j] < m)
                            {
                                m = A[i, c - 1] / A[i, re_j];
                                re_i = i;
                            }
                        }
                    }
                    if (m == 999999999)
                    {
                        if (problem_type_min)
                            A[r - 1, c - 1] = -1 * A[r - 1, c - 1];
                        MessageBox.Show("Нет отрицательных элементов." + Environment.NewLine +
                            "Дальше улучшать нельзя." + Environment.NewLine + "Ответ:" + Convert.ToString(A[r - 1, c - 1]));
                        button3.Enabled = false;
                    }
                }
                //M_method = false;
                dataGridView1.Height += 20;
            }
            #endregion

            if (button3.Enabled)
            {
                dataGridView1.EnableHeadersVisualStyles = false;//Иначе выбирается стандратный фон для заголовков
                default_color = dataGridView1.Rows[0].HeaderCell.Style.BackColor;
                dataGridView1.Rows[re_i].HeaderCell.Style.BackColor = Color.Yellow;
                dataGridView1.Rows[re_i].DefaultCellStyle.BackColor = Color.Yellow;
                dataGridView1.Columns[re_j].HeaderCell.Style.BackColor = Color.Yellow;
                dataGridView1.Columns[re_j].DefaultCellStyle.BackColor = Color.Yellow;
                dataGridView1.Rows[re_i].Cells[re_j].Style.BackColor = Color.Green;
                dataGridView1.Width -= 55;
                this.Size = new System.Drawing.Size(dataGridView1.Width + 50, dataGridView1.Height + 150);
            }
            B_less_than_zero = false;
            M_method = false;
            button3.SetBounds(this.Width - 140, 10 + dataGridView1.Height + 10, 100, 30);
            button4.SetBounds(this.Width - 140, 45 + dataGridView1.Height + 10, 100, 30);
            button3.Show();
            button4.Show();
        }
        private void button2_Click(object sender, EventArgs e)//Создать таблицу
        {
            //dataGridView1.Rows.Clear();
            //dataGridView1.Columns.Clear();
            int i = 0;
            int t_i = 0;
            int c, r; //columns,rows int table
            r_n = 0;//Обнуляем количество решений
            if (textBox1.Text == "Введите количество строк (учитывая функцию цели)")
            {
                textBox1.Show();
                r = 0;
                t_i = 1;
            }
            else
                r = Convert.ToInt16(textBox1.Text);
            if (textBox2.Text == "Введите количество столбцов (количество переменных)")
            {
                textBox2.Show();
                c = 0;
                t_i++;
            }
            else
                c = Convert.ToInt16(textBox2.Text);

            if (t_i > 0)
            {
                MessageBox.Show("Вы оставили незаполненные поля");
            }
            else
            {
                if (textBox1.Visible)
                    textBox1.Hide();
                if (textBox2.Visible)
                    textBox2.Hide();
                button2.Hide();
                if (c != 0)
                {
                    DataGridViewColumn[] column = new DataGridViewColumn[c + 1];
                    for (i = 0; i < c; i++)
                    {
                        column[i] = new DataGridViewColumn();
                        column[i].HeaderText = "x" + (i + 1);
                        column[i].Width = 50;
                        column[i].ReadOnly = false;
                        column[i].Frozen = true;//Флаг, что данная колонка всегда отображается на своем месте
                        column[i].CellTemplate = new DataGridViewTextBoxCell();
                    }
                    column[i] = new DataGridViewColumn();
                    column[i].HeaderText = "b";
                    column[i].Width = 50;
                    column[i].ReadOnly = false;
                    column[i].Frozen = true;
                    column[i].CellTemplate = new DataGridViewTextBoxCell();
                    for (i = 0; i < c + 2; i++)
                    {
                        if (i != c + 1)
                        {
                            dataGridView1.Columns.Add(column[i]);
                            dataGridView1.Columns[i].DefaultCellStyle.Format = "N2";
                        }
                        else
                        {
                            DataGridViewComboBoxColumn combocolumn = new DataGridViewComboBoxColumn();
                            combocolumn.Items.AddRange("≤", "=", "≥");
                            combocolumn.HeaderText = "";
                            combocolumn.Width = 50;
                            combocolumn.ReadOnly = false;
                            combocolumn.Frozen = true;
                            dataGridView1.Columns.Insert(c, combocolumn);
                        }
                    }
                }
                if ((r != 0) & (c != 0))
                {
                    for (i = 0; i < r; i++)
                    {
                        dataGridView1.Rows.Add();
                        if (i < r - 1)
                        {
                            //dataGridView1.Rows[i].HeaderCell.Value = "x" + (c + 1 + i);
                            dataGridView1[c, i].Value = "≤";
                        }
                        else
                        {
                            //dataGridView1.Rows[i].HeaderCell.Value = "L";
                            dataGridView1[c, i].Value = null;
                            dataGridView1.Rows[i].Cells[c].ReadOnly = true;
                        }
                    }
                    /*dataGridView1.ColumnHeadersHeight = 21;*/
                    dataGridView1.SetBounds(10, 10, 50 * (1 + c + 1 + 1) + 5, /*dataGridView1.ColumnHeadersHeight*/21 * (r + 2));
                    this.Size = new System.Drawing.Size(dataGridView1.Width + 50, dataGridView1.Height + 120);
                    dataGridView1.Show();
                    dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView1.RowHeadersWidth = 55;
                    dataGridView1.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    button1.SetBounds(10, 10 + /*dataGridView1.ColumnHeadersHeight*/21 * (r + 2) + 10, 100, 30);
                    button1.Show();
                    comboBox1.SetBounds(150, 10 + /*dataGridView1.ColumnHeadersHeight*/21 * (r + 2) + 10, 100, 30);
                    comboBox1.Show();
                    label1.SetBounds(150, 10 + /*dataGridView1.ColumnHeadersHeight*/21 * (r + 2) + 10 + 20, 100, 30);
                    label1.Show();
                    dataGridView1.AllowUserToAddRows = false; //запрещаем пользователю самому добавлять строки
                    this.CenterToScreen();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)//Пересчитать||Далее
        {
            //Убрать закрашивание
            //Пересчитать
            //Если сделали M-method, то удалить столбец re_j
            //Определить (есть ли B_i<0 или строки с r_i)
            //Дальше выбрать один из 3х методов определения re_i, re_j и получить их, затем по новой до условия остановки.

            int i = 0;
            int j = 0;

            dataGridView1.Rows[re_i].HeaderCell.Style.BackColor = default_color;
            dataGridView1.Columns[re_j].HeaderCell.Style.BackColor = default_color;
            for (j = 0; j < c; j++)
                dataGridView1.Rows[re_i].Cells[j].Style.BackColor = Color.White;
            for (i = 0; i < r; i++)
            {
                dataGridView1.Rows[i].Cells[re_j].Style.BackColor = Color.White;
                if (r < dataGridView1.Rows.Count)
                    dataGridView1.Rows[i + 1].Cells[re_j].Style.BackColor = Color.White;
            }
            string temp = null;
            temp = Convert.ToString(dataGridView1.Rows[re_i].HeaderCell.Value);
            dataGridView1.Rows[re_i].HeaderCell.Value = dataGridView1.Columns[re_j].HeaderCell.Value;
            dataGridView1.Columns[re_j].HeaderCell.Value = temp;
            Aij = Convert.ToDouble(dataGridView1[re_j, re_i].Value);
            for (j = 0; j < c; j++)
            {
                dataGridView1[j, re_i].Value = Convert.ToDouble(dataGridView1[j, re_i].Value) / Aij;
                M[j] = 0;
            }
            for (i = 0; i < r; i++)
            {
                if (i != re_i)
                    dataGridView1[re_j, i].Value = Convert.ToDouble(dataGridView1[re_j, i].Value) / -Aij;
                else
                    dataGridView1[re_j, i].Value = 1 / Aij;
            }
            for (i = 0; i < r; i++)
            {
                if (i != re_i)
                {
                    for (j = 0; j < c; j++)
                    {
                        if (j != re_j)
                        {
                            dataGridView1[j, i].Value = (A[i, j] * Aij - A[i, re_j] * A[re_i, j]) / Aij;
                        }
                        else continue;
                    }
                }
                else continue;
            }
            if (r < dataGridView1.Rows.Count)
                for (j = 0; j < c; j++)
                    if ((Convert.ToString(dataGridView1.Columns[j].HeaderCell.Value).Substring(0, 1)) == "r")
                    {
                        //MessageBox.Show("");
                        dataGridView1.Columns.RemoveAt(j);
                        c--;
                        A = null;
                        A = new double[r, c];
                    }
            for (i = 0; i < r - 1; i++)
            {
                if (Convert.ToDouble(dataGridView1[c - 1, i].Value) < 0)
                {
                    B_less_than_zero = true;
                }
                if ((Convert.ToString(dataGridView1.Rows[i].HeaderCell.Value).Substring(0, 1)) == "r")
                {
                    for (j = 0; j < c; j++)
                    {
                        M[j] -= Convert.ToDouble(dataGridView1[j, i].Value);
                    }
                    M_method = true;
                }
            }

            for (j = 0; j < c; j++)
            {
                for (i = 0; i < r; i++)
                {
                    A[i, j] = Math.Round(Convert.ToDouble(dataGridView1[j, i].Value),5);
                    if (r < dataGridView1.Rows.Count)
                        dataGridView1[j, r].Value = M[j];
                }
            }

            m = 0;
            m_i = 0;
            #region Простой СМ
            if (!(B_less_than_zero | M_method))
            {
                bool temp_bool = false;
                for (j = 0; j < c - 1; j++)
                {
                    if (A[r - 1, j] <= m)
                    {
                        if(m == A[r - 1, j])
                            temp_bool=true;
                        m = A[r - 1, j];
                        re_j = j;
                    }
                }
                if ((m == 0)&(!temp_bool))
                {
                    if (problem_type_min)
                        A[r - 1, c - 1] = -1 * A[r - 1, c - 1];
                    MessageBox.Show("Решение получено." + Environment.NewLine +
                    "Все коэффициенты целевой функции больше или равны нулю." + Environment.NewLine + "Ответ: " + A[r - 1, c - 1]);
                    button3.Enabled = false;
                }
                if (button3.Enabled)
                {
                    m = 999999999;
                    for (i = 0; i < r - 1; i++)
                    {
                        if (A[i, re_j] > 0)
                        {
                            if ((A[i, c - 1] / A[i, re_j]) < m)
                            {
                                m = A[i, c - 1] / A[i, re_j];
                                re_i = i;
                            }
                        }
                    }
                    if (m == 999999999)
                    {
                        if (problem_type_min)
                            A[r - 1, c - 1] = -1 * A[r - 1, c - 1];
                        MessageBox.Show("Решение получено." + Environment.NewLine +
                        "Все коэффициенты целевой функции больше или равны нулю." + Environment.NewLine + "Ответ: " + A[r - 1, c - 1]);
                        button3.Enabled = false;
                    }
                    if (temp_bool)
                    {
                        //R = new double[r, c, ++r_n];//Как не возвращаться к старому решению?
                        /*for ( i = 0; i < r; i++)
                            for ( j = 0; j < c; j++)
                            {
                                R[i, j, r_n - 1] = A[i, j];
                            }*/
                        if (problem_type_min)
                            A[r - 1, c - 1] = -1 * A[r - 1, c - 1];
                        MessageBox.Show("Решение получено." + Environment.NewLine + "Ответ: " + A[r - 1, c - 1] +
                        Environment.NewLine + "Есть другое решение");
                    }
                }
            }
            #endregion

            #region Отрицательные B_i
            //Если есть B_i меньше нуля, то среди них выбираем наибольший по модулю. Это будет разрешающая строка.             
            if (B_less_than_zero)
            {
                for (i = 0; i < r - 1; i++)
                {
                    if (A[i, c - 1] < m)
                    {
                        m = A[i, c - 1];
                        re_i = i;
                    }
                    if (Convert.ToString(dataGridView1.Rows[i].HeaderCell) == "L")//Если есть M строка, то менять от 0 до r будет неправильно
                        break;
                }
                m = 0;
                //В разрешающей строке находим среди отрицательных элементов наибольший по модулю. Это будет разрешающий столбец.
                for (j = 0; j < c - 1; j++)//Столбец B_i не трогаем
                {
                    if (A[re_i, j] < m)
                    {
                        m = A[re_i, j];
                        re_j = j;
                    }
                }
                if (m == 0)
                {
                    MessageBox.Show("Нет отрицательных элементов среди Aij."
                        + Environment.NewLine + "Решения нет." + Environment.NewLine + "Условия несовместны.");
                    button3.Enabled = false;
                }
                M_method = false;//Уточнить преимущество
            }
            #endregion

            #region М-метод
            //Метод искусственного базиса. Добавляем строку M.
            //Каждый элемент строки - сумма по столбцу из строк, где есть искусственный базис, со знаком минус.
            if (M_method)
            {
                for (i = 0; i < r; i++)
                    for (j = 0; j < c; j++)
                        dataGridView1[j, i].Value = A[i, j];
                for (j = 0; j < c - 1; j++)//Столбец B_i не трогаем
                {
                    if (M[j] < m)
                    {
                        m = M[j];
                        re_j = j;
                    }
                }
                if (m == 0)//Возможно, не нужно!!!! Недостижимо?
                {
                    if (problem_type_min)
                        A[r - 1, c - 1] = -1 * A[r - 1, c - 1];
                    MessageBox.Show("Решение получено." + Environment.NewLine + "Ответ: " + Convert.ToString(A[r - 1, c - 1]));
                    button3.Enabled = false;
                }
                if (button3.Enabled)
                {
                    m = 999999999;
                    for (i = 0; i < r - 1; i++)
                    {
                        if (Convert.ToString(dataGridView1.Rows[i].HeaderCell) == "L")//Если есть M строка, то менять от 0 до r будет неправильно
                            break;
                        if (A[i, re_j] > 0)
                        {
                            if (A[i, c - 1] / A[i, re_j] < m)
                            {
                                m = A[i, c - 1] / A[i, re_j];
                                re_i = i;
                            }
                        }
                    }
                    /*if (m == 999999999)//Возможно, не нужно!!!!!!! Недостижимо?
                    {
                        if (problem_type_min)
                            A[r - 1, c - 1] = -1 * A[r - 1, c - 1];
                        MessageBox.Show("Решение получено." + Environment.NewLine + "Ответ: " + Convert.ToString(A[r - 1, c - 1]));
                        button3.Enabled = false;
                    }*/
                }
                //M_method = false;
                //dataGridView1.Height += 20;//Потом
            }
            #endregion

            if (button3.Enabled)
            {
                //dataGridView1.EnableHeadersVisualStyles = false;//Иначе выбирается стандратный фон для заголовков
                dataGridView1.Rows[re_i].HeaderCell.Style.BackColor = Color.Yellow;
                for (j = 0; j < c; j++)
                    dataGridView1.Rows[re_i].Cells[j].Style.BackColor = Color.Yellow;
                dataGridView1.Columns[re_j].HeaderCell.Style.BackColor = Color.Yellow;
                for (i = 0; i < r; i++)
                {
                    dataGridView1.Rows[i].Cells[re_j].Style.BackColor = Color.Yellow;
                    if (r < dataGridView1.Rows.Count)
                        dataGridView1.Rows[r].Cells[re_j].Style.BackColor = Color.Yellow;
                }
                dataGridView1.Rows[re_i].Cells[re_j].Style.BackColor = Color.Green;
                //dataGridView1.Width -= 55;
                //this.Size = new System.Drawing.Size(dataGridView1.Width + 50, dataGridView1.Height + 150);
            }
            B_less_than_zero = false;
            M_method = false;

            #region Старый вариан (без "равенства" и "больше-равно")
            /////////////////////////////
            /*
            double mm = -1;
            if (button3_was_pressed)
            {
                dataGridView1.Rows[re_i].HeaderCell.Style.BackColor = default_color;
                dataGridView1.Columns[re_j].HeaderCell.Style.BackColor = default_color;
                for (int j = 0; j < c; j++)
                    dataGridView1.Rows[re_i].Cells[j].Style.BackColor = Color.White;
                for (int i = 0; i < r; i++)
                    dataGridView1.Rows[i].Cells[re_j].Style.BackColor = Color.White;
                for (int i = 0; i < r; i++)
                    for (int j = 0; j < c; j++)
                    {
                        A[i, j] = Convert.ToDouble(dataGridView1[j, i].Value);
                    }
                m = 0;
                for (int j = 0; j < c - 1; j++)
                {
                    if (A[r - 1, j] < m)
                    {
                        m = A[r - 1, j];
                        m_i = j;
                    }
                }
                mm = m;
                if (m > 0)//Если нет отрицательных или нулевых значений mm>0 ??
                {
                    goto A;
                }
                re_j = m_i;
                dataGridView1.Rows[0].Cells[re_j].Style.BackColor = Color.Yellow;
                if ((A[0, re_j] > 0)&(A[0, c - 1]>0))//Разрешающий элемент и элемент из B больше нуля
                {
                    m = A[0, c - 1] / A[0, re_j];
                }
                else
                    m = 999999999;
                m_i = -1;//Если в столбце B не будет элементов > 0, то не получится найти разрешающий столбец
                if (A[0, c - 1] > 0)
                    m_i = 0;//Если в первой строке минимальное часное от деления, то m_i останется 0
                for (int i = 1; i < r - 1; i++)//было i от 0, стало от 1
                {
                    dataGridView1.Rows[i].Cells[re_j].Style.BackColor = Color.Yellow;
                    if ((A[i, re_j] != 0) & (A[0, c - 1] > 0))
                    {
                        if ((A[i, c - 1] / A[i, re_j]) < m)
                        {
                            m = A[i, c - 1] / A[i, re_j];
                            m_i = i;
                        }
                    }
                    else
                        m = 999999999;
                }
                dataGridView1.Rows[r - 1].Cells[re_j].Style.BackColor = Color.Yellow;
                if (m_i > -1)
                    re_i = m_i;
                else
                {
                    button3.Enabled = false;
                    MessageBox.Show("Невозможно дальше улучшать");
                    mm = 1;
                    button3.Enabled = false;
                }
                for (int j = 0; j < c; j++)
                    dataGridView1.Rows[re_i].Cells[j].Style.BackColor = Color.Yellow;
                dataGridView1.Rows[re_i].HeaderCell.Style.BackColor = Color.Yellow;
                dataGridView1.Columns[re_j].HeaderCell.Style.BackColor = Color.Yellow;
                dataGridView1.Rows[re_i].Cells[re_j].Style.BackColor = Color.Green;
            }
        A: if (mm < 0)//Тогда есть ещё отрицательный элемент
            {
                string temp = null;
                temp = Convert.ToString(dataGridView1.Rows[re_i].HeaderCell.Value);
                dataGridView1.Rows[re_i].HeaderCell.Value = dataGridView1.Columns[re_j].HeaderCell.Value;
                dataGridView1.Columns[re_j].HeaderCell.Value = temp;
                Aij = Convert.ToDouble(dataGridView1[re_j, re_i].Value);
                for (int j = 0; j < c; j++)
                {
                    dataGridView1[j, re_i].Value = Convert.ToDouble(dataGridView1[j, re_i].Value) / Aij;
                }
                for (int i = 0; i < r; i++)
                {
                    if (i != re_i)
                        dataGridView1[re_j, i].Value = Convert.ToDouble(dataGridView1[re_j, i].Value) / -Aij;
                }
                for (int i = 0; i < r; i++)
                {
                    if (i != re_i)
                    {
                        for (int j = 0; j < c; j++)
                        {
                            if (j != re_j)
                            {
                                dataGridView1[j, i].Value = (A[i, j] * Aij - A[i, re_j] * A[re_i, j]) / Aij;
                            }
                            else continue;
                        }
                    }
                    else continue;
                }
                button3_was_pressed = true;
                bool end = true;
                for (int j = 0; j < c - 1; j++)
                {
                    if (Convert.ToDouble(dataGridView1[j, r - 1].Value) < 0)
                    {
                        end = false;
                    }
                }
                if (end)
                {
                    MessageBox.Show("Дальше улучшать результат нельзя." + Environment.NewLine 
             * + "Ответ: " + Convert.ToDouble(dataGridView1[c-1, r - 1].Value));
                    button3.Enabled = false;
                }
            }
            */
            /////////////////////////////////////
            /*else
            {
                MessageBox.Show("Дальше улучшать результат нельзя");
                button3.Enabled = false;
            }*/
            #endregion

        }

        private void button4_Click(object sender, EventArgs e)//Сначала
        {
            this.Width = 336;
            this.Height = 196;
            r = 0;
            c = 0;
            this.CenterToScreen();
            //this.dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.SetBounds(0, 0, 0, 0);
            button2.Show();
            button3.Enabled = true;
            textBox1.Show();
            textBox2.Show();
            textBox1.Clear();
            textBox2.Clear();
            First_menu(sender, e);
            button3_was_pressed = false;
            problem_type_min = false;
            M_method = false;
            B_less_than_zero = false;
            A = null;
            M = null;
        }

        #region Обработка событий
        public void textBox1_GotFocus(object sender, EventArgs e)
        {
            if (textBox1.Text == "Введите количество строк (учитывая функцию цели)")
            {
                textBox1.Text = null;
                textBox1.Font = new Font(textBox1.Font, FontStyle.Regular);
                textBox1.ForeColor = Color.Black;
            }
        }
        public void textBox2_GotFocus(object sender, EventArgs e)
        {
            if (textBox2.Text == "Введите количество столбцов (количество переменных)")
            {
                textBox2.Text = null;
                textBox2.Font = new Font(textBox2.Font, FontStyle.Regular);
                textBox2.ForeColor = Color.Black;
            }
        }
        public void textBox1_LostFocus(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "Введите количество строк (учитывая функцию цели)";
                textBox1.Font = new Font(textBox1.Font, FontStyle.Italic);
                textBox1.ForeColor = Color.Gray;
            }
        }
        public void textBox2_LostFocus(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                textBox2.Text = "Введите количество столбцов (количество переменных)";
                textBox2.Font = new Font(textBox2.Font, FontStyle.Italic);
                textBox2.ForeColor = Color.Gray;
            }
        }
        #endregion
        #region Первоначальный экран
        private void First_menu(object sender, EventArgs e)
        {
            dataGridView1.Hide();
            comboBox1.Hide();
            comboBox1.SelectedIndex = 0;
            label1.Hide();
            button1.Hide();
            button3.Hide();
            button4.Hide();
            textBox1.Font = new Font(textBox1.Font, FontStyle.Italic);
            textBox1.ForeColor = Color.Gray;
            textBox2.Font = new Font(textBox2.Font, FontStyle.Italic);
            textBox2.ForeColor = Color.Gray;
            textBox1.SelectedText = "Введите количество строк (учитывая функцию цели)";
            textBox2.SelectedText = "Введите количество столбцов (количество переменных)";

            textBox1.GotFocus += textBox1_GotFocus;
            textBox2.GotFocus += textBox2_GotFocus;
            textBox1.LostFocus += textBox1_LostFocus;
            textBox2.LostFocus += textBox2_LostFocus;
        }
        #endregion
    }
}
