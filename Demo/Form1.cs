using CardReaderLib;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo
{
    public partial class Form1 : Form
    {
        private CardReader _reader;
        public Form1()
        {
            InitializeComponent();

            _reader = new CardReader();
            _reader.Input += (s, e) =>
            {
                textBox1.Text = s.DevicePath;
                textBox2.Text = s.ManufacturerName;
                textBox3.Text = s.VendorId.ToString();
                textBox4.Text = s.ProductId.ToString();
            };

            //讀卡號事件
            _reader.ReadCardNumber += (s, cardNumber) =>
            {
                textBox5.Text = cardNumber;
            };

            listBox1.DisplayMember = "DevicePath";
        }

        private void UpdateListBox()
        {
            listBox1.Items.Clear();

            foreach (var item in _reader.AllowedDevices)
            {
                listBox1.Items.Add(item);
            }
        }

        private void AddToAllowedList(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                return;
            }

            //加到白名單
            _reader.AddInputDevice(_reader.CurrentDevice);

            UpdateListBox();
        }

        private void RemoveFromAllowedList(object sender, EventArgs e)
        {
            if(listBox1.SelectedItem == null)
            {
                return;
            }

            _reader.RemoveInputDevice((InputDevice)listBox1.SelectedItem);

            UpdateListBox();
        }
    }
}
