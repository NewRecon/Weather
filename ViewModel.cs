using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MVVM
{
    public class ViewModel
    {
        CurrentCity parseCity;
        Root parsedJsonWeather;

        public ObservableCollection<CityTimeInfoModel> cityTimeInfoModels { get; set; }
        public ObservableCollection<TodayWeatherModel> todayWeatherModels { get; set; }
        public ObservableCollection<HourlyWeatherModel> hourlyWeatherModels { get; set; }
        public ObservableCollection<WeaklyWeatherModel> weaklyWeatherModels { get; set; }

        RelayCommand updateCommand;

        //обновить данные при нажатии на кнопку
        public RelayCommand UpdateCommand
        {
            get
            {
                return updateCommand ?? (updateCommand = new RelayCommand(obj =>
                {
                    RequestAndParseData();
                    UpdateData();
                }));
            }
        }

        public ViewModel()
        {
            RequestAndParseData();
            FillData();
        }

        void RequestAndParseData()
        {
            using (var webClient = new WebClient())
            {
                //получаем ip
                var ip = webClient.DownloadString(@"https://ipv4-internet.yandex.net/api/v0/ip");

                //получаем координаты
                var data = webClient.DownloadString(@"http://ip-api.com/json/" + ip.Trim('"'));

                //парсим координаты
                var parsedJson = JsonSerializer.Deserialize<DetectRegionFromAPI>(data);

                //парсим город
                data = webClient.DownloadString($@"https://api.openweathermap.org/data/2.5/weather?lat={parsedJson.lat}&lon={parsedJson.lon}&appid=b46216ac85bfd86207ceabc0e5de7260");
                parseCity = JsonSerializer.Deserialize<CurrentCity>(data);

                //парсим погоду
                data = webClient.DownloadString($@"https://api.openweathermap.org/data/2.5/onecall?lat={parsedJson.lat}&lon={parsedJson.lon}&units=metric&exclude=minutely,alerts&appid=b46216ac85bfd86207ceabc0e5de7260");
                parsedJsonWeather = JsonSerializer.Deserialize<Root>(data);
            }
        }

        void FillData()
        {
            //заполняем город и время
            var currentTime = DateTimeOffset.FromUnixTimeSeconds(parsedJsonWeather.current.dt + parsedJsonWeather.timezone_offset);
            cityTimeInfoModels = new ObservableCollection<CityTimeInfoModel>
            {
                new CityTimeInfoModel
                {
                    City = parseCity.name,
                    Time = currentTime.TimeOfDay.ToString().Substring(0, currentTime.TimeOfDay.ToString().LastIndexOf(':'))
                }
            };

            //заполняем пагоду на сегодня
            todayWeatherModels = new ObservableCollection<TodayWeatherModel>
            {
                new TodayWeatherModel
                {
                    Temp = Convert.ToInt32(parsedJsonWeather.current.temp).ToString() + "C°",
                    WeatherIcon = new BitmapImage(new Uri("https://openweathermap.org/img/wn/" + parsedJsonWeather.current.weather[0].icon.ToString() + ".png")),
                    WeatherInfo = parsedJsonWeather.current.weather[0].main,
                    FeelsLike = "Feels like "+ Convert.ToInt32(parsedJsonWeather.current.feels_like).ToString() + "C°"
                }
            };

            //заполняем почасовую погоду
            DateTimeOffset hourlyTime;
            HourlyWeatherModel hourlyWeather;
            hourlyWeatherModels = new ObservableCollection<HourlyWeatherModel>();
            for (int i = 0; i < parsedJsonWeather.hourly.Count; i++)
            {
                hourlyTime = DateTimeOffset.FromUnixTimeSeconds(parsedJsonWeather.hourly[i].dt + parsedJsonWeather.timezone_offset);
                hourlyWeather = new HourlyWeatherModel();
                hourlyWeather.Temp = Convert.ToInt32(parsedJsonWeather.hourly[i].temp).ToString() + "C°";
                hourlyWeather.WeatherIcon = new BitmapImage(new Uri("https://openweathermap.org/img/wn/" + parsedJsonWeather.hourly[i].weather[0].icon.ToString() + ".png"));
                hourlyWeather.Time = hourlyTime.TimeOfDay.ToString().Substring(0, hourlyTime.TimeOfDay.ToString().LastIndexOf(':'));
                hourlyWeatherModels.Add(hourlyWeather);
            }

            //заполняем дневную погоду на неделю
            WeaklyWeatherModel weaklyWeather;
            weaklyWeatherModels = new ObservableCollection<WeaklyWeatherModel>();
            for (int i = 0; i < parsedJsonWeather.daily.Count; i++)
            {
                weaklyWeather = new WeaklyWeatherModel();
                weaklyWeather.DayOfWeak = DateTimeOffset.FromUnixTimeSeconds(parsedJsonWeather.daily[i].dt).DayOfWeek.ToString();
                weaklyWeather.WeatherIcon = new BitmapImage(new Uri("https://openweathermap.org/img/wn/" + parsedJsonWeather.daily[i].weather[0].icon.ToString() + ".png"));
                weaklyWeather.TempMax = Convert.ToInt32(parsedJsonWeather.daily[i].temp.max).ToString() + "C°";
                weaklyWeather.TempMin = Convert.ToInt32(parsedJsonWeather.daily[i].temp.min).ToString() + "C°";
                weaklyWeatherModels.Add(weaklyWeather);
            }
        }

        void UpdateData()
        {
            //обновляем город и время
            var currentTime = DateTimeOffset.FromUnixTimeSeconds(parsedJsonWeather.current.dt + parsedJsonWeather.timezone_offset);
            CityTimeInfoModel cityTimeInfoModel = new CityTimeInfoModel();
            cityTimeInfoModel.City = parseCity.name;
            cityTimeInfoModel.Time = currentTime.TimeOfDay.ToString().Substring(0, currentTime.TimeOfDay.ToString().LastIndexOf(':'));
            cityTimeInfoModels.Add(cityTimeInfoModel);
            cityTimeInfoModels.RemoveAt(0);

            //обновляем пагоду на сегодня

            TodayWeatherModel todayWeatherModel = new TodayWeatherModel();
            todayWeatherModel.Temp = Convert.ToInt32(parsedJsonWeather.current.temp).ToString() + "C°";
            todayWeatherModel.WeatherIcon = new BitmapImage(new Uri("https://openweathermap.org/img/wn/" + parsedJsonWeather.current.weather[0].icon.ToString() + ".png"));
            todayWeatherModel.WeatherInfo = parsedJsonWeather.current.weather[0].main;
            todayWeatherModel.FeelsLike = "Feels like " + Convert.ToInt32(parsedJsonWeather.current.feels_like).ToString() + "C°";
            todayWeatherModels.Add(todayWeatherModel);
            todayWeatherModels.RemoveAt(0);

            //обновляем почасовую погоду
            DateTimeOffset hourlyTime;
            HourlyWeatherModel hourlyWeather;
            for (int i = 0; i < hourlyWeatherModels.Count; i++)
            {
                hourlyWeatherModels.RemoveAt(i--);
            }
            for (int i = 0; i < parsedJsonWeather.hourly.Count; i++)
            {
                hourlyTime = DateTimeOffset.FromUnixTimeSeconds(parsedJsonWeather.hourly[i].dt + parsedJsonWeather.timezone_offset);
                hourlyWeather = new HourlyWeatherModel();
                hourlyWeather.Temp = Convert.ToInt32(parsedJsonWeather.hourly[i].temp).ToString() + "C°";
                hourlyWeather.WeatherIcon = new BitmapImage(new Uri("https://openweathermap.org/img/wn/" + parsedJsonWeather.hourly[i].weather[0].icon.ToString() + ".png"));
                hourlyWeather.Time = hourlyTime.TimeOfDay.ToString().Substring(0, hourlyTime.TimeOfDay.ToString().LastIndexOf(':'));
                hourlyWeatherModels.Add(hourlyWeather);
            }

            //обновляем дневную погоду на неделю
            WeaklyWeatherModel weaklyWeather;
            for (int i=0; i < weaklyWeatherModels.Count; i++)
            {
                weaklyWeatherModels.RemoveAt(i--);
            }
            for (int i = 0; i < parsedJsonWeather.daily.Count; i++)
            {
                weaklyWeather = new WeaklyWeatherModel();
                weaklyWeather.DayOfWeak = DateTimeOffset.FromUnixTimeSeconds(parsedJsonWeather.daily[i].dt).DayOfWeek.ToString();
                weaklyWeather.WeatherIcon = new BitmapImage(new Uri("https://openweathermap.org/img/wn/" + parsedJsonWeather.daily[i].weather[0].icon.ToString() + ".png"));
                weaklyWeather.TempMax = Convert.ToInt32(parsedJsonWeather.daily[i].temp.max).ToString() + "C°";
                weaklyWeather.TempMin = Convert.ToInt32(parsedJsonWeather.daily[i].temp.min).ToString() + "C°";
                weaklyWeatherModels.Add(weaklyWeather);
            }
        }
    }

    #region ClassesForJsonSerialize
    public class DetectRegionFromAPI
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }
    public class CurrentCity
    {
        public string name { get; set; }
    }
    public class Current
    {
        public int dt { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double dew_point { get; set; }
        public double uvi { get; set; }
        public int clouds { get; set; }
        public int visibility { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public double wind_gust { get; set; }
        public List<Weather> weather { get; set; }
    }
    public class Daily
    {
        public int dt { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public int moonrise { get; set; }
        public int moonset { get; set; }
        public double moon_phase { get; set; }
        public Temp temp { get; set; }
        public FeelsLike feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double dew_point { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public double wind_gust { get; set; }
        public List<Weather> weather { get; set; }
        public int clouds { get; set; }
        public double pop { get; set; }
        public double uvi { get; set; }
        public double? rain { get; set; }
    }
    public class FeelsLike
    {
        public double day { get; set; }
        public double night { get; set; }
        public double eve { get; set; }
        public double morn { get; set; }
    }
    public class Hourly
    {
        public int dt { get; set; }
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double dew_point { get; set; }
        public double uvi { get; set; }
        public int clouds { get; set; }
        public int visibility { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public double wind_gust { get; set; }
        public List<Weather> weather { get; set; }
        public double pop { get; set; }
        public Rain rain { get; set; }
    }
    public class Rain
    {
        public double _1h { get; set; }
    }
    public class Root
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public int timezone_offset { get; set; }
        public Current current { get; set; }
        public List<Hourly> hourly { get; set; }
        public List<Daily> daily { get; set; }
    }
    public class Temp
    {
        public double day { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public double night { get; set; }
        public double eve { get; set; }
        public double morn { get; set; }
    }
    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
    #endregion
}
