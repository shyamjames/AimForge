using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AimForge
{
    // ---------- Models ----------
    public class Mode
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int TargetSize { get; set; }        // px diameter
        public int SpawnIntervalMs { get; set; }
        public int TargetLifetimeMs { get; set; }
        public int DurationSec { get; set; }
        public override string ToString() => Name; // so a ComboBox shows the name
    }

    public class Shot
    {
        public bool IsHit { get; set; }
        public double ReactionMs { get; set; }     // 0 for misses
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class SessionRow
    {
        public int Id { get; set; }
        public string ModeName { get; set; } = "";
        public DateTime PlayedAt { get; set; }
        public int Score { get; set; }
        public double Accuracy { get; set; }       // percent
        public double AvgReactionMs { get; set; }
    }

    // ---------- Database ----------
    public static class Db
    {
        static readonly string ConnStr =
            "Data Source=" + Path.Combine(AppContext.BaseDirectory, "aimforge.db");

        static SqliteConnection Open()
        {
            var c = new SqliteConnection(ConnStr);
            c.Open();
            using var p = c.CreateCommand();
            p.CommandText = "PRAGMA foreign_keys = ON;";
            p.ExecuteNonQuery();
            return c;
        }

        // Call once in Program.cs before Application.Run(...)
        public static void Initialize()
        {
            using var c = Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Modes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    TargetSize INTEGER NOT NULL,
    SpawnIntervalMs INTEGER NOT NULL,
    TargetLifetimeMs INTEGER NOT NULL,
    DurationSec INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS Sessions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ModeId INTEGER NOT NULL REFERENCES Modes(Id) ON DELETE CASCADE,
    PlayedAt TEXT NOT NULL,
    Score INTEGER NOT NULL,
    Accuracy REAL NOT NULL,
    AvgReactionMs REAL NOT NULL
);
CREATE TABLE IF NOT EXISTS Shots (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SessionId INTEGER NOT NULL REFERENCES Sessions(Id) ON DELETE CASCADE,
    IsHit INTEGER NOT NULL,
    ReactionMs REAL NOT NULL,
    X INTEGER NOT NULL,
    Y INTEGER NOT NULL
);";
            cmd.ExecuteNonQuery();

            // Seed default modes on first run
            cmd.CommandText = "SELECT COUNT(*) FROM Modes";
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                AddMode(new Mode { Name = "Easy",   TargetSize = 80, SpawnIntervalMs = 1200, TargetLifetimeMs = 2000, DurationSec = 30 });
                AddMode(new Mode { Name = "Normal", TargetSize = 60, SpawnIntervalMs = 800,  TargetLifetimeMs = 1400, DurationSec = 30 });
                AddMode(new Mode { Name = "Insane", TargetSize = 35, SpawnIntervalMs = 450,  TargetLifetimeMs = 900,  DurationSec = 30 });
            }
        }

        // ---------- Modes CRUD ----------
        public static List<Mode> GetModes()
        {
            var list = new List<Mode>();
            using var c = Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, TargetSize, SpawnIntervalMs, TargetLifetimeMs, DurationSec FROM Modes ORDER BY Id";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Mode
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1),
                    TargetSize = r.GetInt32(2),
                    SpawnIntervalMs = r.GetInt32(3),
                    TargetLifetimeMs = r.GetInt32(4),
                    DurationSec = r.GetInt32(5)
                });
            }
            return list;
        }

        public static void AddMode(Mode m)
        {
            using var c = Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = @"INSERT INTO Modes (Name, TargetSize, SpawnIntervalMs, TargetLifetimeMs, DurationSec)
                                VALUES ($n, $s, $i, $l, $d)";
            cmd.Parameters.AddWithValue("$n", m.Name);
            cmd.Parameters.AddWithValue("$s", m.TargetSize);
            cmd.Parameters.AddWithValue("$i", m.SpawnIntervalMs);
            cmd.Parameters.AddWithValue("$l", m.TargetLifetimeMs);
            cmd.Parameters.AddWithValue("$d", m.DurationSec);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateMode(Mode m)
        {
            using var c = Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = @"UPDATE Modes SET Name=$n, TargetSize=$s, SpawnIntervalMs=$i,
                                TargetLifetimeMs=$l, DurationSec=$d WHERE Id=$id";
            cmd.Parameters.AddWithValue("$n", m.Name);
            cmd.Parameters.AddWithValue("$s", m.TargetSize);
            cmd.Parameters.AddWithValue("$i", m.SpawnIntervalMs);
            cmd.Parameters.AddWithValue("$l", m.TargetLifetimeMs);
            cmd.Parameters.AddWithValue("$d", m.DurationSec);
            cmd.Parameters.AddWithValue("$id", m.Id);
            cmd.ExecuteNonQuery();
        }

        // Also deletes that mode's sessions and shots (ON DELETE CASCADE)
        public static void DeleteMode(int id)
        {
            using var c = Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = "DELETE FROM Modes WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // ---------- Sessions ----------
        // Call once when the game ends. Saves the session and all shots in one transaction.
        public static int SaveSession(int modeId, List<Shot> shots)
        {
            int total = shots.Count;
            var hits = shots.Where(s => s.IsHit).ToList();
            int score = hits.Count;
            double accuracy = total == 0 ? 0 : 100.0 * score / total;
            double avgReaction = hits.Count == 0 ? 0 : hits.Average(s => s.ReactionMs);

            using var c = Open();
            using var tx = c.BeginTransaction();

            using var ins = c.CreateCommand();
            ins.Transaction = tx;
            ins.CommandText = @"INSERT INTO Sessions (ModeId, PlayedAt, Score, Accuracy, AvgReactionMs)
                                VALUES ($m, $t, $s, $a, $r); SELECT last_insert_rowid();";
            ins.Parameters.AddWithValue("$m", modeId);
            ins.Parameters.AddWithValue("$t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            ins.Parameters.AddWithValue("$s", score);
            ins.Parameters.AddWithValue("$a", accuracy);
            ins.Parameters.AddWithValue("$r", avgReaction);
            int sessionId = Convert.ToInt32(ins.ExecuteScalar());

            foreach (var sh in shots)
            {
                using var cmd = c.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"INSERT INTO Shots (SessionId, IsHit, ReactionMs, X, Y)
                                    VALUES ($sid, $h, $r, $x, $y)";
                cmd.Parameters.AddWithValue("$sid", sessionId);
                cmd.Parameters.AddWithValue("$h", sh.IsHit ? 1 : 0);
                cmd.Parameters.AddWithValue("$r", sh.ReactionMs);
                cmd.Parameters.AddWithValue("$x", sh.X);
                cmd.Parameters.AddWithValue("$y", sh.Y);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return sessionId;
        }

        // For the stats charts. Pass a modeId to filter, or null for all modes.
        public static List<SessionRow> GetSessions(int? modeId = null)
        {
            var list = new List<SessionRow>();
            using var c = Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = @"SELECT s.Id, m.Name, s.PlayedAt, s.Score, s.Accuracy, s.AvgReactionMs
                                FROM Sessions s JOIN Modes m ON m.Id = s.ModeId
                                WHERE ($m IS NULL OR s.ModeId = $m)
                                ORDER BY s.PlayedAt";
            cmd.Parameters.AddWithValue("$m", (object?)modeId ?? DBNull.Value);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new SessionRow
                {
                    Id = r.GetInt32(0),
                    ModeName = r.GetString(1),
                    PlayedAt = DateTime.Parse(r.GetString(2)),
                    Score = r.GetInt32(3),
                    Accuracy = r.GetDouble(4),
                    AvgReactionMs = r.GetDouble(5)
                });
            }
            return list;
        }
    }
}
