using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/*
 * Реализовать две задачи: Task1 и Task2. В качестве параметров задачи должны
принимать CancellationToken. Первая задача должна ожидать одну секунду, а после
выводить в консоль сообщение о своём завершении. Вторая задача должна
ожидать 60 кадров, а после — выводить сообщение в консоль.
 * 
 */
public class AsyncExample : MonoBehaviour
{
    static CancellationTokenSource cancelTokenSource ;
    CancellationToken cancelToken;

    private void Start()
    {
        cancelTokenSource = new CancellationTokenSource();
        cancelToken = cancelTokenSource.Token;
        Task task1 = new Task(() => YieldAsync(cancelToken));
        Task task2 = new Task(() => DelayAsync(cancelToken));


        task1.Start();
        task2.Start();
        Thread.Sleep(10);
        cancelTokenSource.Cancel();
        cancelTokenSource.Dispose();
    }
    
  

    async Task YieldAsync(CancellationToken cancelToken)
    {
        var times = 60;
        while (times > 0)
        {
            if (cancelToken.IsCancellationRequested)
            {
                Debug.Log("Операция прервана токеном.");
                return;
            }
            times--;
            Debug.Log(times);
            await Task.Yield();

        }
        Debug.Log("Вторая задача - Exit");
      
    }
    async Task DelayAsync(CancellationToken cancelToken)
    {
      if (cancelToken.IsCancellationRequested)
      {
            Debug.Log("Операция прервана токеном.");
            return ;
      }
        await Task.Delay(1000);
        Debug.Log("Первая задача - Exit");
       
    }

   
}
