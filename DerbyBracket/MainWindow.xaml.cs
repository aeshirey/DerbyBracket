using DerbyBracket.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DerbyBracket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int DefaultResultsToShow = 5;
        private const int LaneCount = 4;
        private Race SelectedRace;
        private RaceBracket Bracket;

        private const string ParametersJson = "RaceParameters1.json";
        private const string BracketJson = "RaceBracket1.json";

        /// <summary>
        /// Indicates whether any of the times in the 'Races' tab have been altered by the user.
        /// If so, the new values should be saved to disk.
        /// </summary>
        private bool TimeChanged = false;

        public JavaScriptSerializer Serializer = new JavaScriptSerializer();
        public MainWindow()
        {
            InitializeComponent();

            if (Utility.NewVersionExists())
            {
                MessageBoxResult result = MessageBox.Show("There is a new version available. Do you want to download it?", "New version available", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.None);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(Utility.DownloadUrl);
                }
            }

            if (File.Exists(ParametersJson))
            {
                var json = File.ReadAllText(ParametersJson);
                var parameters = Serializer.Deserialize<RaceParameters>(json);
                tbRacerNames.Text = string.Join("\r\n", parameters.Racers);

                tbResultsToShow.Text = (parameters.ResultsToShow > 0 ? parameters.ResultsToShow : DefaultResultsToShow).ToString();
            }

            if (File.Exists(BracketJson))
            {
                var json = File.ReadAllText(BracketJson);
                this.Bracket = Serializer.Deserialize<RaceBracket>(json);

                if (this.Bracket != null)
                {
                    datagridHeats.Items.Clear();
                    foreach (var race in this.Bracket.Races)
                    {
                        datagridHeats.Items.Add(race);
                    }

                    // do we have times for every single race?
                    btnShowResults.IsEnabled = datagridHeats
                        .AllItems<Race>()
                        .All(r => r.Racers.All(rcr => rcr.Time.HasValue));

                    tabRaces.IsSelected = true;
                }
            }
        }

        private void BtnGenerateHeats_OnClick(object sender, RoutedEventArgs e)
        {
            var racers = this.tbRacerNames.Text.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int resultsToShow;
            if (!int.TryParse(tbResultsToShow.Text, out resultsToShow))
            {
                resultsToShow = DefaultResultsToShow;
            }

            var parameters = new RaceParameters
            {
                Racers = racers,
                ResultsToShow = resultsToShow
            };

            File.WriteAllText(ParametersJson, Serializer.Serialize(parameters));

            this.tabControl.TabIndex = 1;

            this.Bracket = new RaceBracket(racers, 4);
            File.WriteAllText(BracketJson, Serializer.Serialize(Bracket));

            datagridHeats.Items.Clear();
            foreach (var race in Bracket.Races)
            {
                datagridHeats.Items.Add(race);
            }

            tabRaces.IsSelected = true;
        }

        private void datagridHeats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems[0] as Race;

            if (this.SelectedRace != null && this.SelectedRace != item)
            {
                // save the old values
                double d;
                this.SelectedRace.Racers[0].Time = double.TryParse(Racer1Time.Text, out d) ? d : (double?)null;
                this.SelectedRace.Racers[1].Time = double.TryParse(Racer2Time.Text, out d) ? d : (double?)null;
                this.SelectedRace.Racers[2].Time = double.TryParse(Racer3Time.Text, out d) ? d : (double?)null;
                this.SelectedRace.Racers[3].Time = double.TryParse(Racer4Time.Text, out d) ? d : (double?)null;

                if (TimeChanged)
                {
                    // save the bracket to disk
                    File.WriteAllText(BracketJson, Serializer.Serialize(this.Bracket));
                    TimeChanged = false;
                }

                // do we have times for every single race?
                btnShowResults.IsEnabled = datagridHeats
                    .AllItems<Race>()
                    .All(r => r.Racers.All(rcr => rcr.Time.HasValue));
                //
            }

            this.SelectedRace = item;

            Racer1Time.Text = item.Racers[0].Time?.ToString("#.000") ?? string.Empty;
            Racer2Time.Text = item.Racers[1].Time?.ToString("#.000") ?? string.Empty;
            Racer3Time.Text = item.Racers[2].Time?.ToString("#.000") ?? string.Empty;
            Racer4Time.Text = item.Racers[3].Time?.ToString("#.000") ?? string.Empty;
        }

        private void Racer1Time_TextChanged(object sender, TextChangedEventArgs e)
        {
            TimeChanged = true;
        }

        private void btnShowResults_Click(object sender, RoutedEventArgs e)
        {
            var times = new Dictionary<string, IList<double>>();

            // get all racers
            datagridHeats.AllItems<Race>()
                .SelectMany(r => r.Racers.Select(rcr => rcr.Racer))
                .Distinct()
                .ForEach(racer => times[racer] = new List<double>());

            // collect all times
            foreach (var race in datagridHeats.AllItems<Race>())
            {
                foreach (var racer in race.Racers)
                {
                    times[racer.Racer].Add(racer.Time.Value);
                }
            }


            int resultsToShow;
            if (!int.TryParse(tbResultsToShow.Text, out resultsToShow))
            {
                resultsToShow = DefaultResultsToShow;
            }

            // calculate the average and bests
            datagridResults.Items.Clear();
            times.Select(kvp => new { Racer = kvp.Key, Average = kvp.Value.Average(), Best = kvp.Value.Min() })
                 .OrderBy(obj => obj.Average)
                 .Take(resultsToShow)
                 .ForEach(obj => datagridResults.Items.Add(obj));

            tabStatistics.IsSelected = true;
        }

        private void tbResultsToShow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int rts;
            if (!int.TryParse(tbResultsToShow.Text, out rts))
            {
                tbResultsToShow.Text = DefaultResultsToShow.ToString();
                return;
            }

            rts = e.Delta < 0 ? rts - 1 : rts + 1;

            tbResultsToShow.Text = rts.ToString();
        }

        private void tbAboutContent_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
