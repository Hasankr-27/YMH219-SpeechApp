\# Speech App – C# Windows Forms (Text-to-Speech \& Speech-to-Text)



YMH 219 Nesne Tabanlı Programlama dersi ek uygulama ödevi kapsamında geliştirilmiş, ses ve metin arasında çift yönlü dönüşüm yapan bir Windows Forms uygulamasıdır. Kullanıcı yazdığı metni sesli okutabilir, mikrofondan konuştuğu Türkçe cümleleri metne dönüştürebilir.



\*\*Öğrenci:\*\* Hasan Kara – 250541015



\## Ekran Görüntüleri



| Metinden Sese (TTS) | Sesten Metne (STT) |

|---|---|

| !\[Metinden sese](screenshots/tts.png) | !\[Sesten metne](screenshots/stt.png) |



\## Kullanılan Teknolojiler



\- C#

\- .NET 8

\- Windows Forms

\- \[System.Speech](https://www.nuget.org/packages/System.Speech) (8.0.0) – `SpeechSynthesizer` ile metin okuma

\- \[Vosk](https://alphacephei.com/vosk/) – internetsiz Türkçe konuşma tanıma (`vosk-model-small-tr-0.3`)

\- \[NAudio](https://github.com/naudio/NAudio) – mikrofondan ses alma

\- `System.Text.Json` – Vosk sonuçlarının okunması



\## Kurulum ve Çalıştırma



1\. Depoyu klonlayın:

&#x20;  ```

&#x20;  git clone https://github.com/Hasankr-27/YMH219-SpeechApp.git

&#x20;  ```

2\. \*\*Türkçe konuşma modelini indirin\*\* (yaklaşık 35 MB, boyutu nedeniyle depoya eklenmemiştir):

&#x20;  - İndirme: https://alphacephei.com/vosk/models/vosk-model-small-tr-0.3.zip

&#x20;  - Zip dosyasını `.csproj` dosyasının bulunduğu klasörde `Models` adlı bir klasörün içine çıkartın. Klasör yapısı şöyle olmalıdır:

&#x20;    ```

&#x20;    YMH219-SpeechApp/

&#x20;      YMH219-SpeechApp.csproj

&#x20;      Models/

&#x20;        vosk-model-small-tr-0.3/

&#x20;          am/

&#x20;          conf/

&#x20;          graph/

&#x20;          ...

&#x20;    ```

3\. `YMH219-SpeechApp.slnx` dosyasını Visual Studio 2022 veya üzeriyle açın. NuGet paketleri otomatik olarak indirilir.

4\. `F5` ile çalıştırın.



\## Kullanım



\*\*Metinden Sese (TTS) sekmesi\*\*

1\. Metin kutusuna okunacak metni yazın.

2\. İsteğe bağlı olarak ses ve okuma hızını seçin.

3\. \*\*Oku\*\* ile okutun; \*\*Duraklat / Devam Et\*\*, \*\*Durdur\*\* ve \*\*Temizle\*\* butonlarıyla kontrol edin.



\*\*Sesten Metne (STT) sekmesi\*\*

1\. \*\*🎤 Dinlemeyi Başlat\*\* butonuna basın.

2\. Mikrofona Türkçe konuşun; her cümleden sonra kısa bir duraklama yapın.

3\. Tanınan metin kutuya eklenir ve saat bilgisiyle geçmiş listesine yazılır.

4\. \*\*Dinlemeyi Durdur\*\* ile dinlemeyi bitirin, \*\*Temizle\*\* ile sonuçları silin.



\## Özellikler



\- Metni sesli okuma (`SpeakAsync` ile arka planda, arayüz donmadan)

\- Oku / Duraklat / Devam Et / Durdur / Temizle işlevleri

\- Yüklü sesler arasından ses seçimi ve okuma hızı ayarı (-10 ile 10 arası)

\- İnternet bağlantısı gerektirmeyen Türkçe konuşma tanıma

\- Tanınan cümlelerin saat bilgisiyle geçmiş listesinde gösterilmesi

\- Aktif işlemi gösteren renkli durum göstergesi: \*\*HAZIR\*\*, \*\*METİN OKUNUYOR\*\*, \*\*OKUMA DURAKLATILDI\*\*, \*\*DİNLENİYOR\*\*

\- Hata yönetimi: ses aygıtı, mikrofon veya konuşma modeli bulunamadığında uygulama kapanmadan anlaşılır hata mesajı gösterilir; boş metin okutulmaya çalışıldığında uyarı verilir



\## Proje Yapısı



| Sınıf | Görevi |

|---|---|

| `TextToSpeechService` | `SpeechSynthesizer`'ı sarar: okuma, duraklatma, durdurma, ses ve hız seçimi, durum bildirimi |

| `SpeechToTextService` | Mikrofondan ses alır (NAudio), Vosk ile metne çevirir, sonuçları olay (event) ile bildirir |

| `Form1` | Arayüz: iki sekme, butonlar, durum göstergesi ve servislerin olaylarına göre ekranın güncellenmesi |



Ses işlemleri arayüzden ayrı servis sınıflarında tutulmuştur. Konuşma tanıma altyapısı geliştirme sırasında `System.Speech`'ten Vosk'a değiştirilmiş, servis sınıfının dışa açık yapısı aynı kaldığı için `Form1` üzerinde değişiklik gerekmemiştir. Servisler `IDisposable` arayüzünü uygular ve form kapanırken kaynaklar serbest bırakılır.



\## Bilinen Sınırlamalar



\- Okuma için kullanılan sesler Windows'ta yüklü seslere bağlıdır; Türkçe Windows'ta `System.Speech` genellikle yalnızca İngilizce sesleri (ör. Microsoft David, Zira) görür, bu nedenle Türkçe metinler İngilizce aksanla okunabilir.

\- Küçük Türkçe Vosk modeli kullanıldığı için tanınan metin küçük harflidir, noktalama içermez ve bazı kelimeler yanlış tanınabilir.

\- Konuşma modeli depoda bulunmaz, kurulum adımlarında belirtildiği şekilde ayrıca indirilmelidir.

\- Uygulama Windows'a özeldir (x64).

