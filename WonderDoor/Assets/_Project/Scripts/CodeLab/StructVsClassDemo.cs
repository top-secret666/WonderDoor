using UnityEngine;


public struct PointStruct {  public int x; }
public class PointClass { public int x; }

public class StructVsClassDemo : MonoBehaviour 
{

 private void Start()
    {
        UnityEngine.Debug.Log("StructVsClassDemo started");
        PointStruct c1 = new PointStruct();
        PointClass c2 = new PointClass();
        Test1(c1);
        Test2(c2);
        UnityEngine.Debug.Log("StructVsClassDemo finished");
    }
    private void Test1(PointStruct c1){
        c1.x = 1;
        PointStruct  c2 = c1;
        UnityEngine.Debug.Log($"{c1.x} + {c2.x} = 2! ЕЕЕЙ!!!!");
        UnityEngine.Debug.Log($"Увеличиваем x на 1");
        c2.x++;
        UnityEngine.Debug.Log($"Такова struct чувак увы но это  копия а не ссылка теперь -> {c1.x} + {c2.x} != 4");
    }
    private void Test2(PointClass c2){
        c2.x = 1;
        PointClass c1 = c2;
        UnityEngine.Debug.Log($"{c1.x} + {c2.x} = 2! ЕЕЕЙ!!!!");
        UnityEngine.Debug.Log($"Увеличиваем x на 1");
        c1.x++;
        UnityEngine.Debug.Log($"Опааа я вижу что  ты решил взять сlass вот уже согласен ->  {c1.x} + {c2.x} = 4");
    }
}