using System;
using System.Collections.Generic;
using System.Windows;
using MySql.Data.MySqlClient;

namespace TicketApp
{
    // Простые классы с публичными полями
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
        // Строка подключения к базе
        string connStr = "Server=localhost;Database=salon;Uid=root;Pwd=1234;";

        // Списки для хранения клиентов и услуг
        List<Client> clients = new List<Client>();
        List<Service> services = new List<Service>();

        public MainWindow()
        {
            InitializeComponent();

            LoadClients();      // Загружаем клиентов в список и ComboBox
            LoadServices();     // Загружаем услуги в список и ComboBox
            LoadAppointments(); // Загружаем записи и показываем в DataGrid
        }

        // Загрузка клиентов из базы
        void LoadClients()
        {
            clients.Clear();
            cbClient.Items.Clear();

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT Id, FullName FROM Clients", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Client client = new Client();
                    client.Id = reader.GetInt32(0);
                    client.FullName = reader.GetString(1);
                    clients.Add(client);

                    cbClient.Items.Add(client.FullName);
                }
            }
        }

        // Загрузка услуг из базы
        void LoadServices()
        {
            services.Clear();
            cbService.Items.Clear();

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT Id, Name FROM Services", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Service service = new Service();
                    service.Id = reader.GetInt32(0);
                    service.Name = reader.GetString(1);
                    services.Add(service);

                    cbService.Items.Add(service.Name);
                }
            }
        }

        // Загрузка записей с JOIN для отображения имен клиента и услуги
        void LoadAppointments()
        {
            var appointments = new List<Appointment>();

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string sql = @"SELECT a.Id, c.FullName, s.Name, a.ApptDate, a.Status
                               FROM Appointments a
                               JOIN Clients c ON a.ClientId = c.Id
                               JOIN Services s ON a.ServiceId = s.Id
                               ORDER BY a.ApptDate DESC";

                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Appointment appt = new Appointment();
                    appt.Id = reader.GetInt32(0);
                    appt.ClientName = reader.GetString(1);
                    appt.ServiceName = reader.GetString(2);
                    appt.ApptDate = reader.GetDateTime(3).ToString("g");
                    appt.Status = reader.GetString(4);

                    appointments.Add(appt);
                }
            }

            dgAppts.ItemsSource = appointments;
        }

        // Кнопка добавить клиента
        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            string name = tbClientName.Text;
            string phone = tbClientPhone.Text;
            string email = tbClientEmail.Text;

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string sql = "INSERT INTO Clients (FullName, Phone, Email) VALUES (@name, @phone, @email)";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@email", email);

                cmd.ExecuteNonQuery();
            }

            LoadClients(); // Обновляем список клиентов и ComboBox
        }

        // Кнопка удалить выбранного клиента
        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (dgClients.SelectedItem is Client selectedClient)
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string sql = "DELETE FROM Clients WHERE Id = @id";
                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", selectedClient.Id);

                    cmd.ExecuteNonQuery();
                }

                LoadClients();
            }
            else
            {
                MessageBox.Show("Выберите клиента для удаления.");
            }
        }

        // Кнопка записать на услугу
        private void AddAppt_Click(object sender, RoutedEventArgs e)
        {
            int clientIndex = cbClient.SelectedIndex;
            int serviceIndex = cbService.SelectedIndex;
            DateTime? selectedDate = dpDate.SelectedDate;

            if (clientIndex < 0 || serviceIndex < 0 || selectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента, услугу и дату.");
                return;
            }

            int clientId = clients[clientIndex].Id;
            int serviceId = services[serviceIndex].Id;
            DateTime apptDate = selectedDate.Value;

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string sql = "INSERT INTO Appointments (ClientId, ServiceId, ApptDate, Status) VALUES (@clientId, @serviceId, @date, 'запланирована')";
                var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@clientId", clientId);
                cmd.Parameters.AddWithValue("@serviceId", serviceId);
                cmd.Parameters.AddWithValue("@date", apptDate);

                cmd.ExecuteNonQuery();
            }

            LoadAppointments();
        }

        // Кнопка обновить статус записи
        private void UpdateApptStatus_Click(object sender, RoutedEventArgs e)
        {
            if (dgAppts.SelectedItem is Appointment selectedAppt && cbStatus.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                string newStatus = selectedItem.Content.ToString();

                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string sql = "UPDATE Appointments SET Status = @status WHERE Id = @id";
                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@status", newStatus);
                    cmd.Parameters.AddWithValue("@id", selectedAppt.Id);

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
