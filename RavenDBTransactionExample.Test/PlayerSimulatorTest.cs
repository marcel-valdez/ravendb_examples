using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MMO;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Database.Server;
using TestingTools.Core;
using TestingTools.Extensions;

namespace RavenDBTransactionExample.Test
{

    [TestFixture]
    public class PlayerSimulatorTest
    {
        /// <summary>
        /// Tests if it initializes correctly.
        /// </summary>
        [TestAttribute]
        public void TestIfItInitializesCorrectly()
        {
            using (IDocumentStore dstore = GetDocumentStore())
            using (IDocumentSession session = dstore.OpenSession("test"))
            {
                // Arrange
                PlayerSimulator target;

                // Act
                target = new PlayerSimulator(new Jugador())
                {
                    Arena = new Arena(),
                    Session = session
                };

                // Assert
                Verify.That(target.Jugador).IsNotNull().Now();
                Verify.That(target.Arena).IsNotNull().Now();
                Verify.That(target.Session).IsNotNull().Now();

                // Reset
            }
        }

        /// <summary>
        /// Tests if it attacks A player until death.
        /// </summary>
        [TestAttribute]
        public void TestIfItAttacksAPlayerUntilDeath()
        {
            using (var dstore = GetDocumentStore())
            {
                ClearDocumentStore(dstore);
                // Arrange                
                Jugador agresor = new Jugador()
                {
                    Hp = 100
                };

                Jugador victima = new Jugador()
                {
                    Hp = 100
                };
                Arena arena = new Arena()
                {
                    Id = "arena"
                };

                using (IDocumentSession session = dstore.OpenSession("test"))
                {
                    session.Store(agresor);
                    session.Store(victima);
                    arena.AgregarJugador(agresor, victima);
                    session.Store(arena);
                    session.SaveChanges();

                    PlayerSimulator target = new PlayerSimulator(agresor)
                    {
                        Arena = arena,
                        Session = session
                    };

                    // Act
                    target.SimularAsync(frequency: 0).Wait();

                    // Assert
                    Verify.That(arena.BatallaTerminada).IsTrue().Now();
                    Verify.That(victima.EstaVivo).IsFalse().Now();
                    Verify.That(agresor.EstaVivo).IsTrue().Now();
                    Verify.That(agresor.Hp).IsEqualTo(100).Now();
                    Verify.That(victima.Hp).IsEqualTo(0).Now();
                    Verify.That(agresor.Estadisticas.DanoCausado).IsEqualTo(100).Now();
                    Verify.That(victima.Estadisticas.DanoRecibido).IsEqualTo(100).Now();
                    Verify.That(arena.LogDeAtaque.Sum(log => log.Dano)).IsEqualTo(100).Now();
                }
            }
        }

        // Caso base
        [TestCase(2, 1)]
        // Caso simple con más de 1 agresor.
        [TestCase(3, 2)]
        // Caso donde todos son agresores
        [TestCase(2, 2)]
        // Caso límite para procesador 8 cores
        [TestCase(4, 4)]
        public void TestIfItRegistersTransactionsInTheDocumentStore(int totalJugadores, int totalAgresores)
        {
            // Arrange
            Arena storedArena = null;
            Jugador[] jugadores = null;
            using (IDocumentStore dstore = GetDocumentStore())
            {
                ClearDocumentStore(dstore);
                List<Task> tasks = new List<Task>();
                foreach (PlayerSimulator simulator in ArrangeSimulation(dstore, totalJugadores, totalAgresores))
                {
                    // Act                        
                    tasks.Add(simulator.SimularAsync(100).ContinueWith(t => simulator.Session.Dispose()));
                }

                Task.WaitAll(tasks.ToArray());


                using (IDocumentSession sesion = dstore.OpenSession("test"))
                {                                    
                    jugadores = sesion.Query<Jugador>()
                                   .Customize(q => q.WaitForNonStaleResults())
                                   .ToArray();

                    storedArena = sesion.Load<Arena>("arena");

                    // Assert
                    Verify.That(storedArena).IsNotNull().Now();
                    Verify.That(storedArena.LogDeAtaque.Count).IsGreaterThanOrEqual((totalJugadores - 1) * 2).Now();
                    Verify.That(storedArena.LogDeAtaque.Sum(log => log.Dano)).IsGreaterThanOrEqual((totalJugadores - 1) * 100).Now();

                    string playerState = jugadores.Select(j => j.ToString()).Aggregate((acum, item) => acum + "\n" + item);

                    Verify.That(jugadores.Length).IsEqualTo(totalJugadores, playerState).Now();
                    Verify.That(jugadores.Sum(j => j.Estadisticas.DanoCausado)).IsGreaterThanOrEqual((totalJugadores - 1) * 100, playerState).Now();
                    Verify.That(jugadores.Sum(j => j.Estadisticas.JugadoresMatados)).IsGreaterThanOrEqual(totalJugadores - 1, playerState).Now();
                    Verify.That(jugadores.Sum(j => j.Estadisticas.DanoCausado)).IsEqualTo(jugadores.Sum(j => j.Estadisticas.DanoRecibido), playerState).Now();
                    Verify.That(jugadores.Sum(j => j.Estadisticas.JugadoresMatados)).IsEqualTo(jugadores.Sum(j => j.Estadisticas.Muertes), playerState).Now();
                    Verify.That(jugadores).IsTrueForAll(j => j.Batallas.IndexOf(storedArena.Id) != -1, playerState).Now();
                }
            }
        }

