using System;
using System.Windows.Forms;
using DataRepository;
using Domain;

namespace WH
{
    public partial class Form1 : Form
    {
        public WareProjectDBDataSet WPDS;

        public Form1()
        {
            WPDS = wareProjectDBDataSet;
            InitializeComponent();
        }

       

        private void button1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Not yet");
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.button2.Text == "Создать продукт")
            {
                AddProductForm apf = new AddProductForm();
                apf.ShowDialog();
                if (apf.DialogResult == DialogResult.OK)
                {
                    try { 
                    this.productsTableAdapter1.Insert(apf.prod.name, apf.prod.type.ToString(), apf.prod.volume, apf.prod.ID);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
            
                }
            }
            else {
                try { 
                Product p = new Product();
                p.ID = 1;
                //p.name = this.productsTableAdapter1.getProductName((int)dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[3].Value);
                //p.volume = float.Parse((string)this.productsTableAdapter1.getVolume(p.ID,p.name));
                AddProductForm apf = new AddProductForm(p);
                apf.ShowDialog();
                    if (apf.DialogResult == DialogResult.OK)
                    {
                        this.productsTableAdapter1.Update(WPDS);

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try { 
            //CapacityLabel.Text += this.productsTableAdapter1.totalVolume().ToString();

            this.button4.Enabled = false;
            this.button3.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Form1_Activated(object sender, EventArgs e)
        {

            dataGridView1.Refresh();
            dataGridView2.Refresh();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
         

                this.masterTableAdapter1.Fill(this.wareProjectDBDataSet.master);

                // TODO: данная строка кода позволяет загрузить данные в таблицу "wareProjectDBDataSet.products". При необходимости она может быть перемещена или удалена.
                this.productsTableAdapter1.Fill(this.wareProjectDBDataSet.products);

                this.button4.Enabled = true;
                this.button3.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = true;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try { 
            this.masterTableAdapter1.Dispose();
            this.productsTableAdapter1.Dispose();
            this.wareProjectDBDataSet.products.Clear();
            this.wareProjectDBDataSet.master.Clear();

            this.button4.Enabled = false;
            this.button3.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                MessageBox.Show("Выйти?", "Вы уверенны?", MessageBoxButtons.YesNo);
                
            }
            else if (e.CloseReason == CloseReason.ApplicationExitCall)
            {
                return;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            button2.Text = "Изменить продукт";
        }

        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            

        }

        private void panel2_MouseClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                button2.Text = "Изменить продукт";
            }
            else button2.Text = "Создать продукт";
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                this.productsTableAdapter1.DeleteQuery(dataGridView1.SelectedRows[0].Cells["name"].Value.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
