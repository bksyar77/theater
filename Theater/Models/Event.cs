using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Models
{
    //public class Event
    //{
    //    public string Title { get; set; } = string.Empty;
    //    public DateTime Date { get; set; } = DateTime.Now;
    //    public string Type { get; set; } = string.Empty;

    //    // Список актёров, участвующих в событии
    //}
    public class Event : INotifyPropertyChanged
    {
        private string _title;
        private DateTime _date;
        private string _type;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        // Список актеров, участвующих в событии
        //public List<Actor> Participants { get; set; } = new List<Actor>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
