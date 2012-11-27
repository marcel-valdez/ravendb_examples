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
        }

        /// <summary>
        /// Son los jugadores que están actualmente en la Arena
        /// </summary>
        /// <value>Los jugadores en la Arena.</value>
        public List<Jugador> Jugadores
        {
            get;
            set;
        }

        public Jugador[] JugadoresVivos
        {
            get
            {
                return this.Jugadores.Where(jugador => jugador.EstaVivo).ToArray();
            }
        }
    }
}