        private static IEnumerable<PlayerSimulator> ArrangeSimulation(IDocumentStore dstore, int total = 2, int agresors = 1)
        {
            int agresorCounter = 0;
            List<PlayerSimulator> simulaciones = new List<PlayerSimulator>();
            List<Jugador> jugadores = new List<Jugador>();
            using (var sesion = dstore.OpenSession("test"))
            {
                // Arrange
                Arena arena = new Arena()
                {
                    Id = "arena"
                };

                sesion.Store(arena);                

                for (int i = 0; i < total; i++)
                {
                    string type = agresorCounter++ < agresors ? "agresor" : "victima";
                    var nuevoJugador = new Jugador()
                                    {
                                        Id = type + agresorCounter,
                                        Hp = 100
                                    };
                    sesion.Store(nuevoJugador);                    
                    arena.AgregarJugador(nuevoJugador);
                    sesion.SaveChanges();
                    jugadores.Add(nuevoJugador);
                }
            }

            foreach (Jugador jugador in jugadores)
            {
                if (jugador.Id.Contains("agresor"))
                {
                    IDocumentSession sesionDeSimulacion = dstore.OpenSession("test");
                    sesionDeSimulacion.Advanced.UseOptimisticConcurrency = true;
                    Arena arenaDeSimulacion = sesionDeSimulacion.Load<Arena>("arena");
                    Jugador jugadorEnSimulacion = sesionDeSimulacion.Load<Jugador>(jugador.Id);
                    PlayerSimulator target = new PlayerSimulator(jugadorEnSimulacion)
                    {
                        Arena = arenaDeSimulacion,
                        Session = sesionDeSimulacion
                    };

                    simulaciones.Add(target);
                }
            }

            return simulaciones;
        }

        private static IDocumentStore GetDocumentStore()
        {
            //NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(82);
            //var documentStore = new EmbeddableDocumentStore
            //{
            //    RunInMemory = true,
            //    UseEmbeddedHttpServer = true,                
            //};
            //documentStore.Configuration.Port = 82;
            
            var documentStore = new DocumentStore
                       {   // Se especifica la conexión HTTP (REST)
                           Url = "http://localhost:81"
                       }.Initialize();

            documentStore.Conventions.MaxNumberOfRequestsPerSession = 100;


            return documentStore;
        }

        private static void ClearDocumentStore(IDocumentStore dstore)
        {
            using (var session = dstore.OpenSession("test"))
            {
                var jugadores = (from jugador in session.Query<Jugador>()
                                 select jugador).ToArray();

                foreach (var jugador in jugadores)
                {
                    session.Delete<Jugador>(jugador);
                }

                var arenas = (from arena in session.Query<Arena>()
                              select arena).ToArray();

                foreach (var arena in arenas)
                {
                    session.Delete<Arena>(arena);
                }

                session.SaveChanges();
                // Esperar a que se efectúen todos los cambios
                session.Query<Arena>()
                       .Customize(q => q.WaitForNonStaleResults()).ToArray();
                session.Query<Jugador>()
                       .Customize(q => q.WaitForNonStaleResults()).ToArray();
            }
        }
    }
}
