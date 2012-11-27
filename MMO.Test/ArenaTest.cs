using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Raven.Client.Embedded;
using TestingTools.Core;
using TestingTools.Extensions;

namespace MMO.Test
{
    [TestFixture]
    public class ArenaTest
    {
        [TestAttribute]
        public void TestIfItInitializesCorrectly()
        {
            // Arrange
            Arena target;

            // Act
            target = new Arena();

            // Assert
            Verify.That<IEnumerable<Jugador>>(target.Jugadores).IsNotNull().Now();
            Verify.That(target.LogDeAtaque).IsNotNull().Now();
            Verify.That(target.JugadoresIds).IsNotNull().Now();
        }

        [TestAttribute]
        public void TestIfItKnowsWhatPlayersAreAlive()
        {
            // Arrange
            Arena target = new Arena();
            target.AgregarJugador(
                new Jugador
                {
                    Id = "muerto1",
                    Hp = 0
                }, new Jugador
                {
                    Id = "muerto2",
                    Hp = 0
                },
                new Jugador
                {
                    Id = "vivo1",
                    Hp = 1
                }, new Jugador
                {
                    Id = "vivo2",
                    Hp = 1
                });

            // Act
            Jugador[] jugadoresVivos = target.JugadoresVivos;

            // Assert
            Verify.That(target.JugadoresIds.Count).IsEqualTo(4).Now();
            Verify.That(jugadoresVivos.Length).IsEqualTo(2).Now();
            Verify.That(jugadoresVivos).IsTrueForAll(jugador => jugador.Id.StartsWith("vivo")).Now();
        }

        [TestAttribute]
        public void TestIfItKnowsWhenTheBattleIsOver()
        {
            // Arrange
            Arena target = new Arena();
            target.AgregarJugador(new Jugador
            {
                Hp = 1
            }, new Jugador
            {
                Hp = 1
            });

            // Pre-Assert
            Verify.That(target.BatallaTerminada).IsFalse().Now();

            // Act
            target.Jugadores.ElementAt(0).Hp = 0;
            target.Jugadores.ElementAt(1).Hp = 0;

            // Assert
            Verify.That(target.BatallaTerminada).IsTrue().Now();
        }

        [TestAttribute]
        public void TestIfItCanBeStoredModifiedAndRetrieved()
        {
            // Arrange
            using (var dstore = new EmbeddableDocumentStore()
                        {
                            RunInMemory = true
                        }.Initialize())
            {
                using (var sesion = dstore.OpenSession())
                {
                    var target = new Arena() { Id = "arena" };
                    var jugador1 = new Jugador() { Id = "1", Hp = 100 };
                    var jugador2 = new Jugador() { Id = "2", Hp = 100 };

                    sesion.Store(jugador1);
                    sesion.Store(jugador2);
                    sesion.Store(target);
                    sesion.SaveChanges();

                    target.AgregarJugador(jugador1, jugador2);
                    sesion.SaveChanges();

                    target.LogDeAtaque.Add(jugador1.Ataca(jugador2));
                    sesion.SaveChanges();
                }

                using (var sesion = dstore.OpenSession())
                {
                    var target = sesion.Load<Arena>("arena");

                    // Act
                    ICollection<RegistroDeAtaque> logs = target.LogDeAtaque;

                    // Assert                    
                    Verify.That(logs.Count).IsEqualTo(1).Now();
                }
            }
        }

        [TestAttribute]
        public void TestIfItCanHidrateItself()
        {
            // Arrange
            using (var dstore = new EmbeddableDocumentStore()
            {
                RunInMemory = true
            }.Initialize())
            {
                using (var sesion = dstore.OpenSession())
                {
                    var target = new Arena()
                    {
                        Id = "arena"
                    };
                    var jugador1 = new Jugador()
                    {
                        Id = "1"
                    };
                    var jugador2 = new Jugador()
                    {
                        Id = "2"
                    };
                    var jugador3 = new Jugador()
                    {
                        Id = "3"
                    };

                    sesion.Store(target);
                    sesion.Store(jugador1);
                    sesion.Store(jugador2);
                    sesion.Store(jugador3);

                    target.AgregarJugador(jugador1, jugador2, jugador3);
                    sesion.SaveChanges();
                }

                using (var sesion = dstore.OpenSession())
                {
                    var target = sesion.Load<Arena>("arena");

                    // Act
                    target.Hidratar(sesion);

                    // Assert
                    Verify.That(target.Jugadores.Count()).IsEqualTo(3);
                    Verify.That(target.Jugadores).IsTrueForAll(j => target.JugadoresIds.Contains(j.Id)).Now();

                    // Reset
                }
            }
        }
    }
}
