using System;
using System.Windows.Forms;
using Domain;

namespace WH
{
    public partial class AddProductForm : Form
    {
        public Product prod;
        string editMode;

        public AddProductForm()
        {
            InitializeComponent();
            prod = new Product();
            editMode = "create";
        }

        public AddProductForm(Product p)
        {
            prod = p;
            InitializeComponent();
            editMode = "edit";
        }
        private void AddProductForm_Load(object sender, EventArgs e)
        {
            if (editMode == "edit")
            {
                textBox1.Text = prod.name;
                textBox2.Text = prod.volume.ToString();
                comboBox1.Text = prod.type.ToString();

            }
            comboBox1.Items.Add(TypeOfGoods.eatable.ToString());
            comboBox1.Items.Add(TypeOfGoods.noteatable.ToString());
            label4.Text += prod.ID.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                prod.name = textBox1.Text;
                prod.volume = float.Parse(textBox2.Text);
                if (comboBox1.SelectedItem.ToString() == TypeOfGoods.eatable.ToString())
                {
                    prod.type = TypeOfGoods.eatable;
                }
                else if (comboBox1.SelectedItem.ToString() == TypeOfGoods.noteatable.ToString())
                {
                    prod.type = TypeOfGoods.noteatable;
                }
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                

                this.Dispose();
            }
            catch (Exception) { }
        }
    }
}
