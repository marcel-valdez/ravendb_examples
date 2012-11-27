using System.Collections.Generic;
using System.Linq;
using MMO;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
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
            using (var dstore = GetDocumentStore())
            using (IDocumentSession session = dstore.OpenSession())
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

            // Reset
            ClearDocumentStore();
        }

        /// <summary>
        /// Tests if it attacks A player until death.
        /// </summary>
        [TestAttribute]
        public void TestIfItAttacksAPlayerUntilDeath()
        {
            using (var dstore = GetDocumentStore())
            using (IDocumentSession session = dstore.OpenSession())
            {
                // Arrange
                Arena arena = new Arena();
                Jugador jugador = new Jugador()
                {
                    Hp = 100
                };
                Jugador victima = new Jugador()
                {
                    Hp = 100
                };
                arena.Jugadores.AddRange(new[] { jugador, victima });
                PlayerSimulator target = new PlayerSimulator(jugador)
                {
                    Arena = arena,
                    Session = session
                };

                // Act
                target.SimularAsync(frequency: 10).Wait(2000);

                // Assert
                Verify.That(arena.BatallaTerminada).IsTrue().Now();
                Verify.That(victima.EstaVivo).IsFalse().Now();
                Verify.That(jugador.EstaVivo).IsTrue().Now();
                Verify.That(jugador.Hp).IsEqualTo(100).Now();
                Verify.That(arena.LogDeAtaque.Sum(log => log.Dano)).IsEqualTo(100).Now();
            }

            // Reset
            ClearDocumentStore();
        }

        [TestAttribute]
        public void TestIfItRegistersTransactionsInTheDocumentStore()
        {
            // Arrange
            PlayerSimulator simulator = ArrangeSimulation().First();

            // Act
            simulator.SimularAsync(10).Wait(1000);

            Arena stored = null;
            using (DocumentStore dstore = GetDocumentStore())
            using (IDocumentSession session = dstore.OpenSession())
            {
                // Assert
                stored = session.Query<Arena>().First();
            }

            Verify.That(stored).IsNotNull().Now();
            Verify.That(stored.LogDeAtaque.Count).IsGreaterThan(2).Now();
            Verify.That(stored.LogDeAtaque.Sum(log => log.Dano)).IsEqualTo(100).Now();

            // Reset
            ClearDocumentStore();
        }        
       
        private static IEnumerable<PlayerSimulator> ArrangeSimulation(int total = 2, int agresors = 1)
        {
            int agresorCounter = 0;
            using (DocumentStore dstore = GetDocumentStore())
            using (IDocumentSession session = dstore.OpenSession())
            {
                // Arrange
                Arena arena = new Arena();
                for (int i = 0; i < total; i++)
                {
                    string type = agresorCounter++ < agresors ? "agresor" : "victima";
                    arena.Jugadores.Add(new Jugador()
                    {
                        Id = type + agresorCounter,
                        Hp = 100
                    });

                    if (type == "agresor")
                    {
                        PlayerSimulator target = new PlayerSimulator(arena.Jugadores[0])
                        {
                            Arena = arena,
                            Session = session
                        };

                        yield return target;
                    }

                    session.Store(arena.Jugadores[i]);
                    session.SaveChanges();
                }
                                
                session.Store(arena);
                session.SaveChanges();
            }
        }

        private static DocumentStore GetDocumentStore()
        {
            var documentStore = new EmbeddableDocumentStore
            {
                RunInMemory = true
            };

            documentStore.Initialize();

            return documentStore;
        }

        private static void ClearDocumentStore()
        {
            using (var dstore = GetDocumentStore())
            using (var session = dstore.OpenSession())
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
            }
        }
    }
}
