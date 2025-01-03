using login;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AuthApp
{
    public partial class TypeChambre : Window
    {
        private const string ConnectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;";
        private ObservableCollection<TypeChambreEntity> typeChambres;

        public TypeChambre()
        {
            InitializeComponent();
            typeChambres = new ObservableCollection<TypeChambreEntity>();
            TypeChambreDataGrid.ItemsSource = typeChambres;
            LoadTypeChambreDataAsync();
        }

        private async Task LoadTypeChambreDataAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM TypeChambre";
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        typeChambres.Clear();
                        while (await reader.ReadAsync())
                        {
                            typeChambres.Add(new TypeChambreEntity
                            {
                                IdTypeChambre = reader.GetInt32(0),
                                NomType = reader.GetString(1),
                                // Safely parsing PrixParNuit as decimal, handling possible DBNull
                                PrixParNuit = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading room types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task SaveTypeChambreAsync(TypeChambreEntity typeChambre)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = typeChambre.IdTypeChambre == 0
                        ? "INSERT INTO TypeChambre (Nom, PrixParNuit, Description) VALUES (@NomType, @PrixParNuit, @Description)"
                        : "UPDATE TypeChambre SET Nom = @NomType, PrixParNuit = @PrixParNuit, Description = @Description WHERE IdTypeChambre = @IdTypeChambre";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@NomType", typeChambre.NomType);
                    command.Parameters.AddWithValue("@PrixParNuit", typeChambre.PrixParNuit);
                    command.Parameters.AddWithValue("@Description", typeChambre.Description ?? (object)DBNull.Value);
                    if (typeChambre.IdTypeChambre != 0)
                    {
                        command.Parameters.AddWithValue("@IdTypeChambre", typeChambre.IdTypeChambre);
                    }

                    await command.ExecuteNonQueryAsync();
                    MessageBox.Show(typeChambre.IdTypeChambre == 0 ? "Room type added successfully!" : "Room type updated successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving room type: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddTypeButton_Click(object sender, RoutedEventArgs e)
        {
            TypeIDTextBox.Clear();
            TypeNameTextBox.Clear();
            PriceTextBox.Clear();
            DescriptionTextBox.Clear();
        }

        private async void SaveTypeButton_Click(object sender, RoutedEventArgs e)
        {
            var typeChambre = new TypeChambreEntity
            {
                IdTypeChambre = int.TryParse(TypeIDTextBox.Text, out var id) ? id : 0,
                NomType = TypeNameTextBox.Text,
                PrixParNuit = int.TryParse(PriceTextBox.Text, out var price) ? price : 0,
                Description = DescriptionTextBox.Text
            };

            await SaveTypeChambreAsync(typeChambre);
            await LoadTypeChambreDataAsync();
        }

        private async void DeleteTypeButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if a row is selected in the DataGrid
            if (TypeChambreDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a room type to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Get the selected room type
            var selectedTypeChambre = TypeChambreDataGrid.SelectedItem as TypeChambreEntity;

            if (selectedTypeChambre == null)
            {
                MessageBox.Show("Please select a valid room type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int typeId = selectedTypeChambre.IdTypeChambre;

            // Confirm deletion with the user
            var result = MessageBox.Show("Are you sure you want to delete this room type?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        await connection.OpenAsync();
                        string query = "DELETE FROM TypeChambre WHERE Id = @IdTypeChambre";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@IdTypeChambre", typeId);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Room type deleted successfully!");
                            await LoadTypeChambreDataAsync(); // Refresh the data grid
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete the room type. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting room type: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void TypeChambreDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if a row is selected in the DataGrid
            if (TypeChambreDataGrid.SelectedItem != null)
            {
                // Cast the selected item to TypeChambreEntity
                var selectedTypeChambre = TypeChambreDataGrid.SelectedItem as TypeChambreEntity;

                // Populate the form with the selected room type data
                if (selectedTypeChambre != null)
                {
                    TypeIDTextBox.Text = selectedTypeChambre.IdTypeChambre.ToString();
                    TypeNameTextBox.Text = selectedTypeChambre.NomType;
                    PriceTextBox.Text = selectedTypeChambre.PrixParNuit.ToString();
                    DescriptionTextBox.Text = selectedTypeChambre.Description;
                }
            }
        }

    }

    public class TypeChambreEntity
    {
        public int IdTypeChambre { get; set; }
        public string NomType { get; set; }
        public int PrixParNuit { get; set; }
        public string Description { get; set; }
    }
}
