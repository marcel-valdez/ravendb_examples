using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Raven.Client;
namespace MMO
{
    /// <summary>
    /// Es el arena donde pelearan los jugadores.
    /// </summary>
    public class Arena
    {
        private ICollection<RegistroDeAtaque> mLogDeAtaque;

        [JsonIgnore]
        private readonly List<Jugador> mJugadores;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="Arena" />.
        /// </summary>
        public Arena()
        {
            this.mJugadores = new List<Jugador>();
            this.LogDeAtaque = new List<RegistroDeAtaque>();
            this.JugadoresIds = new List<string>();
        }

        /// <summary>
        /// Es el Id que identifica a esta Arena, se permite que
        /// RavenDb lo genere automáticamente.
        /// </summary>        
        public string Id
        {
            get;
            set;
        }

        public List<string> JugadoresIds
        {
            get;
            set;
        }

        /// <summary>
        /// Son los jugadores que están actualmente en la Arena
        /// </summary>
        /// <value>Los jugadores en la Arena.</value>
        [JsonIgnore]
        public IEnumerable<Jugador> Jugadores
        {
            get
            {
                return mJugadores;
            }
        }

        /// <summary>
        /// Son los jugadores que quedan vivos en la Arena.
        /// </summary>
        /// <value>Los jugadores vivos.</value>
        [JsonIgnore]
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
        [JsonIgnore]
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
        public ICollection<RegistroDeAtaque> LogDeAtaque
        {
            get
            {
                return mLogDeAtaque;
            }

            set
            {
                mLogDeAtaque = value;
            }
        }

        /// <summary>
        /// Hidrata esta arena con los ids de jugadores que ya tiene.
        /// </summary>
        /// <param name="sesion">La sesion.</param>
        public void Hidratar(IDocumentSession sesion)
        {
            this.mJugadores.Clear();
            this.mJugadores.AddRange(sesion.Load<Jugador>(this.JugadoresIds));            
        }

        /// <summary>
        /// Agrega un jugador a la lista de participantes en esta arena
        /// </summary>
        /// <param name="jugadores">Los jugadores.</param>
        public void AgregarJugador(params Jugador[] jugadores)
        {
            foreach (var jugador in jugadores)
            {
                this.mJugadores.Add(jugador);
                this.JugadoresIds.Add(jugador.Id);
                jugador.Batallas.Add(this.Id);
            }
        }
    }
}
