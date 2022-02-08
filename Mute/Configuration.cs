using Mute.Utilities;
using System.IO;
using System.Reflection;

namespace Mute
{
    internal class Configuration
    {
        public static string AssemblyDirectory => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public static bool SpeechUseAuthToken { get; private set; }
        public static string SpeechSubscriptionKey { get; private set; }
        public static string SpeechAuthToken { get; private set; }
        public static string SpeechRegion { get; private set; }
        public static bool SpeechDictation { get; private set; }
        public static string SpeechLanguage { get; private set; }

        public static bool SynthesizerEnabled { get; private set; }
        public static int SynthesizerVoice { get; private set; }

        public static bool LogFileEnabled { get; private set; }

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

            bool.TryParse(iniParser.Value("Synthesizer_Enabled", "True"), out var synthesizerEnabled);
            SynthesizerEnabled = synthesizerEnabled;

            int.TryParse(iniParser.Value("Synthesizer_Voice", "0"), out var synthesizerVoice);
            SynthesizerVoice = synthesizerVoice;

            bool.TryParse(iniParser.Value("LogFile_Enabled", "True"), out bool logFileEnabled);
            LogFileEnabled = logFileEnabled;
        }
    }
}
