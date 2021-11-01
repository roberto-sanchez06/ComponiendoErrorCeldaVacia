﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AppCore.Interfaces;
using AppCore.Services;
using Autofac;
using Domain.Interfaces;
using Infraestructure.EmpleadosRepos;
using Infraestructure.Repository;

namespace NominasTrabajo
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<EmpleadoListRepository>().As<IEmpleadoRepository>();
			builder.RegisterType<EmpleadoService>().As<IEmpleadoService>();

			builder.RegisterType<ProcessesEmpleadosRepository>().As<IProcessesEmpleado>();
			builder.RegisterType<ProcessesEmpleadoServices>().As<IProcesses>();

			builder.RegisterType<ProcessesEmpresaRepository>().As<IProcessesEmpresa>();
			builder.RegisterType<ProcessesEmpresaServices>().As<IEmpresaService>();

			var container = builder.Build();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FrmLogin(container.Resolve<IEmpleadoService>(), container.Resolve<IEmpleadoService>(), container.Resolve<IProcesses>(), container.Resolve<IEmpresaService>()));
		}
	}
}
