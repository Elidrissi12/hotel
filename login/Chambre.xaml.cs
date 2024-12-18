using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
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
                                IdTypeChambre = reader.GetInt32(3)
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
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = chambre.Id == 0
                        ? "INSERT INTO Chambre (Numero, Statut, IdTypeChambre) VALUES (@Numero, @Statut, @IdTypeChambre)"
                        : "UPDATE Chambre SET Numero = @Numero, Statut = @Statut, IdTypeChambre = @IdTypeChambre WHERE Id = @Id";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Numero", chambre.Numero);
                    command.Parameters.AddWithValue("@Statut", chambre.Statut);
                    command.Parameters.AddWithValue("@IdTypeChambre", chambre.IdTypeChambre);

                    if (chambre.Id != 0)
                        command.Parameters.AddWithValue("@Id", chambre.Id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                        MessageBox.Show(chambre.Id == 0 ? "Chambre ajoutée avec succès !" : "Chambre modifiée avec succès !");
                    else
                        MessageBox.Show("Erreur lors de la sauvegarde de la chambre.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde de la chambre: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Ajouter une nouvelle chambre
        private async void AddChambreButton_Click(object sender, RoutedEventArgs e)
        {
            ChambreNumeroTextBox.Clear();
            ChambreStatutTextBox.Clear();
            ChambreTypeIdTextBox.Clear();
        }

        // Sauvegarder une chambre
        private async void SaveChambreButton_Click(object sender, RoutedEventArgs e)
        {
            var chambre = new ChambreEntity
            {
                Id = int.TryParse(ChambreIDTextBox.Text, out var id) ? id : 0,
                Numero = ChambreNumeroTextBox.Text,
                Statut = ChambreStatutTextBox.Text,
                IdTypeChambre = int.TryParse(ChambreTypeIdTextBox.Text, out var typeId) ? typeId : 0
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
            if (ChambreDataGrid.SelectedItem is not ChambreEntity chambre)
            {
                MessageBox.Show("Veuillez sélectionner une chambre pour la modifier.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChambreIDTextBox.Text = chambre.Id.ToString();
            ChambreNumeroTextBox.Text = chambre.Numero;
            ChambreStatutTextBox.Text = chambre.Statut;
            ChambreTypeIdTextBox.Text = chambre.IdTypeChambre.ToString();
        }

        // Supprimer une chambre
        private async void DeleteChambreButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChambreDataGrid.SelectedItem is not ChambreEntity chambre)
            {
                MessageBox.Show("Veuillez sélectionner une chambre à supprimer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cette chambre ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        await connection.OpenAsync();
                        string query = "DELETE FROM Chambre WHERE Id = @Id";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Id", chambre.Id);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Chambre supprimée avec succès !");
                            await LoadChambreDataAsync();
                        }
                        else
                        {
                            MessageBox.Show("Erreur lors de la suppression de la chambre.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la suppression de la chambre: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void typechambreButton_Click(object sender, RoutedEventArgs e)
        {
            page3 x = new page3();
            x.Show();
            this.Close();
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
