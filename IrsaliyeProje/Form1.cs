using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace IrsaliyeProje
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Server=HALIL\SQLEXPRESS;Database=HAS;Trusted_Connection=True;";
        private string officeCode = "S008";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            verigetir();
            LoadCustomersIntoComboBox();
        }

        private void verigetir()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string sql = "SELECT OfficeDescription, OfficeCode FROM cdOfficeDesc WHERE OfficeCode = @OfficeCode";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@OfficeCode", officeCode);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            comboBox1.Items.Clear();
                            while (reader.Read())
                            {
                                comboBox1.Items.Add(new { Text = reader["OfficeDescription"].ToString(), Value = reader["OfficeCode"].ToString() });
                            }
                            comboBox1.DisplayMember = "Text";
                            comboBox1.ValueMember = "Value";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bağlantı hatası: " + ex.Message);
                }
            }
            MagazaGetir();
        }

        private void MagazaGetir()
        {
            string selectedOfficeCode = (comboBox1.SelectedItem as dynamic)?.Value?.ToString();
            if (string.IsNullOrEmpty(selectedOfficeCode))
                return;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string sql = @"
                        SELECT DISTINCT od.OfficeDescription 
                        FROM cdCurrAcc ca
                        INNER JOIN cdOfficeDesc od ON ca.OfficeCode = od.OfficeCode
                        WHERE ca.OfficeCode = @OfficeCode";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@OfficeCode", selectedOfficeCode);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            cmbMagaza.Items.Clear();
                            while (reader.Read())
                            {
                                string officeDescription = reader["OfficeDescription"].ToString();
                                if (!cmbMagaza.Items.Contains(officeDescription))
                                {
                                    cmbMagaza.Items.Add(officeDescription);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bağlantı hatası: " + ex.Message);
                }
            }
            DepoGetir();
        }

        private void DepoGetir()
        {
            string selectedOfficeCode = (comboBox1.SelectedItem as dynamic)?.Value?.ToString();
            if (string.IsNullOrEmpty(selectedOfficeCode))
                return;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string sql = @"
                        SELECT DISTINCT wd.WarehouseDescription
                        FROM cdWarehouseDesc wd
                        INNER JOIN cdWarehouse w ON wd.WarehouseCode = w.WarehouseCode
                        INNER JOIN cdOfficeDesc od ON w.OfficeCode = od.OfficeCode
                        WHERE od.OfficeCode = @OfficeCode";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@OfficeCode", selectedOfficeCode);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            cmbDepo.Items.Clear();
                            while (reader.Read())
                            {
                                string warehouseDescription = reader["WarehouseDescription"].ToString();
                                cmbDepo.Items.Add(warehouseDescription);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bağlantı hatası: " + ex.Message);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            MagazaGetir();
        }

        private void cmbMagaza_SelectedIndexChanged(object sender, EventArgs e)
        {
            DepoGetir();
        }

        public void LoadCustomersIntoComboBox()
        {
            string query = "SELECT cdCurrAccDesc.CurrAccCode, cdCurrAccDesc.CurrAccDescription " +
                           "FROM cdCurrAcc " +
                           "INNER JOIN cdCurrAccDesc ON cdCurrAcc.CurrAccCode = cdCurrAccDesc.CurrAccCode " +
                           "AND cdCurrAcc.CurrAccTypeCode = cdCurrAccDesc.CurrAccTypeCode " +
                           "WHERE cdCurrAcc.OfficeCode = 'M' " +
                           "AND cdCurrAcc.CurrAccTypeCode = '1'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    cmbCari.Items.Clear();

                    while (reader.Read())
                    {
                        cmbCari.Items.Add(reader["CurrAccDescription"].ToString());
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message);
                }
            }

            cmbCari.DropDownStyle = ComboBoxStyle.DropDown;
            cmbCari.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbCari.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

       

        private void cmbDepo_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            string selectedWarehouseCode = cmbDepo.SelectedItem?.ToString();
            string barcode = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(selectedWarehouseCode))
                return;

            string query;

            if (string.IsNullOrEmpty(barcode))
            {
                // Barkod girilmediğinde tüm benzersiz barkodları listele
                query = @"
            SELECT DISTINCT
                prItemBarcode.Barcode
            FROM 
                cdItemDesc
            INNER JOIN 
                prItemBarcode 
                ON cdItemDesc.ItemCode = prItemBarcode.ItemCode 
                AND cdItemDesc.ItemTypeCode = prItemBarcode.ItemTypeCode
            INNER JOIN 
                trStock 
                ON cdItemDesc.ItemCode = trStock.ItemCode
            INNER JOIN 
                cdWarehouse 
                ON trStock.WarehouseCode = cdWarehouse.WarehouseCode
            INNER JOIN 
                trPriceListLine 
                ON cdItemDesc.ItemCode = trPriceListLine.ItemCode
            WHERE 
                cdWarehouse.WarehouseCode = '1-2-8'";
            }
            else
            {
                // Barkod girildiğinde sadece ilgili barkodları ve ürün bilgilerini listele
                query = @"
            SELECT DISTINCT
                prItemBarcode.Barcode,
                cdItemDesc.ItemDescription,
                trPriceListLine.Price
            FROM 
                cdItemDesc
            INNER JOIN 
                prItemBarcode 
                ON cdItemDesc.ItemCode = prItemBarcode.ItemCode 
                AND cdItemDesc.ItemTypeCode = prItemBarcode.ItemTypeCode
            INNER JOIN 
                trStock 
                ON cdItemDesc.ItemCode = trStock.ItemCode
            INNER JOIN 
                cdWarehouse 
                ON trStock.WarehouseCode = cdWarehouse.WarehouseCode
            INNER JOIN 
                trPriceListLine 
                ON cdItemDesc.ItemCode = trPriceListLine.ItemCode
            WHERE 
                prItemBarcode.Barcode = @Barcode
                AND cdWarehouse.WarehouseCode = '1-2-8'";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@WarehouseCode", selectedWarehouseCode);

                    if (!string.IsNullOrEmpty(barcode))
                    {
                        command.Parameters.AddWithValue("@Barcode", barcode);
                    }

                    connection.Open();

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);

                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dataGridView1.Columns.Clear();
                    dataGridView1.DataSource = dataTable;

                    // Set column header text
                    if (dataTable.Columns.Contains("Barcode"))
                    {
                        dataGridView1.Columns["Barcode"].HeaderText = "BARKOD";
                    }
                    if (dataTable.Columns.Contains("ItemDescription"))
                    {
                        dataGridView1.Columns["ItemDescription"].HeaderText = "ÜRÜN ADI";
                    }
                    if (dataTable.Columns.Contains("Price"))
                    {
                        dataGridView1.Columns["Price"].HeaderText = "FİYAT";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }

        private void cmbMagaza_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
    }

   //barkodu girdiğimizde ürün ve ürün açıklaması gelecek