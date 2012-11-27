namespace MMO
{
    public class RegistroDeAtaque
    {
        /// <summary>
        /// El Id de la victima atacada.       
        /// </summary>
        /// <value>El id de la victima.</value>
        public string VictimaId
        {
            get;
            set;
        }

        /// <summary>
        /// Es el dano provocado a la victima.
        /// </summary>
        /// <value>El dano provocado.</value>
        public int Dano
        {
            get;
            set;
        }
    }
}
