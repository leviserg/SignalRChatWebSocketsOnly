using Common.Models;
using Common.Services;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace WpfChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string userName = String.Empty;

        private readonly CustomWebSocketClient webSocketClient;

        public MainWindow()
        {
            InitializeComponent();
            string input = Microsoft.VisualBasic.Interaction.InputBox("WpfChatClient","Enter your name", "Jack Sparrow", -1, -1);
            userNameTxtBox.Text = input;
            userName = input;

            webSocketClient = new CustomWebSocketClient();

            webSocketClient.On<ChatMessage>("Send", message =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    AppendTextToTextBox(message.CreatedAt, message.Sender, message.Body, Brushes.Black);
                }));
            });
        }

        private async void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (webSocketClient.State == WebSocketState.None)
            {
                try
                {
                    await webSocketClient.ConnectAsync("ws://localhost:5065/messages");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                if (webSocketClient.State == WebSocketState.Open)
                {
                    connectionStatus.Content = "Connected";
                    connectionStatus.Foreground = Brushes.Green;
                }
            }
            else if (webSocketClient.State == WebSocketState.Open)
            {
                await webSocketClient.StopAsync();
                connectionStatus.Content = "Disconnected";
                connectionStatus.Foreground = Brushes.Red;
            }
        }

        private async void sendMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (webSocketClient.State == WebSocketState.Open)
            {

                var message = new ChatMessage
                {
                    Body = messageTxtBox.Text,
                    Sender = userNameTxtBox.Text
                };

                try
                {
                    await webSocketClient.SendAsync("SendToOthers", message);
                    AppendTextToTextBox(DateTime.Now.ToString("HH:mm:ss"),"Me", message.Body, Brushes.Green);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    messageTxtBox.Clear();
                }
            }

        }

        public void AppendTextToTextBox(string messageDate, string sender, string text, Brush brush)
        {
            TextRange tr = new TextRange(chatTextBox.Document.ContentEnd, chatTextBox.Document.ContentEnd);
            tr.Text = string.Format("{0} : {1} : {2}{3}", messageDate, sender, text, Environment.NewLine);
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            }
            catch (FormatException) { }
            finally
            {
                chatTextBox.ScrollToEnd();
            }
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex?.Message ?? "Error");
        }

        private async void Shutdown(object sender, EventArgs e)
        {
            await DisposeAsync(webSocketClient);
        }
        async ValueTask DisposeAsync(CustomWebSocketClient webSocketClient)
        {
            if (this.webSocketClient != null)
            {
                await this.webSocketClient.DisposeAsync();
            }
        }

    }


}
