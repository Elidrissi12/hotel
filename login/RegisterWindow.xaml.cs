using System;
using System.Data.SqlClient;
using System.Windows;

namespace AuthApp
{
    public partial class RegisterWindow : Window
    {
        private const string ConnectionString = "Server=ZORO\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;";

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    try
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show("Registration successful!");
                        LoginWindow loginWindow = new LoginWindow();
                        loginWindow.Show();
                        this.Close();
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow l = new LoginWindow();
            l.Show();
            this.Close();
        }
    }
}
