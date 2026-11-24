using System.Speech.Synthesis;

namespace YMH219_SpeechApp
{
    // Metni sesli okuyan servis (System.Speech kütüphanesini sarar)
    public class TextToSpeechService : IDisposable
    {
        private readonly SpeechSynthesizer synth = new SpeechSynthesizer();

        // Okuma durumu değiştiğinde (başladı, duraklatıldı, bitti) haber verir
        public event Action? StateChanged;

        public TextToSpeechService()
        {
            synth.SetOutputToDefaultAudioDevice();   // hoparlör yoksa hata fırlatabilir
            synth.StateChanged += (s, e) => StateChanged?.Invoke();
        }

        // Hazır / Konuşuyor / Duraklatıldı
        public SynthesizerState State => synth.State;

        // Hız: -10 (çok yavaş) ile 10 (çok hızlı) arası
        public int Rate
        {
            get => synth.Rate;
            set => synth.Rate = Math.Clamp(value, -10, 10);
        }

        // Bilgisayarda yüklü seslerin isimleri
        public List<string> GetVoiceNames()
        {
            return synth.GetInstalledVoices()
                        .Where(v => v.Enabled)
                        .Select(v => v.VoiceInfo.Name)
                        .ToList();
        }

        public void SelectVoice(string name)
        {
            synth.SelectVoice(name);
        }

        public void Speak(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Okunacak metin boş.");

            synth.SpeakAsyncCancelAll();   // önceki okuma varsa iptal et
            synth.SpeakAsync(text);        // arka planda oku, pencere donmasın
        }

        public void Pause()
        {
            if (synth.State == SynthesizerState.Speaking)
                synth.Pause();
        }

        public void Resume()
        {
            if (synth.State == SynthesizerState.Paused)
                synth.Resume();
        }

        public void Stop()
        {
            // Duraklatılmışken iptal edilirse motor takılabiliyor, önce devam ettir
            if (synth.State == SynthesizerState.Paused)
                synth.Resume();

            synth.SpeakAsyncCancelAll();
        }

        public void Dispose()
        {
            synth.Dispose();   // ses motorunun kaynaklarını serbest bırak
        }
    }
}