using System;
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
            Verify.That<List<RegistroDeAtaque>>(target.LogDeAtaque).IsNotNull().Now();
            Verify.That<List<RegistroDeDano>>(target.LogDeDano).IsNotNull().Now();
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
            agresor.Ataca(victima);
            

            // Assert
            Verify.That(agresor.LogDeAtaque.Count).IsEqualTo(1).Now();
            Verify.That(victima.LogDeDano.Count).IsEqualTo(1).Now();

            RegistroDeAtaque ataque = agresor.LogDeAtaque[0];
            RegistroDeDano dano = victima.LogDeDano[0];

            Verify.That(ataque).IsNotNull().Now();
            Verify.That(dano).IsNotNull().Now();

            Verify.That(ataque.Dano).IsEqualTo(dano.Dano).Now();
            Verify.That(ataque.VictimaId).IsEqualTo(victima.Id).Now();
            Verify.That(dano.AggresorId).IsEqualTo(agresor.Id).Now();
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
