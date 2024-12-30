﻿using login;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Net.Mail;
using System.Net;
using System.Net.Mail;
using ClosedXML.Excel;

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


        private async Task<decimal> GetRoomPriceAsync(int roomId)
        {
            decimal roomPrice = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    // Récupérer l'ID du type de chambre à partir de la table Chambre
                    string roomTypeQuery = "SELECT IdTypeChambre FROM Chambre WHERE Id = @RoomId";
                    SqlCommand roomTypeCommand = new SqlCommand(roomTypeQuery, connection);
                    roomTypeCommand.Parameters.AddWithValue("@RoomId", roomId);
                    var roomTypeId = await roomTypeCommand.ExecuteScalarAsync();

                    // Si un type de chambre est trouvé, récupérer son prix
                    if (roomTypeId != DBNull.Value)
                    {
                        string priceQuery = "SELECT PrixParNuit FROM TypeChambre WHERE Id = @RoomTypeId";
                        SqlCommand priceCommand = new SqlCommand(priceQuery, connection);
                        priceCommand.Parameters.AddWithValue("@RoomTypeId", roomTypeId);
                        var result = await priceCommand.ExecuteScalarAsync();
                        if (result != DBNull.Value)
                        {
                            roomPrice = Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching room price: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return roomPrice;
        }




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
                // Récupérer le prix du type de chambre
                decimal pricePerDay = 0;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    // Requête pour obtenir le prix du type de chambre
                    string priceQuery = "SELECT PrixParNuit FROM TypeChambre WHERE Id = (SELECT IdTypeChambre FROM Chambre WHERE Id = @IdChambre)";
                    SqlCommand priceCommand = new SqlCommand(priceQuery, connection);
                    priceCommand.Parameters.AddWithValue("@IdChambre", reservation.IdChambre);

                    var result = await priceCommand.ExecuteScalarAsync();
                    if (result != null)
                    {
                        pricePerDay = Convert.ToDecimal(result);
                    }
                    else
                    {
                        MessageBox.Show("Price for the selected room type not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // Exit if price is not found
                    }
                }

                // Calculer le nombre de jours de la réservation
                int numberOfDays = (reservation.DateFin - reservation.DateDebut).Days;

                // Calculer le total
                decimal total = pricePerDay * numberOfDays;

                // Sauvegarder la réservation avec le total calculé
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = reservation.Id == 0
                        ? "INSERT INTO Reservation (DateDebut, DateFin, Total, Statut, IdClient, IdChambre) VALUES (@DateDebut, @DateFin, @Total, @Statut, @IdClient, @IdChambre)"
                        : "UPDATE Reservation SET DateDebut = @DateDebut, DateFin = @DateFin, Total = @Total, Statut = @Statut, IdClient = @IdClient, IdChambre = @IdChambre WHERE Id = @Id";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@DateDebut", reservation.DateDebut);
                    command.Parameters.AddWithValue("@DateFin", reservation.DateFin);
                    command.Parameters.AddWithValue("@Total", total); // Utilisation du total calculé
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
            StatusComboBox.SelectedIndex = -1;
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
                Statut = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Statut non sélectionné",
                IdClient = int.TryParse(ClientIDTextBox.Text, out var clientId) ? clientId : 0,
                IdChambre = int.TryParse(RoomIDTextBox.Text, out var roomId) ? roomId : 0
            };

            // Calcul du nombre de jours réservés
            int daysReserved = (reservation.DateFin - reservation.DateDebut).Days;

            // Vérifier si le nombre de jours est valide
            if (daysReserved <= 0)
            {
                MessageBox.Show("La durée de la réservation est invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Récupérer le prix de la chambre
            decimal roomPrice = await GetRoomPriceAsync(reservation.IdChambre);

            // Calcul du prix total
            reservation.Total = roomPrice * daysReserved;

            // Sauvegarder ou mettre à jour la réservation dans la base de données
            await SaveReservationAsync(reservation);
            LoadReservationDataAsync();

            // Générer un PDF après la sauvegarde
            GenerateReservationPdf(reservation);
            GenerateInvoicePdf(reservation);
            SendConfirmationEmail(reservation);
        }



        // Modify existing reservation
        private async void ModifyReservationButton_Click(object sender, RoutedEventArgs e)
        {
            var statutSelectionne = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(statutSelectionne) || string.IsNullOrEmpty(ClientIDTextBox.Text) || string.IsNullOrEmpty(RoomIDTextBox.Text))

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
                Statut = StatusComboBox.SelectedItem.ToString(),
                IdClient = int.TryParse(ClientIDTextBox.Text, out var clientId) ? clientId : 0,
                IdChambre = int.TryParse(RoomIDTextBox.Text, out var roomId) ? roomId : 0
            };

            await SaveReservationAsync(reservation);
            LoadReservationDataAsync();
        }

        // Delete reservation
        private async void DeleteReservationButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if a row is selected in the DataGrid
            if (ReservationDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a reservation to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get the selected reservation
            var selectedReservation = ReservationDataGrid.SelectedItem as ReservationEntity;

            if (selectedReservation == null)
            {
                MessageBox.Show("Please select a valid reservation.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int reservationId = selectedReservation.Id;

            // Confirm deletion with the user
            var result = MessageBox.Show("Are you sure you want to delete this reservation?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
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
                            await LoadReservationDataAsync(); // Refresh the data grid
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete the reservation. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting reservation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Définissez des polices
            XFont titleFont = new XFont("Arial Bold", 14);
            XFont font = new XFont("Arial", 12);

            // Position de départ pour le texte
            double x = 20;
            double y = 20;

            // ===== Ajout du cadre =====
            double margin = 10; // Marge entre le contenu et le cadre
            double frameX = x - margin;
            double frameY = y - margin;
            double frameWidth = page.Width - 2 * x + 2 * margin; // Largeur du cadre
            double frameHeight = page.Height - 100; // Hauteur du cadre (ajustable selon le contenu)

            // Dessine le cadre
            gfx.DrawRectangle(XPens.Black, frameX, frameY, frameWidth, frameHeight);

            // ===== Ajout du logo =====
            string logoPath = "IMAR.png";
            if (File.Exists(logoPath))
            {
                XImage logo = XImage.FromFile(logoPath);
                gfx.DrawImage(logo, 30, y, 100, 50); // Place le logo à l'intérieur du cadre
                y += 60;
            }
            else
            {
                gfx.DrawString("HOTEL EMSI", titleFont, XBrushes.Gray, x, y);
                y += 40;
            }

            // ===== Titre =====
            gfx.DrawString("Bon de Réservation", titleFont, XBrushes.Black, x, y);
            y += 30;

            // ===== Informations de la réservation =====
            gfx.DrawString($"Reservation ID: {reservation.Id}", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Client ID: {reservation.IdClient}", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Start Date: {reservation.DateDebut:dd/MM/yyyy}", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"End Date: {reservation.DateFin:dd/MM/yyyy}", font, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Total: {reservation.Total:C}", new XFont("Arial Bold", 12), XBrushes.DarkGreen, x, y);
            y += 20;
            gfx.DrawString($"Status: {reservation.Statut}", font, XBrushes.Black, x, y);
            y += 40;

            // ===== Pied de page =====
            gfx.DrawString("Merci pour votre réservation !", font, XBrushes.Black, x, page.Height - 60);
            gfx.DrawString("HOTEL EMSI - Téléphone: +212 6 1234 5678", font, XBrushes.Black, x, page.Height - 40);

            // Sauvegarde
            string fileName = $"Reservation_{reservation.Id}.pdf";
            try
            {
                document.Save(fileName);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void GenerateInvoicePdf(ReservationEntity reservation)
        {
            // Retrieve client information from the database
            string clientName = string.Empty;
            string clientContact = string.Empty;

            // Replace this with your actual database connection and query
            string connectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;"; // Update with your DB connection string
            using (var connection = new SqlConnection(connectionString)) // Use SqliteConnection if using SQLite
            {
                connection.Open();
                string query = "SELECT FirstName, Adresse FROM Client WHERE Id = @ClientId";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", reservation.IdClient); // Assuming reservation has ClientId
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            clientName = reader["FirstName"].ToString();
                            clientContact = reader["Adresse"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Client not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }

            // Create a PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Facture de Réservation";

            // Create a page
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Define fonts
            XFont headerFont = new XFont("Arial", 12);
            XFont subHeaderFont = new XFont("Arial", 12);
            XFont contentFont = new XFont("Arial", 12);

            // Start position
            double x = 20;
            double y = 20;

            // ===== Logo and Title =====
            string logoPath = "IMAR.png"; // Update with your actual logo path
            if (File.Exists(logoPath))
            {
                XImage logo = XImage.FromFile(logoPath);
                gfx.DrawImage(logo, x, y, 100, 50); // Adjust dimensions as needed
                y += 60;
            }

            gfx.DrawString("HOTEL IMAR", headerFont, XBrushes.Black, x, y);
            y += 40;
            gfx.DrawString("Facture de Réservation", subHeaderFont, XBrushes.Black, x, y);
            y += 40;

            // ===== Invoice Information =====
            gfx.DrawString($"Numéro de Facture : {reservation.Id}", contentFont, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Date de Facture : {DateTime.Now:dd/MM/yyyy}", contentFont, XBrushes.Black, x, y);
            y += 40;

            // ===== Client Information =====
            gfx.DrawString("Informations du Client :", subHeaderFont, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Nom : {clientName}", contentFont, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Contact : {clientContact}", contentFont, XBrushes.Black, x, y);
            y += 40;

            // ===== Reservation Details =====
            gfx.DrawString("Détails de la Réservation :", subHeaderFont, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Date de Début : {reservation.DateDebut:dd/MM/yyyy}", contentFont, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Date de Fin : {reservation.DateFin:dd/MM/yyyy}", contentFont, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Chambre : {reservation.IdChambre}", contentFont, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Montant Total : {reservation.Total:C}", contentFont, XBrushes.Black, x, y);
            y += 20;
            gfx.DrawString($"Statut : {reservation.Statut}", contentFont, XBrushes.Black, x, y);

            // Save the PDF document
            string fileName = $"Invoice_{reservation.Id}.pdf";
            document.Save(fileName);

            // Display the file location
            MessageBox.Show($"Invoice PDF saved to: {Path.GetFullPath(fileName)}", "Invoice PDF Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private string GetClientEmail(int clientId)
        {
            string email = string.Empty;

            try
            {
                // Chaîne de connexion à votre base de données
                string connectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;";

                // Requête SQL pour récupérer l'e-mail
                string query = "SELECT Email FROM Client WHERE Id = @ClientId";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClientId", clientId);

                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            email = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération de l'e-mail : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return email;
        }



        private void SendConfirmationEmail(ReservationEntity reservation)
        {
            try
            {
                // Récupérer l'e-mail du client
                string clientEmail = GetClientEmail(reservation.IdClient);
                if (string.IsNullOrEmpty(clientEmail))
                {
                    MessageBox.Show("L'e-mail du client est introuvable.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string smtpAddress = "smtp.gmail.com";
                int portNumber = 587;
                bool enableSSL = true;

                string emailFrom = "elidrissiabdallah689@gmail.com";
                string password = "unqn bqbf zaeh egbz";
                string subject = "Confirmation de réservation";
                string body = $"Bonjour,\n\nVotre réservation (ID: {reservation.Id}) a été confirmée.\n\nDétails:\n" +
                              $"- Date de début : {reservation.DateDebut:dd/MM/yyyy}\n" +
                              $"- Date de fin : {reservation.DateFin:dd/MM/yyyy}\n" +
                              $"- Total : {reservation.Total:C}\n" +
                              "Merci de votre confiance.\n\nCordialement,\nVotre équipe.";


                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFrom);
                    mail.To.Add(clientEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFrom, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }

                MessageBox.Show("E-mail de confirmation envoyé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi de l'e-mail : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a new Excel workbook
                using (var workbook = new XLWorkbook())
                {
                    // Add a worksheet
                    var worksheet = workbook.Worksheets.Add("Reservations");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Id";
                    worksheet.Cell(1, 2).Value = "Start Date";
                    worksheet.Cell(1, 3).Value = "End Date";
                    worksheet.Cell(1, 4).Value = "Total";
                    worksheet.Cell(1, 5).Value = "Status";
                    worksheet.Cell(1, 6).Value = "Client ID";
                    worksheet.Cell(1, 7).Value = "Room ID";

                    // Add data
                    for (int i = 0; i < reservations.Count; i++)
                    {
                        var reservation = reservations[i];
                        worksheet.Cell(i + 2, 1).Value = reservation.Id;
                        worksheet.Cell(i + 2, 2).Value = reservation.DateDebut.ToString("dd/MM/yyyy");
                        worksheet.Cell(i + 2, 3).Value = reservation.DateFin.ToString("dd/MM/yyyy");
                        worksheet.Cell(i + 2, 4).Value = reservation.Total;
                        worksheet.Cell(i + 2, 5).Value = reservation.Statut;
                        worksheet.Cell(i + 2, 6).Value = reservation.IdClient;
                        worksheet.Cell(i + 2, 7).Value = reservation.IdChambre;
                    }

                    // Save the workbook to a file
                    string fileName = $"Reservations_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    workbook.SaveAs(fileName);

                    MessageBox.Show($"Data exported successfully to {fileName}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
