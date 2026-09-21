using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AimForge
{
    public partial class StatsForm : Form
    {
        private Chart chart1;
        public StatsForm()
        {
            InitializeComponent();
            SetupChart();
            LoadStats();
        }

        private void SetupChart()
        {
            chart1 = new Chart { Dock = DockStyle.Fill };
            var area = new ChartArea("main");
            chart1.ChartAreas.Add(area);
            chart1.Legends.Add(new Legend("legend"));
            Controls.Add(chart1);
        }

        private void LoadStats()
        {
            var sessions = Db.GetSessions(null); // all modes

            var reactionSeries = new Series("Avg Reaction (ms)")
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime
            };
            var accuracySeries = new Series("Accuracy (%)")
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                YAxisType = AxisType.Secondary
            };

            foreach (var s in sessions)
            {
                reactionSeries.Points.AddXY(s.PlayedAt, s.AvgReactionMs);
                accuracySeries.Points.AddXY(s.PlayedAt, s.Accuracy);
            }

            chart1.Series.Clear();
            chart1.Series.Add(reactionSeries);
            chart1.Series.Add(accuracySeries);
        }

        private void StatsForm_Load(object sender, EventArgs e)
        {

        }
    }
}
