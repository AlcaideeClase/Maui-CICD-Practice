using GestionEmpleados.Model;
using GestionEmpleados.servicio;

namespace GestionEmpleados
{
    public partial class MainPage : ContentPage
    {
        private GestionService _servicio;

        public MainPage()
        {
            InitializeComponent();
            _servicio = new GestionService();
            CargarDatos();
        }

        private void CargarDepartamentosEmpleados()
        {
            CollDepartamentos.ItemsSource = _servicio.ObtenerDepartamentos();
            CollEmpleados.ItemsSource = _servicio.ObtenerEmpleados();
        }

        private void CargarDatos()
        {
            // Lógica rápida: Si no hay departamentos, crea los del PDF.
            // Esto asume que tienes un método en tu servicio/DAO para contar o listar departamentos.
            var deptos = _servicio.ObtenerDepartamentos();

            if (deptos.Count == 0)
            {
                // Insertamos los datos del PDF
                _servicio.GuardarDepartamento(new Departamento { Nombre = "Recursos Humanos", Localidad = "Madrid" });
                _servicio.GuardarDepartamento(new Departamento { Nombre = "Finanzas", Localidad = "Barcelona" });
                _servicio.GuardarDepartamento(new Departamento { Nombre = "IT", Localidad = "Valencia" });
                _servicio.GuardarDepartamento(new Departamento { Nombre = "Marketing", Localidad = "Sevilla" });
                _servicio.GuardarDepartamento(new Departamento { Nombre = "Ventas", Localidad = "Bilbao" });
            }
            // Recargamos la lista visual
            CargarDepartamentosEmpleados();

            CargarFiltros();
           
        }

        private void OnDepartamentoSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var depto = e.CurrentSelection.FirstOrDefault() as Departamento;
            if (depto == null) return;

            var empleados = _servicio.ObtenerEmpleadosPorDepartamento(depto.Id);

            CollEmpleados.ItemsSource = empleados;

            // Limpiar selección de empleado anterior para evitar errores
            CollEmpleados.SelectedItem = null;
            LimpiarCajas();
        }

        private void LimpiarCajas()
        {
            EntryComision.Text = "";
            EntryApellidos.Text = "";
            EntryOficio.Text = "";
            EntrySalario.Text = "";
            DatePickerFechaAlta.Date = DateTime.Now;
        }

        // Evento SelectionChanged de la lista de Empleados (CollEmpleados)
        private void OnEmpleadoSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var emp = e.CurrentSelection.FirstOrDefault() as Empleado;
            if (emp == null) return;

