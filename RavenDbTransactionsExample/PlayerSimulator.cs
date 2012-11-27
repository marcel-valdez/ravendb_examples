using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMO;
using Raven.Abstractions.Exceptions;
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
        public Task SimularAsync(int frequency)
        {
            return Task.Factory.StartNew(() =>
                {

                    do
                    {
                        lock (random)
                        {
                            Jugador victima = null;
                            try
                            {
                                // Se obtienen los oponentes que siguen vivos
                                Jugador[] oponentes = this.Arena.JugadoresVivos
                                                                .Where(jugador => jugador.Id != this.Jugador.Id)
                                                                .ToArray();
                                if (oponentes.Length > 0)
                                {
                                    // Se obtiene un índice aleatorio entre las víctimas.
                                    int victimaIndex = random.Next(0, oponentes.Length);
                                    // Se obtiene la víctima aleatoria.
                                    victima = oponentes[victimaIndex];
                                    // Si la víctima ya no está viva
                                    if (!victima.EstaVivo)
                                    {
                                        // entonces se cancela el ataque.
                                        continue;
                                    }

                                    // Si no esta vivo el jugador
                                    if (!this.Jugador.EstaVivo)
                                    {
                                        // Se detiene la simulación para este jugador.
                                        break;
                                    }

                                    // Se guarda el log de ataque en el Arena
                                    this.Arena.LogDeAtaque.Add(this.Jugador.Ataca(victima));
                                    // Se guardan los cambios en la sesióna
                                    this.Session.SaveChanges();

                                    this.Session.Advanced.Evict(Arena);
                                    this.Session.Advanced.Evict(Jugador);
                                }
                            }
                            catch (Exception x)
                            {
                                if (x is AggregateException || x is ConcurrencyException ||
                                    (x.InnerException != null && x.InnerException is AggregateException || x.InnerException is ConcurrencyException))
                                {
                                    this.Session.Advanced.GetEtagFor(this.Arena);
                                    Console.WriteLine("Jugador: {0}, canceló una transacción debido al algoritmo 2PC.", this.Jugador.Id);                                    
                                }
                                else
                                {
                                    throw x;
                                }
                            }

                            int espera = random.Next(0, frequency);
                            Console.WriteLine("Jugador {0}, tardó: {1} ms", this.Jugador.Id, espera);
                            Thread.Sleep(espera);
                            // Se sincroniza el estado del jugador.
                            Refrescar(this.Jugador);
                            // Se sincroniza el estado de la arena.
                            Refrescar(this.Arena);
                        }
                    } while (this.Jugador.EstaVivo && !this.Arena.BatallaTerminada);
                });
        }

        /// <summary>
        /// Refresca el estado de un jugador (de ser necesario)
        /// </summary>
        /// <param name="jugador">El jugador.</param>
        private void Refrescar(Jugador jugador)
        {
            // Entonces refrescarlos
            this.Session.Advanced.Refresh(jugador);
        }

        /// <summary>
        /// Refresca el estado del Arena con el servidor (de ser necesario)
        /// </summary>
        private void Refrescar(Arena arena)
        {
            // Recargarla
            //this.Arena = this.Session.Include<Arena>(a => a.JugadoresIds)
            //            .Load(arena.Id);
            this.Session.Advanced.Refresh(this.Arena);
            // Re-hidratarla (llenarle sus elementos)
            this.Arena.Hidratar(this.Session);
        }
    }
}
