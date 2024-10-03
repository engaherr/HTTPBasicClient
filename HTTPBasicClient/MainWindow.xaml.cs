using System.IO;
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
using HeyRed.Mime;
using Microsoft.Win32;

namespace HTTPBasicClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HttpClient _client = new();
        private HttpResponseMessage _response = new();
        string _responseBody = string.Empty;

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
            switch (methodComboBox.Text)
            {
                case "GET":
                    _response = await _client.GetAsync(url);
                    break;
                case "HEAD":
                    _response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                    break;
                case "OPTIONS":
                    _response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Options, url));
                    break;
            }

            if (_response != null)
            {
                statusCodeLabel.Content = $"Respuesta HTTP: {(int)_response.StatusCode} - {_response.StatusCode}";

                string mimeType = _response.Content.Headers.ContentType != null
                    ? _response.Content.Headers.ContentType.MediaType
                    : "Desconocido";

                mimeTypeLabel.Content = _response.Content.Headers.ContentType != null
                    ? $"Tipo de contenido: {mimeType} ({MimeTypesMap.GetExtension(mimeType)})"
                    : "Tipo de contenido: Desconocido";

                _responseBody = await _response.Content.ReadAsStringAsync();
                string responseHeaders = _response.Headers.ToString();
                string responseContentHeaders = _response.Content.Headers.ToString();

                if (mimeType.StartsWith("image"))
                {
                    responseWebBrowser.Navigate(url); 
                    responseWebBrowser.Visibility = Visibility.Visible;
                    responseBodyTextBox.Visibility = Visibility.Collapsed;
                }
                else if (mimeType == "text/html")
                {
                    if (htmlRadioButton.IsChecked == true)
                    {
                        responseWebBrowser.Navigate(url);
                        responseWebBrowser.Visibility = Visibility.Visible;
                        responseBodyTextBox.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        responseBodyTextBox.Text = _responseBody;
                        responseWebBrowser.Visibility = Visibility.Collapsed;
                        responseBodyTextBox.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    responseBodyTextBox.Text = _responseBody;
                    responseWebBrowser.Visibility = Visibility.Collapsed;
                    responseBodyTextBox.Visibility = Visibility.Visible;
                }

                responseHeadersTextBox.Text = "***** Generales y de Respuesta *****\n\n" + responseHeaders
                    + "\n***** De entidad: *****\n\n" + responseContentHeaders;

                if (_response.IsSuccessStatusCode)
                {
                    statusCodeLabel.BorderBrush = Brushes.LightGreen;
                    saveButton.IsEnabled = true;
                }
                else if ((int)_response.StatusCode >= 400 && (int)_response.StatusCode < 600)
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


        private void ClickSaveButton(object sender, RoutedEventArgs e)
        {
            string mimeType = _response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            string extension = MimeTypesMap.GetExtension(mimeType);

            SaveFileDialog saveFileDialog = new()
            {
                FileName = $"respuesta.{extension}",
                Filter = $"{mimeType} Files|*.{extension}|All Files|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                if (mimeType.StartsWith("text"))
                {
                    File.WriteAllText(saveFileDialog.FileName, _responseBody);
                }
                else
                {
                    byte[] responseBytes = _response.Content.ReadAsByteArrayAsync().Result;
                    File.WriteAllBytes(saveFileDialog.FileName, responseBytes);
                }

                MessageBox.Show("Respuesta guardada exitosamente.", "Guardar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}