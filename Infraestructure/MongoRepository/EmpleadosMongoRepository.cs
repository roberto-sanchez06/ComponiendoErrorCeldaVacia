using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppCore.Interfaces;
using Domain.Entities.Empleados;
using Domain.Enums;
using Domain.Interfaces;
using MongoDB.Driver;

namespace Infraestructure.MongoRepository
{
    public class EmpleadosMongoRepository : MongoBaseRepository<Empleado>, IEmpleadoRepository
    {
        private IMongoCollection<Empleado> empleadosDespedidos;
        private IMongoCollection<Empleado> despedidosDelMes;
        private IProcesses processes;
        private IEmpresaService empresaService;
        
        public EmpleadosMongoRepository() : base("Empleados")
        {
            empleadosDespedidos = db.GetCollection<Empleado>("EmpleadosDespedidos");
            empleadosDespedidos = db.GetCollection<Empleado>("DespedidosDelMes");
        }
        //No creo que funcione este metodo
        public void AumentarAntiguedad(Meses meses)
        {
            List<Empleado> empleados = (List<Empleado>)FindAll(1);
            foreach (Empleado a in empleados)
            {
                //le agregue esta linea, verificar si funciona bien
                a.MesesTrabajados++;
                if (a.Vacaciones.MesesTrabajadosVacaciones == 6)
                {
                    a.Vacaciones.MesesTrabajadosVacaciones = 1;
                }
                else
                {
                    a.Vacaciones.MesesTrabajadosVacaciones += 1;
                }
                if (meses == Meses.Enero)
                {
                    a.Aguinaldo.MesesTrabajadosAguinaldo = 1;
                }
                else
                {
                    a.Aguinaldo.MesesTrabajadosAguinaldo += 1;
                }

                if (a.Indemnizacion.MesesTrabajadosIndemnizacion == 12)
                {
                    a.Indemnizacion.MesesTrabajadosIndemnizacion = 1;
                    a.Indemnizacion.AñosTrabajadosIndemnizacion += 1;
                }
                else
                {
                    a.Indemnizacion.MesesTrabajadosIndemnizacion += 1;
                }

                if (a.Prestamo.MesesPrestamo == 0)
                {
                    a.Prestamo.Cuota_Prestamo = 0;
                }
                else
                {
                    a.Prestamo.MesesPrestamo -= 1;
                }
            }
        }

        public bool Delete(Empleado t)
        {
            return Delete(t, collection);
        }

        public bool Despedir(int id)
        {
            Empleado e = GetEmpleadoById(id);
            if (e == null)
            {
                throw new ArgumentException($"No se pudo despedir al empleado con id: {id}");
            }
            bool exitoso = Delete(e);
            //revisar si se va a actualizar con esta linea
            e.Estado = EstadoTrabajador.Inactivo;
            //esta linea la agregue
            e.MesesTrabajados = 1;
            Add(despedidosDelMes, e);
            Add(empleadosDespedidos, e);
            return exitoso;
        }

        public ICollection<Empleado> FindAll(int tipo)
        {
            switch (tipo)
            {
                case 1:
                    return FindAll();
                case 2:
                    return empleadosDespedidos.Find(x=>true).ToList(); 
                case 3:
                    List<Empleado> temp = new List<Empleado>(FindAll());
                    temp.AddRange(FindAll(2));
                    //Con la siguiente linea ya se ponen ordenados
                    ICollection<Empleado> temp2=temp.OrderBy(x => x.Id).ToList();
                    return temp2;
                case 4:
                    return despedidosDelMes.Find(x=>true).ToList();
                default:
                    throw new ArgumentException("No coloco una opcion valida");
            }
        }

