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
using Windows.Storage;
using Windows.Media.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SUSTalker1
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      myConfig = SpeechConfig.FromSubscription("<Key>", "<Loc>");  // your subscription key / location goes here
      mediaPlayer = new MediaPlayer(); // not yet used.
    }

    SpeechConfig myConfig;
    MediaPlayer mediaPlayer;

    async private void Button_Click(object sender, RoutedEventArgs e)
    {
      AudioConfig audioConfig = AudioConfig.FromDefaultSpeakerOutput();
      var devices = await DeviceInformation.FindAllAsync(DeviceClass.AudioRender);

      // optional part: scan devices for one with a name like "Lautsprecher"
      foreach (var device in devices)
      {
        if (device.Name.Contains("Lautsprecher"))
        {
          audioConfig = AudioConfig.FromSpeakerOutput(device.Id);
        }
      }

      var synthesizer = new SpeechSynthesizer(myConfig, audioConfig);
      using (var result = await synthesizer.SpeakTextAsync("Hello from cognitive services."))
      {
        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {

          using (var audioStream = AudioDataStream.FromResult(result))
          {
            var filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "outputaudio_for_playback.wav");
            await audioStream.SaveToWaveFileAsync(filePath);
            mediaPlayer.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(filePath));
            mediaPlayer.Play();
          }
        }
      }
    }
  }
}
