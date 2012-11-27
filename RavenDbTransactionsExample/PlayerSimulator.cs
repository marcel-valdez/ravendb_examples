// ***********************************************************************
// Assembly         : RavenDbTransactionsExample
// Author           : Marcel
// Created          : 11-27-2012
//
// Last Modified By : Marcel
// Last Modified On : 11-27-2012
// ***********************************************************************
// <copyright file="PlayerSimulator.cs" company="Marcel Valdez">
//     Marcel Valdez. All rights reserved.
// </copyright>
// ***********************************************************************
using Raven.Client.Document;
using MMO;

namespace RavenDBTransactionExample.Test
{
    /// <summary>
    /// Esta clase se encarga de simular que alguien esta utilizando un dado jugador.
    /// Su comportamiento es totalmente aleatorio (golpea aleatoriamente a alguien
    /// vivo en la Arena).
    /// </summary>
    public class PlayerSimulator
    {
        /// <summary>
        /// El jugador utilizar en la simulación aleatoria.
        /// </summary>
        private readonly Jugador mJugador;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="PlayerSimulator" />.
        /// </summary>
        /// <returns></returns>
        public PlayerSimulator(Jugador jugador)
        {
            this.mJugador = jugador;
        }

        public Jugador Jugador
        {
            get
            {
                return mJugador;
            }
        }

        public Arena Arena
        {
            get;
            set;
        }

        public DocumentStore DocumentStore
        {
            get;
            set;
        }
    }
}
