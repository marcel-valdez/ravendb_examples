using System;
using NUnit.Framework;
using TestingTools.Core;
using TestingTools.Extensions;

namespace MMO.Test
{
    [TestFixture]
    public class JugadorTest
    {
        [TestAttribute]
        public void TestIfJugadorPuedeAtacar()
        {
            // Arrange
            var aggresor = new Jugador();
            var victim = new Jugador()
            {
                Hp = 100
            };

            // Act
            aggresor.Attack(victim: victim);

            // Assert
            Console.WriteLine(victim.Hp);            
            Verify.That(victim.Hp)
                  .IsLessThan(100)
                  .Now();
            // Reset
        }

        [TestAttribute]
        public void TestIfItCreatesAnAttackLogAndDamageLog()
        {
            // Arrange
            var aggresor = new Jugador();
            var victim = new Jugador()
            {
                Hp = 100
            };


            // Act


            // Assert


            // Reset

        }
    }
}
