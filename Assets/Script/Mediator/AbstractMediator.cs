using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbstractMediator
{
    protected List<AbstractController> controllers = new List<AbstractController>();
    protected List<AbstractSystem> systems = new List<AbstractSystem>();
    protected AbstractMediator() { }
    public void RegisterController<T>(T controller) where T : AbstractController
    {
        controllers.Add(controller);
    }
    public void RegisterSystem<T>(T system) where T : AbstractSystem
    {
        systems.Add(system);
    }
    public T GetController<T>() where T : AbstractController
    {
        var t = controllers.Where(controller => controller is T).ToArray();
        if (t == null || t.Length <= 0)
        {
            Debug.Log("找不到Controller!");
            return null;
        }
        var controller = t[0];
        if (controller != null) return controller as T;
        return default(T);
    }
    public T GetSystem<T>() where T : AbstractSystem
    {
        var t = systems.Where(system => system is T).ToArray();
        if (t == null || t.Length <= 0)
        {
            Debug.Log("找不到System！");
            return null;
        }
        AbstractSystem system = t[0];
        if (system != null) return system as T;
        return default(T);
    }
}
