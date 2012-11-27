using MMO;
using NUnit.Framework;
using Raven.Client.Document;
using Raven.Client.Embedded;
using TestingTools.Core;
using TestingTools.Extensions;

namespace RavenDBTransactionExample.Test
{

    [TestFixture]
    public class PlayerSimulatorTest
    {
        /// <summary>
        /// Tests if it initializes correctly.
        /// </summary>
        [TestAttribute]
        public void TestIfItInitializesCorrectly()
        {
            using (var dstore = GetDocumentStore())
            {
                // Arrange
                PlayerSimulator target;

                // Act
                target = new PlayerSimulator(new Jugador())
                {
                    Arena = new Arena(),
                    DocumentStore = dstore
                };

                // Assert
                Verify.That(target.Jugador).IsNotNull().Now();
                Verify.That(target.Arena).IsNotNull().Now();
                Verify.That(target.DocumentStore).IsNotNull().Now();

                // Reset
            }

        }

        private static DocumentStore GetDocumentStore()
        {
            var documentStore = new EmbeddableDocumentStore
            {
                RunInMemory = true
            };

            documentStore.Initialize();

            return documentStore;
        }
    }
}
