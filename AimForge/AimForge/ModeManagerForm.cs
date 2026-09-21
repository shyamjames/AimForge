using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AimForge
{
    public partial class ModeManagerForm : Form
    {
        public ModeManagerForm()
        {
            InitializeComponent();
            Load += (s, e) => LoadGrid();
            dataGridView1.SelectionChanged += Grid_SelectionChanged;
        }

        private void LoadGrid()
        {
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = Db.GetModes();
            dataGridView1.ClearSelection();
            ClearInputs();
        }

        private Mode Selected()
        {
            if (dataGridView1.CurrentRow == null) return null;
            return dataGridView1.CurrentRow.DataBoundItem as Mode;
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            var m = Selected();
            if (m == null) return;
            txtName.Text = m.Name;
            txtSize.Text = m.TargetSize.ToString();
            txtSpawn.Text = m.SpawnIntervalMs.ToString();
            txtLifetime.Text = m.TargetLifetimeMs.ToString();
            txtDuration.Text = m.DurationSec.ToString();
        }

        private void ClearInputs()
        {
            txtName.Clear(); txtSize.Clear(); txtSpawn.Clear();
            txtLifetime.Clear(); txtDuration.Clear();
            txtName.Focus();
        }

        private Mode ReadInputs()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name is required."); return null;
            }
            int size, spawn, life, dur;
            if (!int.TryParse(txtSize.Text, out size) || size < 10 || size > 200)
            { MessageBox.Show("Target size must be 10-200."); return null; }
            if (!int.TryParse(txtSpawn.Text, out spawn) || spawn < 100)
            { MessageBox.Show("Spawn interval must be >= 100 ms."); return null; }
            if (!int.TryParse(txtLifetime.Text, out life) || life < 200)
            { MessageBox.Show("Target lifetime must be >= 200 ms."); return null; }
            if (!int.TryParse(txtDuration.Text, out dur) || dur < 5 || dur > 300)
            { MessageBox.Show("Duration must be 5-300 seconds."); return null; }

            return new Mode
            {
                Name = txtName.Text.Trim(),
                TargetSize = size,
                SpawnIntervalMs = spawn,
                TargetLifetimeMs = life,
                DurationSec = dur
            };
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var m = ReadInputs();
            if (m == null) return;
            try { Db.AddMode(m); }
            catch (Microsoft.Data.Sqlite.SqliteException)
            { MessageBox.Show("A mode with that name already exists."); return; }
            LoadGrid();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            var sel = Selected();
            if (sel == null) { MessageBox.Show("Select a row first."); return; }
            var m = ReadInputs();
            if (m == null) return;
            m.Id = sel.Id;
            try { Db.UpdateMode(m); }
            catch (Microsoft.Data.Sqlite.SqliteException)
            { MessageBox.Show("A mode with that name already exists."); return; }
            LoadGrid();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var sel = Selected();
            if (sel == null) { MessageBox.Show("Select a row first."); return; }
            var ok = MessageBox.Show("Delete \"" + sel.Name + "\"? Its sessions will be removed too.",
                                     "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (ok != DialogResult.Yes) return;
            Db.DeleteMode(sel.Id);
            LoadGrid();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            ClearInputs();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtLIfetIme_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
