using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Mute.Providers;
using System;
using System.IO;
using System.Threading.Tasks;
using SpeechSynthesizer = System.Speech.Synthesis.SpeechSynthesizer;

namespace Mute
{
    public class Program
    {
        private static SpeechSynthesizer _synthesizer = new();

        public static void Main(string[] args)
            => MainAsync(args)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

        private static SpeechConfig CreateConfigFromResources()
        {
            var resources = TokenFetcher.GetResources();
            return SpeechConfig.FromAuthorizationToken(resources.Token, resources.Region);
        }

        public static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Voices:");
            var voices = _synthesizer.GetInstalledVoices();
            for (var i = 0; i < voices.Count; i++)
                Console.WriteLine($"{i} > {voices[i].VoiceInfo.Name}");

            var voice = voices[Configuration.SynthesizerVoice].VoiceInfo.Name;
            Console.WriteLine($"Selected voice: {voice}");

            _synthesizer.SelectVoice(voice);
            _synthesizer.SetOutputToDefaultAudioDevice();

            SpeechConfig config;
            if (Configuration.SpeechUseAuthToken)
            {
                if (!string.IsNullOrEmpty(Configuration.SpeechAuthToken) && !string.IsNullOrEmpty(Configuration.SpeechRegion))
                    config = SpeechConfig.FromAuthorizationToken(Configuration.SpeechAuthToken, Configuration.SpeechRegion);
                else
                    config = CreateConfigFromResources();
            }
            else
            {
                if (!string.IsNullOrEmpty(Configuration.SpeechSubscriptionKey) && !string.IsNullOrEmpty(Configuration.SpeechRegion))
                    config = SpeechConfig.FromSubscription(Configuration.SpeechSubscriptionKey, Configuration.SpeechRegion);
                else
                    config = CreateConfigFromResources();
            }
            
            if (Configuration.SpeechDictation)
                config.EnableDictation();

            config.SpeechRecognitionLanguage = Configuration.SpeechLanguage;

            config.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "5000");
            config.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "5000");

            var recognizer = new SpeechRecognizer(config, AudioConfig.FromDefaultMicrophoneInput());
            recognizer.Recognized += OnRecognized;

            Console.WriteLine("Now listening, start talking and your voice will be converted from Speech-To-Text-To-Speech!");

            await recognizer.StartContinuousRecognitionAsync();
            await Task.Delay(-1);
        }

        private static void OnRecognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"Recognized: {e.Result.Text}");

                if (Configuration.SynthesizerEnabled)
                    _synthesizer.Speak(e.Result.Text);

                if (Configuration.LogFileEnabled)
                    File.AppendAllText(Path.Combine(Configuration.AssemblyDirectory, "Log.txt"), $"{e.Result.Text}\n");
            }
        }
    }
}
