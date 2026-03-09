using GestionEmpleados.DAO;
using GestionEmpleados.Model;

namespace GestionEmpleados.servicio
{
    class GestionService
    {
        private GestionDAO _dao;

        public GestionService()
        {
            _dao = new GestionDAO();
        }

        public List<Departamento> ObtenerDepartamentos()
        {
            return _dao.ObtenerDepartamentos();
        }

        public List<Empleado> ObtenerEmpleados()
        {
            return _dao.ObtenerEmpleados();
        }

        // Método para validar si hay datos (usado en tu inicialización)
        public List<Departamento> ObtenerDatosCompletos()
        {
            var departamentos = _dao.ObtenerDepartamentos();
            // Nota: Podrías cargar empleados aquí si quieres, pero tu app lo hace al clickar
            return departamentos;
        }

        public void GuardarDepartamento(Departamento dep)
        {
            _dao.InsertarDepartamento(dep);
        }

        public void GuardarEmpleado(Empleado emp)
        {
            _dao.InsertarEmpleado(emp);
        }

        public List<Empleado> ObtenerEmpleadosPorDepartamento(int id)
        {
            return _dao.ObtenerEmpleadosPorDepartamento(id);
        }

        // --- MÉTODOS QUE FALTABAN ---

        public void ActualizarEmpleado(Empleado emp)
        {
            _dao.ActualizarEmpleado(emp);
        }

        public void BorrarEmpleado(Empleado emp)
        {
            _dao.BorrarEmpleado(emp);
        }

        public List<Empleado> BuscarEmpleados(string columna, string valor)
        {
            return _dao.BuscarEmpleados(columna, valor);
        }
    }
}