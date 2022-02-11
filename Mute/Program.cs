using CSCore;
using CSCore.MediaFoundation;
using CSCore.SoundOut;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Mute.Providers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using SpeechSynthesizer = System.Speech.Synthesis.SpeechSynthesizer;

namespace Mute
{
    public class Program
    {
        private static SpeechSynthesizer _synthesizer = new();
        private static Timer _clear;
        private static bool _disabled;

        private static TaskCompletionSource<int> _stopRecognition = new();

        public static void Main(string[] args)
            => MainAsync(args)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

        private static void Speak(string value)
        {
            using (var stream = new MemoryStream())
            {
                _synthesizer.SetOutputToWaveStream(stream);
                _synthesizer.Speak(value);

                using (var output = new WaveOut { Device = new WaveOutDevice(Configuration.SynthesizerOutput) })
                using (var source = new MediaFoundationDecoder(stream))
                {
                    output.Initialize(source);
                    output.Play();
                    output.WaitForStopped();
                }
            }
        }

#if PRIVATE_BUILD
        private static SpeechConfig CreateConfigFromResources()
        {
            var resources = TokenFetcher.GetResources();
            return SpeechConfig.FromAuthorizationToken(resources.Token, resources.Region);
        }
#endif

        public static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Like my work? Feel free to donate to support more developments: https://www.patreon.com/ItsKaitlyn03");

            Console.WriteLine("Outputs:");
            var devices = WaveOutDevice.EnumerateDevices();
            foreach (var device in devices)
            {
                Console.WriteLine($"{device.DeviceId} > {device.Name}");
            }

            Console.WriteLine($"Selected output: {devices.Where(x => x.DeviceId == Configuration.SynthesizerOutput).First().Name}");

            Console.WriteLine("Voices:");
            var voices = _synthesizer.GetInstalledVoices();
            for (var i = 0; i < voices.Count; i++)
                Console.WriteLine($"{i} > {voices[i].VoiceInfo.Name}");

            var voice = voices[Configuration.SynthesizerVoice].VoiceInfo.Name;
            Console.WriteLine($"Selected voice: {voice}");

            _synthesizer.SelectVoice(voice);

            if (Configuration.SpeechLogging)
                File.WriteAllText(Configuration.LatestPath, string.Empty);

            _clear = new Timer(Configuration.SpeechClearAfter);
            _clear.Elapsed += (s, e) => File.WriteAllText(Configuration.LatestPath, string.Empty);
            _clear.AutoReset = false;

            while (true)
            {
                SpeechConfig config = null;
                if (Configuration.SpeechUseAuthToken)
                {
                    if (!string.IsNullOrEmpty(Configuration.SpeechAuthToken) && !string.IsNullOrEmpty(Configuration.SpeechRegion))
                        config = SpeechConfig.FromAuthorizationToken(Configuration.SpeechAuthToken, Configuration.SpeechRegion);
#if PRIVATE_BUILD
                    else
                        config = CreateConfigFromResources();
#endif
                }
                else
                {
                    if (!string.IsNullOrEmpty(Configuration.SpeechSubscriptionKey) && !string.IsNullOrEmpty(Configuration.SpeechRegion))
                        config = SpeechConfig.FromSubscription(Configuration.SpeechSubscriptionKey, Configuration.SpeechRegion);
#if PRIVATE_BUILD
                    else
                        config = CreateConfigFromResources();
#endif
                }

                if (config == null)
                {
                    Console.WriteLine("Config is null.");
                    return;
                }

                if (Configuration.SpeechDictation)
                    config.EnableDictation();

                config.SpeechRecognitionLanguage = Configuration.SpeechLanguage;

                config.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "5000");
                config.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "5000");

                using (var recognizer = new SpeechRecognizer(config, AudioConfig.FromDefaultMicrophoneInput()))
                {
                    recognizer.Canceled += OnCanceled;
                    recognizer.Recognized += OnRecognized;

                    Console.WriteLine("Now listening, start talking and your voice will be converted from Speech-To-Text-To-Speech!");

                    await recognizer.StartContinuousRecognitionAsync();
                    Task.WaitAny(new[] { _stopRecognition.Task });
                    await recognizer.StopContinuousRecognitionAsync();
                }

                _stopRecognition = new();
            }
        }

        private static void OnCanceled(object sender, SpeechRecognitionCanceledEventArgs e)
            => _stopRecognition.TrySetResult(0);

        private static void OnRecognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Console.WriteLine($"Recognized: {e.Result.Text}");

                var isStartKeyword = false;
                var isPauseKeyword = false;

                if (Configuration.KeywordsEnabled)
                {
                    if (Configuration.KeywordsType == "Contains")
                    {
                        isStartKeyword = Configuration.KeywordsStart.Where(x => e.Result.Text.ToUpper().Contains(x.ToUpper())).Count() != 0;
                        isPauseKeyword = Configuration.KeywordsPause.Where(x => e.Result.Text.ToUpper().Contains(x.ToUpper())).Count() != 0;
                    }
                    else if (Configuration.KeywordsType == "StartsWith")
                    {
                        isStartKeyword = Configuration.KeywordsStart.Where(x => e.Result.Text.ToUpper().StartsWith(x.ToUpper())).Count() != 0;
                        isPauseKeyword = Configuration.KeywordsPause.Where(x => e.Result.Text.ToUpper().StartsWith(x.ToUpper())).Count() != 0;
                    }
                    else if (Configuration.KeywordsType == "EndsWith")
                    {
                        isStartKeyword = Configuration.KeywordsStart.Where(x => e.Result.Text.ToUpper().EndsWith(x.ToUpper())).Count() != 0;
                        isPauseKeyword = Configuration.KeywordsPause.Where(x => e.Result.Text.ToUpper().EndsWith(x.ToUpper())).Count() != 0;
                    }
                }

                if (isPauseKeyword && !isStartKeyword && !_disabled)
                {
                    Console.WriteLine("Synthesizer and Speech Logging is now disabled.");
                    _disabled = true;
                }

                if (!_disabled && Configuration.SpeechLogging)
                {
                    File.AppendAllText(Configuration.LogPath, $"{e.Result.Text}\n");
                    File.WriteAllText(Configuration.LatestPath, e.Result.Text);

                    _clear.Stop();
                    _clear.Start();
                }

                if (!_disabled && Configuration.SynthesizerEnabled)
                    Speak(e.Result.Text);

                if (isStartKeyword && !isPauseKeyword && _disabled)
                {
                    Console.WriteLine("Synthesizer and Speech Logging is now enabled.");
                    _disabled = false;
                }
            }
        }
    }
}
