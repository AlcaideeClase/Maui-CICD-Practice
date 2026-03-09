using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEmpleados.Model
{
    internal class Departamento
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public string Localidad { get; set; }

        public List<Empleado> Empleados { get; set; }

        public Departamento()
        {
        }

        public Departamento(string nombre, string localidad)
        {
            Nombre = nombre;
            Localidad = localidad;
            Empleados = new List<Empleado>();
        }




    }
}
