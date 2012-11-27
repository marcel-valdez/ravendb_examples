using System;
using System.Collections.Generic;

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
            this.LogDeAtaque = new List<RegistroDeAtaque>();
            this.LogDeDano = new List<RegistroDeDano>();
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

        /// <summary>
        /// Contiene los registros de daño recibido por este jugador.
        /// </summary>
        /// <value>Los registros de daño recibido.</value>
        public List<RegistroDeDano> LogDeDano
        {
            get;
            set;
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
        /// </summary>
        /// <value>Los puntos de vida, un número mayor o igual a 0.</value>
        public int Hp
        {
            get;
            set;
        }

        /// <summary>
        /// Ataca a la victima especificada.
        /// </summary>
        /// <param name="victima">La victima.</param>
        public void Ataca(Jugador victima)
        {
            int fuerza = random.Next(1, 21);
            int dano = victima.RecibeAtaque(this, fuerza);
            this.LogDeAtaque.Add(new RegistroDeAtaque()
            {
                VictimaId = victima.Id,
                Dano = dano
            });
        }

        /// <summary>
        /// Recibe el ataque del agresor, disminuye el dano con el 'escudo'.
        /// </summary>
        /// <param name="agresor">El agresor que golpea a este jugador.</param>
        /// <returns>El dano real causado por el agresor.</returns>
        protected int RecibeAtaque(Jugador agresor, int fuerza)
        {
            // Puede bloquear hasta 50% del dano.
            double escudo = random.NextDouble() * .5;
            int dano = (int)Math.Ceiling(fuerza * escudo);

            dano = dano > this.Hp ? this.Hp : dano;

            this.Hp -= dano;

            this.LogDeDano.Add(new RegistroDeDano()
            {
                AggresorId = agresor.Id,
                Dano = dano
            });

            return dano;
        }

        /// <summary>
        /// Obtiene un booleano que indica si esta jugador [esta vivo].
        /// </summary>
        /// <value><c>true</c> si [esta vivo]; sino, <c>false</c>.</value>
        public bool EstaVivo
        {
            get
            {
                return this.Hp > 0;
            }
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
