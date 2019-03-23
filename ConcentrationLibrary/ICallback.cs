using System.ServiceModel;
namespace ConcentrationLibrary
{
    public interface ICallback {
        [OperationContract(IsOneWay = true)] void RescanPlayers();
        [OperationContract(IsOneWay = true)] void CardFlipped(string btnXaml, Card card);
        [OperationContract(IsOneWay = true)] void GameStarted();
        [OperationContract(IsOneWay = true)] void GamePaused();
        [OperationContract(IsOneWay = true)] void GameFinished();
        [OperationContract(IsOneWay = true)] void NextPlayer();
    }
}
