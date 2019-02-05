using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Epicture
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int state = 0;
        public static Token token = new Token();
        private string clientId = "7d0143a9a4b64fb";
        private string clientSecret = "148c15c487f17b4d0aa2458244acf40ee1fcab91";
        private string box = "viral";
        ImgurImages images;
        Binding myBinding = new Binding();

        private async void DeSerialize()
        {
            try
            {
                var folder = ApplicationData.Current.RoamingFolder;
                var file = await folder.GetFileAsync("collection.json");
                using (var stream = await file.OpenStreamForReadAsync())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string json = await reader.ReadToEndAsync();
                    token = JsonConvert.DeserializeObject<Token>(json);
                }
            }
            catch (Exception e)
            {
            }
        }

        private async void Serialize()
        {
            var folder = ApplicationData.Current.RoamingFolder;
            var file = await folder.CreateFileAsync("collection.json", CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                string json = JsonConvert.SerializeObject(token);
                await writer.WriteAsync(json);
            }
        }

        public MainPage()
        {
            DeSerialize();
            this.InitializeComponent();
            getImgurGallery();
        }

        private void uploadClick(object sender, RoutedEventArgs e)
        {
            picker();
        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            connect();
        }

        private void connect()
        {
            var authorizationUrl = "https://api.imgur.com/oauth2/authorize/?client_id=" + clientId + "&response_type=token";
            myWeb.LoadCompleted += webView_LoadCompleted;
            myWeb.Navigate(new Uri(authorizationUrl));
        }

        private void webView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            WebView tmp = sender as WebView;

            string myUrl = tmp.Source.ToString();
            if (myUrl.IndexOf("https://api.imgur.com/oauth2/authorize/") == -1)
                myWeb.Visibility = Visibility.Collapsed;
            else
                myWeb.Visibility = Visibility.Visible;
            try
            {
                if (myUrl.IndexOf("access_token=") == -1)
                    throw new Exception("no access token");
                myUrl = myUrl.Substring(myUrl.IndexOf("access_token=") + 13);
                token.accessToken = myUrl.Substring(0, myUrl.IndexOf("&"));
                if (myUrl.IndexOf("refresh_token=") == -1)
                    throw new Exception("no access token");
                myUrl = myUrl.Substring(myUrl.IndexOf("refresh_token=") + 14);
                token.refreshToken = myUrl.Substring(0, myUrl.IndexOf("&"));
                myWeb.Visibility = Visibility.Collapsed;
                Serialize();
                GridView1.Items.Clear();
                images = null;
                getImgurGallery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Couldn't set tokens");
            }
        }

        private async void picker()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode =PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                uploadImage(file);
            }
        }
        private async void uploadImage(StorageFile file)
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                byte[] fileBytes = null;
                using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
                {
                    fileBytes = new byte[stream.Size];
                    using (DataReader reader = new DataReader(stream))
                    {
                        await reader.LoadAsync((uint)stream.Size);
                        reader.ReadBytes(fileBytes);
                    }
                }


                String s = Convert.ToBase64String(fileBytes);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.accessToken);

                Uri requestUri = new Uri("https://api.imgur.com/3/image");
                Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();

                HttpContent content = new StringContent(s);
                HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);
                Debug.WriteLine(response);
                string jsonResponse = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }

        private void home_Click(object sender, RoutedEventArgs e)
        {
            if (state == 0)
                return;
            ComboBox1.Visibility = Visibility.Visible;
            GridView1.Items.Clear();
            images = null;
            state = 0;
            getImgurGallery();
        }
        private void favoriteGallery_Click(object sender, RoutedEventArgs e)
        {
            if (state == 1)
                return;
            ComboBox1.Visibility = Visibility.Collapsed;
            GridView1.Items.Clear();
            images = null;
            state = 1;
            getImgurGallery();
        }

        private void myGallery_Click(object sender, RoutedEventArgs e)
        {
            if (state == 2)
                return;
            ComboBox1.Visibility = Visibility.Collapsed;
            GridView1.Items.Clear();
            images = null;
            state = 2;
            getImgurGallery();
        }

        private async void getImgurGallery()
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                if (token.accessToken == null)
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", clientId);
                else
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.accessToken);
                Uri requestUri;
                if (state == 0)
                    requestUri = new Uri("https://api.imgur.com/3/gallery/hot/" + box + "/month/0");
                else if (state == 1)
                    requestUri = new Uri("https://api.imgur.com/3/account/me/favorites/0/newest");
                else
                    requestUri = new Uri("https://api.imgur.com/3/account/me/images");
                Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();

                HttpResponseMessage response = await httpClient.GetAsync(requestUri);
                Debug.WriteLine(response);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                images = JsonConvert.DeserializeObject<ImgurImages>(jsonResponse);
                foreach (var tmp in images.images)
                {
                    GridView1.Items.Add(tmp);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }

        private async void addFavorite(string id)
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", MainPage.token.accessToken);

                Uri requestUri = new Uri("https://api.imgur.com/3/image/" + id + "/favorite");
                Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();

                HttpContent content = new StringContent("");
                HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);
                Debug.WriteLine(response);
                string jsonResponse = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void favorite_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.Equals("\uEB51") == true || button.Content.Equals("\uEA92") == true)
                button.Content = "\uEB52";
            else
                button.Content = "\uEA92";

            addFavorite((string)button.Tag);
        }

        private void favorite_Loaded(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            button.Content = "\uEB51";
            foreach (var tmp in images.images)
            {
                if (tmp.Id.Equals(button.Tag) == true)
                {
                    if (tmp.Favorite == true)
                        button.Content = "\uEB52";
                }
            }
        }

        private void changedBox_Click(object sender, object e)
        {
            var comboBoxItem = ComboBox1.SelectedItem as ComboBoxItem;
            if (comboBoxItem == null) return;
            string content = comboBoxItem.Content as string;
            if (content.Length > 0 && content.Equals(box) == false)
            {
                box = content;
                GridView1.Items.Clear();
                images = null;
                getImgurGallery();
            }
        }

        private int CalcGridLength()
        {
            double width = ((Frame)Window.Current.Content).ActualWidth;
            for (int i = 1; width > 400; i++)
            {
                width = ((Frame)Window.Current.Content).ActualWidth / i;
            }
            return ((int)width - 5);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            int width = CalcGridLength();
            grid.Width = width;
        }
    }
}
