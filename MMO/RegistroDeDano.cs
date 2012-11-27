namespace MMO
{
    public class RegistroDeDano
    {
        /// <summary>
        /// El Id del jugador que ataco.
        /// </summary>
        /// <value>El Id del agresor.</value>
        public string AggresorId
        {
            get;
            set;
        }

        /// <summary>
        /// El dano recibido.
        /// </summary>
        /// <value>El dano</value>
        public int Dano
        {
            get;
            set;
        }
    }
}
