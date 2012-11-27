using System;
using System.Linq;
using MMO;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace RavebDbSetupExample
{
    class Program
    {
        private const string NOMBRE_BD = "basic_example";
        private const string NOMBRE_INDICE = "Jugadores/PorNombre";
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The args.</param>
        static void Main(string[] args)
        {
            // Se crea una conexión al servidor RavenDB (el cuál puede tener
            // varias bases de datos).
            using (IDocumentStore documentStore = new DocumentStore
                        {   // Se especifica la conexión HTTP (REST)
                            Url = "http://localhost:81"
                        }.Initialize())
            {
                // Se crea una sesión con la base de datos RavenDB, esta operación 
                // es costosa, y es recomendable no hacerla por cada query/insert/delete
                // sino hacer 1 por sesión de usuario, por ejemplo.
                using (IDocumentSession session = documentStore.OpenSession(NOMBRE_BD))
                {                    
                    // Se guarda una nueva entidad Jugador sin problema,
                    // la propiedad Jugador.Id se asigna automáticamente
                    // por RavenDB.
                    // Se guardan los cambios de format atómica, por-documento
                    // i.e. cambios sobre 2 documentos distintos no son atómicos para
                    // ambos documentos en conjunto.
                    session.Store(new Jugador()
                    {
                        Hp = 100,
                        Nombre = "Player 1"
                    });

                    // No se requiere crear una 'tabla' ni un 'esquema', 
                    // simplemente se guarda el objeto y RavenDB se encarga
                    // del resto. Estos dos objetos se guardarían en una misma
                    // colección identificada por el tipo de objeto (Jugador)
                    session.Store(new Jugador()
                    {
                        Hp = 100,
                        Nombre = "Player 2"
                    });

                    session.SaveChanges();

                    // Se crea un índice para hacer búsquedas de jugadores por nombre
                    // sólo si este no existe previamente (por si se ejecuta esta aplicación
                    // más de una vez).            
                    if (!documentStore.DatabaseCommands
                                     .ForDatabase(NOMBRE_BD)
                                     .GetIndexNames(0, 10)
                                     .Contains(NOMBRE_INDICE))
                    {
                        // La operación PutIndex agrega un nuevo índice a la base
                        // de datos.
                        session.Advanced.DocumentStore.DatabaseCommands
                                        .ForDatabase(NOMBRE_BD)
                                        .PutIndex(
                            // Se especifica el nombre del índice
                                            NOMBRE_INDICE,
                            // Se crea un nuevo índice con la API RavenDB
                            // para la construcción de índices.
                                            new IndexDefinitionBuilder<Jugador>
                                            {
                                                // La operación map determina cuáles
                                                // serán los criterios de búsqueda, en
                                                // este caso, el nombre del jugador.
                                                Map = jugadores =>
                                                        from jugador in jugadores
                                                        select new
                                                        {
                                                            jugador.Nombre
                                                        }
                                            });
                    }
                    else
                    {
                        Console.WriteLine("El índice Jugadores/PorNombre ya se tiene.");
                    }


                    // Se hace un query sobre los documentos tipo Jugador
                    // en RavenDB. RavenDB guarda metadatos sobre cada documento
                    // para identificarlo por tipo. En este caso: Jugador.
                    //
                    // Dado que los queries se hacen sobre los índices de los documentos
                    // no sobre los documentos en sí, y esta operación de búsqueda espera
                    // a que la informacíon para todos los documentos Jugador este
                    // disponible a los índices de ese tipo de jugador. Con la operación                    
                    Jugador[] todosJugadores = (from jugador in session
                                                                .Query<Jugador>(NOMBRE_INDICE)
                                    // WaitForNonStale results, se espera a que los índices se actualicen.
                                                                .Customize(q => q.WaitForNonStaleResults())                                                                                                
                                                select jugador)
                                               .ToArray();

                    // Una vez con el arreglo de jugadores, se puede transversar
                    // como si fuera una colección de objetos .Net 
                    foreach (var jugador in todosJugadores)
                    {   // Debe imprimir los 2 nuevos jugadores agregados.
                        Console.WriteLine(jugador.ToString());
                    }                                        

                    // Esta operación es lo que se llama "Bulk Delete", se borran N 
                    // documentos en 1 sola operación, sin tener que hacer múltiples
                    // viajes de ida y regreso a la base de datos.
                    // En este caso se borra utilizando los índices RavenDB (vistas concretas)                    
                    documentStore.DatabaseCommands
                        .ForDatabase(NOMBRE_BD)
                        .DeleteByIndex(
                        // Se especifica el índice a borrar, en RavenDB los índices 
                        // por defecto creados
                        NOMBRE_INDICE,
                        // Se especifica un una búsqueda Lucene (full-text search), la cuál
                        // leerá los documentos JSON en busca del patrón Nombre:*, que 
                        // cualquier documento con la propiedad Name cumplirá.
                        new IndexQuery
                        {
                            Query = "Nombre:*"
                        });
                        // Se especifica que no se permite borrar nada si el índice
                        // utilizado aún no se actualiza por completo.
                        //allowStale: true);
                }
            }
        }
    }
}
