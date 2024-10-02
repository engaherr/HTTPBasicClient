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

        private async void ClickConsultButton(object sender, RoutedEventArgs e)
        {
            string url = urlTextBox.Text;

            if (string.IsNullOrEmpty(url))
                MessageBox.Show("Por favor, ingrese una URL", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    url = "https://" + url;

                urlTextBox.Text = url;
                statusCodeLabel.Content = "Procesando petición...";
                statusCodeLabel.BorderBrush = Brushes.Blue;
                await SendRequestAsync(url);
            }
        }

        private async Task SendRequestAsync(string url)
        {
            HttpResponseMessage response = new();

            try
            {
                switch (methodComboBox.Text)
                {
                    case "GET":
                        response = await _client.GetAsync(url);
                        break;
                    case "HEAD":
                        response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                        break;
                    case "OPTIONS":
                        response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Options, url));
                        break;
                }

                if (response != null)
                {
                    statusCodeLabel.Content = $"Respuesta HTTP: {(int)response.StatusCode} - {response.StatusCode}";
                    mimeTypeLabel.Content = response.Content.Headers.ContentType != null
                        ? $"Tipo de contenido: {response.Content.Headers.ContentType.MediaType}"
                        : "Tipo de contenido: Desconocido";

                    string responseBody = await response.Content.ReadAsStringAsync();
                    string responseHeaders = response.Headers.ToString();
                    string responseContentHeaders = response.Content.Headers.ToString();

                    responseBodyTextBox.Text = responseBody;
                    responseHeadersTextBox.Text = "***** Generales y de Respuesta *****\n\n" + responseHeaders 
                        + "\n***** De entidad: *****\n\n" + responseContentHeaders;

                    if (response.IsSuccessStatusCode)
                    {
                        statusCodeLabel.BorderBrush = Brushes.LightGreen;
                        saveButton.IsEnabled = true;
                    }
                    else if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 600)
                    {
                        statusCodeLabel.BorderBrush = Brushes.Salmon;
                        saveButton.IsEnabled = false;
                    }
                    else
                    {
                        statusCodeLabel.BorderBrush = Brushes.LightYellow;
                        saveButton.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                statusCodeLabel.Content = "Error en la petición";
                statusCodeLabel.BorderBrush = Brushes.Red;
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClickSaveButton(object sender, RoutedEventArgs e)
        {

        }
    }
}