using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Neyrosetka
{
    public partial class Form1 : Form
    {
        private Bitmap cartina, vectorBitmap;
        public IBZ bazaZnaniy;
        private int[] vector;
        public Form1()
        {
            InitializeComponent();
            bazaZnaniy = new BZ();
        }

        private bool IsClicked { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            cartina = new Bitmap(pictureBox1.Width, pictureBox1.Height);//создание нового изображения
            var gr = Graphics.FromImage(cartina);
            gr.FillRectangle(new SolidBrush(Color.Bisque), 0, 0, cartina.Width, cartina.Height);//заполняем цветом
            pictureBox1.Image = cartina;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            Risovatel.Painter(e, Graphics.FromImage(cartina));
            pictureBox1.Image = cartina;

            IsClicked = true;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsClicked)
                Risovatel.Painter(e, Graphics.FromImage(cartina));
            pictureBox1.Image = cartina;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (IsClicked)
            {
                Risovatel.Painter(e, Graphics.FromImage(cartina));
                pictureBox1.Image = cartina;

                vectorBitmap = new Bitmap(Risovatel.ResizeImageMinImage(cartina, 15));
                pictureBox2.Image = Risovatel.ResizeImageMaxImage(vectorBitmap, 300);
                int[,] arr = Parser.BMPToArray(vectorBitmap);
                this.vector = Parser.ArrayToVector(arr);

                IsClicked = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var gr = Graphics.FromImage(cartina);
            gr.FillRectangle(new SolidBrush(Color.White), 0, 0, cartina.Width, cartina.Height);
            pictureBox1.Image = cartina;
        }


        //тренировать
        private void button3_Click(object sender, EventArgs e)
        {

            if (bazaZnaniy.Brain == null)
            {
                IBrain brain = new Brain();//создаем мозг
                brain.Layers = new List<ILayer>();//создаем список слоев
                brain.Layers.Add(new Layer());//создаем входной слой
                brain.Layers[0].Neurons = new List<INeuron>();//создаем список нейронов
                for (int i = 0; i < vector.Length; i++)
                {
                    brain.Layers[0].Neurons.Add(new Neuron());//добавляем новый нейрон в слой
                    //brain.Layers[0].Neurons[i].Dendrites = new List<IDendrite>();//создаем список дендритов
                    //for (int j = 0; j < vector.Length; j++)
                    //{
                    //    brain.Layers[0].Neurons[i].Dendrites.Add(new Dendrite());//добавляем дендрит в нейрон
                    //}
                }
                brain.Layers.Add(new Layer());//создаем выходной слой
                brain.Layers[1].Neurons = new List<INeuron>();//создаем список нейронов
                //for (int i = 0; i < vector.Length; i++)
                //{
                //    brain.Layers[1].Neurons.Add(new Neuron());//добавляем новый нейрон в слой
                //    brain.Layers[1].Neurons[i].Dendrites = new List<IDendrite>();//создаем список дендритов
                //    for (int j = 0; j < vector.Length; j++)
                //    {
                //        brain.Layers[1].Neurons[i].Dendrites.Add(new Dendrite());//добавляем дендрит в нейрон
                //        brain.Layers[1].Neurons[i].Dendrites[j].AddToNeuron(brain.Layers[0].Neurons[i]);//добавляем нейрон в дендрит
                //    }
                //}
                bazaZnaniy.Brain = brain;
            }

            bazaZnaniy.Train(vector, bazaZnaniy.Brain, textBox1.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 1) textBox1.Text = textBox1.Text.Substring(0, 1);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            var c = e.KeyChar;
            e.Handled = !((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'Ё' || c == 'ё' || c == 8 || c == 46);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bazaZnaniy.Baza = bazaZnaniy.LoadXDocument("baza.xml");//загружаем БЗ
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bazaZnaniy.SaveBrain(bazaZnaniy.Brain);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*
            string str = "Array ===================\r\n";
            for (int i = 0; i < vectorBitmap.Height; i++)
            {
                for (int j = 0; j < vectorBitmap.Width; j++)
                    str += "{" + arr[i, j] + "};";
                str += "\r\n";
            }
            str += "List ===================\r\n";
            for (int i = 0; i < vectorBitmap.Height; i++)
            {
                for (int j = i*15; j < vectorBitmap.Width+(i*15); j++)
                    str += "{" + list[j] + "};";
                str += "\r\n";
            }
            richTextBox1.Text = str;*/
            string answer = "";
            string values = "";
            foreach (var valReadChar in bazaZnaniy.ReadChar(vector, bazaZnaniy.Brain, out answer))
            {
                values += "Буква {" + valReadChar.Key + "} : " + valReadChar.Value + "\r\n";
            }

            answer = values + "============ОТВЕТ=============\r\n" + answer;

            richTextBox1.Text = answer;

        }
    }


    public class Parser
    {
        public static int[,] BMPToArray(Bitmap image)
        {
            string[,] arr = new string[image.Height, image.Width];

            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                    arr[i, j] = image.GetPixel(j, i).Name;//ff000000

            int[,] arr1 = new int[image.Height, image.Width];

            for (int i = 0; i < image.Height; i++)
                for (int j = 0; j < image.Width; j++)
                    if (arr[i, j] == "ff000000")
                        arr1[i, j] = 1;
            return arr1;
        }

        public static int[] ArrayToVector(int[,] array) => array.Cast<int>().ToArray();
    }
    public class Risovatel
    {
        public static void Painter(MouseEventArgs e, Graphics gr)
        {
            var pp = new SolidBrush(Color.Black);
            gr.FillRectangle(pp, e.X, e.Y, 20, 20);
        }
        public static Image ResizeImageMinImage(Image originImage, int newSize)
        {
            float width = newSize;
            float height = newSize;
            var image = new Bitmap(originImage);
            float scale = Math.Min(width / image.Width, height / image.Height);
            var bmp = new Bitmap((int)width, (int)height);
            var graph = Graphics.FromImage(bmp);
            graph.InterpolationMode = InterpolationMode.NearestNeighbor;
            var scaleWidth = (int)(image.Width * scale);
            var scaleHeight = (int)(image.Height * scale);
            graph.DrawImage(image, new Rectangle(0, 0, scaleWidth, scaleHeight));
            return bmp;
        }


        public static Image ResizeImageMaxImage(Image originImage, int newSize)
        {
            float width = newSize;
            float height = newSize;
            var image = new Bitmap(originImage);
            float scale = Math.Min(width / image.Width, height / image.Height);
            var bmp = new Bitmap((int)width, (int)height);
            var graph = Graphics.FromImage(bmp);
            graph.InterpolationMode = InterpolationMode.NearestNeighbor;
            var scaleWidth = (int)(image.Width * scale * 1.05);
            var scaleHeight = (int)(image.Height * scale * 1.05);
            graph.DrawImage(image, new Rectangle(0, 0, scaleWidth, scaleHeight));
            return bmp;
        }
    }
}