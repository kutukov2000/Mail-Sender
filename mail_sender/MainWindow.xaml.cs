using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
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
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace mail_sender
{
    [AddINotifyPropertyChangedInterface]
    class ViewModel
    {
        private ObservableCollection<Attachment> attachments;
        public IEnumerable<Attachment> Attachments => attachments;
        public string myMailAddress { get; set; }
        public string accountPassword { get; set; }
        public bool HighPriority { get; set; }
        public ViewModel()
        {
            myMailAddress = "Login";
            HighPriority = false;
            attachments = new ObservableCollection<Attachment>();
        }
        public void AddAttachment(string fileName)
        {
            attachments.Add(new Attachment(fileName));
        }
        public void RemoveAttachment(string fileName)
        {
            Attachment attachmentToRemove = attachments.FirstOrDefault(a => a.Name == fileName);
            if (attachmentToRemove != null)
            {
                attachments.Remove(attachmentToRemove);
            }
        }
        public void Clear()
        {
            attachments.Clear();
            HighPriority = false;
        }

    }
    public partial class MainWindow : Window
    {
        ViewModel model = new ViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = model;

            attachmentsList.Items.Clear();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateEmail(toTxtBox.Text)&&toTxtBox.Text.Length!=0)
            {
                MessageBox.Show("Incorrect recceiver email");
                return;
            }
            try
            {
                // create new mail
                MailMessage mail = new MailMessage(model.myMailAddress, toTxtBox.Text)
                {
                    IsBodyHtml = true,

                    Subject = subjectTxtBox.Text,
                    Priority = model.HighPriority == true ? MailPriority.High : MailPriority.Normal
                };

                //add attachments
                foreach (var item in model.Attachments)
                {
                    mail.Attachments.Add(item);
                }

                // send mail message
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(model.myMailAddress, model.accountPassword),
                    EnableSsl = true
                };

                client.Send(mail);
            }
            catch (Exception ex)
            {

            }
            model.Clear();
            ClearTextBox();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow window = new LoginWindow();
            if (window.ShowDialog() == true)
            {
                model.myMailAddress = window.Login.Text;
                model.accountPassword = window.Password.Password;
            }
        }

        private void AddAttachment_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
                model.AddAttachment(dialog.FileName);
        }

        private void RemoveAttachment_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            Attachment attachment = (Attachment)button.DataContext;

            model.RemoveAttachment(attachment.Name);
        }
        public void ClearTextBox()
        {
            toTxtBox.Clear();
            subjectTxtBox.Clear();
            bodyTxtBox.Clear();
        }
        private bool ValidateEmail(string email)
        {
            // Regex pattern for email validation
            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

            bool isValid = Regex.IsMatch(email, pattern);

            return isValid;
        }
    }
}
