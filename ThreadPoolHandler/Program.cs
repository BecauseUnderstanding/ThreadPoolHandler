using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPoolHandler
{
    public class Person
    {
        public Person(int id, string name) { Id = id; Name = name; }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            int j = 0;
            //前面是辅助(也就是所谓的工作者)线程，后面是I/O线程
            ThreadPool.GetMaxThreads(out i, out j);
            Console.WriteLine(i.ToString() + "   " + j.ToString()); //默认都是1000

            //获取空闲线程，由于现在没有使用异步线程，所以为空
            ThreadPool.GetAvailableThreads(out i, out j);
            Console.WriteLine(i.ToString() + "   " + j.ToString()); //默认都是1000
            Console.WriteLine("-------------------------------------------------------------------------------");

            //ThreadPool.QueueUserWorkItem(new WaitCallback(RunWorkerThread));


            Console.WriteLine("1.使用ThreadPool.QueueUserWorkItem开启一个异步线程，这个方法启动的工作者线程仅仅适合于带单个参数和无返回值的情况。");

            Person p = new Person(1, "刘备");
            //启动工作者线程
            ThreadPool.QueueUserWorkItem(new WaitCallback(RunWorkerThread), p);


            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("2.使用委托的BeginInvoke与EndInvoke委托异步调用线程，但是调用EndInvoke(IAsyncResult)获取运行结果，一旦调用了EndInvoke，即使结果还没来得及返回，主线程也阻塞等待了，添加上while(!result.IsCompleted)来判断开启的异步线程是否执行完毕");
            //BeginInvoke与EndInvoke委托异步调用线程
            //建立委托
            MyDelegate myDelegate = new MyDelegate(GetString);
            //异步调用委托，除最后两个参数外，前面的参数都可以传进去
            IAsyncResult result = myDelegate.BeginInvoke("刘备", 22, null, null);　　//IAsynResult还能轮询判断，功能不弱

            while (!result.IsCompleted)
            {
                Thread.Sleep(500);
                Console.WriteLine("异步线程还没完成，主线程干其他事!");
            }

            Console.WriteLine("主线程继续工作!");
            //调用EndInvoke(IAsyncResult)获取运行结果，一旦调用了EndInvoke，即使结果还没来得及返回，主线程也阻塞等待了
            //注意获取返回值的方式
            string data = myDelegate.EndInvoke(result);
            Console.WriteLine(data);
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("3.添加回掉方法，供异步线程完成回调的方法，这样主线程也不用阻塞");

            //定义异步线程完成回调的方法   这样主线程不用阻塞
            Person p2 = new Person(2, "关羽");
            IAsyncResult result1 = myDelegate.BeginInvoke("刘备", 23, new AsyncCallback(Completed), p2);
            //主线程可以继续工作而不需要等待
            Console.WriteLine("我是主线程，我干我的活，不再理你！");
            Console.WriteLine("我是主线程，我干我的活，不再理你！");
            Console.WriteLine("我是主线程，我干我的活，不再理你！");
            Console.WriteLine("我是主线程，我干我的活，不再理你！");
            Console.WriteLine("我是主线程，我干我的活，不再理你！");
            Thread.Sleep(5000);



            Console.ReadKey();

            
        }


        static void RunWorkerThread(object obj)
        {
            Console.WriteLine("RunWorkerThread开始工作");
            Console.WriteLine("工作者线程启动成功!");


            Thread.Sleep(200);
            Console.WriteLine("线程池线程开始!");
            Person p = obj as Person;
            Console.WriteLine(p.Name);
        }


        delegate string MyDelegate(string name, int age);

        static string GetString(string name, int age)
        {
            Thread.CurrentThread.Name = "异步线程";
            //注意，如果不设置为前台线程，则主线程完成后就直接卸载程序了
            Thread.CurrentThread.IsBackground = false;
            Console.WriteLine("我是不是线程池线程" + Thread.CurrentThread.IsThreadPoolThread);
            Thread.Sleep(2000);
            return string.Format("我是{0}，今年{1}岁!", name, age);
        }


        //供异步线程完成回调的方法
        static void Completed(IAsyncResult result)
        {
            //获取委托对象，调用EndInvoke方法获取运行结果
            AsyncResult _result = (AsyncResult)result;
            MyDelegate myDelegaate = (MyDelegate)_result.AsyncDelegate;
            //获得参数
            string data = myDelegaate.EndInvoke(_result);
            Console.WriteLine(data);

            Person p = result.AsyncState as Person;
            Console.WriteLine("传过来的参数是：" + p.Name);
            //异步线程执行完毕
            Console.WriteLine("异步线程完成咯！");
            Console.WriteLine("回调函数也是由" + Thread.CurrentThread.Name + "调用的！");
        }


    }

  
}
