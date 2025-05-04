public class MemoryModelCommand : Singleton<MemoryModelCommand>
{
    private MemoryModel model;
    private MemoryModelCommand()
    {
        model = ModelContainer.Instance.GetModel<MemoryModel>();
    }
    public void AddMoney(int addition)
    {
        model.Money += addition;
    }
    public void EnterOnlineMode(string roomName,bool isHomeOwner=false)
    {
        model.isOnlineMode.Value = true;
        model.RoomName=roomName;
        model.isHomeOwner = isHomeOwner;
        SceneModelCommand.Instance.LoadScene(SceneName.MiddleScene);
    }
    
    public void ExitOnlineMode()
    {
        model.isOnlineMode.Value = false;
        model.isHomeOwner = false;
        model.RoomName=string.Empty;
    }
    public void InitMemoryModel()
    {
        model.PlayerAttr = null;
        model.Money = 0;
        model.Stage = 1;
    }
    public int GetBigStage()
    {
        return (model.Stage - 1) / 5 + 1;
    }
    public int GetSmallStage()
    {
        return model.Stage - (GetBigStage() - 1) * 5;
    }
}
