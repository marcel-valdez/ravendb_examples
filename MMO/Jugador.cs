using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MMO
{
    /// <summary>
    /// Esta clase representa un jugador dentro del 'campo de batalla'
    /// </summary>
    public class Jugador
    {
        private readonly static Random random = new Random((int)DateTime.Now.ToBinary());

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="Jugador" />.
        /// </summary>
        public Jugador()
        {
            this.Estadisticas = new StatsGlobales();
            this.Batallas = new List<string>();
        }

        /// <summary>
        /// El Id del documento para el jugador. RavenDb lo asigna automáticamente.
        /// </summary>
        /// <value>El identificador del documento para el jugador.</value>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Representa el nombre del jugador dentro del juego.
        /// </summary>
        /// <value>El nombre del jugador.</value>
        public string Nombre
        {
            get;
            set;
        }

        /// <summary>
        /// Representa los puntos de vida del jugador.
        /// Dado que esta propiedad sólo es válida mientras ocurre una batalla,
        /// y no se requiere que se persista, con el atributo JsonIgnore, se le
        /// informa RavenDb que no se requiere que la persista en la BD.
        /// </summary>        
        public int Hp
        {
            get;
            set;
        }

        /// <summary>
        /// Ataca a la victima especificada.
        /// </summary>
        /// <param name="victima">La victima.</param>
        public RegistroDeAtaque Ataca(Jugador victima)
        {            
            int hpPrevio = victima.Hp;
            int fuerza = random.Next(1, 21);
            int dano = victima.RecibeAtaque(fuerza);
            this.Estadisticas.DanoCausado += dano;
            // Si este jugador mato al otro.
            if (hpPrevio > 0 && hpPrevio == dano)
            {
                // Incrementar sus estadísticas
                this.Estadisticas.JugadoresMatados++;
            }

            return new RegistroDeAtaque()
            {
                VictimaId = victima.Id,
                AgresorId = this.Id,
                Dano = dano
            };
        }

        /// <summary>
        /// Recibe el ataque del agresor, disminuye el dano con el 'escudo'.
        /// </summary>
        /// <param name="agresor">El agresor que golpea a este jugador.</param>
        /// <returns>El dano real causado por el agresor.</returns>
        protected int RecibeAtaque(int fuerza)
        {
            int hpPrevio = this.Hp;
            // Puede bloquear hasta 50% del dano.
            double escudo = random.NextDouble() * .5;
            int dano = (int)Math.Ceiling(fuerza * escudo);

            dano = dano > this.Hp ? this.Hp : dano;

            this.Hp -= dano;

            this.Estadisticas.DanoRecibido += dano;

            if (hpPrevio > 0 && !this.EstaVivo)
            {
                this.Estadisticas.Muertes++;
            }
            
            return dano;
        }

        /// <summary>
        /// Obtiene un booleano que indica si esta jugador [esta vivo].
        /// </summary>
        /// <value><c>true</c> si [esta vivo]; sino, <c>false</c>.</value>
        [JsonIgnore]
        public bool EstaVivo
        {
            get
            {
                return this.Hp > 0;
            }
        }

        /// <summary>
        /// Son las estadísticas globales de este jugador en todas sus
        /// batallas.
        /// </summary>        
        public StatsGlobales Estadisticas
        {
            get;
            private set;
        }

        /// <summary>
        /// La lista de batallas en las que ha participado este jugador.
        /// </summary>
        public List<string> Batallas
        {
            get;
            private set;
        }


        /// <summary>
        /// Regresa un string JSON que representa esta instancia. (Sin incluir logs de ataque)
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{{ Id:'{0}', Nombre:'{1}', Hp:{2} }}", Id, Nombre, Hp);
        }
    }
}
