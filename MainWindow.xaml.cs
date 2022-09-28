using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using BankAcc;

using Functions;
using System.Data.SqlClient;
using System.Data;

namespace Bank
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        BankRepository Sber;
        public MainWindow()
        {
            InitializeComponent();
            Sber = new BankRepository();
            ListBoxLog.ItemsSource = BankRepository.Logs;
            DataGridClients.DataContext = Sber.dt.DefaultView;
            ComboBoxClients.DataContext = Sber.dt.DefaultView;
            BankRepository.log+=e => BankRepository.Logs.Add(e);
        }
                
        /// <summary>
        /// Добавление клиента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       private void ButtonAddClient_Click(object sender, RoutedEventArgs e)
        {
         
            try
            {
                if (string.IsNullOrEmpty(TexBoxAccValue.Text) || string.IsNullOrEmpty(TextBoxName.Text) || string.IsNullOrEmpty(TextBoxPhone.Text) ||
                    string.IsNullOrEmpty(ComboBoxStatus.Text)) throw new MyException();

                if (TextBoxName.Text.IsNumberContains()) throw new MyException(TextBoxName.Text, 1);
                if (!TextBoxPhone.Text.IsNumberContains()) throw new MyException(TextBoxPhone.Text, 2);

                int AccValue = Convert.ToInt32(TexBoxAccValue.Text);

                if (AccValue < 0) throw new FormatException();
                int IdType = 0;
                switch (ComboBoxStatus.Text)
                {
                    case "Физическое лицо":IdType=1; break;
                    case "Физическое лицо(VIP)": IdType = 2; break;
                    case "Юридическое лицо": IdType = 3; break;
                }
                Sber.FillingClient(AccValue, TextBoxName.Text, TextBoxPhone.Text, IdType);
            }

            catch (MyException error) when (error.Code == 2)
            {
                MessageBox.Show($"Ошибка в телефоне клиента ");
                Debug.WriteLine(error);
            }
            catch (MyException error) when (error.Code == 1)
            {
                MessageBox.Show($"Ошибка в имени клиента {error.Message}");
                Debug.WriteLine(error);
            }
            catch (FormatException error)
            {
                MessageBox.Show($"Ошибка ввода счёта клиента");
                Debug.WriteLine(error);
            }
            catch (MyException error)
            {
                MessageBox.Show($"Заполните все строки");
                Debug.WriteLine(error);
            }
            catch (Exception error)
            {
                MessageBox.Show($"Ошибка ввода");
                Debug.WriteLine(error);
            }



        }
             
        /// <summary>
        /// Вклад
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOpenDeposit_Click(object sender, RoutedEventArgs e)
        {
           
            try
            {
                Sber.row = (DataRowView)DataGridClients.SelectedItem;
                if ((string.IsNullOrEmpty(TexBoxMoney.Text)) || !TexBoxMoney.Text.IsNumberContains()) throw new MyException();
                int money = Convert.ToInt32(TexBoxMoney.Text);
                int AccValue=Convert.ToInt32(Sber.row.Row["AccValue"]);
                if (money > AccValue || money <= 0)
                {
                    MessageBox.Show("Введите сумму вклада в пределах счёта клиента!");

                }
                else
                {
                    double percent;

                    switch (Convert.ToInt32(Sber.row.Row["IdType"]))
                    {
                        case 1: percent = 0.1; break;
                        case 2: percent = 0.05; break;
                        default: percent = 0.12; break;

                    }

                    Sber.row.Row["Deposit"] = (money * percent) + money;
                    AccValue -= money;
                    Sber.row.Row["AccValue"] =AccValue;
                    Sber.da.Update(Sber.dt);
                    MessageBox.Show($"Вклад открыт под {percent * 100}% годовых");

                    BankRepository.Logs.Add($"Открытие вклада: ID: {Sber.row.Row["Id"]} ");
                }
               
            }
            catch (MyException error)
            {
                MessageBox.Show($"Неверный формат денежного вклада");
                Debug.WriteLine(error);
            }
            catch (NullReferenceException error)
            {
                MessageBox.Show($"Выберите клиента");
                Debug.WriteLine(error);
            }

        }
        /// <summary>
        /// Перевод
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                DataRowView row1 = (DataRowView)DataGridClients.SelectedItem;
                DataRowView row2 = (DataRowView)ComboBoxClients.SelectedItem;
                

                if ((string.IsNullOrEmpty(TexBoxSend.Text)) || !TexBoxSend.Text.IsNumberContains()) throw new MyException();
                int money = Convert.ToInt32(TexBoxSend.Text);
                
                row1["AccVAlue"]= Convert.ToInt32(row1["AccValue"] )- money;
                row2["AccVAlue"] = Convert.ToInt32(row2["AccValue"]) + money;
                Sber.da.Update(Sber.dt);

                BankRepository.Logs.Add($"Перевод от: ID: {row1["Id"]} ");
                BankRepository.Logs.Add($"Перевод кому: ID: {row2["Id"]} ");

            }
            catch (NullReferenceException error)
            {
                MessageBox.Show($"Выберите кому и куда переводить");
                Debug.WriteLine(error);
            }
            catch (MyException error)
            {
                MessageBox.Show($"Неверный формат денежного перевода");
                Debug.WriteLine(error);
            }


        }

        /// <summary>
        /// Начало редактирование записи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GVCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

            Sber.row = (DataRowView)DataGridClients.SelectedItem;
           Sber. row.BeginEdit();
            Sber.da.Update(Sber.dt);
        }
        /// <summary>
        /// Редактирование записи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GVCurrentCellChanged(object sender, EventArgs e)
        {
            if (Sber.row == null) return;
            Sber.row.EndEdit();
            Sber.da.Update(Sber.dt);
        }
        /// <summary>
        /// Удаление
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Sber.row = (DataRowView)DataGridClients.SelectedItem;
            BankRepository.Logs.Add($"Удаление клиента: ID: {Sber.row["Id"]} ");
            Sber.row.Row.Delete();
            Sber.da.Update(Sber.dt);
           
        }
              

    }
}