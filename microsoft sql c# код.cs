//Не проверено
using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.SqlClient;

namespace TicketApp
{
    public class Client
    {
        public int Id;
        public string FullName;
    }

    public class Service
    {
        public int Id;
        public string Name;
    }

    public class Appointment
    {
        public int Id;
        public string ClientName;
        public string ServiceName;
        public string ApptDate;
        public string Status;
    }

    public partial class MainWindow : Window
    {
        // Строка подключения к MS SQL (отредактируй под себя)
        string connStr = @"Server=localhost;Database=salon;Trusted_Connection=True;";

        List<Client> clients = new List<Client>();
        List<Service> services = new List<Service>();

        public MainWindow()
        {
            InitializeComponent();

            LoadClients();
            LoadServices();
            LoadAppointments();
        }

        void LoadClients()
        {
            clients.Clear();
            cbClient.Items.Clear();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, FullName FROM Clients", conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var client = new Client
                    {
                        Id = reader.GetInt32(0),
                        FullName = reader.GetString(1)
                    };
                    clients.Add(client);
                    cbClient.Items.Add(client.FullName);
                }
            }
        }

        void LoadServices()
        {
            services.Clear();
            cbService.Items.Clear();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, Name FROM Services", conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var service = new Service
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };
                    services.Add(service);
                    cbService.Items.Add(service.Name);
                }
            }
        }

        void LoadAppointments()
        {
            var appointments = new List<Appointment>();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = @"
                    SELECT a.Id, c.FullName, s.Name, a.ApptDate, a.Status
                    FROM Appointments a
                    JOIN Clients c ON a.ClientId = c.Id
                    JOIN Services s ON a.ServiceId = s.Id
                    ORDER BY a.ApptDate DESC";

                var cmd = new SqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var appt = new Appointment
                    {
                        Id = reader.GetInt32(0),
                        ClientName = reader.GetString(1),
                        ServiceName = reader.GetString(2),
                        ApptDate = reader.GetDateTime(3).ToString("g"),
                        Status = reader.GetString(4)
                    };
                    appointments.Add(appt);
                }
            }

            dgAppts.ItemsSource = appointments;
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            string name = tbClientName.Text;
            string phone = tbClientPhone.Text;
            string email = tbClientEmail.Text;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = "INSERT INTO Clients (FullName, Phone, Email) VALUES (@name, @phone, @email)";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.ExecuteNonQuery();
            }

            LoadClients();
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (dgClients.SelectedItem is Client client)
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "DELETE FROM Clients WHERE Id = @id";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", client.Id);
                    cmd.ExecuteNonQuery();
                }
                LoadClients();
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления.");
            }
        }

        private void AddAppt_Click(object sender, RoutedEventArgs e)
        {
            int clientIndex = cbClient.SelectedIndex;
            int serviceIndex = cbService.SelectedIndex;
            DateTime? date = dpDate.SelectedDate;

            if (clientIndex < 0 || serviceIndex < 0 || date == null)
            {
                MessageBox.Show("Выберите клиента, услугу и дату.");
                return;
            }

            int clientId = clients[clientIndex].Id;
            int serviceId = services[serviceIndex].Id;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = "INSERT INTO Appointments (ClientId, ServiceId, ApptDate, Status) VALUES (@clientId, @serviceId, @date, 'запланирована')";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@clientId", clientId);
                cmd.Parameters.AddWithValue("@serviceId", serviceId);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.ExecuteNonQuery();
            }

            LoadAppointments();
        }

        private void UpdateApptStatus_Click(object sender, RoutedEventArgs e)
        {
            if (dgAppts.SelectedItem is Appointment appt && cbStatus.SelectedItem is System.Windows.Controls.ComboBoxItem item)
            {
                string newStatus = item.Content.ToString();

                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "UPDATE Appointments SET Status = @status WHERE Id = @id";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@status", newStatus);
                    cmd.Parameters.AddWithValue("@id", appt.Id);
                    cmd.ExecuteNonQuery();
                }

                LoadAppointments();
            }
            else
            {
                MessageBox.Show("Выберите запись и новый статус.");
            }
        }
    }
}