            // REQUISITO: Rellenar las cajas de texto 
            EntryApellidos.Text = emp.Apellidos;
            EntryOficio.Text = emp.Oficio;
            EntrySalario.Text = emp.Salario.ToString();
            EntryComision.Text = emp.Comision.ToString();
            DatePickerFechaAlta.Date = DateTime.Parse(emp.FechaAlta.ToString()); // O convertir si usas string
        }

        private void CargarFiltros()
        {
            List<string> listFiltros = new List<string>
            {
                "APELLIDOS", "OFICIO", "SALARIO", "FECHA_ALT", "COMISIONES"
            };
            CollFiltros.ItemsSource = listFiltros;
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            try
            {
                // 1. Validar Departamento Seleccionado [cite: 131]
                var depto = CollDepartamentos.SelectedItem as Departamento;
                if (depto == null)
                {
                    await DisplayAlert("Error", "Debe seleccionar un departamento primero.", "OK");
                    return;
                }

                // 2. Validar Campos Vacíos 
                if (string.IsNullOrWhiteSpace(EntryApellidos.Text) ||
                    string.IsNullOrWhiteSpace(EntryOficio.Text) ||
                    string.IsNullOrWhiteSpace(EntryComision.Text) ||
                    string.IsNullOrWhiteSpace(EntrySalario.Text))
                {
                    await DisplayAlert("Error", "No se permiten campos vacíos.", "OK");
                    return;
                }

                // Crear objeto y llamar al servicio
                var nuevoEmp = new Empleado
                {
                    IdDepartamento = depto.Id, // Usar el ID del departamento seleccionado
                    Apellidos = EntryApellidos.Text,
                    Comision = Decimal.Parse(EntryComision.Text),
                    Salario = Decimal.Parse(EntrySalario.Text),
                    FechaAlta = DateOnly.FromDateTime(DatePickerFechaAlta.Date),
                    Oficio = EntryOficio.Text
                };

                _servicio.GuardarEmpleado(nuevoEmp);

                // Refrescar lista
                CollEmpleados.ItemsSource = _servicio.ObtenerEmpleadosPorDepartamento(depto.Id);
            }
            catch (Exception ex)
            {
                // Control de excepciones requerido 
                await DisplayAlert("Error Base de Datos", ex.Message, "OK");
            }
        }

        private async void OnBuscarClicked(object sender, EventArgs e)
        {
            try
            {
                // 1. Obtener criterio de búsqueda (De la lista CollFiltros)
                var criterio = CollFiltros.SelectedItem as string; // Porque la lista es de strings
                string textoBusqueda = EntryBuscar.Text;

                // 2. Validaciones [cite: 384]
                if (string.IsNullOrEmpty(criterio) || string.IsNullOrWhiteSpace(textoBusqueda))
                {
                    await DisplayAlert("Error", "Debe seleccionar un criterio y un texto de búsqueda", "OK");
                    return;
                }

                // 3. Ejecutar búsqueda (Necesitas este método en tu Servicio/DAO)
                // La búsqueda debe ser "LIKE" o "CONTAINS" 
                var resultados = _servicio.BuscarEmpleados(criterio, textoBusqueda);

                // 4. Mostrar resultados en la lista de empleados
                CollEmpleados.ItemsSource = resultados;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        }
        private async void  OnActualizarClicked(object sender, EventArgs e)
        {
            try
            {
                // 1. Validar que hay un empleado seleccionado para editar [cite: 133, 302]
                var empleadoSeleccionado = CollEmpleados.SelectedItem as Empleado;
                if (empleadoSeleccionado == null)
                {
                    await DisplayAlert("Error", "Debes seleccionar un empleado", "OK");
                    return;
                }

                // 2. Validar campos vacíos (Igual que en Guardar) 
                if (string.IsNullOrWhiteSpace(EntryApellidos.Text) ||
                    string.IsNullOrWhiteSpace(EntryOficio.Text) ||
                    string.IsNullOrWhiteSpace(EntrySalario.Text) ||
                    string.IsNullOrWhiteSpace(EntryComision.Text))
                {
                    await DisplayAlert("Error", "Los campos no deben estar vacíos", "OK");
                    return;
                }

                // 3. Preparar objeto (Mantenemos el ID del empleado seleccionado)
                var empActualizado = new Empleado
                {
                    Id = empleadoSeleccionado.Id, // ¡IMPORTANTE! Mantener el ID original
                    IdDepartamento = empleadoSeleccionado.IdDepartamento, // Mantenemos su departamento
                    Apellidos = EntryApellidos.Text,
                    Oficio = EntryOficio.Text,
                    Salario = Decimal.Parse(EntrySalario.Text),
                    Comision = Decimal.Parse(EntryComision.Text),
                    // Si el usuario tocó la fecha, usamos la del DatePicker, si no, la original
                    FechaAlta = DateOnly.FromDateTime(DatePickerFechaAlta.Date) 
                };

                // 4. Llamar a servicio (Asegúrate de tener el método Actualizar en tu DAO)
                _servicio.ActualizarEmpleado(empActualizado);

                // 5. Refrescar la lista para ver los cambios
                var depto = CollDepartamentos.SelectedItem as Departamento;
                CollEmpleados.ItemsSource = _servicio.ObtenerEmpleadosPorDepartamento(depto.Id);

                await DisplayAlert("Éxito", "Empleado actualizado correctamente", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }


        }
        private async void OnBorrarClicked(object sender, EventArgs e)
        {
            try
            {
                // 1. Validar selección [cite: 133, 333]
                var empleadoSeleccionado = CollEmpleados.SelectedItem as Empleado;
                if (empleadoSeleccionado == null)
                {
                    await DisplayAlert("Error", "Debes seleccionar un empleado", "OK");
                    return;
                }

                // 2. Ejecutar borrado
                _servicio.BorrarEmpleado(empleadoSeleccionado);

                // 3. Mostrar mensaje de éxito como en el PDF [cite: 274, 280]
                await DisplayAlert("Éxito", $"Empleado {empleadoSeleccionado.Apellidos} ha sido eliminado", "OK");

                // 4. Refrescar lista y limpiar cajas
                var depto = CollDepartamentos.SelectedItem as Departamento;
                CollEmpleados.ItemsSource = _servicio.ObtenerEmpleadosPorDepartamento(depto.Id);
                LimpiarCajas();
                CollEmpleados.SelectedItem = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        }
        private void OnLimpiarClicked(object sender, EventArgs e)
        {
            // Limpia cajas de texto [cite: 238]
            LimpiarCajas();

            // Deselecciona listas
            CollDepartamentos.SelectedItem = null;
            CollEmpleados.SelectedItem = null;
            CollFiltros.SelectedItem = null;

            EntryBuscar.Text = "";
        }
    }

}
