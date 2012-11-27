using System.Collections.Generic;
using NUnit.Framework;
using TestingTools.Core;
using TestingTools.Extensions;

namespace MMO.Test
{
    [TestFixture]
    public class JugadorTest
    {
        [TestAttribute]
        public void TestIfItInitializesCorrectly()
        {
            // Arrange
            Jugador target;

            // Act
            target = new Jugador();

            // Assert                        
            Verify.That(target.Nombre).IsNull().Now();
        }

        [TestAttribute]
        public void TestIfJugadorPuedeAtacar()
        {
            // Checa idempotencia de la prueba.
            for (int i = 0; i < 100; i++)
            {
                // Arrange
                var agresor = new Jugador();
                var victima = new Jugador()
                {
                    Hp = 100
                };

                // Act
                agresor.Ataca(victima: victima);

                // Assert                
                Verify.That(victima.Hp)
                      .IsLessThan(100)
                      .Now();
            }
        }

        [TestAttribute]
        public void TestIfItCreatesAnAttackLogAndDamageLog()
        {
            // Arrange
            var agresor = new Jugador()
            {
                Id = "agresor "
            };

            var victima = new Jugador()
            {
                Id = "victima",
                Hp = 100
            };

            // Act
            RegistroDeAtaque ataque = agresor.Ataca(victima);
            

            // Assert                        
            Verify.That(ataque).IsNotNull().Now();            
            Verify.That(ataque.Dano).IsEqualTo(100 - victima.Hp).Now();
            Verify.That(ataque.VictimaId).IsEqualTo(victima.Id).Now();
            Verify.That(ataque.AgresorId).IsEqualTo(agresor.Id).Now();
        }

        [TestAttribute]
        public void TestIfItNeverHasNegativeHp()
        {
            // Arrange
            var agresor = new Jugador();
            var victima = new Jugador()
            {
                Hp = 0
            };

            // Act
            for (int i = 0; i < 10; i++)
            {
                agresor.Ataca(victima);
            }

            // Assert
            Verify.That(victima.Hp).IsGreaterThanOrEqual(0).Now();
        }

        [TestAttribute]
        public void TestIfItKnowsWhenItsDeadOrAlive()
        {
            // Arrange
            var target = new Jugador()
            {
                Hp = 1
            };

            // Pre-Assert
            Verify.That(target.EstaVivo).IsTrue().Now();

            // Act
            target.Hp = 0;

            // Assert
            Verify.That(target.EstaVivo).IsFalse().Now();
        }
    }
}
