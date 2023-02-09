using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _000
{
    public partial class MainWindow : Window
    {
        private readonly string ENCTAG = "DES0208HuKRoa52R1DjK0g00t388c5e696v12ndrv55>>>";
        private string _path = "";
        private string _content = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Pat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Text files (*.txt)|*.txt";
                openFileDialog.ShowDialog();
                _path = openFileDialog.FileName;

                if (_path.Length > 0)
                {
                    btn_pat.Content = _path;
                    _content = File.ReadAllText(_path);
                }
                else
                {
                    btn_pat.Content = "PAT";
                }
                SetPrecessAvailability();
            }
            catch (Exception)
            {
                ActivateErrorMode();
            }
        }

        private void SetEncDecButtonPositions(bool enc, bool dec)
        {
            if (enc && dec)
                return;
            btn_enc.IsEnabled = enc;
            btn_dec.IsEnabled = dec;
        }

        private bool IsContentEncrypted()
        {
            return _content.Length >= ENCTAG.Length && _content.Substring(0, ENCTAG.Length).Equals(ENCTAG);
        }

        private void ActivateErrorMode()
        {
            SetEncDecButtonPositions(false, false);
            btn_pat.Content = "ERROR";
        }

        private void Enc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(_path, ENCTAG + Encrypt(_content, tb_pw.Password));
                _content = File.ReadAllText(_path);
                SetEncDecButtonPositions(false, true);
            }
            catch (Exception)
            {
                ActivateErrorMode();
            }
        }

        private void Dec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(_path, Decrypt(_content.Remove(0, ENCTAG.Length), tb_pw.Password));
                _content = File.ReadAllText(_path);
                SetEncDecButtonPositions(true, false);
            }
            catch (Exception)
            {
                ActivateErrorMode();
            }
        }

        private void SetPrecessAvailability()
        {
            try
            {
                if (tb_pw.Password.Length >= 4 && tb_pw.Password.Length <= 32 && _path.Length > 0)
                {
                    tb_pw.Background = ToSolidColorBrush("#CDE990");
                    lb_pw.Background = ToSolidColorBrush("#CDE990");
                    bool isEncrypted = IsContentEncrypted();
                    SetEncDecButtonPositions(!isEncrypted, isEncrypted);
                }
                else
                {
                    tb_pw.Background = ToSolidColorBrush("#FFCAD4");
                    lb_pw.Background = ToSolidColorBrush("#FFCAD4");
                    SetEncDecButtonPositions(false, false);
                }
            }
            catch (Exception)
            {
                ActivateErrorMode();
            }
        }

        private SolidColorBrush ToSolidColorBrush(string hex_code)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(hex_code);
        }

        private string Encrypt(string text, string password)
        {
            byte[] src = Encoding.UTF8.GetBytes(text);
            byte[] key = Encoding.ASCII.GetBytes(password32(password));
            using RijndaelManaged aes = new()
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
                KeySize = 256
            };
            using ICryptoTransform encrypt = aes.CreateEncryptor(key, null);
            byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);
            encrypt.Dispose();
            return Convert.ToBase64String(dest);
        }

        private string Decrypt(string text, string password)
        {
            byte[] src = Convert.FromBase64String(text);
            byte[] key = Encoding.ASCII.GetBytes(password32(password));
            using RijndaelManaged aes = new()
            {
                KeySize = 256,
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.ECB
            };
            using ICryptoTransform decrypt = aes.CreateDecryptor(key, null);
            byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
            decrypt.Dispose();
            return Encoding.UTF8.GetString(dest);
        }

        private string password32(string password)
        {
            return password.PadLeft(32, '0');
        }

        private void tb_pw_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SetPrecessAvailability();
        }
    }
}
