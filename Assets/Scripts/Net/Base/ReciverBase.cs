
#region

using System.Collections.Generic;

#endregion

public class ReciverBase
{
    private readonly Dictionary<ResponseId, ResponseBase> _map = new();
    
    public virtual void Regist()
    {
    }
    
    public void Add(ResponseId responseId, ResponseBase response)
    {
        _map.Add(responseId, response);
    }
    
    public ResponseBase Get(ResponseId responseId)
    {
        if (_map.ContainsKey(responseId))
        {
            return _map[responseId];
        }
        
        return null;
    }
}