#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace MusicBeePlugin
{
    public class NeteaseConfig
    {
        public enum OutputFormat
        {
            Original = 0,
            Both = 1,
            Translation = 2
        }

        public OutputFormat format { get; set; } = OutputFormat.Both;
        public bool fuzzy { get; set; } = false;
    }

    public partial class Plugin
    {
        private const string ProviderName = "QQ Music(QQ音乐)";
        private const string ConfigFilename = "qq_config";
        private const string NoTranslateFilename = "qq_notranslate";
        private NeteaseConfig _config = new NeteaseConfig();
        private ComboBox _formatComboBox = null;
        private CheckBox _fuzzyCheckBox = null;

        private MusicBeeApiInterface _mbApiInterface;
        private readonly PluginInfo _about = new PluginInfo();

        // ReSharper disable once UnusedMember.Global
        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            var versions = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');

            _mbApiInterface = new MusicBeeApiInterface();
            _mbApiInterface.Initialise(apiInterfacePtr);
            _about.PluginInfoVersion = PluginInfoVersion;
            _about.Name = "QQ Lyrics";
            _about.Description = "A plugin to retrieve lyrics from QQ Music.(从 QQ 音乐获取歌词的插件。)";
            _about.Author = "Mslxl, Charlie Jiang";
            _about.TargetApplication =
                ""; // current only applies to artwork, lyrics or instant messenger name that appears in the provider drop down selector or target Instant Messenger
            _about.Type = PluginType.LyricsRetrieval;
            _about.VersionMajor = short.Parse(versions[0]); // your plugin version
            _about.VersionMinor = short.Parse(versions[1]);
            _about.Revision = short.Parse(versions[2]);
            _about.MinInterfaceVersion = MinInterfaceVersion;
            _about.MinApiRevision = MinApiRevision;
            _about.ReceiveNotifications = ReceiveNotificationFlags.DownloadEvents;
            _about.ConfigurationPanelHeight =
                90; // height in pixels that musicbee should reserve in a panel for config settings. When set, a handle to an empty panel will be passed to the Configure function

            string noTranslatePath =
                Path.Combine(_mbApiInterface.Setting_GetPersistentStoragePath(), NoTranslateFilename);
            string configPath = Path.Combine(_mbApiInterface.Setting_GetPersistentStoragePath(), ConfigFilename);
            if (File.Exists(configPath))
            {
                try
                {
                    _config = JsonConvert.DeserializeObject<NeteaseConfig>(File.ReadAllText(configPath, Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    _mbApiInterface.MB_Trace("[QQMusic] Failed to load config" + ex);
                }
            }

            if (File.Exists(noTranslatePath))
            {
                File.Delete(noTranslatePath);
                _config.format = NeteaseConfig.OutputFormat.Original;
                SaveSettingsInternal();
            }

            return _about;
        }

        // ReSharper disable once UnusedMember.Global
        public bool Configure(IntPtr panelHandle)
        {
            if (panelHandle == IntPtr.Zero) return false;
            var configPanel = (Panel)Control.FromHandle(panelHandle);
            // Components are automatically disposed when this is called.
            configPanel.Controls.Clear();

            // MB_AddPanel doesn't skin the component correctly either
            //_formatComboBox = (ComboBox)_mbApiInterface.MB_AddPanel(null, PluginPanelDock.ComboBox);
            _formatComboBox = new ComboBox();
            _formatComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _formatComboBox.Items.Add("Only original text");
            _formatComboBox.Items.Add("Original text and translation");
            _formatComboBox.Items.Add("Only translation");
            _formatComboBox.AutoSize = true;
            _formatComboBox.Location = new Point(0, 0);
            _formatComboBox.Width = 300;
            _formatComboBox.SelectedIndex = (int)_config.format;
            configPanel.Controls.Add(_formatComboBox);

            _fuzzyCheckBox = new CheckBox();
            _fuzzyCheckBox.Text = "Fuzzy matching (Don't double check match and use first result directly)";
            _fuzzyCheckBox.Location = new Point(0, 50);
            _fuzzyCheckBox.Checked = _config.fuzzy;
            _fuzzyCheckBox.AutoSize = true;
            configPanel.Controls.Add(_fuzzyCheckBox);
            return false;
        }

        // called by MusicBee when the user clicks Apply or Save in the MusicBee Preferences screen.
        // its up to you to figure out whether anything has changed and needs updating
        // ReSharper disable once UnusedMember.Global
        public void SaveSettings()
        {
            if (_formatComboBox.SelectedIndex < 0 || _formatComboBox.SelectedIndex > 2)
                _config.format = NeteaseConfig.OutputFormat.Both;
            else
                _config.format = (NeteaseConfig.OutputFormat)_formatComboBox.SelectedIndex;
            _config.fuzzy = _fuzzyCheckBox.Checked;
            SaveSettingsInternal();
        }

        private void SaveSettingsInternal()
        {
            string configPath = Path.Combine(_mbApiInterface.Setting_GetPersistentStoragePath(), ConfigFilename);
            var json = JsonConvert.SerializeObject(_config);
            File.WriteAllText(configPath, json, Encoding.UTF8);
        }

        // MusicBee is closing the plugin (plugin is being disabled by user or MusicBee is shutting down)
        // ReSharper disable once UnusedMember.Global
        public void Close(PluginCloseReason reason)
        {
        }

        // uninstall this plugin - clean up any persisted files
        // ReSharper disable once UnusedMember.Global
        public void Uninstall()
        {
            var dataPath = _mbApiInterface.Setting_GetPersistentStoragePath();
            var p = Path.Combine(dataPath, NoTranslateFilename);
            if (File.Exists(p)) File.Delete(p);
            string configPath = Path.Combine(_mbApiInterface.Setting_GetPersistentStoragePath(), ConfigFilename);
            if (File.Exists(configPath)) File.Delete(configPath);
        }

        // ReSharper disable once UnusedMember.Global
        public string? RetrieveLyrics(string sourceFileUrl, string artist, string trackTitle, string album,
            bool synchronisedPreferred, string provider)
        {
            if (provider != ProviderName) return null;
            string? mid = null;
            var specifiedId = _mbApiInterface.Library_GetFileTag(sourceFileUrl, MetaDataType.Custom10)
                              ?? _mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Custom10);

            mid = TryParseQQURL(specifiedId);

            if (mid == null)
            {
                var searchResult = QueryWithFeatRemoved(trackTitle, artist);
                if (searchResult == null) return null;
                mid = searchResult.mid;
            }
            
            if (mid == null)
                return null;

            var lyricResult = RequestLyric(mid);

            if (lyricResult?.lyric == null) return null;
            if (lyricResult.trans == null || _config.format == NeteaseConfig.OutputFormat.Original)
                return lyricResult.lyric; // No need to process translation

            if (_config.format == NeteaseConfig.OutputFormat.Translation)
                return lyricResult.trans ?? lyricResult.lyric;
            // translation
            return LyricProcessor.InjectTranslation(lyricResult.lyric, lyricResult.trans);
        }

        private SearchResultSong? QueryWithFeatRemoved(string trackTitle, string artist)
        {
            var ret = Query(trackTitle, artist);
            if (ret != null) return ret;

            ret = Query(RemoveLeadingNumber(RemoveFeat(trackTitle)), artist);
            return ret;
        }

        private SearchResultSong? Query(string trackTitle, string artist)
        {
            var ret = Query(trackTitle + " " + artist)?.result?.Where(rst =>
                _config.fuzzy || string.Equals(GetFirstSeq(RemoveLeadingNumber(rst.name)), GetFirstSeq(trackTitle),
                    StringComparison.OrdinalIgnoreCase)).ToList();

            if (ret != null && ret.Count > 0) return ret[0];

            ret = Query(trackTitle)?.result?.Where(rst =>
                _config.fuzzy || string.Equals(GetFirstSeq(RemoveLeadingNumber(rst.name)), GetFirstSeq(trackTitle),
                    StringComparison.OrdinalIgnoreCase)).ToList();
            return ret != null && ret.Count > 0 ? ret[0] : null;
        }

        private static SearchResult? Query(string s)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.Referer, "https://c.y.qq.com/");
                //client.Headers.Add(HttpRequestHeader.Cookie, "appver=1.5.0.75771;");

                //var searchPost = new NameValueCollection
                //{
                //    ["s"] = s,
                //    ["limit"] = "6",
                //    ["offset"] = "0",
                //    ["type"] = "1"
                //};
                var nameEncoded = Uri.EscapeUriString(s);
                JObject retObj = JObject.Parse(Encoding.UTF8.GetString(
                    client.DownloadData(
                        $"https://c.y.qq.com/splcloud/fcgi-bin/smartbox_new.fcg?format=json&inCharset=utf-8&outCharset=utf-8&platform=yqq.json&key={nameEncoded}")
                ));
                var retCode = retObj["code"].Value<int>();
                if (retCode != 0) return null;
                var searchResult = new SearchResult
                {
                    code = retCode,
                    result = (from item in retObj["data"]["song"]["itemlist"].Children()
                            select new SearchResultSong()
                                { name = item["name"].Value<string>(), mid = item["mid"].Value<string>() }
                        ).ToArray()
                };


                return searchResult.result.Length <= 0 ? null : searchResult;
            }
        }

        private static LyricResult? RequestLyric(string mid)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:104.0) Gecko/20100101 Firefox/104.0");
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add(HttpRequestHeader.Referer, "https://y.qq.com/");
                client.Encoding = Encoding.UTF8;
                // client.Headers.Add(HttpRequestHeader.Cookie, "appver=1.5.0.75771;");
                var response = Encoding.UTF8.GetString(client.DownloadData(
                    $"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?format=json&inCharset=utf-8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=1&uin=0&loginUin=0&songmid={mid}"));
                var lyricResult = JsonConvert.DeserializeObject<LyricResult>(response);
                if (lyricResult.code == 0)
                {
                    lyricResult.lyric = Encoding.UTF8.GetString(Convert.FromBase64String(lyricResult.lyric));
                    if (!string.IsNullOrEmpty(lyricResult.trans))
                    {
                        lyricResult.trans = Encoding.UTF8.GetString(Convert.FromBase64String(lyricResult.trans!));
                    }
                    return lyricResult;
                }
                return null;
            }
        }

        private static string GetFirstSeq(string s)
        {
            s = s.Replace("\u00A0", " ");
            var pos = s.IndexOf(' ');
            return s.Substring(0, pos == -1 ? s.Length : pos).Trim();
        }

        private string RemoveFeat(string name)
        {
            return Regex.Replace(name, "\\s*\\(feat.+\\)", "", RegexOptions.IgnoreCase);
        }

        private static string RemoveLeadingNumber(string name)
        {
            return Regex.Replace(name, "^\\d+\\.?\\s*", "", RegexOptions.IgnoreCase);
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        private static string? TryParseQQURL(string? input)
        {
            if (input == null)
                return null;
            if (input.StartsWith("qq="))
            {
                input = input.Substring("qq=".Length);
                return input;
            }

            // TODO: 等我下个 QQ 音乐看看是不是原来有 Tag 信息 :(
            // if (input.Contains("music.163.com"))
            // {
            //     var matches = Regex.Matches(input, "id=(\\d+)");
            //     if (matches.Count <= 0)
            //         return 0;
            //
            //     var groups = matches[0].Groups;
            //     if (groups.Count <= 1)
            //         return 0;
            //
            //     var idString = groups[1].Captures[0].Value;
            //     long.TryParse(idString, out var id);
            //     return id;
            // }

            return null;
        }

        public string[] GetProviders()
        {
            return new[] { ProviderName };
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
        }
    }
}