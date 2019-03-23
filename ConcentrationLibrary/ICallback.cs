// Names:   Jordan Travaux & Abel Emun
// Date:    March 20, 2019
// Purpose: Definition of the ICallback interface to be implemented by the client

using System.ServiceModel;

namespace ConcentrationLibrary
{
    public interface ICallback {
        [OperationContract(IsOneWay = true)] void RescanPlayers();
        [OperationContract(IsOneWay = true)] void CardFlipped(string btnXaml);
        [OperationContract(IsOneWay = true)] void GameStarted();
        [OperationContract(IsOneWay = true)] void GamePaused();
        [OperationContract(IsOneWay = true)] void GameFinished(Player winner);
        [OperationContract(IsOneWay = true)] void NextPlayer();
    }
}
