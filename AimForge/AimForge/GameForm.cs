using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;  

namespace AimForge
{
    public partial class GameForm : Form
    {
        private class Target
        {
            public int X, Y, Size;
            public DateTime SpawnedAt;
        }

        private readonly Mode _mode;
        private readonly List<Shot> _shots = new();
        private readonly List<Target> _targets = new();
        private readonly Random _rng = new();

        private DateTime _sessionStart;
        private DateTime _lastSpawn;
        private bool _sessionEnded = false;

        public GameForm(Mode mode)
        {
            InitializeComponent();
            _mode = mode;

            DoubleBuffered = true;
            playPanel.BackColor = Color.Black;

            // enable double buffering on the panel via reflection-free trick:
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, playPanel, new object[] { true });

            playPanel.Paint += PlayPanel_Paint;
            playPanel.MouseDown += PlayPanel_MouseDown;
            gameTimer.Tick += GameTimer_Tick;
            Cursor = Cursors.Cross;
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            _sessionStart = DateTime.Now;
            _lastSpawn = DateTime.Now.AddMilliseconds(-_mode.SpawnIntervalMs); // spawn immediately
            gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            // end session when duration elapses
            if ((now - _sessionStart).TotalSeconds >= _mode.DurationSec)
            {
                EndSession();
                return;
            }

            // spawn a new target if interval passed
            if ((now - _lastSpawn).TotalMilliseconds >= _mode.SpawnIntervalMs)
            {
                SpawnTarget();
                _lastSpawn = now;
            }

            // remove expired targets -> log as miss
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                if ((now - _targets[i].SpawnedAt).TotalMilliseconds >= _mode.TargetLifetimeMs)
                {
                    _shots.Add(new Shot { IsHit = false, ReactionMs = 0, X = _targets[i].X, Y = _targets[i].Y });
                    _targets.RemoveAt(i);
                }
            }

            playPanel.Invalidate();
        }

        private void SpawnTarget()
        {
            int size = _mode.TargetSize;
            int margin = size + 10;
            int x = _rng.Next(margin, Math.Max(margin + 1, playPanel.Width - margin));
            int y = _rng.Next(margin, Math.Max(margin + 1, playPanel.Height - margin - 100)); // keep clear of gun area
            _targets.Add(new Target { X = x, Y = y, Size = size, SpawnedAt = DateTime.Now });
        }

        private void PlayPanel_MouseDown(object sender, MouseEventArgs e)
        {
            var now = DateTime.Now;
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                var t = _targets[i];
                double dx = e.X - t.X;
                double dy = e.Y - t.Y;
                if (dx * dx + dy * dy <= (t.Size / 2.0) * (t.Size / 2.0))
                {
                    double reaction = (now - t.SpawnedAt).TotalMilliseconds;
                    _shots.Add(new Shot { IsHit = true, ReactionMs = reaction, X = t.X, Y = t.Y });
                    _targets.RemoveAt(i);
                    playPanel.Invalidate();
                    return;
                }
            }
            // clicked empty space = miss (not tied to any target)
            _shots.Add(new Shot { IsHit = false, ReactionMs = 0, X = e.X, Y = e.Y });
        }

        private void PlayPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var t in _targets)
            {
                using var brush = new SolidBrush(Color.Red);
                g.FillEllipse(brush, t.X - t.Size / 2, t.Y - t.Size / 2, t.Size, t.Size);
                using var pen = new Pen(Color.White, 2);
                g.DrawEllipse(pen, t.X - t.Size / 2, t.Y - t.Size / 2, t.Size, t.Size);
            }

            // remaining time / score HUD
            var elapsed = (DateTime.Now - _sessionStart).TotalSeconds;
            var remaining = Math.Max(0, _mode.DurationSec - elapsed);
            int score = _shots.Count(s => s.IsHit); // Linq: needs using System.Linq
            g.DrawString($"Time: {remaining:0}s   Score: {score}", Font, Brushes.White, 10, 10);
        }

        private void EndSession()
        {
            if (_sessionEnded) return;
            _sessionEnded = true;
            gameTimer.Stop();

            Db.SaveSession(_mode.Id, _shots);

            MessageBox.Show(
                $"Session complete!\nHits: {_shots.Count(s => s.IsHit)}\nMisses: {_shots.Count(s => !s.IsHit)}",
                "Session Ended");
            Close();
        }
    }
}