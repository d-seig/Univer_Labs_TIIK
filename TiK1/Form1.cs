using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using NAudio.Wave;
using NAudio.FileFormats;
using System.Threading;

namespace TiK1
{
    public partial class Form1 : Form
    {
        WaveIn waveIn;
        WaveFileWriter writer;
        WaveFileReader reader;
        List<Complex> Fur = new List<Complex>(), ReFur = new List<Complex>();
        string outputFilename = "output.wav", filename1 = "converted_1.wav", filename2 = "converted_2.wav";
        Image image = Image.FromFile("//tr.png");
        float h = 0;
        /// <summary>
        /// Инициализация формы
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Перевод данных из буфера в файл
        /// </summary>
        /// <param name="sender">Слушатель кнопки</param>
        /// <param name="e">Привязанное к кнопке событие</param>
        [Obsolete]
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_DataAvailable), sender, e);
            }
            else
            {
                //Записываем данные из буфера в файл
                writer.WriteData(e.Buffer, 0, e.BytesRecorded);
            }
        }
        /// <summary>
        /// Событие при завершении записи
        /// </summary>
        void StopRecording()
        {
            MessageBox.Show("StopRecording");
            waveIn.StopRecording();
        }
        /// <summary>
        /// Событие при окончании записи
        /// </summary>
        /// <param name="sender">Слушатель кнопки</param>
        /// <param name="e">Привязанное к кнопке событие</param>
        private void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler(waveIn_RecordingStopped), sender, e);
            }
            else
            {
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
            }
        }
        private void WaveformPainter1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Кнопка Start
        /// </summary>
        /// <param name="sender">Слушатель кнопки</param>
        /// <param name="e">Привязанное к кнопке событие</param>
        [Obsolete]
        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                waveIn = new WaveIn();
                //Дефолтное устройство для записи (если оно имеется)
                //встроенный микрофон ноутбука имеет номер 0
                waveIn.DeviceNumber = 0;
                //Прикрепляем к событию DataAvailable обработчик, возникающий при наличии записываемых данных
                waveIn.DataAvailable += waveIn_DataAvailable;
                //Прикрепляем обработчик завершения записи
                waveIn.RecordingStopped += new EventHandler<NAudio.Wave.StoppedEventArgs>(waveIn_RecordingStopped);
                //Формат wav-файла - принимает параметры - частоту дискретизации и количество каналов(здесь mono)
                waveIn.WaveFormat = new WaveFormat(8000, 1);
                //Инициализируем объект WaveFileWriter
                writer = new WaveFileWriter(outputFilename, waveIn.WaveFormat);
                //Начало записи
                waveIn.StartRecording();
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }
        /// <summary>
        /// Кнопка Stop
        /// </summary>
        /// <param name="sender">Слушатель кнопки</param>
        /// <param name="e">Привязанное к кнопке событие</param>
        private void Button2_Click(object sender, EventArgs e)
        {
            if (waveIn != null)
            {
                StopRecording();
            }

        }
        /// <summary>
        /// Основная функция программы
        /// </summary>
        /// <param name="sender">Слушатель кнопки</param>
        /// <param name="e">Привязанное к кнопке событие</param>
        [Obsolete]
        private void Button3_Click(object sender, EventArgs e)
        {
            ClearPainter(waveformPainter1);
            ClearPainter(waveformPainter2);
            InitFunc();
            FurieConvertion();
            MessageBox.Show("Преобразование завершено!");
            BackFurieConvertion();
            ThreadingWriteSignals();
            ReadSignals(filename2);
            MessageBox.Show("Сигнал после преобразования Фурье");
            ReadSignals(filename1);
            MessageBox.Show("This is the end of signal");
        }
        /// <summary>
        /// Отрисовка в PictureBox дискретного сигнала
        /// </summary>
        /// <param name="picture">PictureBox, в котором будет производится отрисовка спектра</param>
        /// <param name="value">Текущее значение аргумента</param>
        private void PaintToPictureBox(PictureBox picture, double value)
        {
            var graph = picture.CreateGraphics();
            if (h == picture.Width)
            {
                h -= picture.Width;
                picture.Image = image;
                if (picture.InvokeRequired)
                {
                    picture.BeginInvoke(new MethodInvoker(
                        delegate ()
                        {
                            picture.Image = image;
                        }));
                }
                else
                    picture.Image = image;
                //graph.DrawLine(new Pen(Color.Red), 0, picture.Height / 2, picture.Width, picture.Height / 2);
                //graph.DrawLine(new Pen(Color.Red), 0, 0, 0, picture.Height);
            }
            graph.DrawLine(new Pen(Color.Black), h, (float)(picture.Height / 2 - value), h, (float)(picture.Height / 2 + value));

        }
        /// <summary>
        /// Отрисовка непрерывного сигнала
        /// </summary>
        /// <param name="picture">PictureBox, в котором будет производится отрисовка спектра</param>
        /// <param name="value">Текущее значение аргумента</param>
        /// <param name="prev">Предыдущее значение аргумента</param>
        private void PaintCW(PictureBox picture, double value, double prev)
        {
            var graph = picture.CreateGraphics();
            if (h == picture.Width)
            {
                h -= picture.Width;
                picture.Image = image;
                if (picture.InvokeRequired)
                {
                    picture.BeginInvoke(new MethodInvoker(
                        delegate ()
                        {
                            picture.Image = image;
                        }));
                }
                else
                    picture.Image = image;
                //graph.DrawLine(new Pen(Color.Red), 0, picture.Height / 2, picture.Width, picture.Height / 2);
                //graph.DrawLine(new Pen(Color.Red), 0, 0, 0, picture.Height);
            }
            graph.DrawLine(new Pen(Color.Black), h, (float)(picture.Height / 2 - value), h, (float)(picture.Height / 2 - prev));
        }
        /// <summary>
        /// Функция-инициализатор записанного файла
        /// </summary>
        private void InitFunc()
        {
            Task outplayTask = new Task(
                delegate ()
                {
                    ReadSignals(outputFilename);
                }
            );
            Task paintTask = new Task(
                delegate ()
                {
                    outplayTask.Wait();
                    //reader.Close();
                    reader = new WaveFileReader(outputFilename);
                    float temp = 0;
                    for (int i = 0; i < reader.Length; ++i, ++h)
                    {
                        try
                        {
                            float sampleFrame = reader.ReadNextSampleFrame()[0];
                            waveformPainter1.AddMax(sampleFrame*10); // NullDeveloper'sBrainException // NullReferenceExc
                            //PaintToPictureBox(pictureBox2, sampleFrame * 1000);
                            PaintCW(pictureBox2, sampleFrame * 1000, temp*1000);
                            temp = sampleFrame;
                            //Thread.Sleep(7);
                        }
                        catch (NullReferenceException)
                        {
                            MessageBox.Show($"Values = {i}");
                            break;
                        }
                    }
                    h = 0;
                    reader.Close();
                }
                );
            outplayTask.Start();
            paintTask.Start();
            paintTask.Wait();
            //outplayTask.Dispose();
            //paintTask.Dispose();
        }
        /// <summary>
        /// Используется для отчистки полей WaveformPainter, т.к. в их основе лежит список на 1000 элементов
        /// </summary>
        /// <param name="wfp">Очищаемый WaveFormPainter</param>
        private void ClearPainter(NAudio.Gui.WaveformPainter wfp)
        {
            for (int i = 0; i < 1000; ++i)
            {
                wfp.AddMax(0);
            }
        }
        /// <summary>
        /// Обратное преобразование Фурье (ОПФ)
        /// </summary>
        private void BackFurieConvertion()
        {
            var ReFurThread = Task.Factory.StartNew(delegate () {
                ReFur.Clear(); // обнуление списка значений
                Complex sum;
                float prev = 0;
                for (int i = 0; i < Fur.Count; ++i, ++h)
                {
                    sum = new Complex(0, 0);
                    double y_fur;
                    for (int k = 0; k < Fur.Count; ++k)
                    {
                        y_fur = (2 * Math.PI * i * k) / reader.Length;
                        double complex_real = Math.Cos(y_fur);
                        double complex_imag = Math.Sin(y_fur);
                        sum += Fur[k] * new Complex(complex_real, complex_imag);
                    }
                    ReFur.Add(sum / Fur.Count);
                    waveformPainter2.AddMax((float)(sum / Fur.Count).module * 10);
                    //PaintCW(pictureBox3, (sum / Fur.Count).module * 1000, prev * 1000);
                    PaintToPictureBox(pictureBox3, (sum / Fur.Count).module * 1000);
                    prev = (float)(sum / Fur.Count).module;
                }
                //h = 0;
            });
            ReFurThread.Wait();
            ReFurThread.Dispose();
        }
        /// <summary>
        /// Дискретное преобразование Фурье (ДПФ)
        /// </summary>
        private void FurieConvertion()
        {
            
            reader = new WaveFileReader(outputFilename);
            var ft = Task.Factory.StartNew(delegate () {
                Fur.Clear();
                Complex sum;
                DateTime timeRun = DateTime.Now;
                double y_fur;
                float prev = 0;
                for (int k = 0; k < reader.SampleCount; ++k, ++h)
                {
                    sum = new Complex(0, 0);
                    for (int i = 0; i < reader.SampleCount; ++i)
                    {
                        try
                        {
                            y_fur = (2 * Math.PI * i * k) / reader.Length;
                            double complex_real = Math.Cos(y_fur);
                            double complex_imag = Math.Sin(y_fur);
                            double x_n = Convert.ToDouble(reader.ReadNextSampleFrame()[0]);
                            sum += x_n * new Complex(complex_real, -complex_imag);
                        }
                        catch (NullReferenceException)
                        {
                            reader.Close();
                            reader = new WaveFileReader(outputFilename);
                            break;
                        }
                    }
                    Fur.Add(sum);
                    waveformPainter2.AddMax((float)sum.module / 10);
                    //PaintCW(pictureBox1, (float)sum.module *10, prev*10);
                    PaintToPictureBox(pictureBox1, sum.module * 10);
                    prev = (float)sum.module;
                }
                h = 0;
                MessageBox.Show($"{DateTime.Now - timeRun}");
                MessageBox.Show($"Sample Count: {reader.SampleCount}\n Furie Count: {Fur.Count}");
                reader.Close();
            });
            ft.Wait();
        }
        /// <summary>
        /// Читает и воспроизводит сигнал из файла
        /// </summary>
        /// <param name="filename">Имя читаемого файла</param>
        private void ReadSignals(string filename)
        {
            WaveFileReader reader = new WaveFileReader(filename);
            WaveOutEvent waveEvent = new WaveOutEvent();
            waveEvent.Init(new WaveChannel32(reader));
            waveEvent.Play();
            Thread.Sleep(reader.TotalTime);
            waveEvent.Stop();
            reader.Close();
            reader.Dispose();
            waveEvent.Dispose();
        }
        /// <summary>
        /// Распределенная запись в файлы
        /// </summary>
        private void ThreadingWriteSignals()
        {
            var syncTask = new Task(delegate () {
                WriteSignal(Fur, filename1);
            });
            var syncTask2 = new Task(delegate () {
                WriteSignal(ReFur, filename2);
                syncTask.Wait();
            });
            syncTask.RunSynchronously();
            syncTask2.RunSynchronously();
            syncTask2.Wait();
            syncTask.Dispose();
            syncTask2.Dispose();
        }
        /// <summary>
        /// Запись в файл сигнала, записанного в массив в ходе Фурье-преобразований
        /// </summary>
        /// <param name="sampleSignal">Список семплов сигнала</param>
        /// <param name="filename">Имя читаемого файла</param>
        private void WriteSignal(List<Complex> sampleSignal, string filename)
        {
            WaveFileWriter waveFile = new WaveFileWriter(filename, new WaveFormat(8000, 1));
            for (int i = 0; i < sampleSignal.Count; ++i)
                waveFile.WriteSample((float)sampleSignal[i].module);
            waveFile.Close();
            waveFile.Dispose();
        }
        private void WaveformPainter2_Click(object sender, EventArgs e)
        {

        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Label3_Click(object sender, EventArgs e)
        {

        }

        private void Label4_Click(object sender, EventArgs e)
        {

        }

        private void PictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void PictureBox3_Click(object sender, EventArgs e)
        {

        }
    }
    /// <summary>
    /// Класс комплексных чисел
    /// </summary>
    class Complex
    {
        public Complex(double real, double imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }
        public double real { get; set; }
        public double imaginary { get; set; }
        public double module { get { return Math.Sqrt(real * real + imaginary * imaginary); } }
        /// <summary>
        /// Перегрузка операторов для выполнения операции сложения
        /// </summary>
        /// <param name="FirstNumber"></param>
        /// <param name="SecondNumber"></param>
        /// <returns></returns>
        public static Complex operator +(Complex FirstNumber, Complex SecondNumber) => new Complex(FirstNumber.real + SecondNumber.real, FirstNumber.imaginary + SecondNumber.imaginary);
        /// <summary>
        /// Перегрузка операторов для выполнения операции вычитания
        /// </summary>
        /// <param name="FirstNumber"></param>
        /// <param name="SecondNumber"></param>
        /// <returns></returns>
        public static Complex operator -(Complex FirstNumber, Complex SecondNumber) => new Complex(FirstNumber.real - SecondNumber.real, FirstNumber.imaginary - SecondNumber.imaginary);
        /// <summary>
        /// Перегрузка операторов для выполнения операции умножения
        /// </summary>
        /// <param name="FirstNumber"></param>
        /// <param name="SecondNumber"></param>
        /// <returns></returns>
        public static Complex operator *(Complex FirstNumber, Complex SecondNumber) => new Complex(FirstNumber.real * SecondNumber.real - FirstNumber.imaginary * SecondNumber.imaginary, FirstNumber.real * SecondNumber.imaginary + SecondNumber.real * FirstNumber.imaginary);
        /// <summary>
        /// Перегрузка операторов для выполнения операции деления
        /// </summary>
        /// <param name="FirstNumber"></param>
        /// <param name="SecondNumber"></param>
        /// <returns></returns>
        public static Complex operator /(Complex FirstNumber, Complex SecondNumber) => new Complex((FirstNumber.real * SecondNumber.real + FirstNumber.imaginary * SecondNumber.imaginary) / (SecondNumber.real * SecondNumber.real + SecondNumber.imaginary * SecondNumber.imaginary), (SecondNumber.real * FirstNumber.imaginary - SecondNumber.imaginary * FirstNumber.real) / (SecondNumber.real * SecondNumber.real + SecondNumber.imaginary * SecondNumber.imaginary));
        /// <summary>
        /// Перегрузка операторов для выполнения операции деления на целое число
        /// </summary>
        /// <param name="FirstNumber"></param>
        /// <param name="SecondNumber"></param>
        /// <returns></returns>
        public static Complex operator /(Complex FirstNumber, int SecondNumber) => new Complex(FirstNumber.real / SecondNumber, FirstNumber.imaginary / SecondNumber);
        /// <summary>
        /// Перегрузка операторов для выполнения операции умножение на число с плавающей точкой
        /// </summary>
        /// <param name="FirstNumber"></param>
        /// <param name="SecondNumber"></param>
        /// <returns></returns>
        public static Complex operator *(double FirstNumber, Complex SecondNumber) => new Complex(FirstNumber * SecondNumber.real, FirstNumber * SecondNumber.imaginary);
        /// <summary>
        /// Строковое представление комплексного числа
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch(imaginary)
            {
                case 1: return $"{real.ToString()}+i";
                case -1: return $"{real.ToString()}-i";
                case 0: return real.ToString();
                default: return (imaginary > 0) ? $"{real.ToString()}+{imaginary.ToString()}i" : $"{real.ToString()}{imaginary.ToString()}i";
            }
        }
    }
}