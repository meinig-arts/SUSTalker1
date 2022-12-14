using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Windows.Devices.Enumeration;
using Windows.Media.Playback;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SUSTalker1
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    SpeechConfig myConfig;
    MediaPlayer mediaPlayer;
    SpeechRecognizer speechRecognizer = null;

    public MainPage()
    {
      this.InitializeComponent();
      myConfig = SpeechConfig.FromSubscription("<Key>", "<Loc>");  // your subscription key / location goes here
      mediaPlayer = new MediaPlayer();
    }

    async private void ButtonEnglish_Click(object sender, RoutedEventArgs e)
    {
      AudioConfig audioConfig = AudioConfig.FromDefaultSpeakerOutput();
      var devices = await DeviceInformation.FindAllAsync(DeviceClass.AudioRender);

      // optional part: scan devices for one with a name like "Lautsprecher"
      foreach (var device in devices) {
        if (device.Name.Contains("Lautsprecher")) {
          audioConfig = AudioConfig.FromSpeakerOutput(device.Id);
        }
      }

      myConfig.SpeechSynthesisVoiceName = "";
      var synthesizer = new SpeechSynthesizer(myConfig, audioConfig);
      await synthesizer.SpeakTextAsync("SUS sends its best regards");
    }

    async private void ButtonGerman_Click(object sender, RoutedEventArgs e)
    {
      AudioConfig audioConfig = AudioConfig.FromDefaultSpeakerOutput();
      var devices = await DeviceInformation.FindAllAsync(DeviceClass.AudioRender);

      // optional part: scan devices for one with a name like "Lautsprecher"
      foreach (var device in devices)
      {
        if (device.Name.Contains("Lautsprecher")) {
          audioConfig = AudioConfig.FromSpeakerOutput(device.Id);
        }
      }

      myConfig.SpeechSynthesisVoiceName = "de-CH-LeniNeural";
      var synthesizer = new SpeechSynthesizer(myConfig, audioConfig);
      await synthesizer.SpeakTextAsync("Das ist ein etwas längerer Text, der in Schweitzerdeutsch gesprochen wird.");
    }

    async private void ButtonMic_Click(object sender, RoutedEventArgs e)
    {
      bool isMicAvailable = true;
      try
      {
        var mediaCapture = new Windows.Media.Capture.MediaCapture();
        var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
        settings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio;
        await mediaCapture.InitializeAsync(settings);
      }
      catch (Exception)
      {
        isMicAvailable = false;
      }

      if (!isMicAvailable)
      {
        await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-microphone"));
      }
      else
      {
        ButtonStart.IsEnabled = true;
      }
    }

    async private void ButtonStart_Click(object sender, RoutedEventArgs e)
    {
      AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
      var devices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);
      foreach (var device in devices)
      {
        if (device.Name.Contains("Mikrofon"))
        {
          audioConfig = AudioConfig.FromMicrophoneInput(device.Id);
        }
      }

      myConfig.SpeechRecognitionLanguage = "de-DE";

      if (speechRecognizer == null)
      {
        speechRecognizer = new SpeechRecognizer(myConfig, audioConfig);

        var phraseList = PhraseListGrammar.FromRecognizer(speechRecognizer);
        phraseList.AddPhrase("Start");
        phraseList.AddPhrase("Stopp");
      }

      ButtonStart.IsEnabled = false; ButtonStop.IsEnabled = false;
      var result = await speechRecognizer.RecognizeOnceAsync().ConfigureAwait(false);
      Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
        ButtonStart.IsEnabled = true;
      });
      string str;
      if (result.Reason != ResultReason.RecognizedSpeech)
      {
        str = $"Speech Recognition failed: '{result.Reason.ToString()}'";
      }
      else
      {
        str = result.Text;
      }
      AppendToLog(str);
    }

    async private void ButtonStop_Click(object sender, RoutedEventArgs e)
    {
    }

    void AppendToLog(String _what)
    {
      Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        TxtRaus.Text = TxtRaus.Text + "/ /" + _what);
    }
  }
}
