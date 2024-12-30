using AuthApp;
using System;
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
using System.Windows.Shapes;

namespace login
{
    /// <summary>
    /// Logique d'interaction pour page3.xaml
    /// </summary>
    public partial class page3 : Window
    {
        public page3()
        {
            InitializeComponent();
        }

        private void ManageEmployees_Click(object sender, RoutedEventArgs e)
        {
            page1 r = new page1();
            r.Show();
            this.Close();
        }

        private void ManageClients_Click(object sender, RoutedEventArgs e)
        {
            Client r = new Client();
            r.Show();
            this.Close();
        }

        private void ManageRooms_Click(object sender, RoutedEventArgs e)
        {
            Chambre r = new Chambre();
            r.Show();
            this.Close();
        }

        private void ManageReservations_Click(object sender, RoutedEventArgs e)
        {
            Reservation r = new Reservation();
            r.Show();
            this.Close();
        }

        private void ManagePayments_Click(object sender, RoutedEventArgs e)
        {
            Payment r = new Payment();
            r.Show();
            this.Close();
        }

        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            TypeChambre r = new TypeChambre();
            r.Show();
            this.Close();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            Dashboard r = new Dashboard();
            r.Show();
            this.Close();
        }
    }
}
