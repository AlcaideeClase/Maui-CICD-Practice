using GestionEmpleados.Model;
using Microsoft.Data.Sqlite;

namespace GestionEmpleados.DAO
{
    class GestionDAO
    {
        private string _connectionString;

        public GestionDAO(){
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "empresa_v2.db");
            _connectionString = $"Data Source={dbPath}";

            InicializarBaseDeDatos();
        }

        private void InicializarBaseDeDatos()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Creación manual de tabla DEPARTAMENTO
                string sqlDep = @"CREATE TABLE IF NOT EXISTS Departamento (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    Nombre TEXT,
                                    Localidad TEXT
                                  );";

                // Creación manual de tabla EMPLEADO con Foreign Key
                string sqlEmp = @"CREATE TABLE IF NOT EXISTS Empleado (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    IdDepartamento INTEGER,
                                    Apellidos TEXT,
                                    Oficio TEXT,
                                    Salario REAL,
                                    Comisiones REAL,
                                    FechaAlta TEXT,
                                    FOREIGN KEY(IdDepartamento) REFERENCES Departamento(Id)
                                  );";

                using (var command = new SqliteCommand(sqlDep, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SqliteCommand(sqlEmp, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

        }

        // ---------------- CRUD DEPARTAMENTOS ----------------

        public List<Departamento> ObtenerDepartamentos()
        {
            var lista = new List<Departamento>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Nombre, Localidad FROM Departamento";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var dep = new Departamento();

                        // 1. Leemos el ID (ese siempre existe)
                        dep.Id = reader.GetInt32(0);

                        // 2. Verificamos si NOMBRE es nulo en la BD
                        if (!reader.IsDBNull(1))
                            dep.Nombre = reader.GetString(1);
                        else
                            dep.Nombre = "Sin Nombre"; // O string.Empty

                        // 3. Verificamos si LOCALIDAD es nula en la BD
                        if (!reader.IsDBNull(2))
                            dep.Localidad = reader.GetString(2);
                        else
                            dep.Localidad = "Sin Localidad";

                        lista.Add(dep);
                    }
                }
            }
            return lista;
        }

        public void InsertarDepartamento(Departamento dep)
        {
            if (string.IsNullOrWhiteSpace(dep.Nombre) || string.IsNullOrWhiteSpace(dep.Localidad))
            {
                // No insertamos si faltan datos
                return;
            }
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // OJO: Usar parámetros siempre (evita inyección SQL y errores de formato)
                command.CommandText = "INSERT INTO Departamento (Nombre, Localidad) VALUES (@nom, @loc)";

                command.Parameters.AddWithValue("@nom", dep.Nombre);
                command.Parameters.AddWithValue("@loc", dep.Localidad);

                command.ExecuteNonQuery();
            }
        }

        // ---------------- CRUD EMPLEADOS ----------------

        public List<Empleado> ObtenerEmpleadosPorDepartamento(int idDept)
        {
            var lista = new List<Empleado>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Apellidos, Oficio, Salario, Comisiones, FechaAlta FROM Empleado WHERE IdDepartamento = @idDept";
                command.Parameters.AddWithValue("@idDept", idDept);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var emp = new Empleado();
                        emp.Id = reader.GetInt32(0);
                        emp.Apellidos = reader.GetString(1);
                        emp.Oficio = reader.GetString(2);
                        emp.Salario = reader.GetDecimal(3);
                        emp.Comision = reader.GetDecimal(4);

                        // Parseo manual de fecha (SQLite guarda fechas como texto habitualmente)
                        string fechaStr = reader.GetString(5);
                        emp.FechaAlta = DateOnly.Parse(fechaStr);

                        // Asignamos el FK manual
                        emp.IdDepartamento = idDept;
                        lista.Add(emp);
                    }
                }
            }
            return lista;
        }

        

        public void InsertarEmpleado(Empleado emp)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO Empleado (IdDepartamento, Apellidos, Oficio, Salario, Comisiones, FechaAlta) 
                                        VALUES (@idDep, @ape, @ofi, @sal, @com, @fec)";

                command.Parameters.AddWithValue("@idDep", emp.IdDepartamento);
                command.Parameters.AddWithValue("@ape", emp.Apellidos);
                command.Parameters.AddWithValue("@ofi", emp.Oficio);
                command.Parameters.AddWithValue("@sal", emp.Salario);
                command.Parameters.AddWithValue("@com", emp.Comision);
                command.Parameters.AddWithValue("@fec", emp.FechaAlta.ToString("yyyy-MM-dd")); // Formato ISO simple

                command.ExecuteNonQuery();
            }
        }
        public void ActualizarEmpleado(Empleado emp)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // SQL UPDATE
                command.CommandText = @"UPDATE Empleado 
                                        SET Apellidos=@ape, Oficio=@ofi, Salario=@sal, Comisiones=@com, FechaAlta=@fec 
                                        WHERE Id=@id";

                command.Parameters.AddWithValue("@id", emp.Id); // Importante: WHERE Id = ...
                command.Parameters.AddWithValue("@ape", emp.Apellidos);
                command.Parameters.AddWithValue("@ofi", emp.Oficio);
                command.Parameters.AddWithValue("@sal", emp.Salario);
                command.Parameters.AddWithValue("@com", emp.Comision);
                command.Parameters.AddWithValue("@fec", emp.FechaAlta.ToString("yyyy-MM-dd"));

                command.ExecuteNonQuery();
            }
        }

        public void BorrarEmpleado(Empleado emp)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // SQL DELETE
                command.CommandText = "DELETE FROM Empleado WHERE Id=@id";
                command.Parameters.AddWithValue("@id", emp.Id);
                command.ExecuteNonQuery();
            }
        }

        public List<Empleado> BuscarEmpleados(string columna, string valor)
        {
            var lista = new List<Empleado>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Validación básica para evitar inyección SQL en el nombre de la columna
                // El valor @valor sí se parametriza.
                string columnaSql = "APELLIDOS"; // Por defecto
                if (columna == "OFICIO") columnaSql = "OFICIO";
                else if (columna == "SALARIO") columnaSql = "SALARIO";
                else if (columna == "FECHA_ALT") columnaSql = "FECHA_ALT";
                else if (columna == "COMISIONES") columnaSql = "COMISIONES";

                // SQL Dinámico
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Id, Apellidos, Oficio, Salario, Comisiones, FechaAlta, IdDepartamento FROM Empleado WHERE {columnaSql} LIKE @val";
                command.Parameters.AddWithValue("@val", $"%{valor}%"); // Búsqueda parcial 

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var emp = new Empleado();
                        emp.Id = reader.GetInt32(0);
                        emp.Apellidos = reader.GetString(1);
                        emp.Oficio = reader.GetString(2);
                        emp.Salario = reader.GetDecimal(3);
                        emp.Comision = reader.GetDecimal(4);

                        // Parseo manual de fecha (SQLite guarda fechas como texto habitualmente)
                        string fechaStr = reader.GetString(5);
                        emp.FechaAlta = DateOnly.Parse(fechaStr);

                        // Asignamos el FK manual
                        emp.IdDepartamento = reader.GetInt32(6);
                        lista.Add(emp);
                    }
                }
            }
            return lista;
        }

        public List<Empleado> ObtenerEmpleados()
        {
            var lista = new List<Empleado>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Apellidos, Oficio, Salario, Comisiones, FechaAlta, IdDepartamento FROM Empleado";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var emp = new Empleado();
                        emp.Id = reader.GetInt32(0);
                        emp.Apellidos = reader.GetString(1);
                        emp.Oficio = reader.GetString(2);
                        emp.Salario = reader.GetDecimal(3);
                        emp.Comision = reader.GetDecimal(4);

                        // Parseo manual de fecha (SQLite guarda fechas como texto habitualmente)
                        string fechaStr = reader.GetString(5);
                        emp.FechaAlta = DateOnly.Parse(fechaStr);

                        // Asignamos el FK manual
                        emp.IdDepartamento = reader.GetInt32(0);
                        lista.Add(emp);
                    }
                }
            }
            return lista;
        }
    }
}



