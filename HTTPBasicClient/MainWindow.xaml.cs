using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HTTPBasicClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient _client = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ClickConsultButton(object sender, RoutedEventArgs e)
        {
            string url = urlTextBox.Text;

            if (string.IsNullOrEmpty(url))
                MessageBox.Show("Error", "Debes ingresar el URL a consultar");
            else
            {
                SendRequest(url);
            }
        }

        private void SendRequest(string url)
        {
            HttpResponseMessage response = new();

            try
            {
                switch (methodComboBox.Text)
                {
                    case "GET":
                        response = _client.GetAsync(url).Result;
                        break;
                    case "HEAD":
                        response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).Result;
                        break;
                    case "OPTIONS":
                        response = _client.SendAsync(new HttpRequestMessage(HttpMethod.Options, url)).Result;
                        break;
                }

                if (response != null)
                {
                    statusCodeLabel.Content = $"Respuesta HTTP: {(int)response.StatusCode} - {response.StatusCode}";
                    mimeTypeLabel.Content = response.Content.Headers.ContentType != null ? 
                        $"Tipo de contenido: {response.Content.Headers.ContentType.MediaType}" : "Tipo de contenido: Desconocido";

                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    string responseHeaders = response.Headers.ToString();

                    responseBodyTextBox.Text = responseBody;
                    responseHeadersTextBox.Text = responseHeaders;
                }
            }
            catch (Exception)
            {
            }
        }

        private void ClickSaveButton(object sender, RoutedEventArgs e)
        {

        }
    }
}