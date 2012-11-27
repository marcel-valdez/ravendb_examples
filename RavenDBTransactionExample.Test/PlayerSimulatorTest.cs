using MMO;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using TestingTools.Core;
using TestingTools.Extensions;
using System.Linq;

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
        }

        [TestAttribute]
        public void TestIfItRegistersTransactionsInTheDocumentStore()
        {
            using (DocumentStore dstore = GetDocumentStore())
            using (IDocumentSession session = dstore.OpenSession())
            {
                // Arrange
                Arena arena = new Arena();
                arena.Jugadores.AddRange(new[] { 
                    new Jugador() { Id = "agresor", Hp = 100 }, 
                    new Jugador() { Id = "victima", Hp = 100 } 
                });

                PlayerSimulator target = new PlayerSimulator(arena.Jugadores[0])
                {
                    Arena = arena,
                    Session = session
                };

                // Act
                target.SimularAsync(frequency: 10).Wait(2000);

                // Assert
                
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
    }
}
