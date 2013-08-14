using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;

namespace MrE_Engine
{
    /// <summary>
    /// A sample test form
    /// </summary>
    public partial class Form1 : Form
    {
        private MrEHelper helper = new MrEHelper();

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch watch = new Stopwatch();
            uint i = 1;
            double my_d = 1.000000001;
            string s = "";
            watch.Reset();
            watch.Start();
            double myval = 1.235;
            for (i = 0; i < 50000/*64000000*//*uint.MaxValue*/; i++)
            {
                myval = myval + 1;
                double mult = MrEHelper.GetNormal(1.000000001, 0.000001);
                my_d = my_d * mult + Math.Exp(myval);
                myval = myval - 1;
                my_d = my_d - Math.Exp(myval);
                System.Random ra = new Random(1);
                double myVal = 0;
                for (int k = 1; k < 60; k++)
                {
                    myVal += ra.NextDouble() * (double)k;
                    if(myVal>10)
                        continue;

                }
                
                s += "";
            }
            watch.Stop();
            label5.Text = watch.Elapsed.ToString() + ", \ntotal milliseconds: " + watch.ElapsedMilliseconds.ToString();
            

            s += "";
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double x_sample_dbl = 0;
            int x_sample_int = 0;
            int sampling_count = vScrollBar1.Value;
            double deviation = hScrollBar1.Value;
            int i = 0;
            int max_bars = 1000;

            List<int> x_bars = new List<int>(2*max_bars+1);
            for (i = -max_bars; i <= max_bars; i++)
            {
                x_bars.Add(0);
            }
            int max_200_value = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            for (i = 0; i < sampling_count; i++)
            {
                // imitate 10 variables
                for (int k = 1; k <= 10; k++)
                {
                    x_sample_dbl = MrEHelper.GetNormal(0, deviation);
                }
                x_sample_int = Convert.ToInt32(x_sample_dbl);
                if (x_sample_int < -max_bars)
                    x_sample_int = -max_bars;
                if (x_sample_int > max_bars)
                    x_sample_int = max_bars;
                x_bars[x_sample_int + max_bars]++;
            }
            watch.Stop();
            label4.Text = watch.Elapsed.ToString() + ", \ntotal milliseconds: " + watch.ElapsedMilliseconds.ToString();
            label1.Text = "Using " + sampling_count.ToString() + "samples at deviation " + deviation.ToString() + ", " + x_bars[max_bars + 10] + " samples at 10, " + x_bars[max_bars] + " samples at 0 and " + x_bars[max_bars - 10] + " samples at -10";

            chart1.BeginInit();
            chart2.BeginInit();
            
