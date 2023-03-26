using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MVVM
{
    public class TodayWeatherModel : INotifyPropertyChanged
    {
        string temp;
        BitmapImage weatherIcon;
        string weatherInfo;
        string feelsLike;

        public string Temp
        {
            get
            {
                return temp;
            }
            set
            {
                temp = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage WeatherIcon
        {
            get
            {
                return weatherIcon;
            }
            set
            {
                weatherIcon = value;
                OnPropertyChanged();
            }
        }

        public string WeatherInfo
        {
            get
            {
                return weatherInfo;
            }
            set
            {
                weatherInfo = value;
                OnPropertyChanged();
            }
        }

        public string FeelsLike
        {
            get
            {
                return feelsLike;
            }
            set
            {
                feelsLike = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
