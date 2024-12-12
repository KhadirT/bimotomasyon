using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BimAutomation
{
    public partial class Form1 : Form
    {
        // Bağlantı cümlesi
        private string connectionString = "Server=localhost;Database=BimAutomationDB;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ProductID, ProductName FROM Products";
                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                cmbProducts.DataSource = dt;
                cmbProducts.DisplayMember = "ProductName";
                cmbProducts.ValueMember = "ProductID";
            }
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            // Ürün ekleme ve stok güncelleme işlemi
            int productID = (int)cmbProducts.SelectedValue;
            int quantity = int.Parse(txtQuantity.Text);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string checkStockQuery = "SELECT StockQuantity FROM Products WHERE ProductID = @ProductID";
                SqlCommand cmd = new SqlCommand(checkStockQuery, conn);
                cmd.Parameters.AddWithValue("@ProductID", productID);
                int stockQuantity = (int)cmd.ExecuteScalar();

                if (stockQuantity >= quantity)
                {
                    // Stok güncelleniyor
                    string updateStockQuery = "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE ProductID = @ProductID";
                    SqlCommand updateCmd = new SqlCommand(updateStockQuery, conn);
                    updateCmd.Parameters.AddWithValue("@Quantity", quantity);
                    updateCmd.Parameters.AddWithValue("@ProductID", productID);
                    updateCmd.ExecuteNonQuery();

                    // Satış işlemi kaydediliyor
                    string insertSaleQuery = "INSERT INTO Sales (ProductID, QuantitySold, SaleDate, TotalPrice) VALUES (@ProductID, @Quantity, @SaleDate, @TotalPrice)";
                    SqlCommand saleCmd = new SqlCommand(insertSaleQuery, conn);
                    saleCmd.Parameters.AddWithValue("@ProductID", productID);
                    saleCmd.Parameters.AddWithValue("@Quantity", quantity);
                    saleCmd.Parameters.AddWithValue("@SaleDate", DateTime.Now);
                    saleCmd.Parameters.AddWithValue("@TotalPrice", quantity * decimal.Parse(lblTotalPrice.Text));
                    saleCmd.ExecuteNonQuery();

                    MessageBox.Show("Ürün satıldı ve stok güncellendi!");
                }
                else
                {
                    MessageBox.Show("Yeterli stok yok!");
                }
            }
        }

        private void btnGenerateReceipt_Click(object sender, EventArgs e)
        {
            // Fiş oluşturuluyor
            int productID = (int)cmbProducts.SelectedValue;
            int quantity = int.Parse(txtQuantity.Text);
            decimal totalAmount = decimal.Parse(txtTotalAmount.Text);
            string paymentMethod = txtPaymentMethod.Text;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string insertReceiptQuery = "INSERT INTO Receipts (ReceiptNumber, ReceiptDate, TotalAmount, PaymentMethod) VALUES (@ReceiptNumber, @ReceiptDate, @TotalAmount, @PaymentMethod)";
                SqlCommand cmd = new SqlCommand(insertReceiptQuery, conn);
                cmd.Parameters.AddWithValue("@ReceiptNumber", "FIS" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                cmd.Parameters.AddWithValue("@ReceiptDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                cmd.ExecuteNonQuery();

                // Fiş detayları ekleniyor
                string insertReceiptDetailQuery = "INSERT INTO ReceiptDetails (ReceiptID, ProductID, Quantity, UnitPrice, TotalPrice) VALUES (@ReceiptID, @ProductID, @Quantity, @UnitPrice, @TotalPrice)";
                SqlCommand detailCmd = new SqlCommand(insertReceiptDetailQuery, conn);
                detailCmd.Parameters.AddWithValue("@ReceiptID", cmd.ExecuteScalar());
                detailCmd.Parameters.AddWithValue("@ProductID", productID);
                detailCmd.Parameters.AddWithValue("@Quantity", quantity);
                detailCmd.Parameters.AddWithValue("@UnitPrice", decimal.Parse(lblTotalPrice.Text));
                detailCmd.Parameters.AddWithValue("@TotalPrice", quantity * decimal.Parse(lblTotalPrice.Text));
                detailCmd.ExecuteNonQuery();

                MessageBox.Show("Fiş oluşturuldu!");
            }
        }
    }
}
