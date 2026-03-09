using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace GestionEmpleados.Model
{
    class Empleado
    {
        public int Id { get; set; }
        public string Apellidos { get; set; }
        public string Oficio { get; set; }
        public decimal Salario { get; set; }
        public decimal Comision { get; set; } 
        public DateOnly FechaAlta { get; set; }

        public int IdDepartamento { get; set; }

        public Empleado()
        {
        }

        public Empleado( string apellidos, string oficio, decimal salario, decimal comision, DateOnly fechaAlta)
        {
            Apellidos = apellidos;
            Oficio = oficio;
            Salario = salario;
            Comision = comision;
            FechaAlta = fechaAlta;
        }
    }
}
