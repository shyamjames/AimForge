namespace AimForge
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshModes();
        }

        private void RefreshModes()
        {
            var modes = Db.GetModes();
            cboModes.DataSource = modes;    
            cboModes.DisplayMember = "Name";
            cboModes.ValueMember = "Id";
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (cboModes.SelectedItem is not Mode selected)
            {
                MessageBox.Show("Select a mode first.");
                return;
            }

            using var game = new GameForm(selected);
            game.ShowDialog();
        }

        private void btnModes_Click(object sender, EventArgs e)
        {
            using var f = new ModeManagerForm();
            f.ShowDialog();
            RefreshModes(); // in case modes changed/deleted
        }

        private void btnStats_Click(object sender, EventArgs e)
        {
            using var f = new StatsForm();
            f.ShowDialog();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }   
    }
}
