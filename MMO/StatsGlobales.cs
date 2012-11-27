
namespace MMO
{
    /// <summary>
    /// Contiene las estadísticas globales para un jugador.
    /// </summary>
    public class StatsGlobales
    {
        /// <summary>
        /// El dano total causado por el jugador.
        /// </summary>        
        public int DanoCausado
        {
            get;
            set;
        }

        /// <summary>
        /// El dano total recibido por el jugador.
        /// </summary>
        public int DanoRecibido
        {
            get;
            set;
        }

        /// <summary>
        /// Número de jugadores que el jugador ha matado en batallas.
        /// </summary>
        public int JugadoresMatados
        {
            get;
            set;
        }

        /// <summary>
        /// El número de muertes que este jugador ha sufrido en batallas.
        /// </summary>        
        public int Muertes
        {
            get;
            set;
        }
    }
}
