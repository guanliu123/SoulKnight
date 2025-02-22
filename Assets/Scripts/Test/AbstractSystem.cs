public class AbstractSystem
{
    public AbstractSystem()
    {
        OnInit();
    }
    
    protected virtual void OnInit(){}
    
    public virtual void OnUpdate()
    {
        
    }
}