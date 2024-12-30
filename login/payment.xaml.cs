using login;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace AuthApp
{
    public partial class Payment : Window
    {
        private const string ConnectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;";
        private ObservableCollection<PaymentEntity> payments;

        public Payment()
        {
            InitializeComponent();
            payments = new ObservableCollection<PaymentEntity>();
            PaymentDataGrid.ItemsSource = payments;
            LoadPaymentDataAsync();
        }

        // Method to load payment data asynchronously into the DataGrid
        private async Task LoadPaymentDataAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM Paiement"; // Load all payments
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        payments.Clear();
                        while (await reader.ReadAsync())
                        {
                            payments.Add(new PaymentEntity
                            {
                                Id = reader.GetInt32(0),
                                IdReservation = reader.GetInt32(1),
                                Montant = reader.GetDecimal(2),
                                DatePaiement = reader.GetDateTime(3),
                                MoyenPaiement = reader.GetString(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to save or update a payment
        private async Task SavePaymentAsync(PaymentEntity payment)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = payment.Id == 0
                        ? "INSERT INTO Paiement (IdReservation, Montant, DatePaiement, MoyenPaiement) VALUES (@IdReservation, @Montant, @DatePaiement, @MoyenPaiement)"
                        : "UPDATE Paiement SET IdReservation = @IdReservation, Montant = @Montant, DatePaiement = @DatePaiement, MoyenPaiement = @MoyenPaiement WHERE Id = @Id";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@IdReservation", payment.IdReservation);
                    command.Parameters.AddWithValue("@Montant", payment.Montant);
                    command.Parameters.AddWithValue("@DatePaiement", payment.DatePaiement);
                    command.Parameters.AddWithValue("@MoyenPaiement", payment.MoyenPaiement);
                    if (payment.Id != 0)
                    {
                        command.Parameters.AddWithValue("@Id", payment.Id);
                    }

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show(payment.Id == 0 ? "Payment added successfully!" : "Payment updated successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Error saving payment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add a new payment
        private void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            PaymentIDTextBox.Clear();
            ReservationIDTextBox.Clear();
            AmountTextBox.Clear();
            PaymentDatePicker.SelectedDate = null;
            PaymentMethodTextBox.Clear();
        }

        // Save or update payment on button click
        private async void SavePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var payment = new PaymentEntity
            {
                Id = int.TryParse(PaymentIDTextBox.Text, out var id) ? id : 0,
                IdReservation = int.TryParse(ReservationIDTextBox.Text, out var reservationId) ? reservationId : 0,
                Montant = decimal.TryParse(AmountTextBox.Text, out var montant) ? montant : 0,
                DatePaiement = PaymentDatePicker.SelectedDate ?? DateTime.MinValue,
                MoyenPaiement = PaymentMethodTextBox.Text
            };

            await SavePaymentAsync(payment);
            LoadPaymentDataAsync();
        }

        // Modify an existing payment
        private async void ModifyPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PaymentMethodTextBox.Text) || string.IsNullOrEmpty(AmountTextBox.Text) || string.IsNullOrEmpty(ReservationIDTextBox.Text))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var payment = new PaymentEntity
            {
                Id = int.TryParse(PaymentIDTextBox.Text, out var id) ? id : 0,
                IdReservation = int.TryParse(ReservationIDTextBox.Text, out var reservationId) ? reservationId : 0,
                Montant = decimal.TryParse(AmountTextBox.Text, out var montant) ? montant : 0,
                DatePaiement = PaymentDatePicker.SelectedDate ?? DateTime.MinValue,
                MoyenPaiement = PaymentMethodTextBox.Text
            };

            await SavePaymentAsync(payment);
            LoadPaymentDataAsync();
        }

        // Delete payment
        private async void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if a row is selected in the DataGrid
            if (PaymentDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a payment to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Get the selected payment
            var selectedPayment = PaymentDataGrid.SelectedItem as PaymentEntity;

            if (selectedPayment == null)
            {
                MessageBox.Show("Please select a valid payment.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int paymentId = selectedPayment.Id;

            // Confirm deletion with the user
            var result = MessageBox.Show("Are you sure you want to delete this payment?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        await connection.OpenAsync();
                        string query = "DELETE FROM Paiement WHERE Id = @Id";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Id", paymentId);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Payment deleted successfully!");
                            await LoadPaymentDataAsync(); // Refresh the data grid
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete the payment. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


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

    // Entity class for payments
    public class PaymentEntity
    {
        public int Id { get; set; }
        public int IdReservation { get; set; }
        public decimal Montant { get; set; }
        public DateTime DatePaiement { get; set; }
        public string MoyenPaiement { get; set; }
    }
}
