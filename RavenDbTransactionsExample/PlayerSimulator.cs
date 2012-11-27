using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMO;
using Raven.Client;

namespace RavenDBTransactionExample
{
    /// <summary>
    /// Esta clase se encarga de simular que alguien esta utilizando un dado jugador.
    /// Su comportamiento es totalmente aleatorio (golpea aleatoriamente a alguien
    /// vivo en la Arena).
    /// </summary>
    public class PlayerSimulator
    {
        private static readonly Random random = new Random((int)DateTime.Now.ToBinary());

        /// <summary>
        /// El jugador utilizar en la simulación aleatoria.
        /// </summary>
        private readonly Jugador mJugador;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="PlayerSimulator" />.
        /// </summary>
        /// <returns></returns>
        public PlayerSimulator(Jugador jugador)
        {
            this.mJugador = jugador;
        }

        /// <summary>
        /// Es el jugador que este simulador utilizará para atacar
        /// a sus contrincantes.
        /// </summary>        
        public Jugador Jugador
        {
            get
            {
                return mJugador;
            }
        }

        /// <summary>
        /// Es el arena donde ocurre la pelea.
        /// </summary>        
        public Arena Arena
        {
            get;
            set;
        }

        /// <summary>
        /// Establece la sesión base de datos donde se persistirán los cambios
        /// por cada ataque (transacción)
        /// </summary>        
        public IDocumentSession Session
        {
            get;
            set;
        }

        /// <summary>
        /// Simula que pelea, atacando con una frecuencia determinara en milisegundos.
        /// </summary>
        /// <param name="frequency">La frecuencia con la que ataca este jugador en milisegundos..</param>        
        public async Task SimularAsync(int frequency)
        {
            do
            {
                Thread.Sleep(frequency);
                Jugador[] otros = this.Arena.JugadoresVivos
                                            .Where(jugador => jugador != this.Jugador)
                                            .ToArray();
                if (otros.Length > 0)
                {
                    int victimaIndex = random.Next(0, otros.Length);
                    // Inicia transaccion
                    this.Arena.LogDeAtaque.Add(this.Jugador.Ataca(otros[victimaIndex]));
                    this.Session.SaveChanges();
                    // Termina transaccion
                }
            } while (this.Jugador.EstaVivo && !this.Arena.BatallaTerminada);
        }
    }
}