            Series series = new Series("Normal distribution", 2 * max_bars + 1);
            Series series2 = new Series("Normal distribution", 2 * max_bars + 1);
            chart1.Series.Clear();
            chart2.Series.Clear();
            for (i = -max_bars; i <= max_bars; i++)
            {
                series.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars] * (double)(2 * max_bars + 1) / (sampling_count)));
                if (i >= -200 || i <= 200)
                {
                    series2.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars]));
                    if (x_bars[i + max_bars] > max_200_value)
                        max_200_value = x_bars[i + max_bars];
                }
            }
            series.ChartType = SeriesChartType.Line;
            chart1.ChartAreas[0].AxisX.Minimum = -max_bars;
            chart1.ChartAreas[0].AxisX.Maximum = max_bars;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 9.6;
            chart1.Series.Add(series);

            series2.ChartType = SeriesChartType.Line;
            chart2.ChartAreas[0].AxisX.Minimum = -200;
            chart2.ChartAreas[0].AxisX.Maximum = 200;
            chart2.ChartAreas[0].AxisY.Minimum = 0;
            chart2.ChartAreas[0].AxisY.Maximum = max_200_value + 100; ;
            chart2.Series.Add(series2);
            chart1.EndInit();
            chart2.EndInit();
            label2.Text = "Left tail: " + x_bars[0].ToString();
            label3.Text = "Right tail: " + x_bars[2 * max_bars].ToString();
            string s = "";
            s += "";

        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if(!m_drawing)
            {
                m_drawing = true;
                button2_Click(null, new EventArgs());
                m_drawing = false;
            }

        }
        private bool m_drawing = false;

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (!m_drawing)
            {
                m_drawing = true;
                button2_Click(null, new EventArgs());
                m_drawing = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MrEHelper.MAXNUMBEROFSIGMAPOINTS = 200000; //330000;
            InferenceTask task = new InferenceTask();
             
            task.EnforcePositiveLagrangeMultipliers = false;
            double mean_value;
            if(!double.TryParse(textBox1.Text, out mean_value))
            {
                MessageBox.Show("Failed");
                return;
            }
            // We need to assign prior distribution before adding constraints and variables
            task.JointPriorDistribution = new DistributionGaussian(mean_value, 30); // 40,30

            Example1_CustomConstraint constraint = new Example1_CustomConstraint();
            task.AddVariableConstraint(constraint);

            CustomVariableBase variableX = new CustomVariableBase();
            task.AddVariable(variableX);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (task.Execute())
            {
                watch.Stop();
                label6.Text = watch.Elapsed.ToString() +
                    ", \ntotal milliseconds: " + watch.ElapsedMilliseconds.ToString() +
                    ", \nLagrange multiplier: " + task.VariableConstraints[0].LagrangeMultiplierValue.ToString() +
                    ", \nMaxShiftsOfSigmaPoints: " + task.MaxShiftsOfSigmaPoints.ToString() +
                    ", \nLastIterationOfLagrangeMultipliersShifts: " + task.LastIterationOfLagrangeMultipliersShifts.ToString();


                /*m_SigmaPoints.Sort((x, y) => x.VectorCoordinates[0].CompareTo(y.VectorCoordinates[0]));
                string s = "";
                i = 0;
                foreach (SigmaPoint sigma in m_SigmaPoints)
                {
                    s += sigma.VectorCoordinates[0].ToString() + Environment.NewLine;
                }*/

                int i = 0;
                int max_bars = 1000;
                int max_200_value = 0;

                List<int> x_bars = new List<int>(2 * max_bars + 1);
                for (i = -max_bars; i <= max_bars; i++)
                {
                    x_bars.Add(0);
                }
                task.SigmaPoints.Sort((x, y) => x.VectorCoordinates[0].CompareTo(y.VectorCoordinates[0]));
                for (i = 0; i < task.SigmaPoints.Count; i++)
                {
                    int x_sample_int = Convert.ToInt32(task.SigmaPoints[i].VectorCoordinates[0]);
                    if (x_sample_int < -max_bars)
                        x_sample_int = -max_bars;
                    if (x_sample_int > max_bars)
                        x_sample_int = max_bars;
                    x_bars[x_sample_int + max_bars]++;
                }
                chart1.BeginInit();
                chart2.BeginInit();

                Series series = new Series("Normal distribution", 2 * max_bars + 1);
                Series series2 = new Series("Normal distribution", 2 * max_bars + 1);
                chart1.Series.Clear();
                chart2.Series.Clear();
                for (i = -max_bars; i <= max_bars; i++)
                {
                    series.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars] * (double)(2 * max_bars + 1) / (task.SigmaPoints.Count)));
                    if (i >= -200 || i <= 200)
                    {
                        series2.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars]));
                        if (x_bars[i + max_bars] > max_200_value)
                            max_200_value = x_bars[i + max_bars];
                    }
                }
                series.ChartType = SeriesChartType.Line;
                chart1.ChartAreas[0].AxisX.Minimum = -max_bars;
                chart1.ChartAreas[0].AxisX.Maximum = max_bars;
                chart1.ChartAreas[0].AxisY.Minimum = 0;
                chart1.ChartAreas[0].AxisY.Maximum = 100;
                chart1.Series.Add(series);

                series2.ChartType = SeriesChartType.Line;
                chart2.ChartAreas[0].AxisX.Minimum = -200;
                chart2.ChartAreas[0].AxisX.Maximum = 200;
                chart2.ChartAreas[0].AxisY.Minimum = 0;
                chart2.ChartAreas[0].AxisY.Maximum = 5;
                chart2.Series.Add(series2);
                chart1.EndInit();
                chart2.EndInit();
                label2.Text = "Left tail: " + x_bars[0].ToString();
                label3.Text = "Right tail: " + x_bars[2 * max_bars].ToString();

            }
            else
            {
                MessageBox.Show("Failure");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MrEHelper.MAXNUMBEROFSIGMAPOINTS = 200000;
            InferenceTask task = new MrEExampleForSanovTheoremInferenceTask();
            // We need to assign prior distribution before adding constraints and variables
            task.JointPriorDistribution = new MrEExampleForSanovTheoremDistribution(11, 2, 7);

            MrEExampleForSanovTheoremConstraint constraint = new MrEExampleForSanovTheoremConstraint(2.3, 1, 2, 3);
            task.AddVariableConstraint(constraint);

            CustomVariableBase variableX = new CustomVariableBase();
            variableX.MinValue = 0;
            variableX.MaxValue = 1;

            task.AddVariable(variableX);
            CustomVariableBase variableY = new CustomVariableBase();
            variableY.MinValue = 0;
            variableY.MaxValue = 1;
            task.AddVariable(variableY);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (task.Execute())
            {
                watch.Stop();
                label7.Text = watch.Elapsed.ToString() + 
                    ", \ntotal milliseconds: " + watch.ElapsedMilliseconds.ToString() +
                    ", \nLagrange multiplier: " + task.VariableConstraints[0].LagrangeMultiplierValue.ToString() +
                    ", \nMaxShiftsOfSigmaPoints: " + task.MaxShiftsOfSigmaPoints.ToString() +
                    ", \nLastIterationOfLagrangeMultipliersShifts: " + task.LastIterationOfLagrangeMultipliersShifts.ToString();


                /*m_SigmaPoints.Sort((x, y) => x.VectorCoordinates[0].CompareTo(y.VectorCoordinates[0]));
                string s = "";
                i = 0;
                foreach (SigmaPoint sigma in m_SigmaPoints)
                {
                    s += sigma.VectorCoordinates[0].ToString() + Environment.NewLine;
                }*/

                int i = 0;
                int max_bars = 1000;
                int max_200_value = 0;

                List<int> x_bars = new List<int>(2 * max_bars + 1);
                for (i = -max_bars; i <= max_bars; i++)
                {
                    x_bars.Add(0);
                }
                task.SigmaPoints.Sort((x, y) => x.VectorCoordinates[0].CompareTo(y.VectorCoordinates[0]));
                for (i = 0; i < task.SigmaPoints.Count; i++)
                {
                    int x_sample_int = Convert.ToInt32(task.SigmaPoints[i].VectorCoordinates[0]);
                    if (x_sample_int < -max_bars)
                        x_sample_int = -max_bars;
                    if (x_sample_int > max_bars)
                        x_sample_int = max_bars;
                    x_bars[x_sample_int + max_bars]++;
                }
                chart1.BeginInit();
                chart2.BeginInit();

                Series series = new Series("Normal distribution", 2 * max_bars + 1);
                Series series2 = new Series("Normal distribution", 2 * max_bars + 1);
                chart1.Series.Clear();
                chart2.Series.Clear();
                for (i = -max_bars; i <= max_bars; i++)
                {
                    series.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars] * (double)(2 * max_bars + 1) / (task.SigmaPoints.Count)));
                    if (i >= -200 || i <= 200)
                    {
                        series2.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars]));
                        if (x_bars[i + max_bars] > max_200_value)
                            max_200_value = x_bars[i + max_bars];
                    }
                }
                series.ChartType = SeriesChartType.Line;
                chart1.ChartAreas[0].AxisX.Minimum = -max_bars;
                chart1.ChartAreas[0].AxisX.Maximum = max_bars;
                chart1.ChartAreas[0].AxisY.Minimum = 0;
                chart1.ChartAreas[0].AxisY.Maximum = 100;
                chart1.Series.Add(series);

                series2.ChartType = SeriesChartType.Line;
                chart2.ChartAreas[0].AxisX.Minimum = -200;
                chart2.ChartAreas[0].AxisX.Maximum = 200;
                chart2.ChartAreas[0].AxisY.Minimum = 0;
                chart2.ChartAreas[0].AxisY.Maximum = 5;
                chart2.Series.Add(series2);
                chart1.EndInit();
                chart2.EndInit();
                label2.Text = "Left tail: " + x_bars[0].ToString();
                label3.Text = "Right tail: " + x_bars[2 * max_bars].ToString();

            }
            else
            {
                MessageBox.Show("Failure");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MrEHelper.MAXNUMBEROFSIGMAPOINTS = 200000;
            InferenceTask task = new InferenceTask();
            // We need to assign prior distribution before adding constraints and variables
            task.JointPriorDistribution = new MrEExampleForSanovTheoremDistribution(11, 2, 7);

            MrEExampleForSanovTheoremConstraint constraint = new MrEExampleForSanovTheoremConstraint(2.3, 1, 2, 3);
            task.AddVariableConstraint(constraint);
            MrEExampleForSanovTheoremConstraint2 constraint2 = new MrEExampleForSanovTheoremConstraint2(1);
            task.AddVariableConstraint(constraint2);

            CustomVariableBase variableX = new CustomVariableBase();
            variableX.MinValue = 0;
            variableX.MaxValue = 1;
            task.AddVariable(variableX);

            CustomVariableBase variableY = new CustomVariableBase();
            variableY.MinValue = 0;
            variableY.MaxValue = 1;
            task.AddVariable(variableY);

            CustomVariableBase variableZ = new CustomVariableBase();
            variableZ.MinValue = 0;
            variableZ.MaxValue = 1;
            task.AddVariable(variableZ);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (task.Execute())
            {
                watch.Stop();
                label8.Text = watch.Elapsed.ToString() +
                    ", \ntotal milliseconds: " + watch.ElapsedMilliseconds.ToString() +
                    ", \nLagrange multiplier1: " + task.VariableConstraints[0].LagrangeMultiplierValue.ToString() +
                    ", \nLagrange multiplier2: " + task.VariableConstraints[1].LagrangeMultiplierValue.ToString() +
                    ", \nLastIterationOfLagrangeMultipliersShifts: " + task.LastIterationOfLagrangeMultipliersShifts.ToString();

                int i = 0;
                int max_bars = 1000;
                int max_200_value = 0;

                List<int> x_bars = new List<int>(2 * max_bars + 1);
                for (i = -max_bars; i <= max_bars; i++)
                {
                    x_bars.Add(0);
                }
                task.SigmaPoints.Sort((x, y) => x.VectorCoordinates[0].CompareTo(y.VectorCoordinates[0]));
                for (i = 0; i < task.SigmaPoints.Count; i++)
                {
                    int x_sample_int = Convert.ToInt32(task.SigmaPoints[i].VectorCoordinates[0]);
                    if (x_sample_int < -max_bars)
                        x_sample_int = -max_bars;
                    if (x_sample_int > max_bars)
                        x_sample_int = max_bars;
                    x_bars[x_sample_int + max_bars]++;
                }
                chart1.BeginInit();
                chart2.BeginInit();

                Series series = new Series("Normal distribution", 2 * max_bars + 1);
                Series series2 = new Series("Normal distribution", 2 * max_bars + 1);
                chart1.Series.Clear();
                chart2.Series.Clear();
                for (i = -max_bars; i <= max_bars; i++)
                {
                    series.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars] * (double)(2 * max_bars + 1) / (task.SigmaPoints.Count)));
                    if (i >= -200 || i <= 200)
                    {
                        series2.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars]));
                        if (x_bars[i + max_bars] > max_200_value)
                            max_200_value = x_bars[i + max_bars];
                    }
                }
                series.ChartType = SeriesChartType.Line;
                chart1.ChartAreas[0].AxisX.Minimum = -max_bars;
                chart1.ChartAreas[0].AxisX.Maximum = max_bars;
                chart1.ChartAreas[0].AxisY.Minimum = 0;
                chart1.ChartAreas[0].AxisY.Maximum = 100;
                chart1.Series.Add(series);

                series2.ChartType = SeriesChartType.Line;
                chart2.ChartAreas[0].AxisX.Minimum = -200;
                chart2.ChartAreas[0].AxisX.Maximum = 200;
                chart2.ChartAreas[0].AxisY.Minimum = 0;
                chart2.ChartAreas[0].AxisY.Maximum = 5;
                chart2.Series.Add(series2);
                chart1.EndInit();
                chart2.EndInit();
                label2.Text = "Left tail: " + x_bars[0].ToString();
                label3.Text = "Right tail: " + x_bars[2 * max_bars].ToString();

            }
            else
            {
                MessageBox.Show("Failure");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MrEHelper.MAXNUMBEROFSIGMAPOINTS = 10000;
            InferenceTask task = new InferenceTask();
            // We need to assign prior distribution before adding constraints and variables
            task.JointPriorDistribution = new DistributionUniform();
            int i = 0;
            int count = 15;
            for (i = 1; i <= count; i++)
            {
                // Add variance constraint
                MrEExample10VariateGaussianCovarianceConstraint constraint = 
                    new MrEExample10VariateGaussianCovarianceConstraint(i-1, i-1, 0, 0, Math.Pow(300 + i * 10, 2)); // 1000
                task.AddVariableConstraint(constraint);
            }
            // Do not do covariance constraints for now
            //MrEExampleForSanovTheoremConstraint constraint = new MrEExampleForSanovTheoremConstraint(2.3, 1, 2, 3);
            //task.AddVariableConstraint(constraint);

            for (i = 1; i <= count; i++)
            {
                CustomVariableBase variableX = new CustomVariableBase();
                //variableX.MinValue = -60;
                //variableX.MaxValue = 60;
                task.AddVariable(variableX);
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (task.Execute())
            {
                watch.Stop();
                label9.Text = watch.Elapsed.ToString() +
                    ", \ntotal milliseconds: " + watch.ElapsedMilliseconds.ToString();
                for (i = 0; i < task.VariableConstraints.Count; i++)
                {
                    label9.Text += ", \nLagrange multiplier" + (i + 1).ToString() + ": " + task.VariableConstraints[i].LagrangeMultiplierValue.ToString();
                }
                label9.Text += ", \nLastIterationOfLagrangeMultipliersShifts: " + task.LastIterationOfLagrangeMultipliersShifts.ToString();

                int max_bars = 1000;
                int max_200_value = 0;

                List<int> x_bars = new List<int>(2 * max_bars + 1);
                for (i = -max_bars; i <= max_bars; i++)
                {
                    x_bars.Add(0);
                }
                task.SigmaPoints.Sort((x, y) => x.VectorCoordinates[0].CompareTo(y.VectorCoordinates[0]));
                for (i = 0; i < task.SigmaPoints.Count; i++)
                {
                    int x_sample_int = Convert.ToInt32(task.SigmaPoints[i].VectorCoordinates[0]);
                    if (x_sample_int < -max_bars)
                        x_sample_int = -max_bars;
                    if (x_sample_int > max_bars)
                        x_sample_int = max_bars;
                    x_bars[x_sample_int + max_bars]++;
                }
                chart1.BeginInit();
                chart2.BeginInit();

                Series series = new Series("Normal distribution", 2 * max_bars + 1);
                Series series2 = new Series("Normal distribution", 2 * max_bars + 1);
                chart1.Series.Clear();
                chart2.Series.Clear();
                for (i = -max_bars; i <= max_bars; i++)
                {
                    series.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars] * (double)(2 * max_bars + 1) / (task.SigmaPoints.Count)));
                    if (i >= -200 || i <= 200)
                    {
                        series2.Points.Add(new DataPoint(i, (double)x_bars[i + max_bars]));
                        if (x_bars[i + max_bars] > max_200_value)
                            max_200_value = x_bars[i + max_bars];
                    }
                }
                series.ChartType = SeriesChartType.Line;
                chart1.ChartAreas[0].AxisX.Minimum = -max_bars;
                chart1.ChartAreas[0].AxisX.Maximum = max_bars;
                chart1.ChartAreas[0].AxisY.Minimum = 0;
                chart1.ChartAreas[0].AxisY.Maximum = 100;
                chart1.Series.Add(series);

                series2.ChartType = SeriesChartType.Line;
                chart2.ChartAreas[0].AxisX.Minimum = -200;
                chart2.ChartAreas[0].AxisX.Maximum = 200;
                chart2.ChartAreas[0].AxisY.Minimum = 0;
                chart2.ChartAreas[0].AxisY.Maximum = 5;
                chart2.Series.Add(series2);
                chart1.EndInit();
                chart2.EndInit();
                label2.Text = "Left tail: " + x_bars[0].ToString();
                label3.Text = "Right tail: " + x_bars[2 * max_bars].ToString();

            }
            else
            {
                MessageBox.Show("Failure");
            }
        }
    }
}
