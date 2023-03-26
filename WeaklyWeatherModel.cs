using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MVVM
{
    public class WeaklyWeatherModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string dayOfWeak;
        string tempMax;
        string tempMin;
        BitmapImage weatherIcon;

        public string DayOfWeak
        {
            get
            {
                return dayOfWeak;
            }
            set
            {
                dayOfWeak = value;
                OnPtopertyChanged();
            }
        }

        public string TempMax
        {
            get
            {
                return tempMax;
            }
            set
            {
                tempMax = value;
                OnPtopertyChanged();
            }
        }

        public string TempMin
        {
            get
            {
                return tempMin;
            }
            set
            {
                tempMin = value;
                OnPtopertyChanged();
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
                OnPtopertyChanged();
            }
        }

        public void OnPtopertyChanged([CallerMemberName] string prop="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
