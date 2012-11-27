using System.Collections.Generic;
using NUnit.Framework;
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
            Verify.That<List<Jugador>>(target.Jugadores).IsNotNull().Now();
        }

        [TestAttribute]
        public void TestIfItKnowsWhatPlayersAreAlive()
        {
            // Arrange
            Arena target = new Arena();
            target.Jugadores.AddRange(new[] {
                new Jugador { Id = "muerto", Hp = 0 }, new Jugador { Id = "muerto", Hp = 0 }, 
                new Jugador { Id = "vivo", Hp = 1 }, new Jugador { Id = "vivo", Hp = 1 }
            });

            // Act
            Jugador[] jugadoresVivos = target.JugadoresVivos;

            // Assert
            Verify.That(jugadoresVivos.Length).IsEqualTo(2).Now();
            Verify.That(jugadoresVivos).IsTrueForAll(jugador => jugador.Id == "vivo").Now();
        }
    }
}
