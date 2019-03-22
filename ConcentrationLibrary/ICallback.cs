using System.ServiceModel;
namespace ConcentrationLibrary
{
    public interface ICallback {
        [OperationContract] void PointScored();
        [OperationContract] void CardFlipped(string btnXaml, Card card);
        [OperationContract] void PlayerJoinedGame();
        [OperationContract] void GameStarted();
        [OperationContract] void GamePaused();
        [OperationContract] void GameFinished();
        [OperationContract] void NextPlayer();
    }
}
