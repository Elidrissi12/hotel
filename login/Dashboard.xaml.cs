using login;
using System;
using System.Data.SqlClient;
using System.Windows;




namespace AuthApp
{
    public partial class Dashboard : Window
    {
        private const string ConnectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;";

        public Dashboard()
        {
            InitializeComponent();
            LoadStatistics();
        }

        // Charger les statistiques des réservations
        private void LoadStatistics()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Obtenir le total des réservations
                    string totalQuery = "SELECT COUNT(*) FROM Reservation";
                    SqlCommand totalCmd = new SqlCommand(totalQuery, connection);
                    int totalReservations = (int)totalCmd.ExecuteScalar();
                    TotalReservationsText.Text = totalReservations.ToString();

                    // Obtenir le total des réservations en cours
                    string ongoingQuery = "SELECT COUNT(*) FROM Reservation WHERE Statut = 'Confirmée'";
                    SqlCommand ongoingCmd = new SqlCommand(ongoingQuery, connection);
                    int ongoingReservations = (int)ongoingCmd.ExecuteScalar();
                    OngoingReservationsText.Text = ongoingReservations.ToString();

                    string EmpQuery = "SELECT COUNT(*) FROM Employe";
                    SqlCommand EmpCmd = new SqlCommand(EmpQuery, connection);
                    int totalEmp = (int)EmpCmd.ExecuteScalar();
                    OngoingEmployeeText1.Text = totalEmp.ToString();

                    string ClQuery = "SELECT COUNT(*) FROM Client";
                    SqlCommand ClCmd = new SqlCommand(ClQuery, connection);
                    int totalClient = (int)ClCmd.ExecuteScalar();
                    TotalClientText1.Text = totalReservations.ToString();

                    string roomQuery = "SELECT COUNT(*) FROM Chambre";
                    SqlCommand roomCmd = new SqlCommand(roomQuery, connection);
                    int rooms = (int)roomCmd.ExecuteScalar();
                    TotalRoomsText2.Text = rooms.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de chargement des statistiques : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        // Affichage des statistiques
        private void StatsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }

        // Déconnexion
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginPage = new LoginWindow();
            loginPage.Show();
            this.Close();
        }

        private void acceuilButton_Click(object sender, RoutedEventArgs e)
        {
            page3 loginPage = new page3();
            loginPage.Show();
            this.Close();
        }



        private void ReservationsButton_Click(object sender, RoutedEventArgs e)
        {
            Reservation reservationPage = new Reservation();
            reservationPage.Show();
            this.Close();
        }

        private void PymentButton_Click(object sender, RoutedEventArgs e)
        {
            Payment x = new Payment();
            x.Show();
            this.Close();
        }

        private void RoomtypesButton_Click(object sender, RoutedEventArgs e)
        {
            TypeChambre x = new TypeChambre();
            x.Show();
            this.Close();
        }

        private void EmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            page1 x = new page1();
            x.Show();
            this.Close();
        }

        private void ClientButton_Click(object sender, RoutedEventArgs e)
        {
            Client x = new Client();
            x.Show();
            this.Close();
        }

        private void RoomsButton_Click(object sender, RoutedEventArgs e)
        {
            Chambre x = new Chambre();
            x.Show();
            this.Close();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard x = new Dashboard();
            x.Show();
            this.Close();
        }
    }
}

