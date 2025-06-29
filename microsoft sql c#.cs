//Не тестился
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;

namespace TicketApp {
    public class Request {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string RequestType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public partial class MainWindow : Window {
        private string connString = "Server=.;Database=service;Trusted_Connection=True;";

        public MainWindow() {
            InitializeComponent();
            LoadRequests();
        }

        private DbConnection CreateConn() {
            return new SqlConnection(connString);
        }

        private void LoadRequests() {
            var list = new List<Request>();
            using (var conn = CreateConn()) {
                conn.Open();
                using (var cmd = conn.CreateCommand()) {
                    cmd.CommandText = "SELECT Id, ClientName, RequestType, Description, Status, CreatedAt FROM Requests ORDER BY CreatedAt DESC";
                    using (var rdr = cmd.ExecuteReader()) {
                        while (rdr.Read()) {
                            list.Add(new Request {
                                Id = rdr.GetInt32(rdr.GetOrdinal("Id")),
                                ClientName = rdr.GetString(rdr.GetOrdinal("ClientName")),
                                RequestType = rdr.GetString(rdr.GetOrdinal("RequestType")),
                                Description = rdr.GetString(rdr.GetOrdinal("Description")),
                                Status = rdr.GetString(rdr.GetOrdinal("Status")),
                                CreatedAt = rdr.GetDateTime(rdr.GetOrdinal("CreatedAt"))
                            });
                        }
                    }
                }
            }
            dgRequests.ItemsSource = list;
        }

        private void Add_Click(object sender, RoutedEventArgs e) {
            using (var conn = CreateConn()) {
                conn.Open();
                using (var cmd = conn.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO Requests (ClientName, RequestType, Description, Status) VALUES (@c, @t, @d, 'новая')";

                    var p1 = cmd.CreateParameter();
                    p1.ParameterName = "@c";
                    p1.Value = tbClient.Text;
                    cmd.Parameters.Add(p1);

                    var p2 = cmd.CreateParameter();
                    p2.ParameterName = "@t";
                    p2.Value = (cbType.SelectedItem as ComboBoxItem)?.Content.ToString();
                    cmd.Parameters.Add(p2);

                    var p3 = cmd.CreateParameter();
                    p3.ParameterName = "@d";
                    p3.Value = tbDesc.Text;
                    cmd.Parameters.Add(p3);

                    cmd.ExecuteNonQuery();
                }
            }
            LoadRequests();
        }

        private void UpdateStatus_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedItem is Request sel) {
                using (var conn = CreateConn()) {
                    conn.Open();
                    using (var cmd = conn.CreateCommand()) {
                        cmd.CommandText = "UPDATE Requests SET Status = @st WHERE Id = @id";

                        var p1 = cmd.CreateParameter();
                        p1.ParameterName = "@st";
                        p1.Value = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
                        cmd.Parameters.Add(p1);

                        var p2 = cmd.CreateParameter();
                        p2.ParameterName = "@id";
                        p2.Value = sel.Id;
                        cmd.Parameters.Add(p2);

                        cmd.ExecuteNonQuery();
                    }
                }
                LoadRequests();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedItem is Request sel) {
                using (var conn = CreateConn()) {
                    conn.Open();
                    using (var cmd = conn.CreateCommand()) {
                        cmd.CommandText = "DELETE FROM Requests WHERE Id = @id";

                        var p = cmd.CreateParameter();
                        p.ParameterName = "@id";
                        p.Value = sel.Id;
                        cmd.Parameters.Add(p);

                        cmd.ExecuteNonQuery();
                    }
                }
                LoadRequests();
            }
        }
    }
}
