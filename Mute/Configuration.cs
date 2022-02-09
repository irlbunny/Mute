using Mute.Utilities;
using System.IO;
using System.Reflection;

namespace Mute
{
    internal class Configuration
    {
        public static string AssemblyDirectory => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static string LogPath => Path.Combine(AssemblyDirectory, "log.txt");
        public static string LatestPath => Path.Combine(AssemblyDirectory, "latest.txt");

        public static bool SpeechUseAuthToken { get; private set; }
        public static string SpeechSubscriptionKey { get; private set; }
        public static string SpeechAuthToken { get; private set; }
        public static string SpeechRegion { get; private set; }
        public static bool SpeechDictation { get; private set; }
        public static string SpeechLanguage { get; private set; }
        public static bool SpeechLogging { get; private set; }
        public static int SpeechClearAfter { get; private set; }

        public static bool SynthesizerEnabled { get; private set; }
        public static int SynthesizerVoice { get; private set; }
        public static int SynthesizerOutput { get; private set; }

        public static bool KeywordsEnabled { get; private set; }
        public static string KeywordsType { get; private set; }
        public static string[] KeywordsStart { get; private set; }
        public static string[] KeywordsPause { get; private set; }

        static Configuration()
        {
            var iniParser = new IniParser(Path.Combine(AssemblyDirectory, "Mute.ini"));

            bool.TryParse(iniParser.Value("Speech_UseAuthToken", "True"), out var speechUseAuthToken);
            SpeechUseAuthToken = speechUseAuthToken;
            SpeechSubscriptionKey = iniParser["Speech_SubscriptionKey"];
            SpeechAuthToken = iniParser["Speech_AuthToken"];
            SpeechRegion = iniParser["Speech_Region"];
            bool.TryParse(iniParser.Value("Speech_Dictation", "True"), out var speechDictation);
            SpeechDictation = speechDictation;
            SpeechLanguage = iniParser.Value("Speech_Language", "en-US");
            bool.TryParse(iniParser.Value("Speech_Logging", "True"), out var speechLogging);
            SpeechLogging = speechLogging;
            int.TryParse(iniParser.Value("Speech_ClearAfter", "15000"), out var speechClearAfter);
            SpeechClearAfter = speechClearAfter;

            bool.TryParse(iniParser.Value("Synthesizer_Enabled", "True"), out var synthesizerEnabled);
            SynthesizerEnabled = synthesizerEnabled;
            int.TryParse(iniParser.Value("Synthesizer_Voice", "0"), out var synthesizerVoice);
            SynthesizerVoice = synthesizerVoice;
            int.TryParse(iniParser.Value("Synthesizer_Output", "0"), out var synthesizerOutput);
            SynthesizerOutput = synthesizerOutput;

            bool.TryParse(iniParser.Value("Keywords_Enabled", "False"), out var keywordsEnabled);
            KeywordsEnabled = keywordsEnabled;
            KeywordsType = iniParser["Keywords_Type"] ?? "StartsWith";
            var keywordsStart = iniParser.Parse<string[]>("Keywords_Start");
            KeywordsStart = (keywordsStart != null && keywordsStart.Length > 0) ? keywordsStart : new string[] { "Hi" };
            var keywordsPause = iniParser.Parse<string[]>("Keywords_Pause");
            KeywordsPause = (keywordsPause != null && keywordsPause.Length > 0) ? keywordsPause : new string[] { "Goodbye" };
        }
    }
}
