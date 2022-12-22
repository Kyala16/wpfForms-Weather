using System;
using lib;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Net.Http;
using System.Text.Json;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> countries = new List<string>(200);
        List<string[]> coordinates = new List<string[]>(200);
        string COUNTRY = "";

        public MainWindow()
        {
            InitializeComponent();
            Reader_From_File(countries, coordinates);
            foreach (var el in countries)
            {
                comboBox1.Items.Add(el);
            }
        }

        public async void Button_Click(object sender, RoutedEventArgs e)
        { 
            int index_country = 0;
            if (COUNTRY != "")
            {
                index_country = countries.FindIndex(delegate (string item) { return (item == COUNTRY); });
            }
            var task = await Task.Factory.StartNew<Weather>(() =>
            {
                string api = "c9ce2c4ad2541b09357faaa10ff1ed67";
                var client = new HttpClient();
                var lat = Convert.ToDouble(coordinates[index_country][0].Replace('.', ','));
                var lon = Convert.ToDouble(coordinates[index_country][1].Replace('.', ','));
                var content = client.GetStringAsync($"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={api}");
                //var content = "{\"coord\":{\"lon\":143.9794,\"lat\":63.1223},\"weather\":[{\"id\":801,\"main\":\"Clouds\",\"description\":\"few clouds\",\"icon\":\"02n\"}],\"base\":\"stations\",\"main\":{\"temp\":253.03,\"feels_like\":253.03,\"temp_min\":253.03,\"temp_max\":253.03,\"pressure\":1010,\"humidity\":88,\"sea_level\":1010,\"grnd_level\":911},\"visibility\":10000,\"wind\":{\"speed\":1.14,\"deg\":170,\"gust\":1.59},\"clouds\":{\"all\":16},\"dt\":1666260180,\"sys\":{\"country\":\"RU\",\"sunrise\":1666214629,\"sunset\":1666248828},\"timezone\":36000,\"id\":2122311,\"name\":\"Oymyakon\",\"cod\":200}";
                var jsonObject = JsonSerializer.Deserialize<JsonElement>(content.Result);
                Weather weather = new Weather(jsonObject.GetProperty("sys").GetProperty("country").GetString(),
                            jsonObject.GetProperty("name").GetString(),
                            jsonObject.GetProperty("main").GetProperty("temp").GetDouble(),
                            jsonObject.GetProperty("weather")[0].GetProperty("description").GetString());
                return weather;
            });
            Weather weather = task;
            MessageBox.Show(weather.ToString());
        }

        private void Reader_From_File(List<string> countries, List<string[]> coordinates)
        {
            string path = @"C:\Users\artem\source\repos\WpfApp1\city.txt";
            using (StreamReader reader = File.OpenText(path))
            {
                while (true)
                {
                    string countryWithCoor = reader.ReadLine();
                    if (countryWithCoor == null)
                    {
                        break;
                    }
                    string country = "";
                    int temp_index = 0;
                    for (int i=0;countryWithCoor[i] != '\t'; ++i, ++temp_index)
                    {
                        country += countryWithCoor[i];
                    }
                    ++temp_index;
                    countries.Add(country);
                    coordinates.Add(countryWithCoor.Substring(temp_index, (countryWithCoor.Length - temp_index)).Split(", "));
                }
            }
        }

        private void Country_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            var sel = comboBox.SelectedItem;
            COUNTRY = sel.ToString();
        }
    }
}
