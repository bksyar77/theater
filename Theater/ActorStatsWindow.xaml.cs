using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Theater.Models;

namespace Theater
{
    public partial class ActorStatsWindow : Window
    {
        private class StatsItem
        {
            public string EventType { get; set; }
            public int ParticipationCount { get; set; }
            public double TotalHours { get; set; }
        }

        private readonly Actor _actor;

        public ActorStatsWindow(Actor actor)
        {
            InitializeComponent();
            _actor = actor ?? throw new ArgumentNullException(nameof(actor));
            LoadStats();
        }

        private void LoadStats()
        {
            ActorNameTextBlock.Text = $"Актёр: {_actor.Name}";

            var groups = _actor.Participations
                .Where(p => p.Event != null)
                .GroupBy(p => p.Event.Type ?? "Без типа")
                .Select(g => new StatsItem
                {
                    EventType = g.Key,
                    ParticipationCount = g.Count(),
                    TotalHours = g.Sum(p => p.Hours)
                })
                .OrderByDescending(x => x.TotalHours)
                .ToList();

            StatsDataGrid.ItemsSource = groups;

            var totalPart = groups.Sum(g => g.ParticipationCount);
            var totalHours = groups.Sum(g => g.TotalHours);

            TotalsTextBlock.Text = $"Всего участий: {totalPart}, всего часов: {totalHours:N2}";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
