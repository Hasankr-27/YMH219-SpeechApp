using System.Speech.Synthesis;

namespace YMH219_SpeechApp
{
    public partial class Form1 : Form
    {
        private TextToSpeechService? tts;
        private readonly SpeechToTextService stt = new SpeechToTextService();

        private readonly Label lblStatus;
        private readonly TabControl tabs;

        // TTS sekmesi
        private readonly RichTextBox rtbText;
        private readonly ComboBox cmbVoice;
        private readonly TrackBar trkRate;
        private readonly Label lblRateValue;
        private readonly Button btnSpeak;
        private readonly Button btnPause;
        private readonly Button btnStop;
        private readonly Button btnClear;

        // STT sekmesi
        private readonly Label lblLang;
        private readonly Button btnListen;
        private readonly Button btnStopListen;
        private readonly Button btnClearStt;
        private readonly RichTextBox rtbResult;
        private readonly ListBox lstHistory;

        public Form1()
        {
            InitializeComponent();

            Text = "Speech App - Ses ↔ Metin";
            ClientSize = new Size(620, 480);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10);

            lblStatus = new Label
            {
                AutoSize = false,
                Location = new Point(20, 10),
                Size = new Size(580, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White
            };

            tabs = new TabControl { Location = new Point(20, 55), Size = new Size(580, 410) };
            var tabTts = new TabPage("Metinden Sese (TTS)");
            var tabStt = new TabPage("Sesten Metne (STT)");
            tabs.TabPages.Add(tabTts);
            tabs.TabPages.Add(tabStt);

            // --- TTS sekmesi ---
            rtbText = new RichTextBox
            {
                Location = new Point(15, 15),
                Size = new Size(540, 220),
                Text = "Hello! This is a text to speech demo written in C#."
            };

            var lblVoice = new Label { Text = "Ses:", AutoSize = true, Location = new Point(15, 253) };
            cmbVoice = new ComboBox
            {
                Location = new Point(65, 250),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblRate = new Label { Text = "Hız:", AutoSize = true, Location = new Point(335, 253) };
            trkRate = new TrackBar
            {
                Location = new Point(375, 245),
                Width = 150,
                Minimum = -10,
                Maximum = 10,
                Value = 0,
                TickFrequency = 5
            };
            lblRateValue = new Label { Text = "0", AutoSize = true, Location = new Point(530, 253) };

            btnSpeak = CreateButton("Oku", 15, 315, 125);
            btnPause = CreateButton("Duraklat", 150, 315, 125);
            btnStop = CreateButton("Durdur", 285, 315, 125);
            btnClear = CreateButton("Temizle", 420, 315, 125);

            tabTts.Controls.AddRange(new Control[]
            {
                rtbText, lblVoice, cmbVoice, lblRate, trkRate, lblRateValue,
                btnSpeak, btnPause, btnStop, btnClear
            });

            // --- STT sekmesi ---
            lblLang = new Label
            {
                Text = "Tanıma dili: (dinlemeye başlayınca belirlenir)",
                AutoSize = true,
                Location = new Point(15, 12)
            };

            btnListen = CreateButton("🎤 Dinlemeyi Başlat", 15, 40, 175);
            btnStopListen = CreateButton("Dinlemeyi Durdur", 197, 40, 175);
            btnClearStt = CreateButton("Temizle", 380, 40, 175);

            var lblResult = new Label { Text = "Tanınan metin:", AutoSize = true, Location = new Point(15, 95) };
            rtbResult = new RichTextBox { Location = new Point(15, 118), Size = new Size(540, 110) };

            var lblHistory = new Label { Text = "Geçmiş (saatli):", AutoSize = true, Location = new Point(15, 238) };
            lstHistory = new ListBox { Location = new Point(15, 261), Size = new Size(540, 104) };

            tabStt.Controls.AddRange(new Control[]
            {
                lblLang, btnListen, btnStopListen, btnClearStt,
                lblResult, rtbResult, lblHistory, lstHistory
            });

            Controls.AddRange(new Control[] { lblStatus, tabs });

            // TTS olayları
            btnSpeak.Click += (s, e) => SpeakText();
            btnPause.Click += (s, e) => TogglePause();
            btnStop.Click += (s, e) => tts?.Stop();
            btnClear.Click += (s, e) => { tts?.Stop(); rtbText.Clear(); rtbText.Focus(); };
            cmbVoice.SelectedIndexChanged += (s, e) => ChangeVoice();
            trkRate.ValueChanged += (s, e) =>
            {
                lblRateValue.Text = trkRate.Value.ToString();
                if (tts != null) tts.Rate = trkRate.Value;
            };

            // STT olayları
            btnListen.Click += (s, e) => StartListening();
            btnStopListen.Click += (s, e) => stt.Stop();
            btnClearStt.Click += (s, e) => { rtbResult.Clear(); lstHistory.Items.Clear(); };

            stt.TextRecognized += (text, time) => RunOnUi(() =>
            {
                rtbResult.AppendText(text + " ");
                lstHistory.Items.Add($"[{time:HH:mm:ss}]  {text}");
                lstHistory.TopIndex = lstHistory.Items.Count - 1;
            });
            stt.ListeningChanged += () => RunOnUi(UpdateSttUi);
            stt.ErrorOccurred += message => RunOnUi(() =>
                MessageBox.Show("Dinleme sırasında hata oluştu:\n" + message, "Speech-to-Text Hatası",
                                MessageBoxButtons.OK, MessageBoxIcon.Error));

            FormClosed += (s, e) =>
            {
                tts?.Dispose();
                stt.Dispose();
            };

            InitTextToSpeech();
            UpdateTtsUi();
            UpdateSttUi();
        }

        private Button CreateButton(string text, int x, int y, int width)
        {
            return new Button { Text = text, Location = new Point(x, y), Size = new Size(width, 40) };
        }

        // ================= TTS =================

        private void InitTextToSpeech()
        {
            try
            {
                tts = new TextToSpeechService();
                tts.StateChanged += () => RunOnUi(UpdateTtsUi);

                foreach (var name in tts.GetVoiceNames())
                    cmbVoice.Items.Add(name);

                if (cmbVoice.Items.Count > 0)
                    cmbVoice.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                tts = null;
                MessageBox.Show("Ses motoru başlatılamadı. Hoparlör/ses aygıtı bağlı olmayabilir.\n\n" + ex.Message,
                                "Text-to-Speech Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SpeakText()
        {
            if (tts == null) return;

            try
            {
                tts.Speak(rtbText.Text);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TogglePause()
        {
            if (tts == null) return;

            if (tts.State == SynthesizerState.Speaking)
                tts.Pause();
            else if (tts.State == SynthesizerState.Paused)
                tts.Resume();
        }

        private void ChangeVoice()
        {
            if (tts == null || cmbVoice.SelectedItem == null) return;

            try
            {
                tts.SelectVoice(cmbVoice.SelectedItem.ToString()!);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ses seçilemedi:\n" + ex.Message, "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTtsUi()
        {
            if (tts == null)
            {
                btnSpeak.Enabled = btnPause.Enabled = btnStop.Enabled = false;
                UpdateStatus();
                return;
            }

            btnPause.Text = tts.State == SynthesizerState.Paused ? "Devam Et" : "Duraklat";

            bool busy = tts.State != SynthesizerState.Ready;
            btnSpeak.Enabled = !busy;
            btnPause.Enabled = busy;
            btnStop.Enabled = busy;
            cmbVoice.Enabled = !busy;

            UpdateStatus();
        }

        // ================= STT =================

        private void StartListening()
        {
            try
            {
                stt.Start();
                lblLang.Text = "Tanıma dili: " + stt.LanguageName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Konuşma tanıma başlatılamadı.\n\n" + ex.Message +
                    "\n\nMikrofonun bağlı olduğundan ve Windows Ayarlar > Gizlilik > Mikrofon " +
                    "bölümünde mikrofon izninin açık olduğundan emin olun.",
                    "Speech-to-Text Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSttUi()
        {
            btnListen.Enabled = !stt.IsListening;
            btnStopListen.Enabled = stt.IsListening;
            UpdateStatus();
        }

        // ================= Ortak gösterge =================

        // Hangi işlemin aktif olduğunu üstteki göstergede gösterir
        private void UpdateStatus()
        {
            if (stt.IsListening)
                SetStatus("🎤 DİNLENİYOR – konuşabilirsiniz", Color.RoyalBlue);
            else if (tts == null)
                SetStatus("■ SES MOTORU KULLANILAMIYOR", Color.Firebrick);
            else if (tts.State == SynthesizerState.Speaking)
                SetStatus("🔊 METİN OKUNUYOR", Color.SeaGreen);
            else if (tts.State == SynthesizerState.Paused)
                SetStatus("⏸ OKUMA DURAKLATILDI", Color.DarkOrange);
            else
                SetStatus("● HAZIR", Color.SlateGray);
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.BackColor = color;
        }

        private void RunOnUi(Action action)
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }
    }
}