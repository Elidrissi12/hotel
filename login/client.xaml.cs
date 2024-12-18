using login;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

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
                                PhoneNumber = reader.GetString(4),
                                Adresse = reader.GetString(5)
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
                        ? "INSERT INTO Client (Nom, Prenom, Email, Telephone, Adresse) VALUES (@Nom, @Prenom, @Email, @Telephone, @Adresse)"
                        : "UPDATE Client SET nom = @Nom, Prenom = @Prenom, Email = @Email, Telephone = @Telephone, Adresse = @Adresse WHERE Id = @Id";

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
            if (string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(PrenomTextBox.Text) || string.IsNullOrEmpty(EmailTextBox.Text) || string.IsNullOrEmpty(PhoneNumberTextBox.Text) || string.IsNullOrEmpty(AdresseTextBox.Text))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var client = new ClientEntity
            {
                Id = int.TryParse(ClientIDTextBox.Text, out var id) ? id : 0,
                Name = NameTextBox.Text,
                Prenom = PrenomTextBox.Text,
                Email = EmailTextBox.Text,
                PhoneNumber = PhoneNumberTextBox.Text,
                Adresse = AdresseTextBox.Text,
            };

            await SaveClientAsync(client);
            await LoadClientDataAsync();
        }

        // Delete client
        private async void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            int clientId = int.TryParse(ClientIDTextBox.Text, out var id) ? id : 0;

            if (clientId == 0)
            {
                MessageBox.Show("Please select a valid client to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this client?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
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
                        await LoadClientDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete the client. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ChambreButton_Click(object sender, RoutedEventArgs e)
        {
            page3 r = new page3();
            r.Show();
            this.Close();
        }
    }

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
