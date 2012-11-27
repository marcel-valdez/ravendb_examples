namespace MMO
{
    public struct RegistroDeAtaque
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
        /// Es el Id del agresor.
        /// </summary>
        /// <value>El Id del agresor.</value>
        public string AgresorId
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
