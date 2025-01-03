using login;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AuthApp
{
    public partial class Client : Window
    {
        private const string ConnectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;";
        private ObservableCollection<ClientEntity> clients;

        public Client()
        {
            InitializeComponent();
            clients = new ObservableCollection<ClientEntity>();
            ClientDataGrid.ItemsSource = clients;
            LoadClientDataAsync();
        }

        // Load clients from the database
        private async Task LoadClientDataAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM Client";
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        clients.Clear();
                        while (await reader.ReadAsync())
                        {
                            clients.Add(new ClientEntity
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Prenom = reader.GetString(2),
                                Email = reader.GetString(3),
                                PhoneNumber = reader.GetString(4)
                                
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading clients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Save client to the database
        private async Task SaveClientAsync(ClientEntity client)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = client.Id == 0
                        ? "INSERT INTO Client (LastName, FirstName, Email, Phone, Adresse) VALUES (@Nom, @Prenom, @Email, @Telephone, @Adresse)"
                        : "UPDATE Client SET LastName = @Nom, FirstName = @Prenom, Email = @Email, Phone = @Telephone, Adresse = @Adresse WHERE Id = @Id";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Nom", client.Name);
                    command.Parameters.AddWithValue("@Prenom", client.Prenom);
                    command.Parameters.AddWithValue("@Email", client.Email);
                    command.Parameters.AddWithValue("@Telephone", client.PhoneNumber);
                    command.Parameters.AddWithValue("@Adresse", client.Adresse);

                    if (client.Id != 0)
                    {
                        command.Parameters.AddWithValue("@Id", client.Id);
                    }

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show(client.Id == 0 ? "Client added successfully!" : "Client updated successfully!");

                        // Envoyer un email de notification
                        var emailService = new EmailService();
                        string subject = client.Id == 0 ? "Bienvenue à notre hôtel" : "Modification de vos informations";
                        string body = $@"
                    <p>Bonjour {client.Name} {client.Prenom},</p>
                    <p>Merci de {(client.Id == 0 ? "nous avoir rejoints" : "mettre à jour vos informations")}. Voici un récapitulatif :</p>
                    <ul>
                        <li><strong>Nom :</strong> {client.Name}</li>
                        <li><strong>Prénom :</strong> {client.Prenom}</li>
                        <li><strong>Email :</strong> {client.Email}</li>
                        <li><strong>Téléphone :</strong> {client.PhoneNumber}</li>
                        <li><strong>Adresse :</strong> {client.Adresse}</li>
                    </ul>
                    <p>Nous sommes ravis de vous compter parmi nos clients.</p>
                    <p>Cordialement,<br>L'équipe de l'hôtel</p>";

                        await emailService.SendEmailAsync(client.Email, subject, body);
                    }
                    else
                    {
                        MessageBox.Show("Error saving client.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Add a new client
        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            ClientIDTextBox.Clear();
            NameTextBox.Clear();
            PrenomTextBox.Clear();
            EmailTextBox.Clear();
            PhoneNumberTextBox.Clear();
            AdresseTextBox.Clear();
        }

        // Save or update client
        private async void SaveClientButton_Click(object sender, RoutedEventArgs e)
        {
            var client = new ClientEntity
            {
                Id = int.TryParse(ClientIDTextBox.Text, out var id) ? id : 0,
                Name = NameTextBox.Text,
                Prenom = PrenomTextBox.Text,
                Email = EmailTextBox.Text,
                PhoneNumber = PhoneNumberTextBox.Text,
                Adresse = AdresseTextBox.Text
            };

            await SaveClientAsync(client);
            await LoadClientDataAsync();
        }

        // Modify existing client
        private async void ModifyClientButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation des champs
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PrenomTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(AdresseTextBox.Text))
            {
                MessageBox.Show("Tous les champs sont requis.", "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validation de l'ID du client
            if (!int.TryParse(ClientIDTextBox.Text, out var id) || id <= 0)
            {
                MessageBox.Show("Sélectionnez un client valide pour le modifier.", "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Création de l'objet client avec les données saisies
            var client = new ClientEntity
            {
                Id = id,
                Name = NameTextBox.Text.Trim(),
                Prenom = PrenomTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                PhoneNumber = PhoneNumberTextBox.Text.Trim(),
                Adresse = AdresseTextBox.Text.Trim()
            };

            // Appel de la méthode pour sauvegarder les modifications
            await SaveClientAsync(client);

            // Recharger les données après modification
            await LoadClientDataAsync();
        }

        

        // Delete client
        private async void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if a row is selected in the DataGrid
            if (ClientDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a client to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Get the selected client
            var selectedClient = ClientDataGrid.SelectedItem as ClientEntity;

            if (selectedClient == null)
            {
                MessageBox.Show("Please select a valid client.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int clientId = selectedClient.Id;

            // Confirm deletion with the user
            var result = MessageBox.Show("Are you sure you want to delete this client?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        await connection.OpenAsync();
                        string query = "DELETE FROM Client WHERE Id = @Id";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Id", clientId);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Client deleted successfully!");
                            await LoadClientDataAsync(); // Refresh the data grid
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete the client. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ClientDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ClientDataGrid.SelectedItem is ClientEntity selectedClient)
            {
                // Populate the input fields with the selected client's data
                ClientIDTextBox.Text = selectedClient.Id.ToString();
                NameTextBox.Text = selectedClient.Name;
                PrenomTextBox.Text = selectedClient.Prenom;
                EmailTextBox.Text = selectedClient.Email;
                PhoneNumberTextBox.Text = selectedClient.PhoneNumber;
                AdresseTextBox.Text = selectedClient.Adresse;
            }

        }
    }



    // Navigation vers la page Clients (à implémenter)


    // Affichage des statistiques







    // Entity class for clients
    public class ClientEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Adresse { get; set; }
    }
}
