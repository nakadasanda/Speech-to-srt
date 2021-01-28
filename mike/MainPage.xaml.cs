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
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using System.Text;
using Windows.Security.Cryptography;
// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace mike
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {        //連続音声認識のためのオブジェクト
        private SpeechRecognizer constSpeechRecognizer;
        private CoreDispatcher dispatcher;

        DateTime startdt = DateTime.Now; //開始
        DateTime newdt; //読み取り開始
        DateTime olddt; //読み取り終了
        TimeSpan local_starttime;
        TimeSpan local_endtime;
        int i;

        const int SIZE = 7;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            //バックグラウンドスレッドからUIスレッドを呼び出すためのDispatcher
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            //初期化
            constSpeechRecognizer = new SpeechRecognizer();

            /// time設定
            /// InitialSilenceTimeout: SpeechRecognizer が (認識結果が生成されるまでの) 無音を検出し、音声入力が続かないと見なす時間の長さ。
            /// BabbleTimeout: SpeechRecognizer が、認識できないサウンド(雑音) のリッスンを継続し、音声入力が終了したと見なし、認識処理を終了するまでの時間の長さ。
            /// EndSilenceTimeout: SpeechRecognizer が(認識結果が生成された後の) 無音を検出し、音声入力が終了したと見なす時間の長さ。

          /*constSpeechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(1.0);
            constSpeechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(60);
            constSpeechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(0.1);
          */
            await constSpeechRecognizer.CompileConstraintsAsync();

            //認識の処理定義
            constSpeechRecognizer.HypothesisGenerated += ContinuousRecognitionSession_HypothesisGenerated;
            constSpeechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            constSpeechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Conpleated;

            //処理開始
            await constSpeechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        /// <summary>
        /// 認識時間切れ
        /// </summary>


        private async void ContinuousRecognitionSession_Conpleated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                textBlock1.Text = "Timeout";
            });

            await constSpeechRecognizer.ContinuousRecognitionSession.StartAsync();

        }

        /// <summary>
        /// 認識中の文字
        /// </summary>

        private async void ContinuousRecognitionSession_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            if (olddt == DateTime.MinValue) 
            {
                olddt = DateTime.Now;
            }

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                textBlock1.Text = args.Hypothesis.Text;
            });

        }

        /// <summary>
        /// 認識完了後に画面に表示。
        /// </summary>
        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {

            if (newdt == DateTime.MinValue) {
                newdt = DateTime.Now;
            }

            string resultstr,starttimestr,endtimestr,str;
            local_starttime = (olddt - startdt) ;
            local_endtime = (newdt - startdt) ;
            resultstr = args.Result.Text;

            i++;
           /*foreach (string s in System.Text.RegularExpressions.Regex.Split(args.Result.Text, @"(?<=\G.{4})(?!$)"))
            {
               str += "-" + s + "\n";
            }*/
            starttimestr = local_starttime.ToString(@"hh\:mm\:ss\,fff");
            endtimestr = local_endtime.ToString(@"hh\:mm\:ss\,fff");
            
            string textIn;
            textIn = i + "\n" +
                   starttimestr + " --> " + endtimestr + "\n" +
                   resultstr + "\n\n";
         
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                output.Text += textIn ;
                textBlock1.Text = "Waiting...";

            });
            olddt = DateTime.MinValue;           
            newdt = DateTime.MinValue;
            
        }

        public MainPage()
        {
            this.InitializeComponent();
        }


    }
}