        public Empleado GetEmpleadoById(int id)
        {
            return GetEmpleadoById(FindAll(), id);
        }
        //TODO: Este metodo deberia cambiar para recibir un IMongoCollection por
        //lo que podria pasar a ser privado, de todas maneras en el list repository
        //solo se ocupa pocas veces, aunque creo que de igual manera no afectaria mucho
        public Empleado GetEmpleadoById(ICollection<Empleado> empleados, int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"El id {id} es invalido");
            }
            if (empleados.Count == 0)
            {
                throw new ArgumentException("No hay empleados");
            }
            return empleados.ToList().Find(p => p.Id == id);
        }

        public EmpleadoDgv GetResumenEmpleado(int id, int mes)
        {
            Empleado[] Trabajadores = FindAll(3).ToArray();

            //TODO: No es necesario hacer esto porque el valor del INATEC no lo tiene que tener cada trabajador
            decimal SalarioTrabajadores = 0;

            foreach (Empleado a in Trabajadores)
                SalarioTrabajadores += a.Remuneraciones.SalarioBase;


            Empleado e = GetEmpleadoById(Trabajadores, id);
            if (e == null)
            {
                throw new ArgumentException($"No se puede dar un resumen del empleado con id: {id}");
            }

            throw new NotImplementedException();
        }

        public EmpleadoDgv[] GetResumenEmpleados(int mes)
        {
            throw new NotImplementedException();
        }

        public void QuitarDespedidos(Empleado empleado, int i)
        {
            if (i == 1)
            {
                empleado.Estado = EstadoTrabajador.Activo;
                Create(empleado);
                Delete(empleado, empleadosDespedidos);
            }
            else if (i == 2)
            {
                empleado.Estado = EstadoTrabajador.Activo;
                Delete(empleado, despedidosDelMes);
            }
        }

        public void QuitarDespedidosDelMes()
        {
            despedidosDelMes.DeleteMany(x=>true);
        }
        //Se puede hacer void el Update
        public int Update(Empleado t, int i)
        {
            if (t is null)
            {
                throw new ArgumentNullException(nameof(t));
            }
            if (i == 1)
            {
                if (FindAll(1).Count == 0)
                {
                    throw new ArgumentException("No hay empleados");
                }
                Update(collection, t);
                //return index;
            }
            else if (i == 2)
            {
                if (FindAll(2).Count == 0)
                {
                    throw new ArgumentException("No hay despedidos");
                }
                Update(empleadosDespedidos, t);
                //return index;
            }
            else if (i == 3)
            {
                if (FindAll(4).Count == 0)
                {
                    throw new ArgumentException("No hay despedidos");
                }
                Update(despedidosDelMes, t);
                //return index;
            }
            return 0;
        }
        public override int GetLastId()
        {
            int i= base.GetLastId();
            return i + 3;
        }
        //metodo para que entre con todos los datos calculados a la base de datos,
        //probar si se tiene que pasar por referencia el empleado
        private void HacerCalculos(Empleado e, int mes)
        {
            e.Remuneraciones.TotalIngresos = e.Remuneraciones.SalarioBase + processes.CalculateHorasExtras(e.Remuneraciones.HorasExtras, e.Remuneraciones.SalarioBase);
            e.Deducciones.INSSLaboral = processes.CalculateInss(e.Remuneraciones.TotalIngresos);
            e.Deducciones.IR = processes.CalculateIR(e.Remuneraciones.SalarioBase);
            e.Remuneraciones.IngresoHorasExtras = processes.CalculateHorasExtras(e.Remuneraciones.HorasExtras, e.Remuneraciones.SalarioBase);
            if (e.Estado == EstadoTrabajador.Inactivo && e.Prestamo.Cuota_Prestamo > 0)
            {
                e.Prestamo.Cuota_Prestamo = e.Prestamo.Cuota_Prestamo * e.Prestamo.MesesPrestamo;
                e.Prestamo.MesesPrestamo = 0;
            }
            if ((Meses)mes == Meses.Diciembre)
            {
                e.Aguinaldo.AguinaldoPago = processes.CalculateAguinaldo(e.Remuneraciones.SalarioBase, e.Aguinaldo.MesesTrabajadosAguinaldo);
            }
            if (e.Estado == EstadoTrabajador.Inactivo)
            {
                e.Indemnizacion.IndemnizacionPago = processes.CalculateIndemnizacion(e.Remuneraciones.SalarioBase, e.Indemnizacion.MesesTrabajadosIndemnizacion, e.Indemnizacion.AñosTrabajadosIndemnizacion);
            }
            e.Vacaciones.VacacionesPago = processes.CalculateVacations(e.Vacaciones.MesesTrabajadosVacaciones, e.Remuneraciones.SalarioBase, e.Estado);
        }
        //metodo que elimine a empleados de una coleccion especificada
        private bool Delete(Empleado t, IMongoCollection<Empleado> coleccion)
        {
            //creo que no es necesario poner un try catch
            try
            {
                var filter = Builders<Empleado>.Filter.Eq("Id", t.Id);
                var emp = coleccion.FindOneAndDelete(filter);
                if (emp == null)
                {
                    throw new Exception();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void Add(IMongoCollection<Empleado> coleccion, Empleado empleado)
        {
            coleccion.InsertOne(empleado);
        }
        private void Update(IMongoCollection<Empleado> coleccion, Empleado e)
        {
            try
            {
                //con replace da errores
                var filter = Builders<Empleado>.Filter.Eq("Id", e.Id);
                //buscar la manera de que actualizar no sea tan largo
                var updateFilter = Builders<Empleado>.Update.Set("NombreCompleto", e.NombreCompleto).Set("CodigoINSS", e.CodigoINSS).Set("Deducciones", e.Deducciones)
                    .Set("Imagen", e.Imagen).Set("Remuneraciones", e.Remuneraciones).Set("Cargos", e.Cargos).Set("Estado", e.Estado)
                    .Set("Prestamo",e.Prestamo).Set("Aguinaldo",e.Aguinaldo).Set("Indemnizacion",e.Indemnizacion).Set("Vacaciones",e.Vacaciones)
                    .Set("PagoPendiente",e.PagoPendiente).Set("MesesTrabajados",e.MesesTrabajados);
                var emp = coleccion.FindOneAndUpdate(filter, updateFilter);
                if (emp == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw new ArgumentException($"No se ha podido actualizar al empleado con Id {e.Id}");
            }
        }

        public void RealizarCalculos()
        {
            throw new NotImplementedException();
        }
    }
}
