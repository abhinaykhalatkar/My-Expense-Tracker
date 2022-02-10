
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using My_Expense_Tracker.Properties;
using System.IO;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Xml.Linq;





namespace My_Expense_Tracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string dat = DateTime.Now.Date.ToString();
            SeriesCollection = new LiveCharts.SeriesCollection(); 
            string queryStartDate= dat.Substring(6, 4) + "-" + dat.Substring(3, 2) + "-" + "01" + " 00:00:00:000";
            string lastDay = DateTime.DaysInMonth(Int32.Parse(dat.Substring(6, 4)), Int32.Parse (dat.Substring(3, 2))).ToString();
            string queryEndDate= dat.Substring(6, 4) + "-" + dat.Substring(3, 2) + "-" + lastDay + " 00:00:00:000";
            populateUsingId("searchByDateLoad", "select * from expDB4 where Date between '" + queryStartDate + "' and '" + queryEndDate + "'");


        }
        public LiveCharts.SeriesCollection SeriesCollection { get; set; }
        SqlConnection con = new SqlConnection(@"Data Source=ABHINAY-WORKSTA;Initial Catalog=expenseTrackerDB;Integrated Security=True");
        SqlCommand cmd;
        SqlDataReader dr;

        public bool isValid()
        {
            if (expenseInputTbx.Text == string.Empty || costInputTbx.Text == string.Empty || typeCbx.Text == string.Empty)
            {
                MessageBox.Show("Enter data in all fields.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }



        public void populateUsingId(string popType, string query = "select * from expDB4", int id = -2)
        {
            cmd = new SqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = query;
            dr = cmd.ExecuteReader();
            itemIdLbx.Items.Clear();
            itemNameLbx.Items.Clear();
            itemCostLbx.Items.Clear();
            if (popType == "addLoad")
            {

                while (dr.Read())
                {
                    itemIdLbx.Items.Add(dr["ID"]);
                    itemNameLbx.Items.Add(dr["Name"]);
                    itemCostLbx.Items.Add(dr["Cost"]);
                }


            }
            if (popType == "searchByDateLoad")
            {

                while (dr.Read())
                {
                    itemIdLbx.Items.Add(dr["ID"]);
                    itemNameLbx.Items.Add(dr["Name"]);
                    itemCostLbx.Items.Add(dr["Cost"]);
                }



            }
            con.Close();

            allCost();


        }
        public void allCost()
        {

            int sum = 0;
            foreach (string x in itemCostLbx.Items)
            {
                sum = sum + Int32.Parse(x);
            }
            totalTbx.Text = sum.ToString();
        }




        private void singleDayRbtn_Click(object sender, RoutedEventArgs e)
        {
            multiDayPanel.Visibility = Visibility.Collapsed;
            singleDayPanel.Visibility = Visibility.Visible;
            goBtn.Visibility = Visibility.Collapsed;

        }


        private void singleDayPanel_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)

        {
            itemIdLbx.Items.Clear();
            itemNameLbx.Items.Clear();
            itemCostLbx.Items.Clear();

            
            populateUsingId("searchByDateLoad", "SELECT * FROM expDB4 WHERE date = '"+sqlDateConv(singleDayPanel.SelectedDate.Value.ToString()) + "'");
        }

        public string sqlDateConv(string dat)
        {
            return dat.Substring(6, 4) + "-" + dat.Substring(3, 2) + "-" + dat.Substring(0, 2) + " 00:00:00:000";
        }

        private void multiDayRbtn_Click(object sender, RoutedEventArgs e)
        {
            multiDayPanel.Visibility = Visibility.Visible;
            singleDayPanel.Visibility = Visibility.Collapsed;
            goBtn.Visibility = Visibility.Visible;

        }
        private void goBtn_Click(object sender, RoutedEventArgs e)
            
        {
            try {
                if (dateFrom.SelectedDate == null)
                {
                    MessageBox.Show("Please Enter date from");
                }
                if (dateTo.SelectedDate == null)
                {
                    MessageBox.Show("Please Enter date to");
                }
                else { populateUsingId("searchByDateLoad", "select * from expDB4 where Date between '" + sqlDateConv(dateFrom.SelectedDate.Value.ToString()) + "' and '" + sqlDateConv(dateTo.SelectedDate.Value.ToString()) + "'"); }

            }
            catch(Exception ex) {
            MessageBox.Show(ex.Message);
            }

        }

        private void itemAddBtn_Click(object sender, RoutedEventArgs e)
        {


            if (isValid())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO expDB4 VALUES (@Name,@Type,@Date,@Cost)", con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Name", expenseInputTbx.Text);

                    cmd.Parameters.AddWithValue("@Type", typeCbx.Text);

                    cmd.Parameters.AddWithValue("@Date", addItemDate.SelectedDate.GetValueOrDefault(DateTime.Today));
                    cmd.Parameters.AddWithValue("@Cost", costInputTbx.Text);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    populateUsingId("addLoad", "select * from expDB4");
                    clearTbx();

                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            int id = itemNameLbx.SelectedIndex;
            if (id == -1)
            {
                id = 0;
            }
            object id2 = itemIdLbx.Items.GetItemAt(id);

            try
            {

                con.Open();
                SqlCommand cmd = new SqlCommand("delete from expDB4 where ID= " + id2 + " ", con);
                cmd.ExecuteNonQuery();
                con.Close();
                populateUsingId("addLoad", "select * from expDB4");
                

            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                con.Close();
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            itemAddBtn.IsEnabled = true;
        }

        private void costInputTbx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

        }
        
        private void expenseInputTbx_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (expenseInputTbx.Text == "Enter Expense Name")
            {
                expenseInputTbx.Text = "";
                itemAddBtn.IsEnabled = true;
            }

        }

        private void expenseInputTbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (expenseInputTbx.Text == "")
            {
                expenseInputTbx.Text = "Enter Expense Name";
                itemAddBtn.IsEnabled = false;
            }

        }
        public void clearTbx()
        {
            expenseInputTbx.Text = "";
            costInputTbx.Text = "";
            customTypeTbx.Text = "";

        }

        private void GridView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void allItemBtn_Click(object sender, RoutedEventArgs e)
        {
            populateUsingId("addLoad", "select * from expDB4");
        }

        private void piechart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            populateUsingId("searchByDateLoad", "SELECT* FROM expDB4 WHERE CONCAT_WS('', Name, Type) LIKE '%" + searchBox.Text + "%'");
            
        }

        private void itemNameLbx_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                int id = itemNameLbx.SelectedIndex;
                if (id == -1)
                {
                    id = 0;
                }
                object id2 = itemIdLbx.Items.GetItemAt(id);

                try
                {

                    con.Open();
                    SqlCommand cmd = new SqlCommand("delete from expDB4 where ID= " + id2 + " ", con);
                    cmd.ExecuteNonQuery();
                    con.Close();
                    populateUsingId("addLoad", "select * from expDB4");


                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                MessageBox.Show("select an item to delete.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
