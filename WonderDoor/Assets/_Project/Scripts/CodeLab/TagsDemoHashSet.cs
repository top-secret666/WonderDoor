using UnityEngine;
using System.Collections.Generic;

public class TagsDemoHashSet : MonoBehaviour 
{
    private HashSet<string> tags;

    private void Start()
    {
        UnityEngine.Debug.Log("TagsDemoHashSet started");
        tags = new HashSet<string>();
        AddTag();
        UnityEngine.Debug.Log("TagsDemoHashSet finished");
    }
    
    private void AddTag()
    {
        
         for(int i = 0; i <= 4; i++)
        {
            string testingTag = "Фигня";
            if(!(tags.Add(testingTag))){
            UnityEngine.Debug.Log("Одна и таже Фигня дважды не добавится!!!");
            }
            else
            {
            UnityEngine.Debug.Log("Первый раз не пидорас !!!");
            }
        }
    }
}