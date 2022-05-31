using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StreamUI
{
    public delegate void Del(string message);
    public delegate void ExitDel();

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> audioDevices;
        List<string> AudioDevices
        {
            get => audioDevices;
            set
            {
                audioDevices = value;
            }
        }

        List<string> videoDevices;
        List<string> VideoDevices
        {
            get => videoDevices;
            set
            {
                videoDevices = value;
            }
        }

        private string toDisplay;
        public string ToDisplay
        {
            get => toDisplay;
            set
            {
                Display(value);
                toDisplay = value;
            }
        }


        public MainWindow()
        {
            videoDevices = new List<string>();
            audioDevices = new List<string>();

            InitializeComponent();

            comboBoxVideo.ItemsSource = VideoDevices;
            comboBoxAudio.ItemsSource = AudioDevices;

            //FetchDevices();


            Console.WriteLine(CommandFFMPEG("-version"));

        }


        #region UICommands

        private void ComboBoxVideo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBoxAudio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            FetchDevices();
        }

        #endregion


        public void FetchDevices()
        {
            VideoDevices.Clear();
            AudioDevices.Clear();

            Process proc = CommandFFMPEG("-list_devices true -f dshow -i dummy", errorSubscribers: new Del[] { ParseDevices });
        }

        public void ParseDevices(string message)
        {
            string s = message;
            if (s == null || s[0] != '[') return;
            //s.Remove(s.IndexOf('['));
            //string[] strings = s.Split('\n', '\r');

            Regex deviceRegex = new Regex("\\[dshow @ [0-9,a-f]*\\].*\"(.*)\" \\((.*)\\)");
            Match match = deviceRegex.Match(s);
            if (match.Success)
            {
                //Console.WriteLine(match.Groups[1].Value);
                //Console.WriteLine(match.Groups[2].Value);
                if (match.Groups[2].Value == "video")
                {
                    videoDevices.Add(match.Groups[1].Value);
                }
                if (match.Groups[2].Value == "audio")
                {
                    audioDevices.Add(match.Groups[1].Value);
                }
            }
        }

        public Process CommandFFMPEG(string arguments, Del[] outputSubscribers = null, Del[] errorSubscribers = null, ExitDel[] exitSubscribers = null)
        {
            ProcessStartInfo pstart = new ProcessStartInfo("ffmpeg", string.Format(arguments));
            pstart.CreateNoWindow = true;
            pstart.ErrorDialog = false;
            pstart.RedirectStandardOutput = true;
            pstart.RedirectStandardError = true;
            pstart.UseShellExecute = false;

            Process proc = new Process();
            proc.StartInfo = pstart;
            //StreamReader stdout = proc.StandardOutput;
            //StreamReader stderr = proc.StandardError;

            proc.OutputDataReceived += (sender, args) => AddDisplay(args.Data);
            proc.ErrorDataReceived += (sender, args) => AddDisplay(args.Data);

            if (outputSubscribers != null)
            {
                foreach (Del method in outputSubscribers)
                {
                    proc.OutputDataReceived += (sender, args) => method(args.Data);
                }
            }

            if (errorSubscribers != null)
            {
                foreach (Del method in errorSubscribers)
                {
                    proc.ErrorDataReceived += (sender, args) => method(args.Data);
                }
            }

            if (exitSubscribers != null)
            {
                foreach (ExitDel method in exitSubscribers)
                {
                    proc.Exited += (sender, args) => method();
                }
            }

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            this.textBox.AppendText("ffmpeg " + arguments + "\n");
            //string output = proc.StandardOutput.ReadToEnd();
            //this.textBox.AppendText(output);
            //output = proc.StandardError.ReadToEnd();
            //this.textBox.AppendText(output);
            //this.textBox.ScrollToEnd();

            return proc;
        }

        public void AddDisplay(string message)
        {
            Dispatcher.Invoke(() => Display(message));
        }

        private void Display(string message)
        {
            this.textBox.AppendText("\n"+message);
            this.textBox.ScrollToEnd();
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBox.ScrollToEnd();
        }

        private void StreamButton_Click(object sender, RoutedEventArgs e)
        {
            CommandFFMPEG("-y -loglevel warning -f dshow -i video=\"" + comboBoxVideo.SelectedValue + "\" -r 30 -threads 2 -vcodec libx264 -f flv rtmp://stream.intranet.noxiris.net/live/cy_camera_cam_test");
        }
    }
}
