namespace ExBuddy.Plugins.Skywatcher
{
    using ExBuddy.Helpers;
    using ExBuddy.Interfaces;
    using ExBuddy.Logging;
    using ff14bot.Managers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class FF14AnglerWeatherProvider : IWeatherProvider
    {
        private static readonly object Locker = new object();

        private static readonly Regex WeatherTitleRegex = new Regex(
            @"<(img)\b[^>]*title='(.*)'>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static int lastInterval;

        private static readonly Timer RequestTimer = new Timer(GetEntries);

        private static readonly IDictionary<int, int> ZoneMap = new Dictionary<int, int>
        {
            {129, 1}, // Limsa Lominsa
            {134, 2}, // Middle La Noscea
            {135, 3}, // Lower La Noscea
            {137, 4}, // Eastern La Noscea
            {138, 5}, // Western La Noscea
            {139, 6}, // Upper La Noscea
            {180, 7}, // Outer La Noscea
            {250, 8}, // Wolves' Den Pier
            {339, 9}, // Mist
			{132, 10}, // Gridania
            {148, 11}, // Central Shroud
            {152, 12}, // East Shroud
            {153, 13}, // South Shroud
            {154, 14}, // North Shroud
            {340, 15}, // The Lavender Beds
			{130, 16}, // Ul'dah
            {140, 17}, // Western Thanalan
            {141, 18}, // Central Thanalan
            {145, 19}, // Eastern Thanalan
            {146, 20}, // Southern Thanalan
            {147, 21}, // Northern Thanalan
            {341, 22}, // The Goblet
            {156, 24}, // Mor Dhona
			{155, 23}, // Coerthas Central Highlands
			{418, 25}, // Ishgard
			{397, 26}, // Coerthas Western Highlands
			{401, 27}, // The Sea of Clouds
            {402, 28}, // Azys Lla
            {478, 29}, // Idyllshire
			{398, 30}, // Dravanian Forelands
			{399, 31}, // Dravanian Hinterlands
			{400, 32}, // The Churning Mists
            {635, 33}, // Rhalgar's Reach
            {612, 34}, // The Fringes
            {620, 35}, // The Peaks
            {621, 36}, // The Lochs
            {628, 37}, // Kugane
            {641, 38}, // Shirogane
            {613, 39}, // The Ruby Sea
            {614, 40}, // Yanxia
            {622, 41}, // The Azim Steppe
            //{, 42}, // Eureka Anemos
            //{, 43}, // Eureka Pagos
            //{, 44}, // Eureka Pyros
            //{, 45}, // Eureka Hydatos
            //{, 46}, // Bozjan Southern Front
            //{, 47} // Zandor
            //{, 48} // 
            //{, 49} // 
            //{, 50} // 
            {819, 51}, // The Crystarium
            {820, 52}, // Eulmore
            {813, 53}, // Lakeland
            {814, 54}, // Kholusia
            {815, 55}, // Amh Araeng
            {816, 56}, // Il Mheg
            {817, 57}, // The Rak'tika Greatwood
            {818, 58}, // The Temptest
            {962, 59}, // Old Sharlayan
            {956, 60}, // Labyrinthos
            {963, 61}, // Radz-at-Han
            {957, 62}, // Thavnair
            {958, 63}, // Garlemald
            {959, 64}, // Mare Lamentorum
            {960, 65}, // Ultima Thule
            {961, 66} // Elpis
            //{, 1002} // Island Sanctuary
            //{, 1003} // Elysion
		};

        private static IList<WeatherResult> weatherResults;

        public bool IsEnabled { get; private set; }

        /// <summary>
        ///     Gets the entries.
        /// </summary>
        /// <param name="stateInfo">The state info.</param>
        private static void GetEntries(object stateInfo)
        {
            if (WorldManager.EorzaTime.TimeOfDay.Hours % 8 == 0 || weatherResults == null
                || lastInterval < SkywatcherPlugin.GetIntervalNumber())
            {
                HttpClient client = null;
                try
                {
                    client = new HttpClient();
                    var response = client.GetContentAsync<WeatherResponse>("http://en.ff14angler.com/skywatcher.php").Result;
                    if (response.Interval > lastInterval)
                    {
                        // Ensure we at least have all of the entries for the current time.
                        if (response.Data.Count(w => w.Time == 0) >= 32 || weatherResults == null)
                        {
                            lastInterval = response.Interval;
                            weatherResults = response.Data;
                        }
                        // If there are 32 or more weather forecasts, shift all weather down an interval.
                        else if (weatherResults.Count(w => w.Time == 1) >= 32)
                        {
                            foreach (var w in weatherResults)
                            {
                                w.Time--;
                            }
                        }
                    }
                    else
                    {
                        // New interval not posted, retry every 30 seconds
                        RequestTimer.Change(
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromMilliseconds((int)SkywatcherPlugin.GetTimeTillNextInterval()));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex.Message);
                }
                finally
                {
                    if (client != null)
                    {
                        client.Dispose();
                    }
                }
            }
        }

        private string GetTitleFromHtmlImg(string htmlString)
        {
            var match = WeatherTitleRegex.Match(htmlString);
            if (match.Success)
            {
                return match.Groups[2].Value;
            }

            return "Parse Failure";
        }

        #region IWeatherProvider Members

        public void Disable()
        {
            lock (Locker)
            {
                if (IsEnabled)
                {
                    IsEnabled = false;
                    RequestTimer.Change(-1, -1);
                    weatherResults.Clear();
                    weatherResults = null;
                    lastInterval = 0;
                }
            }
        }

        public void Enable()
        {
            lock (Locker)
            {
                if (!IsEnabled)
                {
                    IsEnabled = true;
                    RequestTimer.Change(0, (int)SkywatcherPlugin.GetTimeTillNextInterval());
                }
            }
        }

        public int? GetCurrentWeatherByZone(int zoneId)
        {
            int ff14AnglerZoneId;
            if (!ZoneMap.TryGetValue(zoneId, out ff14AnglerZoneId))
            {
                return null;
            }

            var weather = weatherResults.FirstOrDefault(s => s.Time == 0 && s.Area == ff14AnglerZoneId);

            if (weather != null)
            {
                return (int)weather.Weather;
            }

            return null;
        }

        public int? GetForecastByZone(int zoneId, TimeSpan timeSpan)
        {
            int time;
            var etTillNextInterval = SkywatcherPlugin.GetEorzeaTimeTillNextInterval();

            if (timeSpan > etTillNextInterval.Add(TimeSpan.FromHours(8)))
            {
                time = 2;
            }
            else if (timeSpan > etTillNextInterval)
            {
                time = 1;
            }
            else
            {
                time = 0;
            }

            int ff14AnglerZoneId;
            if (!ZoneMap.TryGetValue(zoneId, out ff14AnglerZoneId))
            {
                return null;
            }

            var weather = weatherResults.FirstOrDefault(s => s.Time == time && s.Area == ff14AnglerZoneId);

            if (weather != null)
            {
                return (int)weather.Weather;
            }

            return null;
        }

        #endregion IWeatherProvider Members
    }
}