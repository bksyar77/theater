using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Models
{
    //public class Actor
    //{
    //    public string Name { get; set; } = string.Empty;
    //    public string Role { get; set; } = string.Empty;
    //    public double Balance { get; set; } = 0;
    //}
    public class Actor : INotifyPropertyChanged
    {
        private string _name;
        private string _role;
        private double _balance;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged();
            }
        }

        public double Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged();
            }
        }

        // Список событий, в которых участвует актер
        public ObservableCollection<Event> Events { get; set; } = new ObservableCollection<Event>();


        // Старое свойство для обратной совместимости (если нужно)
        //public List<Participation> Participations { get; set; } = new List<Participation>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
