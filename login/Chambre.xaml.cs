using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using login;

namespace AuthApp
{
    public partial class Chambre : Window
    {
        private const string ConnectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;";
        private ObservableCollection<ChambreEntity> chambres;

        public Chambre()
        {
            InitializeComponent();
            chambres = new ObservableCollection<ChambreEntity>();
            ChambreDataGrid.ItemsSource = chambres;
            LoadChambreDataAsync();
            LoadRoomTypesAsync();
        }

        // Charger les données des chambres
        private async Task LoadChambreDataAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM Chambre";
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        chambres.Clear();
                        while (await reader.ReadAsync())
                        {
                            chambres.Add(new ChambreEntity
                            {
                                Id = reader.GetInt32(0),
                                Numero = reader.GetString(1),
                                Statut = reader.GetString(2),
                                IdTypeChambre = reader.GetInt32(3),
                                
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des chambres: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Sauvegarder ou modifier une chambre
        private async Task SaveChambreAsync(ChambreEntity chambre)
        {
            if (chambre == null) throw new ArgumentNullException(nameof(chambre));

            string query = chambre.Id == 0
                ? "INSERT INTO Chambre (Numero, Statut, IdTypeChambre) VALUES (@Numero, @Statut, @IdTypeChambre)"
                : "UPDATE Chambre SET Numero = @Numero, Statut = @Statut, IdTypeChambre = @IdTypeChambre WHERE Id = @Id";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Numero", chambre.Numero);
                        command.Parameters.AddWithValue("@Statut", chambre.Statut);
                        command.Parameters.AddWithValue("@IdTypeChambre", chambre.IdTypeChambre);
                        if (chambre.Id != 0)
                            command.Parameters.AddWithValue("@Id", chambre.Id);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        MessageBox.Show(rowsAffected > 0 ? "Opération réussie !" : "Aucune modification effectuée.",
                                        "Résultat", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Ajouter une nouvelle chambre
        private async void AddChambreButton_Click(object sender, RoutedEventArgs e)
        {
            ChambreNumeroTextBox.Clear();
            ChambreStatutTextBox.Clear();
            ChambreTypeComboBox.SelectedIndex = -1;

        }

        // Sauvegarder une chambre
        private async void SaveChambreButton_Click(object sender, RoutedEventArgs e)
        {
            var chambre = new ChambreEntity
            {
                Id = int.TryParse(ChambreIDTextBox.Text, out var id) ? id : 0,
                Numero = ChambreNumeroTextBox.Text,
                Statut = ChambreStatutTextBox.Text,
                IdTypeChambre = ChambreTypeComboBox.SelectedValue != null
        ? (int)ChambreTypeComboBox.SelectedValue
        : 0
            };

            if (string.IsNullOrWhiteSpace(chambre.Numero) || string.IsNullOrWhiteSpace(chambre.Statut) || chambre.IdTypeChambre == 0)
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await SaveChambreAsync(chambre);
            await LoadChambreDataAsync();
        }

        // Modifier une chambre existante
        private async void ModifyChambreButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure a valid chambre is selected
            if (ChambreDataGrid.SelectedItem is not ChambreEntity selectedChambre)
            {
                MessageBox.Show("Veuillez sélectionner une chambre pour la modifier.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Clear existing inputs before populating
            ChambreIDTextBox.Clear();
            ChambreNumeroTextBox.Clear();
            ChambreStatutTextBox.Clear();
            ChambreTypeComboBox.SelectedIndex = -1;

            // Populate input fields with selected chambre details
            ChambreIDTextBox.Text = selectedChambre.Id.ToString();
            ChambreNumeroTextBox.Text = selectedChambre.Numero;
            ChambreStatutTextBox.Text = selectedChambre.Statut;
            ChambreTypeComboBox.SelectedValue = selectedChambre.IdTypeChambre;

            // Optional: Provide feedback to the user
            MessageBox.Show("Les détails de la chambre sont prêts pour modification.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        // Supprimer une chambre
        private async void DeleteChambreButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (ChambreDataGrid.SelectedItem is not ChambreEntity chambre)
            {
                MessageBox.Show("Veuillez sélectionner une chambre à supprimer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirm deletion with the user
            var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cette chambre ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;

            // Delete operation
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    string query = "DELETE FROM Chambre WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = chambre.Id });

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Chambre supprimée avec succès !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadChambreDataAsync(); // Refresh data
                        }
                        else
                        {
                            MessageBox.Show("Aucune chambre n'a été supprimée. Veuillez réessayer.", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression de la chambre : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void typechambreButton_Click(object sender, RoutedEventArgs e)
        {
            page3 x = new page3();
            x.Show();
            this.Close();
        }

        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                ChambreImage.Source = new BitmapImage(new Uri(selectedFile));
                // Optional: Save image path or binary to the database
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

        private void ChambreDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Check if a chambre is selected
            if (ChambreDataGrid.SelectedItem is ChambreEntity selectedChambre)
            {
                // Populate the input fields with the selected chambre's details
                ChambreIDTextBox.Text = selectedChambre.Id.ToString();
                ChambreNumeroTextBox.Text = selectedChambre.Numero;
                ChambreStatutTextBox.Text = selectedChambre.Statut;
                ChambreTypeComboBox.SelectedValue = selectedChambre.IdTypeChambre;

                // Optional: Provide feedback to the user
                MessageBox.Show("Chambre sélectionnée pour modification.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Clear input fields if no chambre is selected
                ChambreIDTextBox.Clear();
                ChambreNumeroTextBox.Clear();
                ChambreStatutTextBox.Clear();
                ChambreTypeComboBox.SelectedIndex = -1;

            }
        }
        private async Task LoadRoomTypesAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Id, Nom FROM TypeChambre";
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        ChambreTypeComboBox.Items.Clear();

                        while (await reader.ReadAsync())
                        {
                            ChambreTypeComboBox.Items.Add(new
                            {
                                Id = reader.GetInt32(0),
                                Nom = reader.GetString(1)
                            });
                        }

                        ChambreTypeComboBox.DisplayMemberPath = "Nom";
                        ChambreTypeComboBox.SelectedValuePath = "Id";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des types de chambre: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    

    // Classe pour représenter une chambre
    public class ChambreEntity
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public string Statut { get; set; }
        public int IdTypeChambre { get; set; }
        
    }
}
