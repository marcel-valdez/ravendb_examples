using System.Collections.Generic;
using System.Linq;
namespace MMO
{
    /// <summary>
    /// Es el arena donde pelearan los jugadores.
    /// </summary>
    public class Arena
    {
        public Arena()
        {
            this.Jugadores = new List<Jugador>();
            this.LogDeAtaque = new List<RegistroDeAtaque>();
        }

        /// <summary>
        /// Son los jugadores que están actualmente en la Arena
        /// </summary>
        /// <value>Los jugadores en la Arena.</value>
        public List<Jugador> Jugadores
        {
            get;
            private set;
        }

        /// <summary>
        /// Son los jugadores que quedan vivos en la Arena.
        /// </summary>
        /// <value>Los jugadores vivos.</value>
        public Jugador[] JugadoresVivos
        {
            get
            {
                return this.Jugadores.Where(jugador => jugador.EstaVivo).ToArray();
            }
        }

        /// <summary>
        /// Determina si la batalla ha terminado.
        /// </summary>
        /// <value><c>true</c> si es una [batalla terminada]; sino, <c>false</c>.</value>
        public bool BatallaTerminada
        {
            get
            {
                return this.JugadoresVivos.Length <= 1;
            }
        }

        /// <summary>
        /// Contiene los registros de ataques hechos por este jugador.
        /// </summary>
        /// <value>Los registros de ataque.</value>
        public List<RegistroDeAtaque> LogDeAtaque
        {
            get;
            set;
        }
    }
}
