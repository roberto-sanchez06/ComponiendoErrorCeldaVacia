﻿
using AppCore.Interfaces;
using Domain.Entities.Empleados;
using Domain.Enums.CargosEmpleados;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace NominasTrabajo
{
	
	public partial class FrmEmpleado : Form
	{
		public IEmpleadoService empleadoService { get; set; }
		public FrmEmpleado()
		{
			InitializeComponent();
		}		
		private void pictureBox1_Click_1(object sender, EventArgs e)
		{
			this.Close();
		}

		private void pictureBox3_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void FrmEmpleado_Load(object sender, EventArgs e)
		{
			cmbCargos.Items.AddRange(Enum.GetValues(typeof(Cargos)).Cast<object>().ToArray());
			
		}
		public Image byteArrayToImage(byte[] byteArrayIn)
		{
			MemoryStream ms = new MemoryStream(byteArrayIn);
			Image returnImage = Image.FromStream(ms);
			return returnImage;
		}



		private void PBImagen_Click(object sender, EventArgs e)
		{
			OpenFileDialog result = new OpenFileDialog();
			result.Title = "Open Image";
			result.Filter = "Archivo JPG (*.jpg)|*.jpg| Archivo PNG (*.png)|*.png| Archivo BMP (*.bmp)|*bmp";
			if (result.ShowDialog() == DialogResult.OK)
			{
				PBImagen.SizeMode = PictureBoxSizeMode.Zoom;
				PBImagen.Image = Image.FromFile(result.FileName);
			}
			result.Dispose();
		}

		private void pbWaves_Click(object sender, EventArgs e)
		{
			
		}

		private void txtNombre__TextChanged(object sender, EventArgs e)
		{

		}

		private void btnAgregar_Click(object sender, EventArgs e)
		{
            try
            {
				verificarDatos(txtNombre.Texts, txtSalario.Texts, txtCodigoInss.Texts, txtHorasTrabajadas.Texts);
                Remuneraciones rem = new Remuneraciones()
                {
                    SalarioBase = decimal.Parse(txtSalario.Texts)
                };
                Empleado empleado = new Empleado(txtNombre.Texts, rem, txtCodigoInss.Texts, int.Parse(txtHorasTrabajadas.Texts))
                {
                    Cargos = (Cargos)cmbCargos.SelectedIndex,
                    Id = empleadoService.GetLastId() + 1
                };
                empleadoService.Create(empleado);
                Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
		}
		private void verificarDatos(string nombre, string salario, string noINSS, string hrs)
        {
			if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(noINSS) || string.IsNullOrEmpty(salario) || string.IsNullOrEmpty(hrs) || cmbCargos.SelectedIndex==-1)
            {
				throw new ArgumentException("Hay campos vacios, rellenelos por favor");
            }
            if (noINSS.Length != 8)
            {
				throw new ArgumentException("El numero del INSS no puede tener menos o mas de 8 digitos");
            }
            if (int.Parse(hrs) < 240)
            {
				throw new ArgumentException("No se puede trabajar menos de 240 horas al mes");
            }
            if (decimal.Parse(salario) <= 0)
            {
				throw new ArgumentException("Un trabajador no puede ganar eso");
            }
        }

        private void txtHorasTrabajadas_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                MessageBox.Show("Solo se pueden colocar numeros enteros","ERROR",MessageBoxButtons.OK,MessageBoxIcon.Error);
				e.Handled = true;
			}
        }

        private void txtSalario_KeyPress(object sender, KeyPressEventArgs e)
        {
			//poner validacion de decimales
			if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
			{
				MessageBox.Show("Solo se pueden colocar numeros", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
				e.Handled = true;
			}
		}

        private void txtCodigoInss_KeyPress(object sender, KeyPressEventArgs e)
        {
			if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
			{
				MessageBox.Show("Solo se pueden colocar numeros enteros", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
				e.Handled = true;
			}
		}
    }

}