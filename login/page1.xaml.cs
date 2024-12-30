using login;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace AuthApp
{
    public partial class page1 : Window
    {
        private const string ConnectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;";
        private ObservableCollection<Employee> employees;

        public page1()
        {
            InitializeComponent();
            employees = new ObservableCollection<Employee>();
            EmployeeDataGrid.ItemsSource = employees;
            LoadEmployeeDataAsync();
        }

        // Méthode pour charger les données des employés dans le DataGrid de manière asynchrone
        private async Task LoadEmployeeDataAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM Employe "; // Statut = 1 signifie employés actifs
                    SqlCommand command = new SqlCommand(query, connection);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        employees.Clear(); // Clear previous data
                        while (await reader.ReadAsync())
                        {
                            employees.Add(new Employee
                            {
                                Id = reader.GetInt32(0),
                                Nom = reader.GetString(1),
                                Prenom = reader.GetString(2),
                                Email = reader.GetString(3),
                                Telephone = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Role = reader.IsDBNull(5) ? null : reader.GetString(5),
                                
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Méthode pour sauvegarder un employé dans la base de données de manière asynchrone
        private async Task SaveEmployeeAsync(Employee employee)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = employee.Id == 0
                        ? "INSERT INTO Employe (Nom, Prenom, Email, Telephone, Role, Statut) VALUES (@Nom, @Prenom, @Email, @Telephone, @Role, 1)"
                        : "UPDATE Employe SET Nom = @Nom, Prenom = @Prenom, Email = @Email, Telephone = @Telephone, Role = @Role WHERE Id = @Id";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Nom", employee.Nom);
                    command.Parameters.AddWithValue("@Prenom", employee.Prenom);
                    command.Parameters.AddWithValue("@Email", employee.Email);
                    command.Parameters.AddWithValue("@Telephone", employee.Telephone);
                    command.Parameters.AddWithValue("@Role", employee.Role);
                    if (employee.Id != 0) command.Parameters.AddWithValue("@Id", employee.Id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    // Verify if the operation was successful
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show(employee.Id == 0 ? "Employee added successfully!" : "Employee updated successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Error saving employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Handler for the "Save" button
        private async void SaveEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var employee = new Employee
            {
                Id = int.TryParse(EmployeeIDTextBox.Text, out var id) ? id : 0,
                Nom = EmployeeNameTextBox.Text,
                Prenom = EmployeePrenomTextBox.Text,
                Email = EmployeeEmailTextBox.Text,
                Telephone = EmployeeTelephoneTextBox.Text,
                Role = PositionTextBox.Text
            };

            await SaveEmployeeAsync(employee);
            LoadEmployeeDataAsync();
        }

        // Handler for the "Cancel" button
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeeIDTextBox.Clear();
            EmployeeNameTextBox.Clear();
            EmployeePrenomTextBox.Clear();
            EmployeeEmailTextBox.Clear();
            EmployeeTelephoneTextBox.Clear();
            PositionTextBox.Clear();
            SalaryTextBox.Clear();
        }

        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeeIDTextBox.Clear();
            EmployeeNameTextBox.Clear();
            EmployeePrenomTextBox.Clear();
            EmployeeEmailTextBox.Clear();
            EmployeeTelephoneTextBox.Clear();
            PositionTextBox.Clear();
            SalaryTextBox.Clear();
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private async void ModifyEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(EmployeeNameTextBox.Text) ||
                string.IsNullOrEmpty(EmployeePrenomTextBox.Text) ||
                string.IsNullOrEmpty(EmployeeEmailTextBox.Text) ||
                string.IsNullOrEmpty(PositionTextBox.Text))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var employee = new Employee
            {
                Id = int.TryParse(EmployeeIDTextBox.Text, out var id) ? id : 0,
                Nom = EmployeeNameTextBox.Text,
                Prenom = EmployeePrenomTextBox.Text,
                Email = EmployeeEmailTextBox.Text,
                Telephone = EmployeeTelephoneTextBox.Text,
                Role = PositionTextBox.Text
                
            };

            await SaveEmployeeAsync(employee); // Use the existing SaveEmployeeAsync for both saving and updating
            LoadEmployeeDataAsync(); // Reload the employee list
        }

        private async void DeleteEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            int employeeId = int.TryParse(EmployeeIDTextBox.Text, out var id) ? id : 0;

            if (employeeId == 0)
            {
                MessageBox.Show("Please select a valid employee to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this employee?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM Employe WHERE Id = @Id";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", employeeId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Employee deleted successfully!");
                        LoadEmployeeDataAsync(); // Reload the employee list
                    }
                    else
                    {
                        MessageBox.Show("Error deleting employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a SaveFileDialog to let the user choose the file name and location
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    Title = "Save Employee Data"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Create a new workbook
                    using (var workbook = new XLWorkbook())
                    {
                        // Create a worksheet
                        var worksheet = workbook.Worksheets.Add("Employees");

                        // Add header row
                        worksheet.Cell(1, 1).Value = "ID";
                        worksheet.Cell(1, 2).Value = "Nom";
                        worksheet.Cell(1, 3).Value = "Prenom";
                        worksheet.Cell(1, 4).Value = "Email";
                        worksheet.Cell(1, 5).Value = "Telephone";
                        worksheet.Cell(1, 6).Value = "Role";

                        // Add employee data
                        for (int i = 0; i < employees.Count; i++)
                        {
                            var employee = employees[i];
                            worksheet.Cell(i + 2, 1).Value = employee.Id;
                            worksheet.Cell(i + 2, 2).Value = employee.Nom;
                            worksheet.Cell(i + 2, 3).Value = employee.Prenom;
                            worksheet.Cell(i + 2, 4).Value = employee.Email;
                            worksheet.Cell(i + 2, 5).Value = employee.Telephone;
                            worksheet.Cell(i + 2, 6).Value = employee.Role;
                        }

                        // Save the workbook to the selected path
                        workbook.SaveAs(saveFileDialog.FileName);
                        MessageBox.Show("Employee data exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

    // Modèle Employee pour représenter les données d'un employé
    public class Employee
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Role { get; set; }
    }
}
