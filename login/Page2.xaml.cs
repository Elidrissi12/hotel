using login;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;

namespace AuthApp
{
    public partial class Reservation : Window
    {
        private const string ConnectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;";
        private ObservableCollection<ReservationEntity> reservations;

        public Reservation()
        {
            InitializeComponent();
            reservations = new ObservableCollection<ReservationEntity>();
            ReservationDataGrid.ItemsSource = reservations;
            LoadReservationDataAsync();
        }

        // Method to load reservation data asynchronously into the DataGrid
        private async Task LoadReservationDataAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM Reservation"; // Load all reservations
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        reservations.Clear();
                        while (await reader.ReadAsync())
                        {
                            reservations.Add(new ReservationEntity
                            {
                                Id = reader.GetInt32(0),
                                DateDebut = reader.GetDateTime(1),
                                DateFin = reader.GetDateTime(2),
                                Total = reader.GetDecimal(3),
                                Statut = reader.GetString(4),
                                IdClient = reader.GetInt32(5),
                                IdChambre = reader.GetInt32(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reservations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to save a reservation
        private async Task SaveReservationAsync(ReservationEntity reservation)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = reservation.Id == 0
                        ? "INSERT INTO Reservation (DateDebut, DateFin, Total, Statut, IdClient, IdChambre) VALUES (@DateDebut, @DateFin, @Total, @Statut, @IdClient, @IdChambre)"
                        : "UPDATE Reservation SET DateDebut = @DateDebut, DateFin = @DateFin, Total = @Total, Statut = @Statut, IdClient = @IdClient, IdChambre = @IdChambre WHERE Id = @Id";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@DateDebut", reservation.DateDebut);
                    command.Parameters.AddWithValue("@DateFin", reservation.DateFin);
                    command.Parameters.AddWithValue("@Total", reservation.Total);
                    command.Parameters.AddWithValue("@Statut", reservation.Statut);
                    command.Parameters.AddWithValue("@IdClient", reservation.IdClient);
                    command.Parameters.AddWithValue("@IdChambre", reservation.IdChambre);
                    if (reservation.Id != 0)
                    {
                        command.Parameters.AddWithValue("@Id", reservation.Id);
                    }

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show(reservation.Id == 0 ? "Reservation added successfully!" : "Reservation updated successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Error saving reservation.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving reservation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add a new reservation
        private async void AddReservationButton_Click(object sender, RoutedEventArgs e)
        {
            ReservationIDTextBox.Clear();
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            TotalTextBox.Clear();
            StatusTextBox.Clear();
            ClientIDTextBox.Clear();
            RoomIDTextBox.Clear();
        }

        // Save or update reservation on button click
        private async void SaveReservationButton_Click(object sender, RoutedEventArgs e)
        {
            var reservation = new ReservationEntity
            {
                Id = int.TryParse(ReservationIDTextBox.Text, out var id) ? id : 0,
                DateDebut = StartDatePicker.SelectedDate ?? DateTime.MinValue,
                DateFin = EndDatePicker.SelectedDate ?? DateTime.MinValue,
                Total = decimal.TryParse(TotalTextBox.Text, out var total) ? total : 0,
                Statut = StatusTextBox.Text,
                IdClient = int.TryParse(ClientIDTextBox.Text, out var clientId) ? clientId : 0,
                IdChambre = int.TryParse(RoomIDTextBox.Text, out var roomId) ? roomId : 0
            };

            await SaveReservationAsync(reservation);
            LoadReservationDataAsync();

            // Générer un PDF après la sauvegarde
            GenerateReservationPdf(reservation);
        }


        // Modify existing reservation
        private async void ModifyReservationButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StatusTextBox.Text) || string.IsNullOrEmpty(ClientIDTextBox.Text) || string.IsNullOrEmpty(RoomIDTextBox.Text))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var reservation = new ReservationEntity
            {
                Id = int.TryParse(ReservationIDTextBox.Text, out var id) ? id : 0,
                DateDebut = StartDatePicker.SelectedDate ?? DateTime.MinValue,
                DateFin = EndDatePicker.SelectedDate ?? DateTime.MinValue,
                Total = decimal.TryParse(TotalTextBox.Text, out var total) ? total : 0,
                Statut = StatusTextBox.Text,
                IdClient = int.TryParse(ClientIDTextBox.Text, out var clientId) ? clientId : 0,
                IdChambre = int.TryParse(RoomIDTextBox.Text, out var roomId) ? roomId : 0
            };

            await SaveReservationAsync(reservation);
            LoadReservationDataAsync();
        }

        // Delete reservation
        private async void DeleteReservationButton_Click(object sender, RoutedEventArgs e)
        {
            int reservationId = int.TryParse(ReservationIDTextBox.Text, out var id) ? id : 0;

            if (reservationId == 0)
            {
                MessageBox.Show("Please select a valid reservation to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this reservation?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM Reservation WHERE Id = @Id";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", reservationId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Reservation deleted successfully!");
                        LoadReservationDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete the reservation. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void GenerateReservationPdf(ReservationEntity reservation)
        {
            // Créez un document PDF
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Reservation Bon";

            // Créez une page
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Définissez une police
            XFont font = new XFont("Arial", 12);

            // Position de départ pour le texte
            double x = 20;
            double y = 20;

            // ===== Ajout du logo =====
            string logoPath = "IDRISSI.png"; // Remplacez par le chemin de votre logo
            if (File.Exists(logoPath))
            {
                XImage logo = XImage.FromFile(logoPath);
                gfx.DrawImage(logo, 20, 20, 100, 50); // (x, y, largeur, hauteur)
                y += 60; // Décalez la position pour éviter le chevauchement avec le texte
            }
            else
            {
                MessageBox.Show("Logo introuvable : " + Path.GetFullPath(logoPath), "Erreur Logo", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Ajoutez des informations au document PDF
            gfx.DrawString("HOTEL EMSI", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString("Bon de Réservation", font, XBrushes.Black, x, y);
            y += 20;  // Décalez vers le bas

            gfx.DrawString($"Reservation ID: {reservation.Id}", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Client ID: {reservation.IdClient}", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Start Date: {reservation.DateDebut:dd/MM/yyyy}", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"End Date: {reservation.DateFin:dd/MM/yyyy}", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Total: {reservation.Total:C}", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Status: {reservation.Statut}", font, XBrushes.Black, x, y);
            y += 40;

            // Sauvegardez le document PDF
            string fileName = $"Reservation_{reservation.Id}.pdf";
            document.Save(fileName);

            // Afficher l'emplacement du fichier PDF pour vérifier
            MessageBox.Show($"PDF saved to: {Path.GetFullPath(fileName)}", "PDF Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            
        }


        private void clientButton_Click(object sender, RoutedEventArgs e)
        {
            page3 r = new page3();
            r.Show();
            this.Close();
        }
    }

    // Entity class for reservations
    public class ReservationEntity
    {
        public int Id { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public decimal Total { get; set; }
        public string Statut { get; set; }
        public int IdClient { get; set; }
        public int IdChambre { get; set; }
    }
}
