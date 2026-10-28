using UnityEngine;
using System.Text; 
using System.Diagnostics;

public class StringsDemo : MonoBehaviour 
{
    private string text;
    private Stopwatch sw;
    private StringBuilder sb;
    private void Start()
    {
        UnityEngine.Debug.Log("StringsDemo started");
        sw = new Stopwatch();
        sb = new StringBuilder();
        Test1();
        Test2();
        UnityEngine.Debug.Log("StringsDemo finished");
    }
    
    private void Test1()
    {
        text = "";
        sw.Start();
        for(int i = 0; i < 10000; i++)
        {
            text += "Фигня";
        }
        sw.Stop();
        UnityEngine.Debug.Log("Test1: " + sw.Elapsed.TotalMilliseconds + "ms");
        sw.Reset();
    }
  private void Test2()
    {
        text = "";
        sw.Start();
        for(int i = 0; i < 10000; i++)
        {
           sb.Append("ФИГНЯ");
        }
        sw.Stop();
        UnityEngine.Debug.Log("Test2: " + sw.Elapsed.TotalMilliseconds + "ms");
        sw.Reset();
    }
}