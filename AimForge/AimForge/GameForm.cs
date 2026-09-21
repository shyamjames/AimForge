using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

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

        // --- assets ---
        private Image _gunImg, _crosshairImg, _muzzleImg;
        private SoundPlayer _shotSound;

        // --- gun/crosshair state ---
        private Point _mousePos;
        private float _gunAngle;          // radians
        private float _recoilOffset;      // px, decays after each shot
        private DateTime _muzzleFlashUntil = DateTime.MinValue;

        private const int GunLength = 140;   // drawn size of gun sprite
        private const int MuzzleFlashLifeMs = 80;
        private const float RecoilKick = 14f;
        private const float RecoilDecayPerTick = 1.5f;

        public GameForm(Mode mode)
        {
            InitializeComponent();
            _mode = mode;

            DoubleBuffered = true;
            playPanel.BackColor = Color.Black;
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, playPanel, new object[] { true });

            LoadAssets();

            playPanel.Paint += PlayPanel_Paint;
            playPanel.MouseDown += PlayPanel_MouseDown;
            playPanel.MouseMove += PlayPanel_MouseMove;
            gameTimer.Tick += GameTimer_Tick;
            Cursor.Hide(); // custom crosshair replaces system cursor
        }

        private void LoadAssets()
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "Assets");
            _gunImg = Image.FromFile(Path.Combine(dir, "gun.png"));
            _crosshairImg = Image.FromFile(Path.Combine(dir, "crosshair.png"));
            _muzzleImg = Image.FromFile(Path.Combine(dir, "muzzle-flash.png"));
            _shotSound = new SoundPlayer(Path.Combine(dir, "gunshot.wav"));
            _shotSound.Load();
        }

        // Draws an Image with white treated as transparent (colorkey)
        private static void DrawWithWhiteTransparent(Graphics g, Image img, Rectangle dest)
        {
            using var attr = new ImageAttributes();
            attr.SetColorKey(Color.White, Color.White);
            g.DrawImage(img, dest, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attr);
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            _sessionStart = DateTime.Now;
            _lastSpawn = DateTime.Now.AddMilliseconds(-_mode.SpawnIntervalMs);
            gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            if ((now - _sessionStart).TotalSeconds >= _mode.DurationSec)
            {
                EndSession();
                return;
            }

            if ((now - _lastSpawn).TotalMilliseconds >= _mode.SpawnIntervalMs)
            {
                SpawnTarget();
                _lastSpawn = now;
            }

            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                if ((now - _targets[i].SpawnedAt).TotalMilliseconds >= _mode.TargetLifetimeMs)
                {
                    _shots.Add(new Shot { IsHit = false, ReactionMs = 0, X = _targets[i].X, Y = _targets[i].Y });
                    _targets.RemoveAt(i);
                }
            }

            if (_recoilOffset > 0)
                _recoilOffset = Math.Max(0, _recoilOffset - RecoilDecayPerTick);

            playPanel.Invalidate();
        }

        private Point GunPivot => new(playPanel.Width / 2, playPanel.Height - 20);

        private void PlayPanel_MouseMove(object sender, MouseEventArgs e)
        {
            _mousePos = e.Location;
            var pivot = GunPivot;
            _gunAngle = (float)Math.Atan2(_mousePos.Y - pivot.Y, _mousePos.X - pivot.X);
            playPanel.Invalidate();
        }

        private void SpawnTarget()
        {
            int size = _mode.TargetSize;
            int margin = size + 10;
            int x = _rng.Next(margin, Math.Max(margin + 1, playPanel.Width - margin));
            int y = _rng.Next(margin, Math.Max(margin + 1, playPanel.Height - margin - 120));
            _targets.Add(new Target { X = x, Y = y, Size = size, SpawnedAt = DateTime.Now });
        }

        private void PlayPanel_MouseDown(object sender, MouseEventArgs e)
        {
            var now = DateTime.Now;
            bool hit = false;

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
                    hit = true;
                    break;
                }
            }

            if (!hit)
                _shots.Add(new Shot { IsHit = false, ReactionMs = 0, X = e.X, Y = e.Y });

            FireEffects();
        }

        private void FireEffects()
        {
            _recoilOffset = RecoilKick;
            _muzzleFlashUntil = DateTime.Now.AddMilliseconds(MuzzleFlashLifeMs);
            try { _shotSound.Play(); } catch { /* ignore if wav missing */ }
            playPanel.Invalidate();
        }

        private void PlayPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // targets
            foreach (var t in _targets)
            {
                using var brush = new SolidBrush(Color.Red);
                g.FillEllipse(brush, t.X - t.Size / 2, t.Y - t.Size / 2, t.Size, t.Size);
                using var pen = new Pen(Color.White, 2);
                g.DrawEllipse(pen, t.X - t.Size / 2, t.Y - t.Size / 2, t.Size, t.Size);
            }

            // gun, rotated toward cursor, with recoil pulling it back along its own axis
            var pivot = GunPivot;
            float recoilDx = (float)(-Math.Cos(_gunAngle) * _recoilOffset);
            float recoilDy = (float)(-Math.Sin(_gunAngle) * _recoilOffset);

            var state = g.Save();
            g.TranslateTransform(pivot.X + recoilDx, pivot.Y + recoilDy);
            g.RotateTransform((float)(_gunAngle * 180.0 / Math.PI));

            int gunH = GunLength * _gunImg.Height / _gunImg.Width;
            var gunRect = new Rectangle(0, -gunH / 2, GunLength, gunH);
            DrawWithWhiteTransparent(g, _gunImg, gunRect);

            // muzzle flash at barrel tip, only while active
            if (DateTime.Now < _muzzleFlashUntil)
            {
                int flashSize = 60;
                var flashRect = new Rectangle(GunLength - 10, -flashSize / 2, flashSize, flashSize);
                DrawWithWhiteTransparent(g, _muzzleImg, flashRect);
            }

            g.Restore(state);

            // crosshair follows mouse, replaces system cursor
            int chSize = 40;
            g.DrawImage(_crosshairImg, _mousePos.X - chSize / 2, _mousePos.Y - chSize / 2, chSize, chSize);

            // HUD
            var elapsed = (DateTime.Now - _sessionStart).TotalSeconds;
            var remaining = Math.Max(0, _mode.DurationSec - elapsed);
            int score = _shots.Count(s => s.IsHit);
            g.DrawString($"Time: {remaining:0}s   Score: {score}", Font, Brushes.White, 10, 10);
        }

        private void EndSession()
        {
            if (_sessionEnded) return;
            _sessionEnded = true;
            gameTimer.Stop();

            Db.SaveSession(_mode.Id, _shots);

            Cursor = Cursors.Default;
            MessageBox.Show(
                $"Session complete!\nHits: {_shots.Count(s => s.IsHit)}\nMisses: {_shots.Count(s => !s.IsHit)}",
                "Session Ended");
            Close();
        }
    }
}