using System;
namespace MMO
{
    /// <summary>
    /// Esta clase representa un jugador dentro del 'campo de batalla'
    /// </summary>
    public class Jugador
    {
        private readonly static Random random = new Random((int)DateTime.Now.ToBinary());

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
        /// Attacks the specified victim.
        /// </summary>
        /// <param name="victim">The victim.</param>
        public void Attack(Jugador victim)
        {
            int damage = random.Next(1, 21);
            victim.Hp -= damage;
        }

        public override string ToString()
        {
            return string.Format("Jugador {{ id:'{0}', nombre:'{1}', hp:{2} }}", Id, Nombre, Hp);
        }
    }
}
