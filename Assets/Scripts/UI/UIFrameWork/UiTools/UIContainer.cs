using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIContainer:MonoBehaviour
{
    [Serializable]
    public class UISet
    {
        //名字，可以不是物体本身的名字，方便取别名
        public string name = "";
        public Transform tf;
        public COMPONENT_TYPE type;
    }

    //使用和Unity组件一样的名字方便管理和理解
    public enum COMPONENT_TYPE
    {
        Transform,
        Text,
        TextMeshProUGUI,
        TMP_InputField,
        Button,
        Toggle,
        Image
    }
    
    [HideInInspector]
    public List<UISet> sets = new List<UISet>();
    
    //保存名字和组件的对应关系
    public Dictionary<string, Component> components = new Dictionary<string, Component>();

    private bool isInit=false;
    
    //初始化对应关系
    private void Init()
    {
        isInit = true;
        foreach (UISet t in sets)
        {
            if (string.IsNullOrEmpty(t.name))
            {
                continue;
            }

            if (t.tf == null)
            {
                continue;
            }
            if (components.ContainsKey(t.name))
            {
                continue;
            }

            switch (t.type)
            {
                case COMPONENT_TYPE.Text:
                {
                    components.Add(t.name,t.tf.GetComponent<Text>());
                }break;
                
                case COMPONENT_TYPE.Button:
                {
                    components.Add(t.name,t.tf.GetComponent<Button>());
                }break;
                
                case COMPONENT_TYPE.TextMeshProUGUI:
                {
                    components.Add(t.name,t.tf.GetComponent<TMPro.TextMeshProUGUI>());
                }break;
                
                case COMPONENT_TYPE.TMP_InputField:
                {
                    components.Add(t.name,t.tf.GetComponent<TMPro.TMP_InputField>());
                }break;
                
                case COMPONENT_TYPE.Toggle:
                {
                    components.Add(t.name,t.tf.GetComponent<Toggle>());
                }break;
                case COMPONENT_TYPE.Image:
                {
                    components.Add(t.name,t.tf.GetComponent<Image>());
                }break;
                default:
                {
                    components.Add(t.name,t.tf);
                }break;
            }
        }
    }

    public Component GetXXX(string name)
    {
        if (!isInit)
        {
            Init();
        }
        Component component;
        components.TryGetValue(name, out component);

        return component;
    }
}
