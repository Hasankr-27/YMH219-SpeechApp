using Microsoft.VisualBasic;
using NAudio.Wave;
using System.Text.Json;
using Vosk;

namespace YMH219_SpeechApp
{
    // Mikrofondan gelen konuşmayı Vosk ile (internetsiz) metne çeviren servis
    public class SpeechToTextService : IDisposable
    {
        private static readonly string ModelPath =
            Path.Combine(AppContext.BaseDirectory, "Models", "vosk-model-small-tr-0.3");

        private Model? model;                 // Türkçe dil modeli (bir kere yüklenir)
        private VoskRecognizer? recognizer;   // sesi metne çeviren tanıyıcı
        private WaveInEvent? microphone;      // mikrofondan ses alan NAudio nesnesi

        public bool IsListening { get; private set; }
        public string LanguageName => "Türkçe (Vosk, çevrimdışı)";

        public event Action<string, DateTime>? TextRecognized;   // tanınan metin + saat
        public event Action? ListeningChanged;                   // dinleme başladı/bitti
        public event Action<string>? ErrorOccurred;              // dinleme sırasında hata

        public void Start()
        {
            if (IsListening) return;

            if (WaveInEvent.DeviceCount == 0)
                throw new InvalidOperationException("Bilgisayara bağlı bir mikrofon bulunamadı.");

            if (model == null)
            {
                if (!Directory.Exists(ModelPath))
                    throw new DirectoryNotFoundException(
                        "Konuşma tanıma modeli bulunamadı. Model klasörü şurada olmalı:\n" + ModelPath);

                Vosk.Vosk.SetLogLevel(-1);   // Vosk'un konsol çıktılarını kapat
                model = new Model(ModelPath);
            }

            recognizer = new VoskRecognizer(model, 16000.0f);

            microphone = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 1),   // 16 kHz, mono: Vosk'un beklediği format
                BufferMilliseconds = 100
            };
            microphone.DataAvailable += OnAudioReceived;
            microphone.RecordingStopped += OnRecordingStopped;
            microphone.StartRecording();   // mikrofon meşgulse hata fırlatır

            IsListening = true;
            ListeningChanged?.Invoke();
        }

        public void Stop()
        {
            microphone?.StopRecording();   // durunca OnRecordingStopped çalışır
        }

        // Mikrofondan her 100 ms'de bir ses parçası gelir
        private void OnAudioReceived(object? sender, WaveInEventArgs e)
        {
            // AcceptWaveform true dönerse bir cümle tamamlanmıştır
            if (recognizer != null && recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
                RaiseText(recognizer.Result());
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (recognizer != null)
            {
                RaiseText(recognizer.FinalResult());   // yarım kalan son cümleyi de al
                recognizer.Dispose();
                recognizer = null;
            }

            microphone?.Dispose();
            microphone = null;

            IsListening = false;
            ListeningChanged?.Invoke();

            if (e.Exception != null)
                ErrorOccurred?.Invoke(e.Exception.Message);
        }

        // Vosk sonucu JSON verir: {"text" : "merhaba nasılsın"}
        private void RaiseText(string json)
        {
            using var doc = JsonDocument.Parse(json);
            string text = doc.RootElement.GetProperty("text").GetString() ?? "";

            if (text != "")
                TextRecognized?.Invoke(text, DateTime.Now);
        }

        public void Dispose()
        {
            microphone?.Dispose();
            recognizer?.Dispose();
            model?.Dispose();
        }
    }
}